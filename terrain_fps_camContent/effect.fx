float4x4 xViewProjection;
float4x4 xWorld;
float4x4 xShViewProjection;

float3 xEye;
float3 xShEye;
float3	 xLightDirection;
bool	 xEnableLight;
float	 xLightPower;
float	 xAmbient;
float3   xCamPos;

bool	 xEnableFog;
float	 xFogStart,xFogEnd;
float4	 xFogColor;

bool	 xEnablePLight;
float3	 xPLight;
float    xPLightScatter;

bool     xGrayScale;
bool     xInvertedColors;
bool     xUnderWater;
bool     xSpecular;

Texture xTexture0;
sampler TextureSampler0 = sampler_state { 
										texture = <xTexture0>; 
										magfilter = anisotropic; 
										minfilter = anisotropic; 
										mipfilter = linear; 
										AddressU = wrap; 
										AddressV = wrap;
									   };

Texture xTextureSh;
sampler2D mapSampler = sampler_state { 
										texture = <xTextureSh>; 
										magfilter = anisotropic; 
										minfilter = anisotropic; 
										mipfilter = linear; 
										AddressU = clamp; 
										AddressV = clamp;
									   };
float xDepthBias;

float offset_lookup( sampler2D map,
                     float2 loc,
                     float2 offset,
					 float scale,
					 float realDistance)
{
	float BiasedDistance = realDistance + xDepthBias;
	float LightEfficiency = 0;

	LightEfficiency = BiasedDistance <= tex2D(map, loc + offset / scale );

	return LightEfficiency;
}


uint xClipping;
float xClipHeight;

struct VertexToPixel
{
	float4 Position		   : POSITION;
	float  LightingFactor  : TEXCOORD0;
	float2 TextureCoords   : TEXCOORD1; 
	float4 TextureWeights  : TEXCOORD2;
	float  Depth		   : TEXCOORD3;
	float3 Position3D	   : TEXCOORD4;
	float3 Normal3D		   : TEXCOORD5;
	float  LightDistance   : TEXCOORD6;
	float4 ShadowMapSamplingPos : TEXCOORD7;
	float4 Color : COLOR;
};

struct PixelToFrame
{
	float4 Color		: COLOR0;
};


float3 GetLightDirection(float3 pos3D, float3 lightPos)
{
	return normalize(pos3D - lightPos);
}



//------- Technique: Textured --------

VertexToPixel TexturedVS( float4 inPos : POSITION, float3 inNormal: NORMAL, float2 inTexCoords: TEXCOORD0)
{	
	VertexToPixel Output = (VertexToPixel)0;
	float4x4 preWorldViewProjection = mul (xWorld, xViewProjection);
    
	Output.Position = mul(inPos, preWorldViewProjection);
	
	Output.LightDistance = length(Output.Position3D - xPLight);
	
	float3 Normal = normalize(mul(normalize(inNormal), xWorld));	
	Output.LightingFactor = 1;


			Output.Position3D = mul(inPos, xWorld);
			Output.Normal3D = normalize(mul(inNormal, (float3x3)xWorld));
			Output.LightDistance = length(Output.Position3D - xPLight);

    Output.Depth = Output.Position.z;

	Output.TextureCoords = inTexCoords;

	
	Output.ShadowMapSamplingPos = mul(inPos, mul(xWorld,xShViewProjection));

	return Output;    
}

PixelToFrame TexturedPS(VertexToPixel PSIn)
{
	PixelToFrame Output = (PixelToFrame)0;		
	
	
	float blendDistance = 10.00f;
	float blendWidth = 20.0f;
	float blendFactor = clamp((PSIn.Depth-blendDistance)/blendWidth, 0, 1);

	float4 farColor;
	farColor = tex2D(TextureSampler0, PSIn.TextureCoords/10);
    
	float4 nearColor;
	nearColor = tex2D(TextureSampler0, PSIn.TextureCoords);

	Output.Color=lerp(nearColor,farColor, blendFactor);

	if(xSpecular)
	{
		float3 eyeVector = normalize(xCamPos - PSIn.Position3D);
		float3 reflectionVector = -reflect(xLightDirection, PSIn.Normal3D);
		float specular = dot(normalize(reflectionVector), normalize(eyeVector));
		specular = pow(abs(specular), 50);        
		Output.Color.rgb += abs(specular);
	}

	if(xEnablePLight)
	{
		float3 lightDir = normalize(PSIn.Position3D - xPLight);
		float diffuseLightingFactor = dot(-lightDir, PSIn.Normal3D);
		diffuseLightingFactor=saturate(diffuseLightingFactor);
		float power = saturate((1 - PSIn.LightDistance / xPLightScatter)) * xLightPower;
		diffuseLightingFactor*=power;

		Output.Color.rgb *= diffuseLightingFactor + xAmbient;
	}
	if(xEnableLight)
	{
		float3 normal = PSIn.Normal3D;
		float3 lightdirection = GetLightDirection(PSIn.Position3D, xShEye);

		//SHADOW
		float2 ShTex;
			ShTex.x = PSIn.ShadowMapSamplingPos.x/PSIn.ShadowMapSamplingPos.w/2.0f +0.5f;
			ShTex.y = -PSIn.ShadowMapSamplingPos.y/PSIn.ShadowMapSamplingPos.w/2.0f +0.5f;

		float diffuseLightingFactor1 = 0;
		if ((saturate(ShTex.x) == ShTex.x) && (saturate(ShTex.y) == ShTex.y))
		{
			float depthStoredInShadowMap = tex2D(mapSampler,ShTex).x;
			float realDistance = PSIn.ShadowMapSamplingPos.z/PSIn.ShadowMapSamplingPos.w;
			if (realDistance + 0 <= depthStoredInShadowMap+xDepthBias)
			{
				diffuseLightingFactor1 = dot(-lightdirection, normal);
				diffuseLightingFactor1 = saturate(diffuseLightingFactor1);
				diffuseLightingFactor1 *= 1.5f;
			}

			/*
			float realDistance = PSIn.ShadowMapSamplingPos.z/PSIn.ShadowMapSamplingPos.w;
			float sum = 0;
			float scale = 2048.0f;
			
			for (float y = -1.5f; y <= 1.5f; y += 1.0f)
				for (float x = -1.5f; x <= 1.5f; x += 1.0f)
				{
					sum += offset_lookup(mapSampler, ShTex, float2(x, y), scale, realDistance);
				}
			

			diffuseLightingFactor1 = dot(-lightdirection, normal);
			diffuseLightingFactor1 = saturate(diffuseLightingFactor1);
			diffuseLightingFactor1 *= 1.5f;
			diffuseLightingFactor1 *= sum / 16.0f;
			*/

		}
		
		Output.Color*=diffuseLightingFactor1+xAmbient;

		//Output.Color.rgb *= saturate(PSIn.LightingFactor * xLightPower) + xAmbient;
	}

	
	if(xEnableFog)
	{
		float l = saturate((PSIn.Depth - xFogStart) / (xFogEnd - xFogStart));
		float3 Color = lerp(Output.Color,xFogColor, l);
		Output.Color.x = Color.x;
		Output.Color.y = Color.y;
		Output.Color.z = Color.z;
	}
	

	if(xUnderWater)
	{
		Output.Color.z+=0.2f; Output.Color.x-=0.2f; Output.Color.y-=0.2f;
	}
	if(xGrayScale)
	{
		float avg=(Output.Color.x+Output.Color.y+Output.Color.z)/3;
		Output.Color.x=avg;
		Output.Color.y=avg;
		Output.Color.z=avg;
	}
	if(xInvertedColors)
	{
		Output.Color.x=1-Output.Color.x;
		Output.Color.y=1-Output.Color.y;
		Output.Color.z=1-Output.Color.z;
	}
	
	if(xClipping==1)
	{
		if(PSIn.Position3D.y<xClipHeight)
		{
			Output.Color*=-1;
			clip(Output.Color);
		}
	}
	if(xClipping==2)
	{
		if(PSIn.Position3D.y>xClipHeight)
		{
			Output.Color*=-1;
			clip(Output.Color);
		}
	}

	return Output;
}

technique Textured
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 TexturedVS();
		PixelShader  = compile ps_3_0 TexturedPS();
	}
}


//------- Technique: ColorLight --------

VertexToPixel ColoredVS( float4 inPos : POSITION, float3 inNormal: NORMAL, float4 inColor: COLOR)
{	
	VertexToPixel Output = (VertexToPixel)0;
	float4x4 preWorldViewProjection = mul (xWorld, xViewProjection);
    
	Output.Position = mul(inPos, preWorldViewProjection);
	Output.Color = inColor;
	Output.Depth = Output.Position.z;
	
	float3 Normal = normalize(mul(normalize(inNormal), xWorld));	
	Output.LightingFactor = 1;
	
			Output.Position3D = mul(inPos, xWorld);
			Output.Normal3D = normalize(mul(inNormal, (float3x3)xWorld));
			Output.LightDistance = length(Output.Position3D - xPLight);

    
	return Output;    
}

PixelToFrame ColoredPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
    
	Output.Color = PSIn.Color;

	if(xSpecular)
	{
		float3 eyeVector = normalize(xCamPos - PSIn.Position3D);
		float3 reflectionVector = reflect(xLightDirection, PSIn.Normal3D);
		float specular = dot(normalize(reflectionVector), normalize(eyeVector));
		specular = pow(abs(specular), 100);        
		Output.Color.rgb += specular;
	}
	
	if(xEnablePLight)
	{
		float3 lightDir = normalize(PSIn.Position3D - xPLight);
		float diffuseLightingFactor = dot(-lightDir, PSIn.Normal3D);
		diffuseLightingFactor=saturate(diffuseLightingFactor);
		float power = clamp((1 - PSIn.LightDistance / xPLightScatter),0,1) * xLightPower;
		diffuseLightingFactor*=power;

		Output.Color.rgb *= diffuseLightingFactor + xAmbient;
	}
	if(xEnableLight)
	{
		float3 normal = PSIn.Normal3D;
		float3 lightdirection = GetLightDirection(PSIn.Position3D, xShEye);

		float diffuseLightingFactor1 = dot(-lightdirection, normal);
			diffuseLightingFactor1 = saturate(diffuseLightingFactor1);
			diffuseLightingFactor1 *= 1.5f;
		
		Output.Color*=diffuseLightingFactor1+xAmbient;
	}

	
	if(xEnableFog)
	{
		float l = saturate((PSIn.Depth - xFogStart) / (xFogEnd - xFogStart));
		float3 Color = lerp(Output.Color,xFogColor, l);
		Output.Color.x = Color.x;
		Output.Color.y = Color.y;
		Output.Color.z = Color.z;
	}
	

	if(xUnderWater)
	{
		Output.Color.z+=0.2f; Output.Color.x-=0.2f; Output.Color.y-=0.2f;
	}
	if(xGrayScale)
	{
		float avg=(Output.Color.x+Output.Color.y+Output.Color.z)/3;
		Output.Color.x=avg;
		Output.Color.y=avg;
		Output.Color.z=avg;
	}
	if(xInvertedColors)
	{
		Output.Color.x=1-Output.Color.x;
		Output.Color.y=1-Output.Color.y;
		Output.Color.z=1-Output.Color.z;
	}

	if(xClipping==1)
	{
		if(PSIn.Position3D.y<xClipHeight)
		{
			Output.Color*=-1;
			clip(Output.Color);
		}
	}
	if(xClipping==2)
	{
		if(PSIn.Position3D.y>xClipHeight)
		{
			Output.Color*=-1;
			clip(Output.Color);
		}
	}

	return Output;
}

technique ColorLight
{
	pass Pass0
	{   
		VertexShader = compile vs_3_0 ColoredVS();
		PixelShader  = compile ps_3_0 ColoredPS();
	}
}
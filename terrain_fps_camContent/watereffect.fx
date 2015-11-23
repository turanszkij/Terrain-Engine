struct VertexToPixel
{
	float4 Position		   : POSITION;
	float  LightingFactor  : TEXCOORD0;
	//float2 TextureCoords   : TEXCOORD1;
	float  Depth		   : TEXCOORD1;
	float  Random		   : TEXCOORD2;
	float3 Position3D	   : TEXCOORD3;
	float3 Normal3D		   : TEXCOORD4;
	float  LightDistance   : TEXCOORD5;
	float4 ReflectionMapSamplingPos    : TEXCOORD6;
	float4 RefractionMapSamplingPos : TEXCOORD7;
    float2 Bump1        : TEXCOORD8;
	float2 Bump2 : TEXCOORD9;
};

struct PixelToFrame
{
	float4 Color		: COLOR0;
};

					


float4x4 xView;
float4x4 xProjection;
float4x4 xWorld;

float3   xCamPos;
float3   xShEye;
bool	 xEnableLight;
float3	 xLightDirection;
float	 xLightPower;
float	 xAmbient;

bool	 xEnablePLight;
float3	 xPLight;
float    xPLightScatter;

bool     xGrayScale;
bool	 xInvertedColors;
bool     xUnderWater;
bool     xDetailWater;

bool	 xEnableFog = true;
float	 xFogStart,xFogEnd;
float4	 xFogColor;

float   xWaveHeight;
float   xWaveLength;
Texture xWaterBumpMap;
sampler WaterBumpMapSampler = sampler_state { 
												texture = <xWaterBumpMap> ; 
												magfilter = anisotropic; 
												minfilter = anisotropic; 
												mipfilter=linear; 
												AddressU = mirror; 
												AddressV = mirror;
											};

float4x4 xReflectionView;
Texture xReflectionMap;
sampler ReflectionSampler = sampler_state { 
											texture = <xReflectionMap> ;
											magfilter = LINEAR; 
											minfilter = LINEAR; 
											mipfilter=LINEAR; 
											AddressU = clamp; 
											AddressV = clamp;
										  };

Texture xRefractionMap;
sampler RefractionSampler = sampler_state { 
											texture = <xRefractionMap> ; 
											magfilter = LINEAR; 
											minfilter = LINEAR; 
											mipfilter=LINEAR; 
											AddressU = clamp; 
											AddressV = clamp;
										  };

// Wind Effect
float3 WindDirection;
float WindWaveSize;
float WindRandomness;
float WindSpeed;
float WindAmount;
float WindTime;

float3 GetLightDirection(float3 pos3D, float3 lightPos)
{
	return normalize(pos3D - lightPos);
}

VertexToPixel WaterVS(float3 inPos: POSITION0, float3 inNormal: NORMAL, float2 inTexCoords: TEXCOORD0, float inRand: TEXCOORD1)
{
	VertexToPixel Output = (VertexToPixel)0;

	float3 position = inPos;
	
			position+=(WindDirection + float3(0,xWaveHeight,0))*sin(WindTime*WindSpeed/2.0f+(dot(position, WindDirection)+inRand*WindRandomness)) * WindAmount;
			inNormal+= (WindDirection + float3(0,xWaveHeight,0))*cos(WindTime*WindSpeed/2.0f+(dot(position, WindDirection)+inRand*WindRandomness)) * WindAmount;
			
			normalize(inNormal);
	
	float4x4 preWorldViewProjection=mul(xWorld,mul(xView, xProjection));
	Output.Position = mul(float4(position,1),preWorldViewProjection);
    Output.ReflectionMapSamplingPos = mul(float4(position,1), mul(xWorld,mul(xReflectionView, xProjection)));
	Output.RefractionMapSamplingPos = mul(float4(position,1), preWorldViewProjection);

	Output.Depth = Output.Position.z;


		Output.Position3D = mul(inPos, xWorld);
		Output.Normal3D = normalize(mul(inNormal, (float3x3)xWorld));
		Output.LightDistance = length(Output.Position3D - xPLight);

	
	float2 moveVector = float2(0, WindTime*-WindSpeed*0.04f);
    Output.Bump1 = 4*inTexCoords+moveVector;
	moveVector = float2(WindTime*-WindSpeed*0.05f, WindTime*-WindSpeed*0.04f);
	Output.Bump2 = 4*inTexCoords-moveVector;
	
	

    return Output;
}




float3x3 compute_tangent_frame(float3 N, float3 P, float2 UV)
{
	float3 dp1 = ddx(P);
	float3 dp2 = ddy(P);
	float2 duv1 = ddx(UV);
	float2 duv2 = ddy(UV);

	float3x3 M = float3x3(dp1, dp2, cross(dp1, dp2));
	float2x3 inverseM = float2x3( cross( M[1], M[2] ), cross( M[2], M[0] ) );
	float3 T = mul(float2(duv1.x, duv2.x), inverseM);
	float3 B = mul(float2(duv1.y, duv2.y), inverseM);

	return float3x3(normalize(T), normalize(B), N);
}

PixelToFrame WaterPS(VertexToPixel PSIn)
{
    PixelToFrame Output = (PixelToFrame)0;

	
	float3 normalVector=PSIn.Normal3D;
	float3 eyeVector = normalize(xCamPos - PSIn.Position3D);
	float3 lightdirection = GetLightDirection(PSIn.Position3D, xShEye);
	float4 bumpColor = tex2D(WaterBumpMapSampler, PSIn.Bump1);
	float4 bumpColor2 = tex2D(WaterBumpMapSampler, PSIn.Bump2);
	float3 perturbation = lerp(bumpColor,bumpColor2,0.5f) * 2.0f - 1.0f;


	//TANGENT SPACE FOR NORMALMAP
	float3x3 tangentFrame = compute_tangent_frame(normalVector, eyeVector, PSIn.Bump1.yx );
	normalVector = normalize(mul(perturbation, tangentFrame));

	//REFLECTION COLOR
	float2 ProjectedTexCoords;
	ProjectedTexCoords.x = PSIn.ReflectionMapSamplingPos.x/PSIn.ReflectionMapSamplingPos.w/2.0f + 0.5f;
	ProjectedTexCoords.y = -PSIn.ReflectionMapSamplingPos.y/PSIn.ReflectionMapSamplingPos.w/2.0f + 0.5f;
	float2 perturbatedReflTexCoords = ProjectedTexCoords + (float2)perturbation;
	float4 reflectiveColor = tex2D(ReflectionSampler, perturbatedReflTexCoords);
	

	
	//REFRACTION COLOR
	float2 ProjectedRefrTexCoords;
	ProjectedRefrTexCoords.x = PSIn.RefractionMapSamplingPos.x/PSIn.RefractionMapSamplingPos.w/2.0f + 0.5f;
	ProjectedRefrTexCoords.y = -PSIn.RefractionMapSamplingPos.y/PSIn.RefractionMapSamplingPos.w/2.0f + 0.5f;    
	float2 perturbatedRefrTexCoords = ProjectedRefrTexCoords + (float2)perturbation;    
	float4 refractiveColor = tex2D(RefractionSampler, perturbatedRefrTexCoords);

	
	


	//FRESNEL TERM
	float fresnelTerm = dot(normalVector, eyeVector);
	float4 combinedColor = lerp(reflectiveColor, refractiveColor, fresnelTerm);


	//DULL COLOR
	float4 dullColor = float4(0.0f, 0.4f, 0.7f, 1.5f);
	Output.Color = lerp(combinedColor, dullColor, 0.3f);

	

	
	//SPECUALR LIGHT
	float3 reflectionVector = -reflect(lightdirection, normalVector);
	float specular = dot(normalize(reflectionVector), normalize(eyeVector));
	specular = pow(abs(specular), 250);
	Output.Color.rgb += specular;
	
	
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
		float l = saturate((PSIn.Depth - xFogStart) / (xFogEnd - 8 - xFogStart));
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

    return Output;
}

technique Water
{
    pass Pass0
    {        
        VertexShader = compile vs_3_0 WaterVS();
        PixelShader = compile ps_3_0 WaterPS();        
    }
}
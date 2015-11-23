struct VertexToPixel
{
	float4 Position		   : POSITION;
	float2 TextureCoords   : TEXCOORD0; 
	float3 Position3D      : TEXCOORD1;
};

struct PixelToFrame
{
	float4 Color		: COLOR0;
};

float4x4 xViewProjection;
float4x4 xWorld;
float3   xCamPos;
float3   xShEye;

bool     xGrayScale;
bool     xInvertedColors;
bool     xUnderWater;

Texture xTexture;
sampler SkyBoxSampler = sampler_state { 
										texture= <xTexture>; 
										magfilter = linear; 
										minfilter = linear; 
										mipfilter=LINEAR; 
										AddressU = clamp; 
										AddressV = clamp;
									};


uint xClipping;
float xClipHeight;

float3 GetLightDirection(float3 pos3D, float3 lightPos)
{
	return normalize(pos3D - lightPos);
}

//------- Technique: Sky --------

VertexToPixel SkyVS( float4 inPos : POSITION, float2 inTexCoords: TEXCOORD0)
{	
	VertexToPixel Output = (VertexToPixel)0;
    
	Output.Position = mul(inPos, mul(xWorld, xViewProjection));
	Output.TextureCoords = inTexCoords;

	Output.Position3D = mul(inPos, xWorld);
    
	return Output;    
}

PixelToFrame SkyPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
	
	Output.Color = tex2D(SkyBoxSampler, PSIn.TextureCoords);
	
	float3 lightdirection = GetLightDirection(PSIn.Position3D, xShEye);
	float3 eyeVector = normalize(xCamPos - PSIn.Position3D);
	float3 reflectionVector = lightdirection;
	float specular = dot(normalize(reflectionVector), normalize(eyeVector));
	specular = pow(abs(specular), 200);
	Output.Color.rgb += specular;
	

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

technique Sky
{
	pass Pass0
	{   
		VertexShader = compile vs_2_0 SkyVS();
		PixelShader  = compile ps_2_0 SkyPS();
	}
}


struct VertexToPixel
{
    float4 Position   	: POSITION; 
    float2 TextureCoords: TEXCOORD0;
	float3 Position3D   : TEXCOORD1;
	float3 Normal3D     : TEXCOORD2;
	float  LightDistance: TEXCOORD3;
};

struct PixelToFrame
{
    float4 Color : COLOR0;
};


float4x4 xView;
float4x4 xProjection;
float4x4 xWorld;
float3 xCamPos;
float3 xCamUp;
float xPointSpriteSize;
bool     xGrayScale;
bool     xInvertedColors;
bool     xUnderWater;

bool xEnablePLight;
float3	 xPLight;
float    xPLightScatter;
float	 xLightPower;


Texture xTexture;
sampler TextureSampler = sampler_state { texture = <xTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

uint xClipping;
float xClipHeight;

VertexToPixel PointSpriteVS(float3 inPos: POSITION0, float2 inTexCoord: TEXCOORD0)
{
    VertexToPixel Output = (VertexToPixel)0;

    float3 center = mul(inPos, xWorld);
    float3 eyeVector = center - xCamPos;

    float3 sideVector = cross(eyeVector,xCamUp);
    sideVector = normalize(sideVector);
    float3 upVector = cross(sideVector,eyeVector);
    upVector = normalize(upVector);

    float3 finalPosition = center;
    finalPosition += (inTexCoord.x-0.5f)*sideVector*0.5f*xPointSpriteSize;
    finalPosition += (0.5f-inTexCoord.y)*upVector*0.5f*xPointSpriteSize;

    float4 finalPosition4 = float4(finalPosition, 1);

    float4x4 preViewProjection = mul (xView, xProjection);
    Output.Position = mul(finalPosition4, preViewProjection);

	if(xEnablePLight)
	{
		Output.Position3D = mul(inPos, xWorld);
		Output.LightDistance = length(Output.Position3D - xPLight);
	}

    Output.TextureCoords = inTexCoord;

    return Output;
}

PixelToFrame PointSpritePS(VertexToPixel PSIn) : COLOR0
{
    PixelToFrame Output = (PixelToFrame)0;
    Output.Color = tex2D(TextureSampler, PSIn.TextureCoords);

	if(xEnablePLight)
	{
		float3 lightDir = normalize(PSIn.Position3D - xPLight);
		float power = clamp((1 - PSIn.LightDistance / xPLightScatter),0,1) * xLightPower;
		float diffuseLightingFactor=power;

		Output.Color.rgb *= diffuseLightingFactor;
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

technique PointSprites
{
	pass Pass0
	{   
		VertexShader = compile vs_2_0 PointSpriteVS();
		PixelShader  = compile ps_2_0 PointSpritePS();
	}
}
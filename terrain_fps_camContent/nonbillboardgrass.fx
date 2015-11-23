struct VertexToPixel
{
float4 Position		   : POSITION;
float  LightingFactor  : TEXCOORD0;
float2 TextureCoords   : TEXCOORD1;
float  Depth		   : TEXCOORD2;
float  Random		   : TEXCOORD3;
float3 Position3D	   : TEXCOORD4;
float3 Normal3D		   : TEXCOORD5;
float  LightDistance   : TEXCOORD6;
};

struct PixelToFrame
{
float4 Color		: COLOR0;
};


Texture xTexture;
sampler textureSampler = sampler_state {
texture = (xTexture);
magfilter = anisotropic;
minfilter = anisotropic;
mipfilter = linear;
AddressU = clamp;
AddressV = clamp;
};



float4x4 xView;
float4x4 xProjection;
float4x4 xWorld;


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

bool	 xEnableFog = true;
float	 xFogStart,xFogEnd;
float4	 xFogColor;

float	 xGrassFade;
uint xClipping;
float xClipHeight;

// Wind Effect
float3 WindDirection;
float WindWaveSize;
float WindRandomness;
float WindSpeed;
float WindAmount;
float WindTime;

// Parameters describing the billboard itself.
float BillboardWidth;
float BillboardHeight;

float AlphaTestDirection;
float AlphaTestThreshold=0.55f;

float3 GetLightDirection(float3 pos3D, float3 lightPos)
{
	return normalize(pos3D - lightPos);
}

//------- Technique: GrassBB --------
VertexToPixel CylBillboardVS(float3 inPos: POSITION0, float3 inNormal: NORMAL, float2 inTexCoords: TEXCOORD0, float inRand: TEXCOORD1, float3 inDir: TEXCOORD2)
{
VertexToPixel Output = (VertexToPixel)0;

float squishFactor = 0.75 + abs(inRand) / 2;

float width = BillboardWidth * squishFactor;
float height = BillboardHeight / squishFactor;

if (inRand < 0)
       width = -width;

	float3 rightVector = normalize(cross(inDir, inNormal));

	float3 position = inPos;

	position += rightVector * (inTexCoords.x - 0.5) * width;
    
	position += inNormal * (1 - inTexCoords.y) * height;
		
			float waveOffset = dot(position, WindDirection) * WindWaveSize;
			waveOffset += inRand * WindRandomness;
			float wind = sin(WindTime * WindSpeed + waveOffset) * WindAmount;
			wind *= (1 - inTexCoords.y);
			position += WindDirection * wind;
			
			normalize(inNormal);
	Output.Position = mul(float4(position,1),mul(xWorld,mul(xView, xProjection)));
	Output.Depth = Output.Position.z;

	
	float4 viewPosition=mul(float4(       inPos+(rightVector * (inTexCoords.x - 0.5) * width*clamp(xGrassFade/Output.Depth,0,1))+(inNormal * (1 - inTexCoords.y) * height*clamp(xGrassFade/Output.Depth,0,1))+(WindDirection * wind)         ,1),xView);
	Output.Position = mul(xWorld,mul(viewPosition, xProjection));

	//if(Output.Depth<xGrassFade)
	{

		Output.Position3D = mul(inPos, xWorld);
		Output.Normal3D = normalize(mul(inNormal, (float3x3)xWorld));
		Output.LightDistance = length(Output.Position3D - xPLight);

	Output.TextureCoords = inTexCoords;

	}
    return Output;
}

PixelToFrame BillboardPS(VertexToPixel PSIn) : COLOR0
{
    PixelToFrame Output = (PixelToFrame)0;

    Output.Color = tex2D(textureSampler, PSIn.TextureCoords);

	
	clip((Output.Color.a - AlphaTestThreshold) * AlphaTestDirection);

	
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

	float diffuseLightingFactor1 = dot(lightdirection, normal);
		diffuseLightingFactor1 = saturate(diffuseLightingFactor1);
		//diffuseLightingFactor1 *= 1.2f;
		
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

  technique GrassBB
  {
  pass Pass0
  {
  VertexShader = compile vs_2_0 CylBillboardVS();
  PixelShader = compile ps_2_0 BillboardPS();
  }
  }
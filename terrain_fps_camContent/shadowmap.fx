float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;


struct VertextoPixel
{
	float4 pos				: POSITION;
	float4 pos2D			: TEXCOORD0;
};


VertextoPixel VS(float3 inPos : POSITION)
{
	VertextoPixel Out = (VertextoPixel)0;

	Out.pos = mul( float4(inPos,1), mul(xWorld,mul(xView,xProjection)));
	Out.pos2D = Out.pos;


	return Out;
}

float4 PS(VertextoPixel PSIn) : COLOR0
{
	return PSIn.pos2D.z/PSIn.pos2D.w;
}

technique ShadowMap
{
	pass Pass0
	{
		VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 PS();  
	}
}
#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#else
	#define VS_SHADERMODEL vs_4_0//_level_9_1
	#define PS_SHADERMODEL ps_4_0//_level_9_1
#endif

Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
	float4 World : SV_POSITION;
	float4 Color : COLOR0;
	float2 Coords : TEXCOORD0;
};
float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 color = tex2D(SpriteTextureSampler, input.Coords);
	if (color.a == 0)
		color.rgba = 1;
	return color;
}

technique SpriteDrawing
{
	pass P0 { PixelShader = compile PS_SHADERMODEL MainPS(); }
};
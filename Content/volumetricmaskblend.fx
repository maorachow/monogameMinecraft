#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

 
sampler maskSampler = sampler_state
{
    Texture = (maskTex);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = NONE;
};
sampler backgroundSampler = sampler_state
{
    Texture = (backgroundTex);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = NONE;
};
float2 screenSpaceLightPos;
float flareWeight;
struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
    float2 TexCoords : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
    float2 TexCoords : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = input.Position;
	output.Color = input.Color;
    output.TexCoords = input.TexCoords;
	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 result = tex2D(maskSampler, input.TexCoords).rgba;
    
    if (tex2D(maskSampler, input.TexCoords).r<0.000001)
    {
        
        float insensity = 1 - length(screenSpaceLightPos - input.TexCoords) * flareWeight;
        result.rgb = float3(255.0 / 255.0, 255.0 / 255.0,224.0 / 255.0) * insensity;
    }
    else
    {
        result.rgb = float3(0, 0, 0);

    }
    
    return result;
}

technique VolumetricMaskBlend
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
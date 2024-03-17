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
float4 sunPosWS;
matrix SunViewProjection;
float4 lightColor;
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
    float4 ssLightPos = mul(sunPosWS, SunViewProjection);
    ssLightPos.xy /= ssLightPos.w;
    ssLightPos.y = -ssLightPos.y;
    ssLightPos *= 0.5;
    ssLightPos.xy += float2(0.5, 0.5);
    float4 result = tex2D(maskSampler, input.TexCoords).rgba;
    
    if (length(tex2D(maskSampler, input.TexCoords).rgb) < 0.000001)
    {
        
        float insensity = 1 - length(screenSpaceLightPos.xy - input.TexCoords) * flareWeight;
        result.rgb = lightColor * insensity;
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
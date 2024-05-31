#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

 
sampler2D gPositionWS = sampler_state
{
    Texture = <PositionWSTex>;
 
    MipFilter = Point;
    MagFilter = Point;
    MinFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};


float3 CameraPos;
matrix View;
matrix ViewProjection;
float3 LightDir;
struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 TexCoords : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float2 TexCoords : TEXCOORD0;
};



float2 GetScreenCoordFromWorldPos(float3 worldPos)
{
    float4 offset = float4(worldPos, 1.0);
    offset = mul(offset, ViewProjection);
    offset.xyz /= offset.w;
    offset.xy = offset.xy * 0.5 + 0.5 + (offset.z * 0.0000000001);
    offset.y = 1 - offset.y;
    return offset.xy;
}
float GetViewDepthFromWorldPos(float3 worldPos)
{
    float4 marchDepthView = mul(float4(worldPos, 1), View);
       
    marchDepthView.z = marchDepthView.z + marchDepthView.x * 0.00001 + marchDepthView.y * 0.000001;
    return -marchDepthView.z;
}

float Random2DTo1D(float2 value, float a, float2 b)
{
	            //avaoid artifacts
    float2 smallValue = sin(value);
	            //get scalar value from 2d vector	
    float random = dot(smallValue, b);
    random = frac(sin(random) * a);
    return random;
}
float Random2DTo1D(float2 value)
{
    return (
		            Random2DTo1D(value, 14375.5964, float2(15.637, 76.243))
		          
	            );
}

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = input.Position;
    output.TexCoords = input.TexCoords;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 worldPos = tex2D(gPositionWS, input.TexCoords).xyz;
    float3 marchDir = normalize(LightDir);
    if (marchDir.y < 0.1)
    {
        return float4(0, 0, 0, 1);
    }
    if (length(worldPos - CameraPos) > 30)
    {
        return float4(1, 1, 1, 1);
    }
        float3 rayOrigin = worldPos;
    float randomNoise = Random2DTo1D(input.TexCoords) ;
    for (int i = 0; i < 32; i++)
    {
        float3 marchPos = rayOrigin + marchDir * 0.05 * (i + 1 + randomNoise);
       
        float2 uv = GetScreenCoordFromWorldPos(marchPos);
    //    return float4(uv.xy,1, 1);
        float3 sampleWorldPos = tex2D(gPositionWS, uv).xyz;
        float sampleViewDepth = GetViewDepthFromWorldPos(sampleWorldPos);
        float testDepth = GetViewDepthFromWorldPos(marchPos);
        if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
        {
            return float4(1, 1, 1, 1);
        }
        if (sampleViewDepth < testDepth && abs(sampleViewDepth-testDepth)<0.1)
        {
            return float4(0, 0, 0, 1);
        }
        
        
    }
	
    
    return float4(1,1, 1, 1);
}

technique ContactShadow
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
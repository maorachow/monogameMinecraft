#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif
 
sampler gProjectionDepth = sampler_state
{
    Texture = (ProjectionDepthTex);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = NONE;
};
sampler gNormal = sampler_state
{
    Texture = (NormalTex);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = NONE;
};
sampler gAlbedo = sampler_state
{
    Texture = (AlbedoTex);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = NONE;
};
sampler gRoughness = sampler_state
{
    Texture = (RoughnessMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = NONE;
};
float4x4 matInverseView;
float4x4 matInverseProjection;
float4x4 matView;
float4x4 matProjection;
float3 CameraPos;
struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position =input.Position;
    output.TexCoord = input.TexCoord;

	return output;
}
float GetDepth(float2 texCoord)
{
    return tex2D(gProjectionDepth, texCoord).r;
}
struct RayTraceOutput
{
    bool Hit;
    float2 UV;
};
float3 GetUVFromPosition(float3 worldPos)
{
    float4 viewPos = mul(float4(worldPos, 1), matView);
    float4 projectionPos = mul(viewPos, matProjection);
    projectionPos.xyz /= projectionPos.w;
    projectionPos.y = -projectionPos.y;
    projectionPos.xy = projectionPos.xy * 0.5 + 0.5;
    return projectionPos.xyz;
}

float3 GetWorldPosition(float2 vTexCoord, float depth)
{
    // Get the depth value for this pixel
    float z = depth;
    // Get x/w and y/w from the viewport position
    float x = vTexCoord.x * 2 - 1;
    float y = (1 - vTexCoord.y) * 2 - 1;
    float4 vProjectedPos = float4(x, y, z, 1.0f);
    // Transform by the inverse projection matrix
    float4 vPositionVS = mul(vProjectedPos, matInverseProjection);
    float4 vPositionWS = mul(vPositionVS, matInverseView);
    // Divide by w to get the view-space position
    return vPositionWS.xyz / vPositionWS.w;
}
RayTraceOutput TraceRay(float2 TexCoord)
{
    
    float InitDepth = GetDepth(TexCoord);
    if (InitDepth <= 0.00001)
    {
        return (RayTraceOutput) 0;
    }
// Now get the position
    float3 reflPosition = GetWorldPosition(TexCoord, InitDepth);
// Get the Normal Data
    float3 normalData = tex2D(gNormal,TexCoord).xyz;
//tranform normal back into [-1,1] range
    float3 reflNormal = 2.0f * normalData - 1.0f;
    float3 vDir = normalize(reflPosition - CameraPos);
    
    float diff = max(dot(reflNormal, vDir), -1.0);
    if (diff <= -0.7)
    {
        return (RayTraceOutput) 0;
    }
     
    float3 reflectDir = normalize(reflect(vDir, normalize(reflNormal)));
    RayTraceOutput output = (RayTraceOutput) 0;
    float3 curPos = 0;
 
    // The Current UV
    float3 curUV = 0;
 
    // The Current Length
    float curLength = 1;

    // Now loop
     
    for (int i = 0;i <32; i++)
    {
        // Has it hit anything yet
        if (output.Hit == false)
        {
            // Update the Current Position of the Ray
            curPos = reflPosition + reflectDir * curLength;
            // Get the UV Coordinates of the current Ray
            curUV = GetUVFromPosition(curPos);
            // The Depth of the Current Pixel
            float curDepth = GetDepth(curUV.xy);
             
                if (abs(curUV .z - curDepth) < 0.00001)
                {
                    // If it's hit something, then return the UV position
                    output.Hit = true;
                    output.UV = curUV .xy;
                    break;
                }
            if (curDepth < 0.00001||curUV.z<0.00001)
            {
                    // If it's hit something, then return the UV position
                output.Hit = false;
                output.UV = curUV.xy;
                break;
            }
               // curDepth = GetDepth(curUV.xy + (float2(0.01, 0.01) * 2));
           

            // Get the New Position and Vector
            float3 newPos = GetWorldPosition(curUV.xy, curDepth);
            curLength = length(reflPosition - newPos);
        }
    }
    return output;
}
float4 MainPS(VertexShaderOutput input) : COLOR
{

    
    RayTraceOutput ray = TraceRay(input.TexCoord);
    float amount = 0.1;
    if (ray.Hit == true)
    {
                        // Fade at edges
        if (ray.UV.x < 0 || ray.UV.x > 1 || ray.UV.y < 0 || ray.UV.y > 1  )
        {
            return float4(0, 0, 0, 1);
        }
        return float4(tex2D(gAlbedo, ray.UV.xy).xyz * tex2D(gRoughness, input.TexCoord.xy).r, 1);
    }
    else
    {
        return float4(0,0,0, 1);

    }
    
   
}

technique SSR
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
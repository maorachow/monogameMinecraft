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

sampler gPositionWS = sampler_state
{
    Texture = (PositionWSTex);
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
matrix ViewProjection;
matrix View;
bool binarySearch;
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
/*RayTraceOutput TraceRay(float2 TexCoord)
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
    if (diff <=- 0.7)
    {
        return (RayTraceOutput) 0;
    }
    
    float3 reflectDir = normalize(reflect(vDir, normalize(reflNormal)));
    RayTraceOutput output = (RayTraceOutput) 0;
    float3 curPos = reflPosition;
 
    // The Current UV
    float3 curUV = 0;
 
    // The Current Length
    float curLength = 0.5;

    // Now loop
     
    for (int i = 0;i <64; i++)
    {
        // Has it hit anything yet
        if (output.Hit == false)
        {
            // Update the Current Position of the Ray
            curPos = reflPosition+reflectDir * curLength;
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
 
   //return float4((input.TexCoord).xy, 1, 1);
    RayTraceOutput ray = TraceRay(input.TexCoord);
    float amount = 0.1;
    if (ray.Hit == true)
    {
                        // Fade at edges
        if (ray.UV.x < 0 || ray.UV.x > 1 || ray.UV.y < 0 || ray.UV.y > 1  )
        {
            return float4(0, 0, 0, 0);
        }
        return float4(tex2D(gAlbedo, ray.UV.xy).xyz * tex2D(gRoughness, input.TexCoord.xy).r*0.4, 0.1);
    }
    else
    {
        return float4(0,0,0, 0);

    }
    
   
}*/

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
float4 MainPS(VertexShaderOutput input) : COLOR
{
 
    float3 worldPos = tex2D(gPositionWS, input.TexCoord).xyz;
    float3 normal = tex2D(gNormal, input.TexCoord).xyz * 2 - 1;
    worldPos = worldPos + normal * 0.1 * length(worldPos - CameraPos) / 100;
    float3 vDir = normalize(worldPos-CameraPos);
    float3 rDir = normalize(reflect(vDir, normalize(normal)));
    
    float3 rayOrigin = worldPos + float3(0, 0, float(binarySearch)*0.00000001);
    
    float ssrThickness = 0.2;
    float3 preMarchPos = rayOrigin;
    float noiseValue = Random2DTo1D(input.TexCoord);
    for (int i = 0; i < 16; i++)
    {
        float3 marchPos = rayOrigin + (rDir) * 0.5 * pow((i + 1 + noiseValue), 1.41);
     //   ssrThickness += (0.1);
   /*     float4 offset = float4(marchPos, 1.0);
        offset = mul(offset, ViewProjection);
        offset.xyz /= offset.w;
        offset.xy = offset.xy * 0.5 + 0.5 + (offset.z * 0.0000000001);
        offset.y = 1 - offset.y;*/
        float2 uv = GetScreenCoordFromWorldPos(marchPos);
    /*    float4 marchDepthView = mul(float4(marchPos, 1), View);
       
        marchDepthView.z = marchDepthView.z + marchDepthView.x * 0.00001 + marchDepthView.y * 0.000001;*/
        float testDepth = GetViewDepthFromWorldPos(marchPos);
        
        float3 worldPosSampled = tex2D(gPositionWS, uv.xy).xyz;
     /*   float4 sampleViewPosDepth = mul(float4(worldPosSampled, 1), View);
        sampleViewPosDepth.z = sampleViewPosDepth.z + sampleViewPosDepth.x * 0.000001 + sampleViewPosDepth.y * 0.0000001;*/
        float sampleDepth = GetViewDepthFromWorldPos(worldPosSampled);
        
        if ((uv.x) < 0 || (uv.y) < 0 || (uv.x) > 1 || (uv.y) > 1)
        {
            return float4(0, 0, 0, 1);
        }
        if (testDepth > sampleDepth && abs((testDepth) - (sampleDepth)) < length(preMarchPos - marchPos) * 1.2)
        {
            float3 finalPoint = preMarchPos;
            float _sign = 1.0;
            float3 direction = marchPos - preMarchPos;
            float2 uv1 = 0;
            float3 worldPosSampled1=0;

            float testDepth=0;

            float sampleDepth=0;
         //   for (int j = 0; j < 2; j++)
          //  {
            
            if (binarySearch>0)
            {
               direction *= 0.5;
                finalPoint += direction * _sign;
                uv1 = GetScreenCoordFromWorldPos(finalPoint);
                 worldPosSampled1 = tex2D(gPositionWS, uv1.xy).xyz;
                 testDepth = GetViewDepthFromWorldPos(finalPoint);
                 sampleDepth = GetViewDepthFromWorldPos(worldPosSampled1);
                _sign = -sign(testDepth - sampleDepth);
            
            direction *= 0.5;
            finalPoint += direction * _sign;
            uv1 = GetScreenCoordFromWorldPos(finalPoint);
             worldPosSampled1 = tex2D(gPositionWS, uv1.xy).xyz;
             testDepth = GetViewDepthFromWorldPos(finalPoint);
             sampleDepth = GetViewDepthFromWorldPos(worldPosSampled1);
            _sign = -sign(testDepth - sampleDepth);
            
            direction *= 0.5;
            finalPoint += direction * _sign;
            uv1 = GetScreenCoordFromWorldPos(finalPoint);
            worldPosSampled1 = tex2D(gPositionWS, uv1.xy).xyz;
            testDepth = GetViewDepthFromWorldPos(finalPoint);
            sampleDepth = GetViewDepthFromWorldPos(worldPosSampled1);
            _sign = -sign(testDepth - sampleDepth);
            
            direction *= 0.5;
            finalPoint += direction * _sign;
            uv1 = GetScreenCoordFromWorldPos(finalPoint);
            worldPosSampled1 = tex2D(gPositionWS, uv1.xy).xyz;
            testDepth = GetViewDepthFromWorldPos(finalPoint);
            sampleDepth = GetViewDepthFromWorldPos(worldPosSampled1);
            _sign = -sign(testDepth - sampleDepth);
                
                 
            }
            else
            {
                uv1 = GetScreenCoordFromWorldPos(marchPos);
            }
            
         
           
      //      }
        //    uv1 = GetScreenCoordFromWorldPos(finalPoint);
            return float4(tex2D(gAlbedo, uv1.xy).xyz, 1);

        }
        preMarchPos = marchPos;

    }
        return float4(0,0,0, 1);
   
}
technique SSR
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
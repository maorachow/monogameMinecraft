 /*


#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif
#if MGFX
// This unused parameter helps avoiding crashes due to compiler optimizations in monogame
float4 Float4Parameter0 : Float4Parameter0;
#endif
matrix projection;
matrix invProjection;
sampler2D gPosition  = sampler_state
{
    Texture = (PositionDepthTex);
 
    MipFilter = Linear;
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};
sampler2D gNormal = sampler_state
{
    Texture = <NormalTex>;
 
    MipFilter = Linear;
    MagFilter = Point;
    MinFilter = Point;
    AddressU = Wrap;
    AddressV = Wrap;
};
sampler2D gProjectionDepth = sampler_state
{
    Texture = <ProjectionDepthTex>;
 
    MipFilter = Linear;
    MagFilter = Point;
    MinFilter = Point;
    AddressU = Wrap;
    AddressV = Wrap;
};
sampler2D gTangent = sampler_state
{
    Texture = <TangentTex>;
 
    MipFilter = Linear;
    MagFilter = Point;
    MinFilter = Point;
    AddressU = Border;
    AddressV = Border;
};
sampler2D texNoise = sampler_state
{
    Texture = <NoiseTex>;
 
    MipFilter = Linear;
    MagFilter = Point;
    MinFilter = Point;
    AddressU = Wrap;
    AddressV = Wrap;
};
float3 samples[64];

matrix invertView;
float2 noiseScale = float2(800.0 / 4.0, 600.0 / 4.0);
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

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    output.Position = input.Position;
    output.TexCoords = float2(input.TexCoords.x,input.TexCoords.y);

    return output;
}
 
float3 PositionFromDepth(float2 vTexCoord)
{
    // Get the depth value for this pixel
    float z = tex2D(gProjectionDepth, vTexCoord).r;
    // Get x/w and y/w from the viewport position
    float x = vTexCoord.x * 2 - 1;
    float y = (1 - vTexCoord.y) * 2 - 1;
    float4 vProjectedPos = float4(x, y, z, 1.0f);
    // Transform by the inverse projection matrix
    float4 vPositionVS = mul(vProjectedPos, invProjection);
    // Divide by w to get the view-space position
    return vPositionVS.xyz/vPositionVS.w;
}
 
float doAmbientOcclusion1(in float2 tcoord, in float3 p, in float3 cnorm)
{
    float3 diff = PositionFromDepth(tcoord) - p;
    float3 v = normalize(diff);
    float d = length(diff) * 1;

    return max(0.0, dot(cnorm, v) - 0) * (1.0 / (1.0 + d)) * 1;
}


float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 p = PositionFromDepth(input.TexCoords);
    float rad = 0.3 / p.z;
   
    const float2 vec[4] =
    {
        float2(1, 0),
		float2(-1, 0),
		float2(0, 1),
		float2(0, -1)
    };
    float3 normal = tex2D(gNormal, input.TexCoords) * 2 - 1;
     
   // float3 fragPos = mul(projFragPos, invProjection);
    //fragPos = VSPositionFromDepth(input.TexCoords);
    float occlusion = 0.0;
 
    for (int i = 0; i < 64;i++)
    {
        float2 sample = samples[i].xy;
        
        occlusion += doAmbientOcclusion1(input.TexCoords + vec[i%4]*rad+samples[i]*0.1, p, normal);
         
    }
    
    
    occlusion =  (occlusion /64);
    return float4(occlusion, occlusion, occlusion, 1);

}

 
 */

float4x4 param_inverseViewProjectionMatrix;
float4x4 param_inverseViewMatrix;
float3 param_frustumCornersVS[4];
float4x4 g_matInvProjection;
 float param_randomSize=1;
 float param_sampleRadius=1;
 float param_intensity=1;
 float param_scale=1;
float param_bias;
float2 param_screenSize;

texture param_normalMap;
texture param_depthMap;
texture param_randomMap;



sampler normalSampler = sampler_state
{
    Texture = (param_normalMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = NONE;
};
sampler depthSampler = sampler_state
{
    Texture = (param_depthMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    MipFilter = NONE;
};
sampler randomSampler = sampler_state
{
    Texture = (param_randomMap);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = POINT;
    MinFilter = POINT;
    MipFilter = NONE;
};



// Define VS input
struct VSIn
{
    float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

// Define VS output and therefor PS input
struct VSOut
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
  
};

// Define PS output
struct PSOut
{
    float4 Color : COLOR0;
};



// Reconstruct view-space position from the depth buffer
float3 getPosition(in float2 vTexCoord, in float3 in_vFrustumCornerVS)
{
    float fPixelDepth = tex2D(depthSampler, vTexCoord).r;
    return float3(fPixelDepth * in_vFrustumCornerVS);
}

float3 getPosition(float2 vTexCoord)
{
    // Get the depth value for this pixel
    float z = tex2D(depthSampler, vTexCoord);
    // Get x/w and y/w from the viewport position
    float x = vTexCoord.x * 2 - 1;
    float y = (1 - vTexCoord.y) * 2 - 1;
    float4 vProjectedPos = float4(x, y, z, 1.0f);
    // Transform by the inverse projection matrix
    float4 vPositionVS = mul(vProjectedPos, g_matInvProjection);
    // Divide by w to get the view-space position
    return vPositionVS.xyz / vPositionVS.w;
}

// Calculate the occlusion term
float doAmbientOcclusion(in float2 tcoord, in float3 p, in float3 cnorm)
{
    float3 diff = getPosition(tcoord ) - p;
    float3 v = normalize(diff);
    float d = length(diff) * param_scale;

    return max(0.0, dot(cnorm, v) - param_bias) * (1.0 / (1.0 + d)) * param_intensity;
}



/**************************************************
Vertex shader.
**************************************************/
VSOut MainVS(VSIn input)
{
    VSOut output;

    output.Position = float4(input.Position, 1);
    output.TexCoord = input.TexCoord.xy;
    

    return output;
}



/**************************************************
Pixel shader.
**************************************************/
PSOut MainPS(VSOut input)
{
    PSOut output;

    const float2 vec[4] =
    {
        float2(1, 0),
		float2(-1, 0),
		float2(0, 1),
		float2(0, -1)
    };

    float3 p = getPosition(input.TexCoord );
    float3 n = normalize(tex2D(normalSampler, input.TexCoord).xyz * 2.0f - 1.0f);
    float2 rand = normalize(tex2D(randomSampler, param_screenSize * input.TexCoord / param_randomSize).xy * 2.0f - 1.0f);

    float ao = 0.0f;
    float rad = param_sampleRadius/ p.z;

    int numIterations =4;
    for (int j = 0; j < numIterations; ++j)
    {
        float2 coord1 = reflect(vec[j], rand) * rad;
        float2 coord2 = float2(coord1.x - coord1.y, coord1.x + coord1.y) * 0.707f;

        ao += doAmbientOcclusion(input.TexCoord + coord1 * 0.25, p, n);
        ao += doAmbientOcclusion(input.TexCoord + coord2 * 0.50, p, n);
        ao += doAmbientOcclusion(input.TexCoord + coord1 * 0.75, p, n);
        ao += doAmbientOcclusion(input.TexCoord + coord2 * 1.00, p, n);
    }

    ao /= (float) numIterations ;
    ao = saturate(ao * param_intensity);

    output.Color = 1 - ao;

    return output;
}



technique Default
{
    pass SSAO
    {
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 MainPS();
    }
}
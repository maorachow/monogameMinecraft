 


#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix projection;
sampler2D gPositionDepth = sampler_state
{
    Texture = <PositionDepthTex>;
 
    MipFilter = Linear;
    MagFilter = Point;
    MinFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};
sampler2D gNormal = sampler_state
{
    Texture = <NormalTex>;
 
    MipFilter = Linear;
    MagFilter = Point;
    MinFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};
sampler2D gProjectionDepth = sampler_state
{
    Texture = <ProjectionDepthTex>;
 
    MipFilter = Linear;
    MagFilter = Point;
    MinFilter = Point;
    AddressU = Border;
    AddressV = Clamp;
};
sampler2D gTangent = sampler_state
{
    Texture = <TangentTex>;
 
    MipFilter = Linear;
    MagFilter = Point;
    MinFilter = Point;
    AddressU = Wrap;
    AddressV = Wrap;
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
float3 samples[16];
matrix g_matInvProjection;
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
    output.TexCoords = float2(input.TexCoords.x, input.TexCoords.y);

    return output;
}

 
float3 VSPositionFromDepth(float2 vTexCoord)
{
    // Get the depth value for this pixel
    float z = tex2D(gProjectionDepth, vTexCoord).r;
    // Get x/w and y/w from the viewport position
    float x = vTexCoord.x*2-1 ;
    float y = (1 - vTexCoord.y) * 2 - 1;
    float4 vProjectedPos = float4(x, y, z, 1 );
    // Transform by the inverse projection matrix
    float4 vPositionVS = mul(vProjectedPos, g_matInvProjection);
   
    // Divide by w to get the view-space position
    return vPositionVS.xyz / vPositionVS.w;
}
 
 
float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 fragPos = VSPositionFromDepth(input.TexCoords );
    //fragPos = tex2D(gPositionDepth, input.TexCoords).xyz*2-1 ;
    float3 normal = tex2D(gNormal, input.TexCoords).rgb * 2 - 1;
    float3 randomVec = tex2D(texNoise, input.TexCoords  *4000).xyz * 2 - 1;
        
    float3 tangent = normalize(randomVec - normal * dot(randomVec, normal));
    float3 bitangent = cross(normal, tangent);
    float3x3 TBN = float3x3(tangent, bitangent, normal);
    float occlusion = 0.0;
    float radius =0.5;
      float3 sampleZero = mul(float3(0, 0.0001, 0),TBN);
    sampleZero = fragPos + sampleZero * radius;
        
        // project sample position (to sample texture) (to get position on screen/texture)
    float4 offsetZero = float4(sampleZero, 1.0);
    offsetZero = mul(projection, offsetZero); // from view to clip-space
        offsetZero.xyz /= offsetZero.w; // perspective divide
    offsetZero.xy = offsetZero.xy * 0.5 + 0.5; // transform to range 0.0 - 1.0
     
        // get sample depth
    float sampleDepthZero = (-tex2D(gProjectionDepth, offsetZero.xy).r);
   
    for (int i = 0; i < 16; ++i)
    {
   
        float3 sample = mul(samples[i], TBN);
        sample =  fragPos+ sample * radius;
        
        // project sample position (to sample texture) (to get position on screen/texture)
        float4 offset = float4(sample, 1.0);
        offset = mul(projection , offset); // from view to clip-space
        offset.xyz /= offset.w; // perspective divide
        offset.xyz = offset.xyz * 0.5 + 0.5; // transform to range 0.0 - 1.0
     
        // get sample depth
        float sampleDepth = (-tex2D(gProjectionDepth, offset.xy).r) ; // Get depth value of kernel sample
      
        // range check & accumulate
       float rangeCheck = smoothstep(0.0, 1.0, radius / abs(fragPos.z - sampleDepth));
        occlusion += (sampleDepth > sampleDepthZero ? 1.0 : 0.0);
    }
    
    occlusion = 1.0- (occlusion / 16);
    return occlusion;

}

technique SSAOEffect
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
/*matrix View;
matrix InverseProjection;

Texture2D NormalMap;
Texture2D DepthMap;

int Samples = 8;

float Strength = 4;

float SampleRadius = 1; //0.05 - 0.5

float2 Resolution = float2(3840, 2160);

SamplerState texSampler
{
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

struct VertexInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct PixelInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

PixelInput SSAOVertexShader(VertexInput input)
{
    PixelInput pi = (PixelInput) 0;

    pi.Position = input.Position;
    pi.TexCoord = input.TexCoord;

    return pi;
}



float4 getPosition(in float2 uv)
{
    float depth = DepthMap.SampleLevel(texSampler, uv, 0).r;
	

	//compute screen-space position	
    float4 position;
    position.xy = uv.xy * 2.0f - 1.0f;
    position.y = -position.y;
    position.z = 1;
    position.w = 1.0f;
	
    position = mul(position, InverseProjection);
    position.xyz /= position.w;
    position.z = depth;
    return position;
}



float3 randomNormal(float2 tex)
{
    float noiseX = (frac(sin(dot(tex, float2(15.8989f, 76.132f) * 1.0f)) * 46336.23745f));
    float noiseY = (frac(sin(dot(tex, float2(11.9899f, 62.223f) * 2.0f)) * 34748.34744f));
    float noiseZ = (frac(sin(dot(tex, float2(13.3238f, 63.122f) * 3.0f)) * 59998.47362f));
    return normalize(float3(noiseX, noiseY, noiseZ));
}

float weightFunction(float3 vec3, float radius)
{
	// NVIDIA's weighting function
    return 1.0 - pow(length(vec3) / radius, 2.0);
}



float4 SSAOPixelShader(PixelInput input) : COLOR0
{

    const float3 kernel[] =
    {
        float3(0.2024537f, 0.841204f, -0.9060141f),
	float3(-0.2200423f, 0.6282339f, -0.8275437f),
	float3(-0.7578573f, -0.5583301f, 0.2347527f),
	float3(-0.4540417f, -0.252365f, 0.0694318f),
	float3(0.3677659f, 0.1086345f, -0.4466777f),
	float3(0.8775856f, 0.4617546f, -0.6427765f),
	float3(-0.8433938f, 0.1451271f, 0.2202872f),
	float3(-0.4037157f, -0.8263387f, 0.4698132f),
	float3(0.7867433f, -0.141479f, -0.1567597f),
	float3(0.4839356f, -0.8253108f, -0.1563844f),
	float3(0.4401554f, -0.4228428f, -0.3300118f),
	float3(0.0019193f, -0.8048455f, 0.0726584f),
	float3(-0.0483353f, -0.2527294f, 0.5924745f),
	float3(-0.4192392f, 0.2084218f, -0.3672943f),
	float3(-0.6657394f, 0.6298575f, 0.6342437f),
	float3(-0.0001783f, 0.2834622f, 0.8343929f),
    };

    float2 texCoord = float2(input.TexCoord);

//get normal data from the NormalMap
    float4 normalData = NormalMap.Sample(texSampler, texCoord);


//tranform normal back into [-1,1] range
    float3 currentNormal = 2.0f * normalData.xyz - 1.0f;

//transform
    currentNormal = normalize(mul(currentNormal, View));

    float linearDepth = DepthMap.Sample(texSampler, texCoord).r;


    if (linearDepth < 0.00000001f)
    {
        return float4(1, 1, 1, 1);
    }


    float3 currentPos = getPosition(texCoord); //Position in VS

    float currentDistance = -currentPos.z;

    float2 aspectRatio = float2(min(1.0f, Resolution.y / Resolution.x), min(1.0f, Resolution.x / Resolution.y));

    float amount = 1.0f;

    float3 noise = randomNormal(texCoord);

//HBAO 2 dir
    int sampleshalf = Samples * 0.5;
    for (int i = 0; i < sampleshalf; i++)
    {
        float3 kernelVec = reflect(kernel[i], noise);
        kernelVec.xy *= aspectRatio;

        float radius = SampleRadius;

        kernelVec.xy = (kernelVec.xy / currentDistance) * radius;

        float biggestAnglePos = 0.0f;

        float biggestAngleNeg = 0.0f;

        float wAO = 0.0;

        for (int b = 1; b <= 4; b++)
        {
            float3 sampleVec = getPosition(texCoord + kernelVec.xy * b / 4.0f) - currentPos;

            float sampleAngle = dot(normalize(sampleVec), currentNormal);

            sampleAngle *= step(0.3, sampleAngle);

            if (sampleAngle > biggestAnglePos)
            {
                wAO += saturate(weightFunction(sampleVec, radius) * (sampleAngle - biggestAnglePos));

                biggestAnglePos = sampleAngle;
            }

            sampleVec = getPosition(texCoord - kernelVec.xy * b / 4.0f) - currentPos;

            sampleAngle = dot(normalize(sampleVec), currentNormal);

            if (sampleAngle > biggestAngleNeg)
            {
                wAO += saturate(weightFunction(sampleVec, radius) * (sampleAngle - biggestAngleNeg));

                biggestAngleNeg = sampleAngle;
            }
        }
		

        amount -= wAO / Samples * Strength;
    }

    return float4(amount, amount, amount, amount);
}

technique SSAO
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 SSAOVertexShader();
        PixelShader = compile ps_3_0 SSAOPixelShader();
    }
}*/


matrix World;
matrix View;
matrix Projection;
matrix LightSpaceMat;
float3 viewPos;
float Alpha;
float fogStart = 256;
float fogRange = 512;
float fogDensity=0.1;
texture Texture;
float3 DiffuseColor = float3(1, 1, 1);
bool renderShadow;
sampler2D textureSampler = sampler_state
{
    Texture = <Texture>;
 
    MipFilter = Linear;
    MagFilter = Point;
    MinFilter = Point;
    AddressU = Wrap;
    AddressV = Wrap;
};
 

sampler ShadowMapSampler = sampler_state
{
    texture = <ShadowMap>;
    magfilter = Linear;
    minfilter = Linear;
    mipfilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexureCoordinate : TEXCOORD0;
    
};

struct VertexShaderOutput
{
   
    float4 Position : SV_POSITION;
    float3 Normal :TEXCOORD3;
    float2 TexureCoordinate : TEXCOORD0;
    float4 Color:COLOR;
    float3 PositionFog : TEXCOORD1;
    float4 LightSpacePosition : TEXCOORD2;
};

struct PixelShaderOutput
{
    float4 Color : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.PositionFog = worldPosition;
    //if(Alpha<1){output.Position.y=0;}
    float3 N =  normalize(input.Normal);
    float3 L = normalize(  float3(1,5,3) );  
    float NdotL = min(max(dot(N, L), -0.1),0.5);
    output.Color=float4(0.5,0.5,0.5,1);
    output.Color+=float4(NdotL,NdotL,NdotL,1);
    output.Normal=input.Normal;
    output.TexureCoordinate = input.TexureCoordinate;
    output.LightSpacePosition = mul(worldPosition, LightSpaceMat);
    
    return output;
}
float ShadowCalculation(float4 fragPosLightSpace,float3 normal)
{
   
    float3 projCoords = fragPosLightSpace.xyz /fragPosLightSpace.w;
   
    projCoords.xy= projCoords * 0.5 + 0.5;
    projCoords.y = 1 - projCoords.y;
    bool isOutBounds = false;
    if (projCoords.x < 0 || projCoords.x > 1 || projCoords.y < 0 || projCoords.y > 1)
    {
        isOutBounds = true;
    }
    float closestDepth = tex2D(ShadowMapSampler, projCoords.xy).r;
   
    float currentDepth = projCoords.z;
    float shadow;
    float shadowBias =-0.003;
    float2 texelSize = 1.0 / 2048.0;
    for (int x = -1; x <= 1; ++x)
    {
        for (int y = -1; y <= 1; ++y)
        {
            float pcfDepth = tex2D(ShadowMapSampler, projCoords.xy + float2(x, y) * texelSize).r;
         //   shadow += currentDepth - shadowBias > pcfDepth ? 1.0 : 0.0;
            if (pcfDepth - shadowBias < currentDepth)
            {
                shadow+= 0;
            }
            else
            {
                shadow+= 1;
            }
        }
    }
    shadow /= 9.0;
    /*if (closestDepth - shadowBias < currentDepth)
    {
        shadow = 0;
    }
    else
    {
        shadow = 1;
    }*/
    if (closestDepth <= 0.01)
    {
        shadow = 1.0;
    }
 //   float shadow = currentDepth/closestDepth ;
   
    /*if (projCoords.z > 1.0)
    {
        shadow = 1.0;
    }*/
    if (isOutBounds == true)
    {
        shadow = 1.0;
    }
    return shadow;
}
PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
    
    PixelShaderOutput output = (PixelShaderOutput)0;

    output.Color = tex2D(textureSampler, input.TexureCoordinate);
   
    output.Color *= input.Color;
    output.Color.a *=Alpha;
   // input.Position.w =0;
  //  output.Color.a =0;
    
    float3 viewPostrans =  input.PositionFog-viewPos;
    float eyeDist = length(viewPostrans);
    float fogIntensity = max((eyeDist - fogStart),0) / (fogRange - fogStart);
     fogIntensity = clamp(fogIntensity, 0,1);
    if (output.Color.r < 0.01 && output.Color.g < 0.01 && output.Color.b < 0.01)
    {
        output.Color.a = 0;
    }
  
    float shadow = ShadowCalculation(input.LightSpacePosition,input.Normal);
   
        output.Color.rgb *= (0.5 + (shadow * 0.5));
   
    
    output.Color.rgb = lerp(output.Color.rgb,float3(100, 149,  237 )/float3(255,255,255) , fogIntensity);
   // output.Color = float4(1, 1, 1, 1);
    return output;
}

technique BlockTechnique
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
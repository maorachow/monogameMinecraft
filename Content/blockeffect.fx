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
    float3 Normal :NORMAL0;
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
    float3 L = normalize(  float3(1,3,5) );  
    float NdotL = min(max(dot(N, L), -0.1),0.5);
    output.Color=float4(0.5,0.5,0.5,1);
    output.Color+=float4(NdotL,NdotL,NdotL,1);
    output.Normal=input.Normal;
    output.TexureCoordinate = input.TexureCoordinate;
    output.LightSpacePosition = mul(worldPosition, LightSpaceMat);
    
    return output;
}
float ShadowCalculation(float4 fragPosLightSpace)
{
   
    float3 projCoords = fragPosLightSpace.xyz /fragPosLightSpace.w;
   
    projCoords = projCoords * 0.5 + 0.5;
    projCoords.y = 1 - projCoords.y;
        
    float closestDepth = tex2D(ShadowMapSampler, projCoords.xy).r;
    if (closestDepth <=0.0001)
    {
        return 0;
    }
    float currentDepth = projCoords.z;
    float bias = 0.0005;
   
        
    float shadow = currentDepth-bias>closestDepth?1:0   ;
    if (projCoords.z > 1.0)
    {
        shadow = 0.0;
    }
    return shadow;
}
PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
    
    PixelShaderOutput output = (PixelShaderOutput)0;

    output.Color = tex2D(textureSampler, input.TexureCoordinate);
    if(output.Color.r <0.1&&output.Color.g <0.1&&output.Color.b <0.1){
        output.Color.a=0;
        }
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
  
    float shadow = ShadowCalculation(input.LightSpacePosition);
      if (renderShadow)
    {
        output.Color.rgb *= (0.5 + (shadow * 0.5));
    }
    
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
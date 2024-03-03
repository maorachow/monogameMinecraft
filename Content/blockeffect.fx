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
float3 LightPos = float3(20, 70, 30);
float3 LightDir = float3(20, 40, 30);
float3 LightColor = float3(1, 1, 1);
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
 
sampler2D normalSampler = sampler_state
{
    Texture = <TextureNormal>;
 
    MipFilter = Linear;
    MagFilter = Point;
    MinFilter = Point;
    AddressU = Wrap;
    AddressV = Wrap;
};
 
sampler ShadowMapSampler = sampler_state
{
    texture = <ShadowMap>;
    magfilter = Point;
    minfilter = Point;
    mipfilter = Point;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : TEXCOORD1;
    float2 TexureCoordinate : TEXCOORD0;
    float3 Tangent : TEXCOORD2;
};

struct VertexShaderOutput
{
   
    float4 Position : SV_POSITION;
    float3 Normal :TEXCOORD3;
    float3x3 TBN : TEXCOORD4;
    float2 TexureCoordinate : TEXCOORD0;
    
    float3 FragPos : TEXCOORD1;
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
    output.Position = mul(viewPosition, Projection) ;
    output.FragPos = worldPosition.xyz;
    //if(Alpha<1){output.Position.y=0;}
   
    output.Normal=input.Normal;
    float3 BitTangent = cross(input.Normal, input.Tangent);
    float3 T = normalize(input.Tangent);
    float3 B = normalize(BitTangent);
    float3 N = normalize(input.Normal);
    float3x3 TBN = float3x3(T, B, N);
    output.TBN = TBN;
    output.TexureCoordinate = input.TexureCoordinate;
    output.LightSpacePosition = mul(worldPosition, LightSpaceMat);
    
    return output;
}
float ShadowCalculation(float4 fragPosLightSpace)
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
    float2 texelSize = 1.0 / 8192.0;
    for (int x = -1; x <= 1; ++x)
    {
        for (int y = -1; y <= 1; ++y)
        {
            float pcfDepth = tex2D(ShadowMapSampler, projCoords.xy + float2(x, y) * texelSize).r;
         //   shadow += currentDepth - shadowBias > pcfDepth ? 1.0 : 0.0;
            if (pcfDepth - shadowBias > currentDepth)
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
    if (closestDepth <= 0.03)
    {
        shadow =0.0;
    }
 //   float shadow = currentDepth/closestDepth ;
   
    if (projCoords.z > 1.0)
    {
        shadow = 0.0;
    }
    if (isOutBounds == true)
    {
        shadow =0.0;
    }
    return shadow;
}
PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
    
    PixelShaderOutput output = (PixelShaderOutput)0;
    float3 objectColor = tex2D(textureSampler, input.TexureCoordinate).rgb;
    float3 ambient = LightColor * 0.2;
    float3 texNormal = tex2D(normalSampler, input.TexureCoordinate).rgb;
    texNormal = normalize(texNormal * 2.0 - 1.0);
    texNormal = normalize(mul(texNormal, input.TBN));
    float3 normal = texNormal;
    float3 lightDir = normalize(LightDir) ;
    float diff = max(dot(normal, lightDir), 0.0);
    float3 diffuse = diff * LightColor ;
     
    float3 specLightDir = normalize(LightPos - input.FragPos);
    float3 viewDir = normalize(viewPos - input.FragPos);
    float3 reflectDir = reflect(-specLightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 16);
    float3 specular = 0.8 * spec * LightColor;
    float shadow = ShadowCalculation(input.LightSpacePosition);
    float3 result = (ambient +(1-shadow)*(diffuse + specular))* objectColor;
    
    output.Color = float4(result, 1);
    output.Color.a *=Alpha;
   // input.Position.w =0;
  //  output.Color.a =0;
    
    float3 viewPostrans =  input.FragPos-viewPos;
    float eyeDist = length(viewPostrans);
    float fogIntensity = max((eyeDist - fogStart),0) / (fogRange - fogStart);
     fogIntensity = clamp(fogIntensity, 0,1);
    if (output.Color.r < 0.01 && output.Color.g < 0.01 && output.Color.b < 0.01)
    {
        output.Color.a = 0;
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
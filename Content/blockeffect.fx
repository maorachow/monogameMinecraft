matrix World;
matrix View;
matrix Projection;
matrix LightSpaceMat;
matrix LightSpaceMatFar;
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
bool receiveAO;
float shadowBias;
bool receiveBackLight = false;
bool receiveReflection;

bool receiveShadow;
float3 LightPositions[4];
float3 LightPosition1;
float3 LightPosition2;
float3 LightPosition3;
float3 LightPosition4;
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
sampler2D depthSampler = sampler_state
{
    Texture = <TextureDepth>;
 
    MipFilter = Linear;
    MagFilter = Point;
    MinFilter = Point;
    AddressU = Wrap;
    AddressV = Wrap;
};
sampler2D AOSampler = sampler_state
{
    Texture = <TextureAO>;
 
    MipFilter = Linear;
    MagFilter = Point;
    MinFilter = Point;
    AddressU = Border;
    AddressV = Border;
};
sampler2D reflectionSampler = sampler_state
{
    Texture = <TextureReflection>;
 
    MipFilter = Linear;
    MagFilter = Linear;
    MinFilter = Linear;
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
sampler ShadowMapFarSampler = sampler_state
{
    texture = <ShadowMapFar>;
    magfilter = Point;
    minfilter = Point;
    mipfilter = Point;
    AddressU = Wrap;
    AddressV = Wrap;
};
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexureCoordinate : TEXCOORD0;
    float3 Tangent : TANGENT0;
};

struct VertexShaderOutput
{
   
    float4 Position : SV_POSITION;
    float3 Normal :TEXCOORD3;
    float3x3 TBN : TEXCOORD4;
    float2 TexureCoordinate : TEXCOORD0;
    float3 TangentFragPos : TEXCOORD10;
    float3 TangentViewPos : TEXCOORD11;
    float3 FragPos : TEXCOORD1;
    float4 LightSpacePosition : TEXCOORD2;
   
    float2 ScreenSpaceUV : TEXCOORD12;
    
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
    float4 screenPosition = mul(viewPosition, Projection);
    screenPosition.xyz /= screenPosition.w;
    screenPosition.xy = screenPosition.xy*0.5 + 0.5;
    screenPosition.y = 1 - screenPosition.y;
    output.ScreenSpaceUV = screenPosition;
     
    float3x3 worldMat = (float3x3) World;
    float3 BitTangent = cross(input.Normal, input.Tangent);
    float3 T = normalize(mul(input.Tangent, World));
    float3 B = normalize(mul(BitTangent, World));
    float3 N = normalize(mul(input.Normal, World));
    float3x3 TBN = float3x3(T, B, N);
    output.TBN = TBN;
    output.TangentFragPos = mul(output.TBN, worldPosition.xyz);
    output.TangentViewPos = mul(output.TBN, viewPos);
    output.TexureCoordinate = input.TexureCoordinate;
    output.LightSpacePosition = mul(worldPosition, LightSpaceMat);
   // output.LightSpacePositionFar = mul(worldPosition, LightSpaceMatFar);
    return output;
}
float ShadowCalculation(float4 fragPosLightSpace,sampler2D sp,float bias)
{
   
    float3 projCoords = fragPosLightSpace.xyz /fragPosLightSpace.w;
   
    projCoords.xy= projCoords * 0.5 + 0.5;
    projCoords.y = 1 - projCoords.y;
    bool isOutBounds = false;
    if (projCoords.x < 0 || projCoords.x > 1 || projCoords.y < 0 || projCoords.y > 1)
    {
        isOutBounds = true;
    }
    float closestDepth = tex2D(sp, projCoords.xy).r;
   
    float currentDepth = projCoords.z;
    float shadow;
     
    float2 texelSize = 1.0 / 2048.0;
    for (int x = -1; x <= 1; ++x)
    {
        for (int y = -1; y <= 1; ++y)
        {
            float pcfDepth = tex2D(sp, projCoords.xy + float2(x, y) * texelSize).r;
         //   shadow += currentDepth - shadowBias > pcfDepth ? 1.0 : 0.0;
            if (pcfDepth - shadowBias+bias > currentDepth)
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

float2 ParallaxMapping(float2 texCoords, float3 viewDir)
{
   /* float height = tex2D(depthSampler, texCoords).r;
    float2 p = viewDir.xy / viewDir.z * (height * 0.003);
    return texCoords - p;*/
    
     // number of depth layers
    const float numLayers = 10;
    // calculate the size of each layer
    float layerDepth = 1.0 / numLayers;
    // depth of current layer
    float currentLayerDepth = 0.0;
    // the amount to shift the texture coordinates per layer (from vector P)
    float2 P = viewDir.xy * 0.005;
    float2 deltaTexCoords = P / numLayers;
    float2 currentTexCoords = texCoords;
    float currentDepthMapValue = tex2D(depthSampler, currentTexCoords).r;
    int i = 5;
    [unroll]
    while (currentLayerDepth < currentDepthMapValue&&i>0)
    {
        i--;
    // shift texture coordinates along direction of P
        currentTexCoords -= deltaTexCoords;
    // get depthmap value at current texture coordinates
        currentDepthMapValue = tex2D(depthSampler, currentTexCoords).r;
    // get depth of next layer
        currentLayerDepth += layerDepth;
    }

    return currentTexCoords;
}
float3 CaculatePointLight(float3 lightPos,float3 fragPos,float3 normal,float3 viewDir)
{
    if (abs(lightPos.x) < 0.001 && abs(lightPos.x) < 0.001 && abs(lightPos.x) < 0.001)
    {
        return float3(0, 0, 0);
    }
    float3 lightDir = normalize(lightPos - fragPos);
    
    float diff = max(dot(normal, lightDir), 0.0);
    float diffBack = max(dot(normal, -lightDir), 0) * 0.3;
    if (receiveBackLight)
    {
        diff += diffBack;
    }
    float3 reflectDir = reflect(-lightDir, normal);
    float3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), 128);
    
    float distance = length(lightPos - fragPos);
    float attenuation = 1.0 / (5 * (distance * distance));
    
    float3 ambient = 0;
    float3 diffuse = diff * float3(255 / 255, 255 / 255, 150.0 / 255.0);
    float3 specular = spec;
    ambient *= attenuation;
    diffuse *= attenuation;
    specular *= attenuation;
   return (ambient + diffuse + specular);
}
PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
    
    PixelShaderOutput output = (PixelShaderOutput)0;
    
    //
    float2 texCoordsOrigin = input.TexureCoordinate*16;
   
    float2 ceilTexCoords = float2(floor(texCoordsOrigin.x)+1, floor(texCoordsOrigin.y)+1);
    float2 floorTexCoords = float2(floor(texCoordsOrigin.x), floor(texCoordsOrigin.y));
    
    float3 paraViewDir = normalize(input.TangentViewPos - input.TangentFragPos);
    float2 texCoords = ParallaxMapping(input.TexureCoordinate, paraViewDir);
 
    float2 texCoordLimitBottom = floorTexCoords/16;
    float2 texCoordLimitUp = ceilTexCoords/16;
  /*  if ((abs(texCoords.x - input.TexureCoordinate.x) > 0.0625 / 64 || abs(texCoords.y - input.TexureCoordinate.y) > 0.0625 / 64))
    {
        texCoords = input.TexureCoordinate;
    }*/
    texCoords.x = clamp(texCoords.x, texCoordLimitBottom.x, texCoordLimitUp.x - 0.0002);
    texCoords.y = clamp(texCoords.y, texCoordLimitBottom.y, texCoordLimitUp.y - 0.0002);
   // texCoords.x = texCoordLimitUp.x-0.001;
  //  texCoords.y = texCoordLimitUp.y - 0.001;
  /*  if (tex2D(textureSampler, texCoords).a < 0. 0001)
    {
        texCoords = input.TexureCoordinate;
    }*/
   
  
    float3 ambient = 0.2 * LightColor;
    
    if (receiveAO==true)
    {
         ambient = (clamp(tex2D(AOSampler, input.ScreenSpaceUV.xy).r ,0.1,1))* 0.2 * LightColor;  
    }
      
     

    float3 objectColor = tex2D(textureSampler, texCoords).rgb;
    if (receiveReflection == true)
    { 
        if (length(viewPos - input.FragPos) < 70)
        {
            if (tex2D(reflectionSampler, input.ScreenSpaceUV.xy).a > 0.001)
            {
              objectColor *=tex2D(reflectionSampler, input.ScreenSpaceUV.xy).rgb;  
            }
           
           
        }
       

    }
  
    float3 texNormal = tex2D(normalSampler, texCoords).rgb;
   
    texNormal = normalize(texNormal * 2.0 - 1.0);
    texNormal = normalize(mul(texNormal, input.TBN));
    float3 normal = texNormal;
    float3 lightDir = normalize(LightDir) ;
     
    float3 viewDir = normalize(viewPos - input.FragPos);
    
   
      float diff = max(dot(normal, lightDir), 0.0);
    float diffBack = max(dot(normal, -lightDir), 0)*0.3;
    if (receiveBackLight)
    {
        diff += diffBack;
    }
    float3 diffuse = diff * LightColor ;  
   
    
     
    float3 specLightDir = normalize(LightPos - input.FragPos);
 
    float3 halfwayDir = normalize(lightDir + viewDir);
     
    float spec = pow(max(dot(normal, halfwayDir), 0.0), 16);
    float3 specular = 0.8 * spec * LightColor;
    float shadow; 
    float shadow1;
    float4 LightSpacePositionFar = mul(float4(input.FragPos, 1), LightSpaceMatFar);
    
    
    if (receiveShadow == true)
    {
        shadow = ShadowCalculation(input.LightSpacePosition, ShadowMapSampler, 0);
       if (length(viewPos - input.FragPos)>32)
            {
        shadow1 = ShadowCalculation(LightSpacePositionFar, ShadowMapFarSampler, 0);
            }
        else
        {
        shadow1 = 0;
        }  
    }
    
   
    float3 result;
    float shadowFinal = clamp(  shadow+shadow1, 0, 1);
    if (receiveShadow == true)
    {
       result = (ambient + (1 - shadowFinal) * (diffuse + specular)) * objectColor;  
    }
    else{
        
        result = (ambient) * objectColor;
    }
    
   
     
        result+=CaculatePointLight(LightPosition1, input.FragPos, normal, viewDir);
    result += CaculatePointLight(LightPosition2, input.FragPos, normal, viewDir);
    result += CaculatePointLight(LightPosition3, input.FragPos, normal, viewDir);
    result += CaculatePointLight(LightPosition4, input.FragPos, normal, viewDir);
     
    
        output.Color = float4(result, 1);
        output.Color.a *=Alpha;
   
    
    float3 viewPostrans =  input.FragPos-viewPos;
    float eyeDist = length(viewPostrans);
    float fogIntensity = max((eyeDist - fogStart),0) / (fogRange - fogStart);
     fogIntensity = clamp(fogIntensity, 0,1);
    if (tex2D(textureSampler, texCoords).a<=0.01)
    {
        discard;
    }
  //  output.Color.a = tex2D(textureSampler, texCoords).a;
    
    
    
    output.Color.rgb = lerp(output.Color.rgb, float3(100, 149, 237) / float3(255, 255, 255), fogIntensity);
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
matrix World;
matrix View;
matrix Projection;
float3 viewPos;
float Alpha;
float fogStart = 256;
float fogRange = 512;
float fogDensity=0.1;
texture Texture;

sampler2D textureSampler = sampler_state
{
    Texture = <Texture>;
 
    MipFilter = Linear;
    MagFilter = Point;
    MinFilter = Point;
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
      

    return output;
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
    output.Color.rgb = lerp(output.Color.rgb,float3(100, 149,  237 )/float3(255,255,255) , fogIntensity);
    
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
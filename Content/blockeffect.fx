matrix World;
matrix View;
matrix Projection;

float Alpha = 1;
float LightIntensity = 1;

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
    float Light : NORMAL0;
    float2 TexureCoordinate : TEXCOORD0;
    
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float Light : NORMAL0;
    float2 TexureCoordinate : TEXCOORD0;
    
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

    output.TexureCoordinate = input.TexureCoordinate;
        
  

    return output;
}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
    PixelShaderOutput output = (PixelShaderOutput)0;
    output.Color = tex2D(textureSampler, input.TexureCoordinate);
        output.Color.a = Alpha;
   // output.Color.a *= 0.5;

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
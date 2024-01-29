matrix World;
matrix LightSpaceMat;
 
struct VertexShaderInput
{
    float4 Position : POSITION0;
  
    
};
struct VertexShaderOutput
{
   
    float4 Position : SV_POSITION;
    float2 Depth : TEXCOORD0;
};

struct PixelShaderOutput
{
    float4 Color : COLOR0;
};


VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    float4 worldPosition = mul(input.Position, World);
    float4 projectionPosition = mul(worldPosition, LightSpaceMat);
    output.Position = projectionPosition;
    output.Depth = output.Position.zw;
    return output;
}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
    PixelShaderOutput output = (PixelShaderOutput) 0;
    output.Color = float4(input.Depth.x / input.Depth.y, input.Depth.x / input.Depth.y, input.Depth.x / input.Depth.y, 1);
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
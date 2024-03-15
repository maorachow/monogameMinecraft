﻿ 

matrix World;
matrix View;
matrix Projection;
float3x3 TransposeInverseView;
int renderTarget;
struct VertexShaderInput
{
    float4 Position : Position;
    float3 Normal : NORMAL0;
    float2 TexureCoordinate : TEXCOORD0;
    float3 Tangent : TANGENT0;
};

struct VertexShaderOutput
{
    float4 PositionScreenSpace  : SV_Position;
    float4 PositionV : TEXCOORD1;
    
    float4 PositionP : TEXCOORD4;
    float3 Normal : TEXCOORD2;
    float3 Tangent : TEXCOORD3;
};
struct PixelShaderOutput
{
    
    float4 ViewPosition : COLOR0;
    float4 ProjectionDepth : COLOR1;
    float4 Normal : COLOR2;
    float4 PureBlack : COLOR3;
};

 
 
VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.PositionScreenSpace = mul(viewPosition, Projection);
    
    output.PositionV = viewPosition;
    output.PositionP = mul(viewPosition, Projection);
    float3x3 worldView =   World*View;
    
    output.Tangent = mul(input.Tangent, TransposeInverseView);
    
    output.Normal = mul(input.Normal, TransposeInverseView);
     
	return output;
}
 
float LinearizeDepth(float depth)
{
    float NEAR = 0.1;
     float FAR = 50.0f;
    float z = depth * 2.0 - 1.0; 
    return (2.0 * NEAR * FAR) / (FAR + NEAR - z * (FAR - NEAR));
}
PixelShaderOutput MainPS(VertexShaderOutput input) 
{
    PixelShaderOutput psOut = (PixelShaderOutput) 0;
    
        psOut.ViewPosition.xyzw = input.PositionV.xyzw;
    float3 position = input.PositionP.xyz / input.PositionP.w;
    
    psOut.ProjectionDepth.rgb = position.z;
    psOut.ProjectionDepth.a = 1;
    psOut.Normal = float4(normalize(input.Normal) * 0.5 + 0.5, 1);
    psOut.PureBlack = float4(1,1, 1, 1);
    return psOut;

}

technique GBuffer
{
	pass P0
	{
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 MainPS();
    }
};
/*matrix World;
matrix View;
matrix Projection;

 

float FarClip;

struct VS_INPUT
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
};

struct VS_OUTPUT
{
    float4 Position : POSITION0;
    float3 Normal : TEXCOORD0;
    float4 vPositionVS : TEXCOORD1;
};

VS_OUTPUT DepthVertexShaderFunction(VS_INPUT IN)
{
    VS_OUTPUT Output;
    float4x4 WorldView;
    float4x4 ITWorldView;
    float4x4 WorldViewProjection;
    WorldViewProjection = World * View * Projection;
    WorldView = World * View;
    ITWorldView = World * View;
    Output.Position = mul(IN.Position, mul(mul(World, View),);
    Output.vPositionVS = mul(IN.Position, WorldView);
    Output.Normal = mul(IN.Normal, ITWorldView);

    return Output;
}

float4 DepthPixelShaderFunction(VS_OUTPUT IN) : COLOR
{
    float depth = IN.vPositionVS.z / 50;
    IN.Normal = normalize(IN.Normal);
    return float4(IN.Normal.x, IN.Normal.y, IN.Normal.z, depth);
}

technique Depth
{
    pass Pass1
    {

        VertexShader = compile vs_3_0 DepthVertexShaderFunction();
        PixelShader = compile ps_3_0 DepthPixelShaderFunction();
    }
}*/
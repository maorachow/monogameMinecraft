#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif
 

matrix World;
matrix View;
matrix Projection;



sampler2D textureSampler = sampler_state
{
    Texture = <TextureE>;
 
    MipFilter = Linear;
    MagFilter = Point;
    MinFilter = Point;
    AddressU = Wrap;
    AddressV = Wrap;
};
float3 DiffuseColor = float3(1, 1, 1);


struct VertexShaderInput
{
	float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float4 PositionV : TEXCOORD2;
    float4 PositionWS : TEXCOORD3;
    float3 Normal : TEXCOORD1;
    float2 TexureCoordinate : TEXCOORD0;
};
struct PixelShaderOutput
{
    float4 ProjectionDepth : COLOR0;
  //  float4 Normal : COLOR2;
    
    float4 NormalWS : COLOR1;
    float4 Albedo : COLOR2;
    float4 PsoitionWS : COLOR3;
	
};
VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.PositionV = viewPosition;
    output.PositionWS = worldPosition;
	output.TexureCoordinate = input.TexureCoordinate;
    output.Normal = input.Normal;
	return output;
}

PixelShaderOutput MainPS(VertexShaderOutput input) : COLOR
{
    PixelShaderOutput psOut = (PixelShaderOutput) 0;
    psOut.ProjectionDepth = float4((-input.PositionV.z / 100).xxx,1);
    psOut.NormalWS = float4(input.Normal, 1);
    psOut.Albedo = float4((tex2D(textureSampler, input.TexureCoordinate).xyz * DiffuseColor).xyz,1);
    psOut.PsoitionWS = input.PositionWS;
    return psOut;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
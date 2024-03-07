 

matrix World;
matrix View;
matrix Projection;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexureCoordinate : TEXCOORD0;
    float3 Tangent : TANGENT0;
};

struct VertexShaderOutput
{
    float4 PositionT : SV_Position;
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
};

float3x3 inverse_mat3(float3x3 m)
{
    float Determinant =
       m[0][0] * (m[1][1] * m[2][2] - m[2][1] * m[1][2])
     - m[1][0] * (m[0][1] * m[2][2] - m[2][1] * m[0][2])
     + m[2][0] * (m[0][1] * m[1][2] - m[1][1] * m[0][2]);
    
    float3x3 Inverse;
    Inverse[0][0] = +(m[1][1] * m[2][2] - m[2][1] * m[1][2]);
    Inverse[1][0] = -(m[1][0] * m[2][2] - m[2][0] * m[1][2]);
    Inverse[2][0] = +(m[1][0] * m[2][1] - m[2][0] * m[1][1]);
    Inverse[0][1] = -(m[0][1] * m[2][2] - m[2][1] * m[0][2]);
    Inverse[1][1] = +(m[0][0] * m[2][2] - m[2][0] * m[0][2]);
    Inverse[2][1] = -(m[0][0] * m[2][1] - m[2][0] * m[0][1]);
    Inverse[0][2] = +(m[0][1] * m[1][2] - m[1][1] * m[0][2]);
    Inverse[1][2] = -(m[0][0] * m[1][2] - m[1][0] * m[0][2]);
    Inverse[2][2] = +(m[0][0] * m[1][1] - m[1][0] * m[0][1]);
    Inverse /= Determinant;
    
    return Inverse;
} 
 
float LinearizeDepth(float depth)//0.6
{
    float near_plane = 1;
    float far_plane = 500;
    float z = depth * 2.0 - 1.0; // Back to NDC //0.2
    return (2.0 * near_plane * far_plane) / (far_plane + near_plane - z * (far_plane - near_plane));
}
VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.PositionT = mul(viewPosition, Projection);
    
    output.PositionV = viewPosition;
    output.PositionP = mul(viewPosition, Projection);
    float3x3 worldView =   World*View;
    float3x3 normalMatrix = transpose(inverse_mat3(worldView));
     output.Tangent = mul(input.Tangent , normalMatrix);
    
    output.Normal = mul(input.Normal, normalMatrix);
     
	return output;
}

PixelShaderOutput MainPS(VertexShaderOutput input) 
{
    PixelShaderOutput psOut = (PixelShaderOutput) 0;
   
    psOut.ViewPosition.xyz = input.PositionV.xyz *0.5+0.5 ;
    psOut.ViewPosition.a = (input.PositionP.z / input.PositionP.w)  ;
   
    psOut.ProjectionDepth.rgb = (input.PositionP.z / input.PositionP.w) * 0.5 + 0.5;
    psOut.ProjectionDepth.a = 1;
    psOut.Normal = float4(normalize(input.Normal) * 0.5 + 0.5, 1);
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
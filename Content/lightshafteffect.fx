float gNumSamples = 64;

 
 

 

sampler2D gTextureMask = sampler_state
{
    Texture = (maskTex);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = NONE;
};

float2 gScreenLightPos;

float gDensity;

float gDecay;

float gWeight;

float gExposure;

 

struct PS_IN
{
    float4 position : SV_POSITION;
    float4 color : COLOR0;
    float2 texCoord : TEXCOORD0;
};

 
 
float4 PixelShaderFunction(PS_IN input) : SV_TARGET0
{
    float2 TexCoord = input.texCoord;
	// Calculate vector from pixel to light source in screen space.
   
    float2 DeltaTexCoord = (TexCoord.xy - gScreenLightPos.xy);
//    float Len = length(DeltaTexCoord);
	// Divide by number of samples and scale by control factor.
    DeltaTexCoord *=1.0 / 100 * gDensity;
	// Store initial sample.
    float3 Color = tex2D(gTextureMask, TexCoord);
	// Set up illumination decay factor.
    float IlluminationDecay = 1.0;
    float3 Sample;
	// Evaluate summation from Equation 3 ( see https://developer.nvidia.com/gpugems/GPUGems3/gpugems3_ch13.html) NUM_SAMPLES iterations.
    [unroll]
    for (int i = 0; i < 100; ++i)
    {
		// Step sample location along ray.
        TexCoord -= DeltaTexCoord;
		// Retrieve sample at new location.
        Sample = tex2D(gTextureMask, TexCoord);
		// Apply sample attenuation scale/decay factors.
        Sample *= IlluminationDecay * gWeight;
		// Accumulate combined color.
        Color += Sample;
		// Update exponential decay factor.
        IlluminationDecay *= gDecay;
    }
    
	// Output final color with a further scale control factor.
        return float4(Color.xyz * gExposure, 1.0);
}
 

technique LightShaft
{

    pass p0
    {

       

        PixelShader = compile ps_3_0 PixelShaderFunction();

    }

}

 
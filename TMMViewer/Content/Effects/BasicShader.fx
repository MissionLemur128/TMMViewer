#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#endif

float4x4 _world;
float4x4 _view;
float4x4 _projection;

float3 _diffuseColor;

float3 _sunLightDirection;
float3 _sunLightColor;
float3 _ambientLightColor;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Normal : NORMAL0;
    float2 UV : TEXCOORD0;
    //float4 BoneWeights : BLENDWEIGHT0;
   // int4 BoneIndices : BLENDINDICES0;
    float4 test : Color0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Normal : NORMAL0;
    float2 UV : TEXCOORD0;
    float3 NormalView : TEXCOORD1;
    float test : Color0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    float4 worldPosition = mul(input.Position, _world);
    float4 viewPosition = mul(worldPosition, _view);
    output.Position = mul(viewPosition, _projection);
    output.Normal = input.Normal;
    output.UV = input.UV;
    output.NormalView = mul(input.Normal, _view).xyz;
    output.test = (input.test.x + input.test.y * 100) * 0.01;
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 normal = normalize(input.Normal.xyz);
    float3 lightDirection = normalize(_sunLightDirection);
    float lightIntensity = max(dot(normal, lightDirection), 0.0);
    float3 lightColor = _ambientLightColor + _sunLightColor * lightIntensity * 0.5;
    
    // magic formula to highlight edges based on the x component of the view transformed normal
    float edgeHighlight = 1 - 0.5 * saturate(pow(abs(input.NormalView.x) * 0.15, 2));
    
    float height = input.test;
    
    //return float4(height, height, height, 1.0);
    //return float4(normal, 1.0);
    return float4(lightColor * _diffuseColor * edgeHighlight, 1.0);
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
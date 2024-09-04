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
    float3 Normal : NORMAL0;
    float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float2 UV : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    float4 worldPosition = mul(input.Position, _world);
    float4 viewPosition = mul(worldPosition, _view);
    output.Position = mul(viewPosition, _projection);
    output.Normal = input.Normal;
    output.UV = input.UV;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 normal = normalize(input.Normal);
    float3 lightDirection = normalize(_sunLightDirection);
    float lightIntensity = max(dot(normal, lightDirection), 0.0);
    float3 lightColor = _ambientLightColor + _sunLightColor * lightIntensity;
    
    return float4(lightColor * _diffuseColor, 1.0);
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
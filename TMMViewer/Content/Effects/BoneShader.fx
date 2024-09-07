#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#endif

#include "./Shared/Common.fxh"

struct DefaultShaderInput
{
    float4 Position : SV_POSITION;
    float4 Normal : NORMAL0;
};

struct DefaultShaderOutput
{
    float4 Position : SV_POSITION;
    float Normal : TEXCOORD0;
};

DefaultShaderOutput VS_Default(in DefaultShaderInput input)
{
    DefaultShaderOutput output = (DefaultShaderOutput) 0;
    
    output.Position = LocalToProjection(input.Position);
    output.Normal = max(0, normalize(mul(mul(float4(input.Normal.xyz, 0), _world), _view)).z);
    return output;
}

float4 PS_Default(DefaultShaderOutput input) : COLOR
{
    float v = input.Normal * 0.75 + 0.25;
    return float4(v * 0.5, v, v * 0.7, 1.0);
}

technique Default
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL VS_Default();
        PixelShader = compile PS_SHADERMODEL PS_Default();
    }
};
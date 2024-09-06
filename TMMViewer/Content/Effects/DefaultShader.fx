#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#endif

#include "./Shared/Common.fxh"
#include "./Shared/CommonLighting.fxh"

float3 _diffuseColor;

struct DefaultShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Normal : NORMAL0;
    float2 UV : TEXCOORD0;
    float3 NormalView : TEXCOORD1;
    float Mask : Color0;
};


///
/// Vertex Shaders
///
DefaultShaderOutput VS_Default(in VertexShaderInput input)
{
    DefaultShaderOutput output = (DefaultShaderOutput) 0;

    float4 worldPosition = mul(input.Position, _world);
    float4 viewPosition = mul(worldPosition, _view);
    output.Position = mul(viewPosition, _projection);
    output.Normal = input.Normal;
    output.UV = input.UV;
    output.NormalView = mul(input.Normal, _view).xyz;
    return output;
}

VertexColorShaderOutput VS_BoneWeights(in VertexShaderInput input)
{
    VertexColorShaderOutput output = (VertexColorShaderOutput) 0;    
    output.Position = LocalToProjection(input.Position);
    output.Color = float4(input.BoneWeights.wzy, 1);
    return output;
}

VertexColorShaderOutput VS_Mask(in VertexShaderInput input)
{
    VertexColorShaderOutput output = (VertexColorShaderOutput) 0;
    output.Position = LocalToProjection(input.Position);
    float mask = (input.Mask.x + input.Mask.y * 256) * 0.005;
    output.Color = float4(mask, mask, mask, 1);
    return output;
}

VertexColorShaderOutput VS_Normals(in VertexShaderInput input)
{
    VertexColorShaderOutput output = (VertexColorShaderOutput) 0;
    output.Position = LocalToProjection(input.Position);
    output.Color = input.Normal * 0.5 + 0.5;
    return output;
}

///
/// Pixel Shaders
///
float4 PS_Default(DefaultShaderOutput input) : COLOR
{
    float3 normal = normalize(input.Normal.xyz);
    float3 lightDirection = normalize(_sunLightDirection);
    float lightIntensity = max(dot(normal, lightDirection), 0.0);
    float3 lightColor = _ambientLightColor + _sunLightColor * lightIntensity * 0.5;
    
    // magic formula to highlight edges based on the x component of the view transformed normal
    float edgeHighlight = 1 - 0.5 * saturate(pow(abs(input.NormalView.x) * 0.15, 2));
    return float4(lightColor * _diffuseColor * edgeHighlight, 1.0);
}



technique Solid
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL VS_Default();
        PixelShader = compile PS_SHADERMODEL PS_Default();
    }
};

technique BoneWeights
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL VS_BoneWeights();
        PixelShader = compile PS_SHADERMODEL PS_ColorOnly();
    }
};

technique Mask
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL VS_Mask();
        PixelShader = compile PS_SHADERMODEL PS_ColorOnly();
    }
};

technique Normals
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL VS_Normals();
        PixelShader = compile PS_SHADERMODEL PS_ColorOnly();
    }
};
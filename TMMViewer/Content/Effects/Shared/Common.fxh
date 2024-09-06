/*
    Uniforms
*/
float4x4 _world;
float4x4 _view;
float4x4 _projection;

/*
    Structs
*/
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Normal : NORMAL0;
    float2 UV : TEXCOORD0;
    float4 BoneWeights : BLENDWEIGHT0;
    int4 BoneIndices : BLENDINDICES0;
    float4 Mask : Color0;
};

struct VertexColorShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : Color0;
};

/*
    PS / VS Functions
*/
float4 PS_ColorOnly(VertexColorShaderOutput input) : COLOR
{
    return input.Color;
}

/*
    Helper Functions
*/
float4 LocalToProjection(float4 localPosition)
{
    float4 worldPosition = mul(localPosition, _world);
    float4 viewPosition = mul(worldPosition, _view);
    return mul(viewPosition, _projection);
}

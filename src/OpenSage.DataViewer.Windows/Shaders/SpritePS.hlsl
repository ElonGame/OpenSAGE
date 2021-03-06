#include "Sprite.hlsli"

Texture2D<float4> BaseTexture : register(t0);
SamplerState LinearSampler : register(s0);

struct TextureConstants
{
    uint MipMapLevel;
};

ConstantBuffer<TextureConstants> TextureCB : register(b0);

float4 main(PSInput input) : SV_TARGET
{
    return BaseTexture.SampleLevel(
        LinearSampler, 
        input.TexCoords, 
        TextureCB.MipMapLevel);
}
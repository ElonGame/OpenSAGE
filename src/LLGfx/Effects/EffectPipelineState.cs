﻿namespace LLGfx.Effects
{
    public struct EffectPipelineState
    {
        public RasterizerStateDescription RasterizerState { get; }

        public DepthStencilStateDescription DepthStencilState { get; }

        public BlendStateDescription BlendState { get; }

        public EffectPipelineState(
            RasterizerStateDescription rasterizerState,
            DepthStencilStateDescription depthStencilState,
            BlendStateDescription blendState)
        {
            RasterizerState = rasterizerState;
            DepthStencilState = depthStencilState;
            BlendState = blendState;
        }

        public EffectPipelineStateHandle GetHandle()
        {
            return EffectPipelineStateFactory.GetHandle(ref this);
        }
    }
}

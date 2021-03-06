﻿namespace LLGfx
{
    public sealed partial class SwapChain : GraphicsObject
    {
        public int BackBufferWidth => PlatformBackBufferWidth;
        public int BackBufferHeight => PlatformBackBufferHeight;

        public RenderTarget GetNextRenderTarget()
        {
            return PlatformGetNextRenderTarget();
        }
    }
}

﻿using Metal;

namespace OpenZH.Graphics.LowLevel
{
    partial class ShaderLibrary
    {
        internal IMTLLibrary DeviceLibrary { get; private set; }

        private void PlatformConstruct(GraphicsDevice graphicsDevice)
        {
            DeviceLibrary = AddDisposable(graphicsDevice.Device.CreateDefaultLibrary());
        }
    }
}

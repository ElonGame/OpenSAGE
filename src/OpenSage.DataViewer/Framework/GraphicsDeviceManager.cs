﻿using System;
using LLGfx;

namespace OpenSage.DataViewer.Framework
{
    public sealed class GraphicsDeviceManager : IDisposable
    {
        public GraphicsDevice GraphicsDevice { get; }

        public GraphicsDeviceManager()
        {
            GraphicsDevice = new GraphicsDevice();
        }

        public void Dispose()
        {
            GraphicsDevice.Dispose();
        }
    }
}

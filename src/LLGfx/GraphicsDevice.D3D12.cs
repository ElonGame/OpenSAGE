﻿using System;
using SharpDX.Direct3D12;
using LLGfx.Util;

namespace LLGfx
{
    partial class GraphicsDevice
    {
        private Fence _numCompletedFramesFence;
        private long _numCompletedFrames;
        private long _currentFrame = 1;

        internal Device Device { get; private set; }

        internal DescriptorTablePool DescriptorHeapDsv { get; private set; }

        internal DescriptorTablePool DescriptorHeapCbvUavSrv { get; private set; }

        internal DynamicUploadHeap DynamicUploadHeap { get; private set; }

        private PixelFormat PlatformBackBufferFormat => PixelFormat.Bgra8UNorm;

        private void PlatformConstruct()
        {
#if DEBUG
            DebugInterface.Get().EnableDebugLayer();
#endif

            Device = AddDisposable(new Device(null, SharpDX.Direct3D.FeatureLevel.Level_11_0));

            DescriptorHeapDsv = AddDisposable(new DescriptorTablePool(
                Device,
                DescriptorHeapType.DepthStencilView,
                100)); // TODO: Might need to increase this

            DescriptorHeapCbvUavSrv = AddDisposable(new DescriptorTablePool(
                Device,
                DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView,
                1_000_000)); // Maximum descriptor count for Tier 1 hardware

            _numCompletedFramesFence = AddDisposable(Device.CreateFence(0, FenceFlags.None));

            const uint initialDynamicUploadHeapSize = 1024 * 1024; // TODO
            DynamicUploadHeap = AddDisposable(new DynamicUploadHeap(Device, initialDynamicUploadHeapSize));
        }

        internal void FinishFrame()
        {
            _numCompletedFrames = Math.Max(_numCompletedFrames, _numCompletedFramesFence.CompletedValue);

            DynamicUploadHeap.FinishFrame(_currentFrame, _numCompletedFrames);

            CommandQueue.DeviceCommandQueue.Signal(_numCompletedFramesFence, _currentFrame);

            _currentFrame += 1;
        }
    }
}

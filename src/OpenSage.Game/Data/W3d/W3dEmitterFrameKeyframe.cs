﻿using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dEmitterFrameKeyframe
    {
        public float Time { get; private set; }

        public float Frame { get; private set; }

        internal static W3dEmitterFrameKeyframe Parse(BinaryReader reader)
        {
            return new W3dEmitterFrameKeyframe
            {
                Time = reader.ReadSingle(),
                Frame = reader.ReadSingle()
            };
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.Data.Ini;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Map
{
    public sealed class GlobalLighting : Asset
    {
        public const string AssetName = "GlobalLighting";

        private static readonly TimeOfDay[] TimeOfDayValues = Enum.GetValues(typeof(TimeOfDay)).Cast<TimeOfDay>().ToArray();

        public TimeOfDay Time { get; private set; }
        
        public Dictionary<TimeOfDay, GlobalLightingConfiguration> LightingConfigurations { get; private set; }

        public MapColorArgb ShadowColor { get; private set; }

        internal static GlobalLighting Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var time = reader.ReadUInt32AsEnum<TimeOfDay>();

                var lightingConfigurations = new Dictionary<TimeOfDay, GlobalLightingConfiguration>();

                for (var i = 0; i < TimeOfDayValues.Length; i++)
                {
                    lightingConfigurations[TimeOfDayValues[i]] = GlobalLightingConfiguration.Parse(reader);
                }

                var shadowColor = MapColorArgb.Parse(reader);

                return new GlobalLighting
                {
                    Time = time,
                    LightingConfigurations = lightingConfigurations,
                    ShadowColor = shadowColor
                };
            });
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteAssetTo(writer, () =>
            {
                writer.Write((uint) Time);

                for (var i = 0; i < TimeOfDayValues.Length; i++)
                {
                    LightingConfigurations[TimeOfDayValues[i]].WriteTo(writer);
                }

                ShadowColor.WriteTo(writer);
            });
        }
    }

    public enum TimeOfDay : uint
    {
        [IniEnum("MORNING")]
        Morning = 1,

        [IniEnum("AFTERNOON")]
        Afternoon,

        [IniEnum("EVENING")]
        Evening,

        [IniEnum("NIGHT")]
        Night
    }
}

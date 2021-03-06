﻿using System;
using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public abstract class ModuleData
    {
        internal static T ParseModule<T>(IniParser parser, Dictionary<string, Func<IniParser, T>> moduleParseTable)
            where T : ModuleData
        {
            var moduleTypePosition = parser.CurrentPosition;
            var moduleType = parser.ParseIdentifier();
            var tag = parser.ParseIdentifier();

            // ODDITY: ZH AirforceGeneral.ini:5534, missing _ between ModuleTag and SalvageData
            if (parser.CurrentTokenType == IniTokenType.Identifier)
            {
                tag += "_" + parser.ParseIdentifier();
            }

            if (!moduleParseTable.TryGetValue(moduleType, out var moduleParser))
            {
                throw new IniParseException($"Unknown module type: {moduleType}", moduleTypePosition);
            }

            var result = moduleParser(parser);

            result.Tag = tag;

            return result;
        }

        public string Tag { get; protected set; }
    }
}

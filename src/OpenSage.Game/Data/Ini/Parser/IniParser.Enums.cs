﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenSage.Data.Ini.Parser
{
    partial class IniParser
    {
        private static readonly Dictionary<Type, Dictionary<string, Enum>> CachedEnumMap = new Dictionary<Type, Dictionary<string, Enum>>();

        private static Dictionary<string, Enum> GetEnumMap<T>()
            where T : struct
        {
            var enumType = typeof(T);
            if (!CachedEnumMap.TryGetValue(enumType, out var stringToValueMap))
            {
                lock (CachedEnumMap)
                {
                    stringToValueMap = Enum.GetValues(enumType)
                        .Cast<Enum>()
                        .Distinct()
                        .SelectMany(x => GetIniNames(enumType, x).Select(y => new { Name = y, Value = x }))
                        .Where(x => x.Name != null)
                        .ToDictionary(x => x.Name, x => x.Value, StringComparer.OrdinalIgnoreCase);

                    // It might have been added by another thread.
                    if (!CachedEnumMap.ContainsKey(enumType))
                    {
                        CachedEnumMap.Add(enumType, stringToValueMap);
                    }
                }
            }
            return stringToValueMap;
        }

        public T ParseEnum<T>()
            where T : struct
        {
            return ParseEnum<T>(NextToken(IniTokenType.Identifier));
        }

        public static T ParseEnum<T>(IniToken token)
            where T : struct
        {
            var stringToValueMap = GetEnumMap<T>();

            if (stringToValueMap.TryGetValue(token.StringValue.ToUpper(), out var enumValue))
                return (T)(object)enumValue;

            throw new IniParseException($"Invalid value for type '{typeof(T).Name}': '{token.StringValue}'", token.Position);
        }

        public T ParseEnumFlags<T>()
            where T : struct
        {
            var stringToValueMap = GetEnumMap<T>();

            var result = (T) (object) 0;

            do
            {
                var token = NextToken(IniTokenType.Identifier);

                if (!stringToValueMap.TryGetValue(token.StringValue.ToUpper(), out var enumValue))
                    throw new IniParseException($"Invalid value for type '{typeof(T).Name}': '{token.StringValue}'", token.Position);

                // Ugh.
                result = (T) (object) ((int) (object) result | (int) (object) enumValue);
            } while (Current.TokenType != IniTokenType.EndOfLine);

            return result;
        }

        public BitArray<T> ParseEnumBitArray<T>()
            where T : struct
        {
            var stringToValueMap = GetEnumMap<T>();

            var result = new BitArray<T>();

            do
            {
                var token = NextToken(IniTokenType.Identifier);

                var stringValue = token.StringValue.ToUpper();
                switch (stringValue)
                {
                    case "ALL":
                        result.SetAll(true);
                        break;

                    case "NONE":
                        result.SetAll(false);
                        break;

                    default:
                        var value = true;
                        
                        if (stringValue.StartsWith("-") || stringValue.StartsWith("+"))
                        {
                            value = stringValue[0] == '+';
                            stringValue = stringValue.Substring(1);
                        }
                        if (!stringToValueMap.TryGetValue(stringValue, out var enumValue))
                        {
                            throw new IniParseException($"Invalid value for type '{typeof(T).Name}': '{stringValue}'", token.Position);
                        }

                        // Ugh.
                        result.Set((T)(object)enumValue, true);

                        break;
                }
            } while (Current.TokenType != IniTokenType.EndOfLine);

            return result;
        }

        private static string[] GetIniNames(Type enumType, Enum value)
        {
            var field = enumType.GetTypeInfo().GetDeclaredField(value.ToString());
            var iniEnumAttribute = field.GetCustomAttribute<IniEnumAttribute>();
            return iniEnumAttribute?.Names ?? new string[0];
        }
    }
}

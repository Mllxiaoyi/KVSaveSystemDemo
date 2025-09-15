using KVSaveSystem;

namespace ZeroFormatter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::ZeroFormatter.Formatters;
    using global::ZeroFormatter.Internal;
    using global::ZeroFormatter.Segments;
    using global::ZeroFormatter.Comparers;

    public static partial class ZeroFormatterInitializer
    {
        public static void HandRegisterSpecial()
        {
            var kvPairFormatter = new KVPairFormatter<DefaultResolver>();
            ZeroFormatter.Formatters.Formatter<DefaultResolver, KVPair>.Register(kvPairFormatter);
            Formatter.RegisterList<DefaultResolver, KVPair>();
            
            Formatter.RegisterDictionary<DefaultResolver, string, ISaveDataObj>();
            Formatter.RegisterList<DefaultResolver, ISaveDataObj>();
            
            // ZeroFormatter.Formatters.Formatter.AppendDynamicUnionResolver((unionType, resolver) =>
            // {
            //     if (unionType == typeof(ISaveDataObj))
            //     {
            //         resolver.RegisterUnionKeyType(typeof(byte));
            //         resolver.RegisterSubType(key: (byte)1, subType: typeof(FloatKvSaveDataObj));
            //         resolver.RegisterSubType(key: (byte)2, subType: typeof(StringKvSaveDataObj));
            //         resolver.RegisterSubType(key: (byte)3, subType: typeof(IntKvSaveDataObj));
            //         resolver.RegisterFallbackType(typeof(StringKvSaveDataObj));
            //     }
            // });
        }
    }
}

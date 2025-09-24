#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168
namespace ZeroFormatter
{
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using global::ZeroFormatter.Formatters;
    using global::ZeroFormatter.Internal;
    using global::ZeroFormatter.Segments;
    using global::ZeroFormatter.Comparers;

    public static partial class ZeroFormatterInitializer
    {
        static bool registered = false;

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Register()
        {
            if(registered) return;
            registered = true;
            // Enums
            ZeroFormatter.Formatters.Formatter<ZeroFormatter.Formatters.DefaultResolver, global::Reporter._LogType>.Register(new ZeroFormatter.DynamicObjectSegments.Reporter__LogTypeFormatter<ZeroFormatter.Formatters.DefaultResolver>());
            ZeroFormatter.Comparers.ZeroFormatterEqualityComparer<global::Reporter._LogType>.Register(new ZeroFormatter.DynamicObjectSegments.Reporter__LogTypeEqualityComparer());
            ZeroFormatter.Formatters.Formatter<ZeroFormatter.Formatters.DefaultResolver, global::Reporter._LogType?>.Register(new ZeroFormatter.DynamicObjectSegments.NullableReporter__LogTypeFormatter<ZeroFormatter.Formatters.DefaultResolver>());
            ZeroFormatter.Comparers.ZeroFormatterEqualityComparer<global::Reporter._LogType?>.Register(new NullableEqualityComparer<global::Reporter._LogType>());
            
            ZeroFormatter.Formatters.Formatter<ZeroFormatter.Formatters.DefaultResolver, global::KVSaveSystem.ArchiveOperationType>.Register(new ZeroFormatter.DynamicObjectSegments.KVSaveSystem.ArchiveOperationTypeFormatter<ZeroFormatter.Formatters.DefaultResolver>());
            ZeroFormatter.Comparers.ZeroFormatterEqualityComparer<global::KVSaveSystem.ArchiveOperationType>.Register(new ZeroFormatter.DynamicObjectSegments.KVSaveSystem.ArchiveOperationTypeEqualityComparer());
            ZeroFormatter.Formatters.Formatter<ZeroFormatter.Formatters.DefaultResolver, global::KVSaveSystem.ArchiveOperationType?>.Register(new ZeroFormatter.DynamicObjectSegments.KVSaveSystem.NullableArchiveOperationTypeFormatter<ZeroFormatter.Formatters.DefaultResolver>());
            ZeroFormatter.Comparers.ZeroFormatterEqualityComparer<global::KVSaveSystem.ArchiveOperationType?>.Register(new NullableEqualityComparer<global::KVSaveSystem.ArchiveOperationType>());
            
            // Objects
            ZeroFormatter.Formatters.Formatter<ZeroFormatter.Formatters.DefaultResolver, global::KVPair>.Register(new ZeroFormatter.DynamicObjectSegments.KVPairFormatter<ZeroFormatter.Formatters.DefaultResolver>());
            ZeroFormatter.Formatters.Formatter<ZeroFormatter.Formatters.DefaultResolver, global::KVSaveSystem.FloatKvSaveDataObj>.Register(new ZeroFormatter.DynamicObjectSegments.KVSaveSystem.FloatKvSaveDataObjFormatter<ZeroFormatter.Formatters.DefaultResolver>());
            ZeroFormatter.Formatters.Formatter<ZeroFormatter.Formatters.DefaultResolver, global::KVSaveSystem.StringKvSaveDataObj>.Register(new ZeroFormatter.DynamicObjectSegments.KVSaveSystem.StringKvSaveDataObjFormatter<ZeroFormatter.Formatters.DefaultResolver>());
            ZeroFormatter.Formatters.Formatter<ZeroFormatter.Formatters.DefaultResolver, global::KVSaveSystem.IntKvSaveDataObj>.Register(new ZeroFormatter.DynamicObjectSegments.KVSaveSystem.IntKvSaveDataObjFormatter<ZeroFormatter.Formatters.DefaultResolver>());
            // Structs
            // Unions
            {
                var unionFormatter = new ZeroFormatter.DynamicObjectSegments.KVSaveSystem.ISaveDataObjFormatter<ZeroFormatter.Formatters.DefaultResolver>();
                ZeroFormatter.Formatters.Formatter<ZeroFormatter.Formatters.DefaultResolver, global::KVSaveSystem.ISaveDataObj>.Register(unionFormatter);
            }
            // Generics
        }
    }
}
#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168
namespace ZeroFormatter.DynamicObjectSegments
{
    using global::System;
    using global::ZeroFormatter.Formatters;
    using global::ZeroFormatter.Internal;
    using global::ZeroFormatter.Segments;

    public class KVPairFormatter<TTypeResolver> : Formatter<TTypeResolver, global::KVPair>
        where TTypeResolver : ITypeResolver, new()
    {
        public override int? GetLength()
        {
            return null;
        }

        public override int Serialize(ref byte[] bytes, int offset, global::KVPair value)
        {
            var segment = value as IZeroFormatterSegment;
            if (segment != null)
            {
                return segment.Serialize(ref bytes, offset);
            }
            else if (value == null)
            {
                BinaryUtil.WriteInt32(ref bytes, offset, -1);
                return 4;
            }
            else
            {
                var startOffset = offset;

                offset += (8 + 4 * (1 + 1));
                offset += ObjectSegmentHelper.SerializeFromFormatter<TTypeResolver, string>(ref bytes, startOffset, offset, 0, value.Key);
                offset += ObjectSegmentHelper.SerializeFromFormatter<TTypeResolver, string>(ref bytes, startOffset, offset, 1, value.Value);

                return ObjectSegmentHelper.WriteSize(ref bytes, startOffset, offset, 1);
            }
        }

        public override global::KVPair Deserialize(ref byte[] bytes, int offset, global::ZeroFormatter.DirtyTracker tracker, out int byteSize)
        {
            byteSize = BinaryUtil.ReadInt32(ref bytes, offset);
            if (byteSize == -1)
            {
                byteSize = 4;
                return null;
            }
            return new KVPairObjectSegment<TTypeResolver>(tracker, new ArraySegment<byte>(bytes, offset, byteSize));
        }
    }

    public class KVPairObjectSegment<TTypeResolver> : global::KVPair, IZeroFormatterSegment
        where TTypeResolver : ITypeResolver, new()
    {
        static readonly int[] __elementSizes = new int[]{ 0, 0 };

        readonly ArraySegment<byte> __originalBytes;
        readonly global::ZeroFormatter.DirtyTracker __tracker;
        readonly int __binaryLastIndex;
        readonly byte[] __extraFixedBytes;

        CacheSegment<TTypeResolver, string> _Key;
        CacheSegment<TTypeResolver, string> _Value;

        // 0
        public override string Key
        {
            get
            {
                return _Key.Value;
            }
            set
            {
                _Key.Value = value;
            }
        }

        // 1
        public override string Value
        {
            get
            {
                return _Value.Value;
            }
            set
            {
                _Value.Value = value;
            }
        }


        public KVPairObjectSegment(global::ZeroFormatter.DirtyTracker dirtyTracker, ArraySegment<byte> originalBytes)
        {
            var __array = originalBytes.Array;

            this.__originalBytes = originalBytes;
            this.__tracker = dirtyTracker = dirtyTracker.CreateChild();
            this.__binaryLastIndex = BinaryUtil.ReadInt32(ref __array, originalBytes.Offset + 4);

            this.__extraFixedBytes = ObjectSegmentHelper.CreateExtraFixedBytes(this.__binaryLastIndex, 1, __elementSizes);

            _Key = new CacheSegment<TTypeResolver, string>(__tracker, ObjectSegmentHelper.GetSegment(originalBytes, 0, __binaryLastIndex, __tracker));
            _Value = new CacheSegment<TTypeResolver, string>(__tracker, ObjectSegmentHelper.GetSegment(originalBytes, 1, __binaryLastIndex, __tracker));
        }

        public bool CanDirectCopy()
        {
            return !__tracker.IsDirty;
        }

        public ArraySegment<byte> GetBufferReference()
        {
            return __originalBytes;
        }

        public int Serialize(ref byte[] targetBytes, int offset)
        {
            if (__extraFixedBytes != null || __tracker.IsDirty)
            {
                var startOffset = offset;
                offset += (8 + 4 * (1 + 1));

                offset += ObjectSegmentHelper.SerializeCacheSegment<TTypeResolver, string>(ref targetBytes, startOffset, offset, 0, ref _Key);
                offset += ObjectSegmentHelper.SerializeCacheSegment<TTypeResolver, string>(ref targetBytes, startOffset, offset, 1, ref _Value);

                return ObjectSegmentHelper.WriteSize(ref targetBytes, startOffset, offset, 1);
            }
            else
            {
                return ObjectSegmentHelper.DirectCopyAll(__originalBytes, ref targetBytes, offset);
            }
        }
    }


}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168
namespace ZeroFormatter.DynamicObjectSegments.KVSaveSystem
{
    using global::System;
    using global::ZeroFormatter.Formatters;
    using global::ZeroFormatter.Internal;
    using global::ZeroFormatter.Segments;

    public class FloatKvSaveDataObjFormatter<TTypeResolver> : Formatter<TTypeResolver, global::KVSaveSystem.FloatKvSaveDataObj>
        where TTypeResolver : ITypeResolver, new()
    {
        public override int? GetLength()
        {
            return null;
        }

        public override int Serialize(ref byte[] bytes, int offset, global::KVSaveSystem.FloatKvSaveDataObj value)
        {
            var segment = value as IZeroFormatterSegment;
            if (segment != null)
            {
                return segment.Serialize(ref bytes, offset);
            }
            else if (value == null)
            {
                BinaryUtil.WriteInt32(ref bytes, offset, -1);
                return 4;
            }
            else
            {
                var startOffset = offset;

                offset += (8 + 4 * (1 + 1));
                offset += ObjectSegmentHelper.SerializeFromFormatter<TTypeResolver, string>(ref bytes, startOffset, offset, 0, value.Key);
                offset += ObjectSegmentHelper.SerializeFromFormatter<TTypeResolver, float>(ref bytes, startOffset, offset, 1, value.Value);

                return ObjectSegmentHelper.WriteSize(ref bytes, startOffset, offset, 1);
            }
        }

        public override global::KVSaveSystem.FloatKvSaveDataObj Deserialize(ref byte[] bytes, int offset, global::ZeroFormatter.DirtyTracker tracker, out int byteSize)
        {
            byteSize = BinaryUtil.ReadInt32(ref bytes, offset);
            if (byteSize == -1)
            {
                byteSize = 4;
                return null;
            }
            return new FloatKvSaveDataObjObjectSegment<TTypeResolver>(tracker, new ArraySegment<byte>(bytes, offset, byteSize));
        }
    }

    public class FloatKvSaveDataObjObjectSegment<TTypeResolver> : global::KVSaveSystem.FloatKvSaveDataObj, IZeroFormatterSegment
        where TTypeResolver : ITypeResolver, new()
    {
        static readonly int[] __elementSizes = new int[]{ 0, 4 };

        readonly ArraySegment<byte> __originalBytes;
        readonly global::ZeroFormatter.DirtyTracker __tracker;
        readonly int __binaryLastIndex;
        readonly byte[] __extraFixedBytes;

        CacheSegment<TTypeResolver, string> _Key;

        // 0
        public override string Key
        {
            get
            {
                return _Key.Value;
            }
            set
            {
                _Key.Value = value;
            }
        }

        // 1
        public override float Value
        {
            get
            {
                return ObjectSegmentHelper.GetFixedProperty<TTypeResolver, float>(__originalBytes, 1, __binaryLastIndex, __extraFixedBytes, __tracker);
            }
            set
            {
                ObjectSegmentHelper.SetFixedProperty<TTypeResolver, float>(__originalBytes, 1, __binaryLastIndex, __extraFixedBytes, value, __tracker);
            }
        }


        public FloatKvSaveDataObjObjectSegment(global::ZeroFormatter.DirtyTracker dirtyTracker, ArraySegment<byte> originalBytes)
        {
            var __array = originalBytes.Array;

            this.__originalBytes = originalBytes;
            this.__tracker = dirtyTracker = dirtyTracker.CreateChild();
            this.__binaryLastIndex = BinaryUtil.ReadInt32(ref __array, originalBytes.Offset + 4);

            this.__extraFixedBytes = ObjectSegmentHelper.CreateExtraFixedBytes(this.__binaryLastIndex, 1, __elementSizes);

            _Key = new CacheSegment<TTypeResolver, string>(__tracker, ObjectSegmentHelper.GetSegment(originalBytes, 0, __binaryLastIndex, __tracker));
        }

        public bool CanDirectCopy()
        {
            return !__tracker.IsDirty;
        }

        public ArraySegment<byte> GetBufferReference()
        {
            return __originalBytes;
        }

        public int Serialize(ref byte[] targetBytes, int offset)
        {
            if (__extraFixedBytes != null || __tracker.IsDirty)
            {
                var startOffset = offset;
                offset += (8 + 4 * (1 + 1));

                offset += ObjectSegmentHelper.SerializeCacheSegment<TTypeResolver, string>(ref targetBytes, startOffset, offset, 0, ref _Key);
                offset += ObjectSegmentHelper.SerializeFixedLength<TTypeResolver, float>(ref targetBytes, startOffset, offset, 1, __binaryLastIndex, __originalBytes, __extraFixedBytes, __tracker);

                return ObjectSegmentHelper.WriteSize(ref targetBytes, startOffset, offset, 1);
            }
            else
            {
                return ObjectSegmentHelper.DirectCopyAll(__originalBytes, ref targetBytes, offset);
            }
        }
    }

    public class StringKvSaveDataObjFormatter<TTypeResolver> : Formatter<TTypeResolver, global::KVSaveSystem.StringKvSaveDataObj>
        where TTypeResolver : ITypeResolver, new()
    {
        public override int? GetLength()
        {
            return null;
        }

        public override int Serialize(ref byte[] bytes, int offset, global::KVSaveSystem.StringKvSaveDataObj value)
        {
            var segment = value as IZeroFormatterSegment;
            if (segment != null)
            {
                return segment.Serialize(ref bytes, offset);
            }
            else if (value == null)
            {
                BinaryUtil.WriteInt32(ref bytes, offset, -1);
                return 4;
            }
            else
            {
                var startOffset = offset;

                offset += (8 + 4 * (1 + 1));
                offset += ObjectSegmentHelper.SerializeFromFormatter<TTypeResolver, string>(ref bytes, startOffset, offset, 0, value.Key);
                offset += ObjectSegmentHelper.SerializeFromFormatter<TTypeResolver, string>(ref bytes, startOffset, offset, 1, value.Value);

                return ObjectSegmentHelper.WriteSize(ref bytes, startOffset, offset, 1);
            }
        }

        public override global::KVSaveSystem.StringKvSaveDataObj Deserialize(ref byte[] bytes, int offset, global::ZeroFormatter.DirtyTracker tracker, out int byteSize)
        {
            byteSize = BinaryUtil.ReadInt32(ref bytes, offset);
            if (byteSize == -1)
            {
                byteSize = 4;
                return null;
            }
            return new StringKvSaveDataObjObjectSegment<TTypeResolver>(tracker, new ArraySegment<byte>(bytes, offset, byteSize));
        }
    }

    public class StringKvSaveDataObjObjectSegment<TTypeResolver> : global::KVSaveSystem.StringKvSaveDataObj, IZeroFormatterSegment
        where TTypeResolver : ITypeResolver, new()
    {
        static readonly int[] __elementSizes = new int[]{ 0, 0 };

        readonly ArraySegment<byte> __originalBytes;
        readonly global::ZeroFormatter.DirtyTracker __tracker;
        readonly int __binaryLastIndex;
        readonly byte[] __extraFixedBytes;

        CacheSegment<TTypeResolver, string> _Key;
        CacheSegment<TTypeResolver, string> _Value;

        // 0
        public override string Key
        {
            get
            {
                return _Key.Value;
            }
            set
            {
                _Key.Value = value;
            }
        }

        // 1
        public override string Value
        {
            get
            {
                return _Value.Value;
            }
            set
            {
                _Value.Value = value;
            }
        }


        public StringKvSaveDataObjObjectSegment(global::ZeroFormatter.DirtyTracker dirtyTracker, ArraySegment<byte> originalBytes)
        {
            var __array = originalBytes.Array;

            this.__originalBytes = originalBytes;
            this.__tracker = dirtyTracker = dirtyTracker.CreateChild();
            this.__binaryLastIndex = BinaryUtil.ReadInt32(ref __array, originalBytes.Offset + 4);

            this.__extraFixedBytes = ObjectSegmentHelper.CreateExtraFixedBytes(this.__binaryLastIndex, 1, __elementSizes);

            _Key = new CacheSegment<TTypeResolver, string>(__tracker, ObjectSegmentHelper.GetSegment(originalBytes, 0, __binaryLastIndex, __tracker));
            _Value = new CacheSegment<TTypeResolver, string>(__tracker, ObjectSegmentHelper.GetSegment(originalBytes, 1, __binaryLastIndex, __tracker));
        }

        public bool CanDirectCopy()
        {
            return !__tracker.IsDirty;
        }

        public ArraySegment<byte> GetBufferReference()
        {
            return __originalBytes;
        }

        public int Serialize(ref byte[] targetBytes, int offset)
        {
            if (__extraFixedBytes != null || __tracker.IsDirty)
            {
                var startOffset = offset;
                offset += (8 + 4 * (1 + 1));

                offset += ObjectSegmentHelper.SerializeCacheSegment<TTypeResolver, string>(ref targetBytes, startOffset, offset, 0, ref _Key);
                offset += ObjectSegmentHelper.SerializeCacheSegment<TTypeResolver, string>(ref targetBytes, startOffset, offset, 1, ref _Value);

                return ObjectSegmentHelper.WriteSize(ref targetBytes, startOffset, offset, 1);
            }
            else
            {
                return ObjectSegmentHelper.DirectCopyAll(__originalBytes, ref targetBytes, offset);
            }
        }
    }

    public class IntKvSaveDataObjFormatter<TTypeResolver> : Formatter<TTypeResolver, global::KVSaveSystem.IntKvSaveDataObj>
        where TTypeResolver : ITypeResolver, new()
    {
        public override int? GetLength()
        {
            return null;
        }

        public override int Serialize(ref byte[] bytes, int offset, global::KVSaveSystem.IntKvSaveDataObj value)
        {
            var segment = value as IZeroFormatterSegment;
            if (segment != null)
            {
                return segment.Serialize(ref bytes, offset);
            }
            else if (value == null)
            {
                BinaryUtil.WriteInt32(ref bytes, offset, -1);
                return 4;
            }
            else
            {
                var startOffset = offset;

                offset += (8 + 4 * (1 + 1));
                offset += ObjectSegmentHelper.SerializeFromFormatter<TTypeResolver, string>(ref bytes, startOffset, offset, 0, value.Key);
                offset += ObjectSegmentHelper.SerializeFromFormatter<TTypeResolver, int>(ref bytes, startOffset, offset, 1, value.Value);

                return ObjectSegmentHelper.WriteSize(ref bytes, startOffset, offset, 1);
            }
        }

        public override global::KVSaveSystem.IntKvSaveDataObj Deserialize(ref byte[] bytes, int offset, global::ZeroFormatter.DirtyTracker tracker, out int byteSize)
        {
            byteSize = BinaryUtil.ReadInt32(ref bytes, offset);
            if (byteSize == -1)
            {
                byteSize = 4;
                return null;
            }
            return new IntKvSaveDataObjObjectSegment<TTypeResolver>(tracker, new ArraySegment<byte>(bytes, offset, byteSize));
        }
    }

    public class IntKvSaveDataObjObjectSegment<TTypeResolver> : global::KVSaveSystem.IntKvSaveDataObj, IZeroFormatterSegment
        where TTypeResolver : ITypeResolver, new()
    {
        static readonly int[] __elementSizes = new int[]{ 0, 4 };

        readonly ArraySegment<byte> __originalBytes;
        readonly global::ZeroFormatter.DirtyTracker __tracker;
        readonly int __binaryLastIndex;
        readonly byte[] __extraFixedBytes;

        CacheSegment<TTypeResolver, string> _Key;

        // 0
        public override string Key
        {
            get
            {
                return _Key.Value;
            }
            set
            {
                _Key.Value = value;
            }
        }

        // 1
        public override int Value
        {
            get
            {
                return ObjectSegmentHelper.GetFixedProperty<TTypeResolver, int>(__originalBytes, 1, __binaryLastIndex, __extraFixedBytes, __tracker);
            }
            set
            {
                ObjectSegmentHelper.SetFixedProperty<TTypeResolver, int>(__originalBytes, 1, __binaryLastIndex, __extraFixedBytes, value, __tracker);
            }
        }


        public IntKvSaveDataObjObjectSegment(global::ZeroFormatter.DirtyTracker dirtyTracker, ArraySegment<byte> originalBytes)
        {
            var __array = originalBytes.Array;

            this.__originalBytes = originalBytes;
            this.__tracker = dirtyTracker = dirtyTracker.CreateChild();
            this.__binaryLastIndex = BinaryUtil.ReadInt32(ref __array, originalBytes.Offset + 4);

            this.__extraFixedBytes = ObjectSegmentHelper.CreateExtraFixedBytes(this.__binaryLastIndex, 1, __elementSizes);

            _Key = new CacheSegment<TTypeResolver, string>(__tracker, ObjectSegmentHelper.GetSegment(originalBytes, 0, __binaryLastIndex, __tracker));
        }

        public bool CanDirectCopy()
        {
            return !__tracker.IsDirty;
        }

        public ArraySegment<byte> GetBufferReference()
        {
            return __originalBytes;
        }

        public int Serialize(ref byte[] targetBytes, int offset)
        {
            if (__extraFixedBytes != null || __tracker.IsDirty)
            {
                var startOffset = offset;
                offset += (8 + 4 * (1 + 1));

                offset += ObjectSegmentHelper.SerializeCacheSegment<TTypeResolver, string>(ref targetBytes, startOffset, offset, 0, ref _Key);
                offset += ObjectSegmentHelper.SerializeFixedLength<TTypeResolver, int>(ref targetBytes, startOffset, offset, 1, __binaryLastIndex, __originalBytes, __extraFixedBytes, __tracker);

                return ObjectSegmentHelper.WriteSize(ref targetBytes, startOffset, offset, 1);
            }
            else
            {
                return ObjectSegmentHelper.DirectCopyAll(__originalBytes, ref targetBytes, offset);
            }
        }
    }


}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168
namespace ZeroFormatter.DynamicObjectSegments.KVSaveSystem
{
    using global::System;
    using global::ZeroFormatter.Formatters;
    using global::ZeroFormatter.Internal;
    using global::ZeroFormatter.Segments;

    public class ISaveDataObjFormatter<TTypeResolver> : Formatter<TTypeResolver, global::KVSaveSystem.ISaveDataObj>
        where TTypeResolver : ITypeResolver, new()
    {
        readonly global::System.Collections.Generic.IEqualityComparer<byte> comparer;
        readonly byte[] unionKeys;
        
        public ISaveDataObjFormatter()
        {
            comparer = global::ZeroFormatter.Comparers.ZeroFormatterEqualityComparer<byte>.Default;
            unionKeys = new byte[4];
            unionKeys[0] = new global::KVSaveSystem.FloatKvSaveDataObj().TypeKey;
            unionKeys[1] = new global::KVSaveSystem.IntKvSaveDataObj().TypeKey;
            unionKeys[2] = new global::KVSaveSystem.StringKvSaveDataObj().TypeKey;
            unionKeys[3] = new global::KVSaveSystem.StringKvSaveDataObj().TypeKey;
            
        }

        public override int? GetLength()
        {
            return null;
        }

        public override int Serialize(ref byte[] bytes, int offset, global::KVSaveSystem.ISaveDataObj value)
        {
            if (value == null)
            {
                return BinaryUtil.WriteInt32(ref bytes, offset, -1);
            }

            var startOffset = offset;

            offset += 4;
            offset += Formatter<TTypeResolver, byte>.Default.Serialize(ref bytes, offset, value.TypeKey);

            if (value is global::KVSaveSystem.FloatKvSaveDataObj)
            {
                offset += Formatter<TTypeResolver, global::KVSaveSystem.FloatKvSaveDataObj>.Default.Serialize(ref bytes, offset, (global::KVSaveSystem.FloatKvSaveDataObj)value);
            }
            else if (value is global::KVSaveSystem.IntKvSaveDataObj)
            {
                offset += Formatter<TTypeResolver, global::KVSaveSystem.IntKvSaveDataObj>.Default.Serialize(ref bytes, offset, (global::KVSaveSystem.IntKvSaveDataObj)value);
            }
            else if (value is global::KVSaveSystem.StringKvSaveDataObj)
            {
                offset += Formatter<TTypeResolver, global::KVSaveSystem.StringKvSaveDataObj>.Default.Serialize(ref bytes, offset, (global::KVSaveSystem.StringKvSaveDataObj)value);
            }
            else if (value is global::KVSaveSystem.StringKvSaveDataObj)
            {
                offset += Formatter<TTypeResolver, global::KVSaveSystem.StringKvSaveDataObj>.Default.Serialize(ref bytes, offset, (global::KVSaveSystem.StringKvSaveDataObj)value);
            }
            
            else
            {
                throw new Exception("Unknown subtype of Union:" + value.GetType().FullName);
            }
        
            var writeSize = offset - startOffset;
            BinaryUtil.WriteInt32(ref bytes, startOffset, writeSize);
            return writeSize;
        }

        public override global::KVSaveSystem.ISaveDataObj Deserialize(ref byte[] bytes, int offset, global::ZeroFormatter.DirtyTracker tracker, out int byteSize)
        {
            if ((byteSize = BinaryUtil.ReadInt32(ref bytes, offset)) == -1)
            {
                byteSize = 4;
                return null;
            }
        
            offset += 4;
            int size;
            var unionKey = Formatter<TTypeResolver, byte>.Default.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;

            global::KVSaveSystem.ISaveDataObj result;
            if (comparer.Equals(unionKey, unionKeys[0]))
            {
                result = Formatter<TTypeResolver, global::KVSaveSystem.FloatKvSaveDataObj>.Default.Deserialize(ref bytes, offset, tracker, out size);
            }
            else if (comparer.Equals(unionKey, unionKeys[1]))
            {
                result = Formatter<TTypeResolver, global::KVSaveSystem.IntKvSaveDataObj>.Default.Deserialize(ref bytes, offset, tracker, out size);
            }
            else if (comparer.Equals(unionKey, unionKeys[2]))
            {
                result = Formatter<TTypeResolver, global::KVSaveSystem.StringKvSaveDataObj>.Default.Deserialize(ref bytes, offset, tracker, out size);
            }
            else if (comparer.Equals(unionKey, unionKeys[3]))
            {
                result = Formatter<TTypeResolver, global::KVSaveSystem.StringKvSaveDataObj>.Default.Deserialize(ref bytes, offset, tracker, out size);
            }
            else
            {
                result = new global::KVSaveSystem.StringKvSaveDataObj();
            }

            return result;
        }
    }


}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168
namespace ZeroFormatter.DynamicObjectSegments
{
    using global::System;
    using global::System.Collections.Generic;
    using global::ZeroFormatter.Formatters;
    using global::ZeroFormatter.Internal;
    using global::ZeroFormatter.Segments;


    public class Reporter__LogTypeFormatter<TTypeResolver> : Formatter<TTypeResolver, global::Reporter._LogType>
        where TTypeResolver : ITypeResolver, new()
    {
        public override int? GetLength()
        {
            return 4;
        }

        public override int Serialize(ref byte[] bytes, int offset, global::Reporter._LogType value)
        {
            return BinaryUtil.WriteInt32(ref bytes, offset, (Int32)value);
        }

        public override global::Reporter._LogType Deserialize(ref byte[] bytes, int offset, global::ZeroFormatter.DirtyTracker tracker, out int byteSize)
        {
            byteSize = 4;
            return (global::Reporter._LogType)BinaryUtil.ReadInt32(ref bytes, offset);
        }
    }


    public class NullableReporter__LogTypeFormatter<TTypeResolver> : Formatter<TTypeResolver, global::Reporter._LogType?>
        where TTypeResolver : ITypeResolver, new()
    {
        public override int? GetLength()
        {
            return 5;
        }

        public override int Serialize(ref byte[] bytes, int offset, global::Reporter._LogType? value)
        {
            BinaryUtil.WriteBoolean(ref bytes, offset, value.HasValue);
            if (value.HasValue)
            {
                BinaryUtil.WriteInt32(ref bytes, offset + 1, (Int32)value.Value);
            }
            else
            {
                BinaryUtil.EnsureCapacity(ref bytes, offset, offset + 5);
            }

            return 5;
        }

        public override global::Reporter._LogType? Deserialize(ref byte[] bytes, int offset, global::ZeroFormatter.DirtyTracker tracker, out int byteSize)
        {
            byteSize = 5;
            var hasValue = BinaryUtil.ReadBoolean(ref bytes, offset);
            if (!hasValue) return null;

            return (global::Reporter._LogType)BinaryUtil.ReadInt32(ref bytes, offset + 1);
        }
    }



    public class Reporter__LogTypeEqualityComparer : IEqualityComparer<global::Reporter._LogType>
    {
        public bool Equals(global::Reporter._LogType x, global::Reporter._LogType y)
        {
            return (Int32)x == (Int32)y;
        }

        public int GetHashCode(global::Reporter._LogType x)
        {
            return (int)x;
        }
    }



}
#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168
namespace ZeroFormatter.DynamicObjectSegments.KVSaveSystem
{
    using global::System;
    using global::System.Collections.Generic;
    using global::ZeroFormatter.Formatters;
    using global::ZeroFormatter.Internal;
    using global::ZeroFormatter.Segments;


    public class ArchiveOperationTypeFormatter<TTypeResolver> : Formatter<TTypeResolver, global::KVSaveSystem.ArchiveOperationType>
        where TTypeResolver : ITypeResolver, new()
    {
        public override int? GetLength()
        {
            return 4;
        }

        public override int Serialize(ref byte[] bytes, int offset, global::KVSaveSystem.ArchiveOperationType value)
        {
            return BinaryUtil.WriteInt32(ref bytes, offset, (Int32)value);
        }

        public override global::KVSaveSystem.ArchiveOperationType Deserialize(ref byte[] bytes, int offset, global::ZeroFormatter.DirtyTracker tracker, out int byteSize)
        {
            byteSize = 4;
            return (global::KVSaveSystem.ArchiveOperationType)BinaryUtil.ReadInt32(ref bytes, offset);
        }
    }


    public class NullableArchiveOperationTypeFormatter<TTypeResolver> : Formatter<TTypeResolver, global::KVSaveSystem.ArchiveOperationType?>
        where TTypeResolver : ITypeResolver, new()
    {
        public override int? GetLength()
        {
            return 5;
        }

        public override int Serialize(ref byte[] bytes, int offset, global::KVSaveSystem.ArchiveOperationType? value)
        {
            BinaryUtil.WriteBoolean(ref bytes, offset, value.HasValue);
            if (value.HasValue)
            {
                BinaryUtil.WriteInt32(ref bytes, offset + 1, (Int32)value.Value);
            }
            else
            {
                BinaryUtil.EnsureCapacity(ref bytes, offset, offset + 5);
            }

            return 5;
        }

        public override global::KVSaveSystem.ArchiveOperationType? Deserialize(ref byte[] bytes, int offset, global::ZeroFormatter.DirtyTracker tracker, out int byteSize)
        {
            byteSize = 5;
            var hasValue = BinaryUtil.ReadBoolean(ref bytes, offset);
            if (!hasValue) return null;

            return (global::KVSaveSystem.ArchiveOperationType)BinaryUtil.ReadInt32(ref bytes, offset + 1);
        }
    }



    public class ArchiveOperationTypeEqualityComparer : IEqualityComparer<global::KVSaveSystem.ArchiveOperationType>
    {
        public bool Equals(global::KVSaveSystem.ArchiveOperationType x, global::KVSaveSystem.ArchiveOperationType y)
        {
            return (Int32)x == (Int32)y;
        }

        public int GetHashCode(global::KVSaveSystem.ArchiveOperationType x)
        {
            return (int)x;
        }
    }



}
#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

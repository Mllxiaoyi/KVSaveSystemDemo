using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using Type = System.Type;

namespace KVSaveSystem
{
    public class KvSaveDataFactory
    {
        public static ISaveDataObj GetSaveDataObj<T>(T t)
        {
            switch (t)
            {
                case int intValue:
                    return new IntKvSaveDataObj { Value = intValue };
        
                case float floatValue:
                    return new FloatKvSaveDataObj { Value = floatValue };
                
                case string stringValue:
                    return new StringKvSaveDataObj { Value = stringValue };
        
                default:
                    throw new NotSupportedException($"Type {typeof(T).Name} is not supported");
            }
        }
    }
}
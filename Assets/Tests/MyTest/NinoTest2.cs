using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using KVSaveSystem;
using Nino.Core;
using UnityEngine;

public class NinoTest2 : MonoBehaviour
{
    private void Start()
    {
        Dictionary<string, ISaveDataObj> dic = new Dictionary<string, ISaveDataObj>();
        dic.Add("111", new KvSaveDataObj<int>(){Value = 1});
        dic.Add("113", new KvSaveDataObj<float>(){Value = 1.1f});
        dic.Add("112", new KvSaveDataObj<string>(){Value = "11"});
        
        
        var bytes = NinoSerializer.Serialize(dic);
        dic = NinoDeserializer.Deserialize<Dictionary<string, ISaveDataObj>>(bytes);
        foreach (var kv in dic)
        {
            Debug.Log($"key: {kv.Key}, value: {kv.Value}");
        }

        Task.Run(() =>
        {
            KvSaveSystem.SetInt("111", 1);
            KvSaveSystem.SetFloat("113", 1.1f);
            KvSaveSystem.SetString("112", "11");

            bytes = NinoSerializer.Serialize(KvSaveSystem.GetGroup("Default").DataDic);
            dic = NinoDeserializer.Deserialize<Dictionary<string, ISaveDataObj>>(bytes);
            foreach (var kv in dic)
            {
                Debug.Log($"key: {kv.Key}, value: {kv.Value}");
            }
        });
    }
}

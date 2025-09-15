using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using KVSaveSystem;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class WriteLineAndStringBuilderSpeedTest : MonoBehaviour
{
    public Text text;
    
    [Button("测试字符串拼接方式")]
    void TestStringConcatenation(int lines)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        var sb = new StringBuilder();
        for (int i = 0; i < lines; i++)
        {
            sb.Append("Line ").Append(i).Append(Environment.NewLine);
        }

        string res = sb.ToString();
        File.WriteAllText("concat.txt", res);
        stopwatch.Stop();
        Debug.Log($"字符串拼接方式写入 {lines} 行耗时: {stopwatch.ElapsedMilliseconds} ms");
    }

    [Button("测试WriteLine方式")]
    void TestWriteLine(int lines)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        using var writer = new StreamWriter("writeline.txt");
        for (int i = 0; i < lines; i++)
        {
            writer.WriteLine($"Line {i}");
        }

        stopwatch.Stop();
        Debug.Log($"WriteLine方式写入 {lines} 行耗时: {stopwatch.ElapsedMilliseconds} ms");
    }

    [Button("测试换行兼容")]
    void TestEnter(int lines)
    {
        using (StreamWriter writer = new StreamWriter("writeEnter.txt"))
        {
            writer.Write(@"Line1\r\nLine2\r\n");
            writer.WriteLine("Line3");
            writer.WriteLine("Line4");
        }


        using (StreamReader reader = new StreamReader("writeEnter.txt"))
        {
            while (!reader.EndOfStream)
            {
                string content = reader.ReadLine();
                Debug.Log($"文件内容:\n{content}");
            }
        }
    }
    
    [Button("测试文本")]
    void Test()
    {
        if (text == null)
            return;
        
        if (text.text.Contains("\t"))
            Debug.LogError("包含\t");
    }
    
    [Button("测试Json方式")]
    void TestJsonConcatenation()
    {
        Dictionary<string, string> dic = new Dictionary<string, string>();
        dic.Add("ok", "ok");
        dic.Add("tt", "{\"name\": \"Alice\", \"level\": 5}");
        dic.Add("mm", "1");
        Debug.LogError("{\"name\": \"Alice\", \"level\": 5}");
        if ("{\"name\": \"Alice\", \"level\": 5}".Contains("\\\""))
            Debug.LogError("Contains");
        string json = JsonConvert.SerializeObject(dic);
        Debug.LogError(json);
        if (json.Contains("\\\""))
            Debug.LogError("Contains");
    }
    
    [Button("测试写入")]
    void TestWriteInConcatenation()
    {
        KvSaveSystem.SetValue("key1", 1);
        KvSaveSystem.SetValue("key2", 1.1f);
        KvSaveSystem.SetValue("key3", "111");
        KvSaveSystem.Save(true);
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class MemoryMappedFileSpeedTest : MonoBehaviour
{
    public int TestCount => TestParamSingleton.TestCount;
    public int RepeatTimes => TestParamSingleton.RepeatTimes;

    private Dictionary<string, string> _dict = new Dictionary<string, string>();

    [Button("测试 Dictionary Write")]
    private void DoTestDictionaryWrite()
    {
        var datas = UtilsForTest.GenerateTestKvPairListData(TestCount);
        var dataCnt = datas.Count;
        _dict.Clear();

        Stopwatch stopwatch = Stopwatch.StartNew();
        for (int r = 0; r < RepeatTimes; r++)
        {
            for (int i = 0; i < dataCnt; i++)
            {
                var data = datas[i];
                _dict[data.Key] = data.Value;
            }
        }

        stopwatch.Stop();
        Debug.Log($"Dictionary Write: {stopwatch.ElapsedMilliseconds} ms");
    }

    [Button("测试 Dictionary Read")]
    private void DoTestDictionaryRead()
    {
        var datas = UtilsForTest.GenerateTestKvPairListData(TestCount);
        var dataCnt = datas.Count;
        Dictionary<string, string> dict = new Dictionary<string, string>(dataCnt);

        Stopwatch stopwatch = Stopwatch.StartNew();
        for (int r = 0; r < RepeatTimes; r++)
        {
            foreach (var kvp in dict)
            {
                // read
                string key = kvp.Key;
                string value = kvp.Value;
            }
        }

        stopwatch.Stop();
        Debug.Log($"Dictionary Read: {stopwatch.ElapsedMilliseconds} ms");
    }


    [Button("测试 MemoryStream Write")]
    private void DoTestMemoryStreamWrite()
    {
        var datas = UtilsForTest.GenerateTestKvPairListData(TestCount);
        var dataCnt = datas.Count;
        List<string> stringList = new List<string>(datas.Count);

        for (int i = 0; i < dataCnt; i++)
        {
            string str = datas[i].ToString();
            stringList.Add(str);
        }

        Stopwatch stopwatch = Stopwatch.StartNew();
        for (int r = 0; r < RepeatTimes; r++)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
            {
                for (int i = 0; i < dataCnt; i++)
                {
                    streamWriter.WriteLine(stringList[i]);
                }
            }
        }

        stopwatch.Stop();
        Debug.Log($"MemoryStream Write: {stopwatch.ElapsedMilliseconds} ms");
    }

    [Button("测试 MemoryMappedFile Write (ViewAccessor)")]
    private void DoTestMemoryMappedFileWrite()
    {
        var datas = UtilsForTest.GenerateTestKvPairListData(TestCount);

        string filePath = Path.Combine(Application.persistentDataPath, "memoryMappedFileSave.txt");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        File.Create(filePath).Dispose();

        long fileSize = 0;
        int dataCnt = datas.Count;
        int intSize = sizeof(int);
        List<byte[]> stringBytesList = new List<byte[]>(datas.Count);

        for (int i = 0; i < dataCnt; i++)
        {
            string str = datas[i].ToString();
            byte[] stringBytes = Encoding.UTF8.GetBytes(str);
            int stringLength = stringBytes.Length;

            // 计算文件大小
            fileSize += intSize; // 字符串长度
            fileSize += stringLength; // 字符串内容

            stringBytesList.Add(stringBytes);
        }

        Stopwatch stopwatch = Stopwatch.StartNew();

        for (int r = 0; r < RepeatTimes; r++)
        {
            using (var mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, fileSize))
            using (var accessor = mmf.CreateViewAccessor(0, fileSize))
            {
                long position = 0;

                for (int i = 0; i < dataCnt; i++)
                {
                    byte[] stringBytes = stringBytesList[i];
                    int stringLength = stringBytes.Length;

                    // Write the length of the string
                    accessor.Write(position, stringLength);
                    position += intSize;

                    // Write the string content
                    accessor.WriteArray(position, stringBytes, 0, stringLength);
                    position += stringLength;
                }
            }
        }

        stopwatch.Stop();
        Debug.Log($"MemoryMappedFile Write: {stopwatch.ElapsedMilliseconds} ms");
    }

    [Button("测试 MemoryMappedFile Read (ViewAccessor)")]
    private void DoTestMemoryMappedFileRead()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "memoryMappedFileSave.txt");

        if (!File.Exists(filePath))
        {
            Debug.LogError("File does not exist.");
            return;
        }

        long fileSize = new FileInfo(filePath).Length;
        if (fileSize == 0)
        {
            Debug.LogError("File is empty.");
            return;
        }

        List<KVPair> datas = new List<KVPair>(100000);
        Stopwatch stopwatch = Stopwatch.StartNew();

        using (var mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null))
        {
            using (var accessor = mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read))
            {
                long position = 0;

                while (position < fileSize)
                {
                    if (position + sizeof(int) > fileSize)
                        break;

                    // 读取字符串长度
                    int stringLength = accessor.ReadInt32(position);
                    position += sizeof(int);

                    if (stringLength < 0 || position + stringLength > fileSize)
                    {
                        Debug.LogError("Invalid string length detected.");
                        break;
                    }

                    // 读取字符串内容
                    byte[] stringBytes = new byte[stringLength];
                    accessor.ReadArray(position, stringBytes, 0, stringLength);
                    position += stringLength;

                    string strValue = Encoding.UTF8.GetString(stringBytes);
                    datas.Add(JsonConvert.DeserializeObject<KVPair>(strValue));
                }
            }
        }

        stopwatch.Stop();
        Debug.Log($"MemoryMappedFile Read: {stopwatch.ElapsedMilliseconds} ms");
    }

    [Button("测试 MemoryMappedFile Write (ViewStream)")]
    private void DoTestMemoryMappedFileStreamWrite()
    {
        var datas = UtilsForTest.GenerateTestKvPairListData(TestCount);
        var dataCnt = datas.Count;
        long fileSize = 0;
        List<string> stringList = new List<string>(datas.Count);

        for (int i = 0; i < dataCnt; i++)
        {
            string str = datas[i].ToString();
            fileSize += Encoding.UTF8.GetByteCount(str) + Environment.NewLine.Length;
            stringList.Add(str);
        }

        string filePath = Path.Combine(Application.persistentDataPath, "memoryMappedFileStreamSave.txt");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        File.Create(filePath).Dispose();

        Stopwatch stopwatch = Stopwatch.StartNew();

        for (int r = 0; r < RepeatTimes; r++)
        {
            using (var mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, fileSize))
            using (var stream = mmf.CreateViewStream())
            using (var streamWriter = new StreamWriter(stream, Encoding.UTF8))
            {
                for (int i = 0; i < dataCnt; i++)
                {
                    streamWriter.WriteLine(stringList[i]);
                }
            }
        }

        stopwatch.Stop();
        Debug.Log($"MemoryMappedFile Stream Write: {stopwatch.ElapsedMilliseconds} ms");
    }

    [Button("测试 MemoryMappedFile Read (ViewStream)")]
    private void DoTestMemoryMappedFileStreamRead()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "memoryMappedFileStreamSave.txt");
        List<KVPair> datas = new List<KVPair>(100000);

        Stopwatch stopwatch = Stopwatch.StartNew();
        using (var mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null))
        using (var accessor = mmf.CreateViewStream(0, 0, MemoryMappedFileAccess.Read))
        using (var streamReader = new StreamReader(accessor, Encoding.UTF8))
        {
            while (streamReader.Peek() > 0)
            {
                string strValue = streamReader.ReadLine();
                datas.Add(JsonConvert.DeserializeObject<KVPair>(strValue));
                //Debug.Log("Read: " + strValue);
            }
        }
        stopwatch.Stop();
        Debug.Log($"MemoryMappedFile Read: {stopwatch.ElapsedMilliseconds} ms");
    }

    [Button("测试 I/O Write")]
    private void DoTestIOWrite()
    {
        var datas = UtilsForTest.GenerateTestKvPairListData(TestCount);
        var dataCnt = datas.Count;
        List<string> stringList = new List<string>(datas.Count);

        for (int i = 0; i < dataCnt; i++)
        {
            string str = datas[i].ToString();
            stringList.Add(str);
        }

        string filePath = Path.Combine(Application.persistentDataPath, "ioSave.txt");

        Stopwatch stopwatch = Stopwatch.StartNew();
        for (int r = 0; r < RepeatTimes; r++)
        {
            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (var streamWriter = new StreamWriter(stream, Encoding.UTF8))
            {
                for (int i = 0; i < dataCnt; i++)
                {
                    streamWriter.WriteLine(stringList[i]);
                }
            }
        }
        stopwatch.Stop();
        Debug.Log($"I/O Write: {stopwatch.ElapsedMilliseconds} ms");
    }

    [Button("测试 I/O Read")]
    private void DoTestIORead()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "ioSave.txt");
        List<KVPair> datas = new List<KVPair>(100000);

        Stopwatch stopwatch = Stopwatch.StartNew();
        using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        using (var streamReader = new StreamReader(stream, Encoding.UTF8))
        {
            while (streamReader.Peek() > 0)
            {
                string strValue = streamReader.ReadLine();
                datas.Add(JsonConvert.DeserializeObject<KVPair>(strValue));
                //Debug.Log("Read: " + strValue);
            }
        }
        stopwatch.Stop();
        Debug.Log($"I/O Read: {stopwatch.ElapsedMilliseconds} ms");
    }
}
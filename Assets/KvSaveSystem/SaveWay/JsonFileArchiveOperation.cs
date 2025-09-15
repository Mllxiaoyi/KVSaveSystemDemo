using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace KVSaveSystem
{
    public class JsonFileArchiveOperation
    {
        public static void SaveToDisk(KvSaveDataGroup dataGroup)
        {
            var filePath = SaveConfig.GetGroupFilePath(dataGroup.GroupName);
            var tmpFilePath = filePath + ".bak";

            var jsonContent = JsonConvert.SerializeObject(dataGroup.DataDic);
            if (File.Exists(tmpFilePath))
            {
                File.Delete(tmpFilePath);
            }

            using (FileStream fileStream = new FileStream(tmpFilePath, FileMode.Create, FileAccess.Write))
                //using (CryptoStream csEncrypt = new CryptoStream(fileStream, KvSaveSystem.AESEncryptor, CryptoStreamMode.Write))
            using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8))
            {
                // 优化：避免每次写入都触发磁盘写入操作。数据会写入内存缓冲区，在缓冲区满或显式调用 Flush() 时才会写入磁盘。
                writer.AutoFlush = false;
                writer.WriteLine("#FileModeDataArchiveOperation#");
                writer.WriteLine(jsonContent);
                writer.WriteLine("#EndOfGroup#");
                writer.Flush();
            }

            // 原子化
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            File.Copy(tmpFilePath, filePath);
        }

        public static void LoadFromDisk(string groupName)
        {
            var filePath = SaveConfig.GetGroupFilePath(groupName);
            if (!File.Exists(filePath))
                return;

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                //using (CryptoStream csDecrypt = new CryptoStream(fs, KvSaveSystem.GetAESDecryptor(fs), CryptoStreamMode.Read))
            using (StreamReader reader = new StreamReader(fs, Encoding.UTF8))
            {
                // 读取文件内容
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null || line.StartsWith("#EndOfGroup#"))
                    {
                        break; // 结束标记
                    }

                    if (line == "#FileModeDataArchiveOperation#")
                        continue;

                    Dictionary<string, ISaveDataObj> datas =
                        JsonConvert.DeserializeObject<Dictionary<string, ISaveDataObj>>(line);
                    foreach (var kv in datas)
                    {
                        KvSaveSystem.SetValue(kv.Key, kv.Value, groupName);
                    }
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;

namespace KVSaveSystem
{
    public static class KvSaveSystem
    {
        public const string DEFAULT_GROUP_NAME = "Default";

        private const string EMPTY_STRING = "";

        private const string SAVE_FILE_EXTENSION = ".sav";

        private static Dictionary<string, KvSaveDataGroup> _cache = new Dictionary<string, KvSaveDataGroup>();

        private static bool _isSaving = false;

        private const int keySize = 32; // AES-256
        private const int ivSize = 16;
        
        // 固定的AES密钥 (32字节用于AES-256)
        private static readonly byte[] FixedAESKey = 
        {
            0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF,
            0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10,
            0x0F, 0x1E, 0x2D, 0x3C, 0x4B, 0x5A, 0x69, 0x78,
            0x87, 0x96, 0xA5, 0xB4, 0xC3, 0xD2, 0xE1, 0xF0
        };
        
        // 固定的IV (16字节)
        private static readonly byte[] FixedIV = 
        {
            0xF0, 0xE1, 0xD2, 0xC3, 0xB4, 0xA5, 0x96, 0x87,
            0x78, 0x69, 0x5A, 0x4B, 0x3C, 0x2D, 0x1E, 0x0F
        };

        private static ICryptoTransform _aesEncryptor;
        private static ICryptoTransform _aesDecryptor;

        public static ICryptoTransform AESEncryptor
        {
            get
            {
                if (_aesEncryptor == null)
                {
                    using (var alg = Aes.Create())
                    {
                        alg.Mode = CipherMode.CBC;
                        alg.Padding = PaddingMode.PKCS7;
                        alg.Key = FixedAESKey;
                        alg.IV = FixedIV;
                        _aesEncryptor = alg.CreateEncryptor();
                    }
                }

                return _aesEncryptor;
            }
        }

        public static ICryptoTransform AESDecryptor
        {
            get
            {
                if (_aesDecryptor == null)
                {
                    using (var alg = Aes.Create())
                    {
                        alg.Mode = CipherMode.CBC;
                        alg.Padding = PaddingMode.PKCS7;
                        alg.Key = FixedAESKey;
                        alg.IV = FixedIV;
                        _aesDecryptor = alg.CreateDecryptor();
                    }
                }

                return _aesDecryptor;
            }
        }

        public static ICryptoTransform GetAESDecryptor()
        {
            return AESDecryptor;
        }


        public static void SetString(string key, string value, string groupName = EMPTY_STRING)
        {
            groupName = GetKeyGroupName(key, groupName);
            SetValue(key, value, groupName);
        }

        public static string GetString(string key, string defaultValue, string groupName = EMPTY_STRING)
        {
            groupName = GetKeyGroupName(key, groupName);
            return GetValue(key, defaultValue, groupName);
        }

        public static void SetInt(string key, int value, string groupName = EMPTY_STRING)
        {
            groupName = GetKeyGroupName(key, groupName);
            SetValue(key, value, groupName);
        }

        public static int GetInt(string key, int defaultValue, string groupName = EMPTY_STRING)
        {
            groupName = GetKeyGroupName(key, groupName);
            return GetValue(key, defaultValue, groupName);
        }

        public static void SetFloat(string key, float value, string groupName = EMPTY_STRING)
        {
            groupName = GetKeyGroupName(key, groupName);
            SetValue(key, value, groupName);
        }

        public static float GetFloat(string key, float defaultValue, string groupName = EMPTY_STRING)
        {
            groupName = GetKeyGroupName(key, groupName);
            return GetValue(key, defaultValue, groupName);
        }

        public static string GetKeyGroupName(string key, string inputGroupName)
        {
            if (inputGroupName != EMPTY_STRING)
                return inputGroupName;

            return DEFAULT_GROUP_NAME;
        }


        /// <summary>
        /// 设置存档数据
        /// </summary>
        public static void SetValue<T>(string key, T value, string groupName = DEFAULT_GROUP_NAME)
        {
            if (!_cache.TryGetValue(groupName, out var groupData))
            {
                groupData = new KvSaveDataGroup(groupName);
                _cache[groupName] = groupData;
            }

            groupData.SetData(key, value);
        }

        /// <summary>
        /// 获取存档数据
        /// </summary>
        public static T GetValue<T>(string key, T defaultValue = default, string groupName = DEFAULT_GROUP_NAME)
        {
            if (!_cache.TryGetValue(groupName, out var groupData))
            {
                return defaultValue;
            }

            return groupData.GetData(key, defaultValue);
        }

        /// <summary>
        /// 同步保存数据
        /// </summary>
        /// <param name="isForce"></param>
        public static void Save(bool isForce = false)
        {
            try
            {
                if (!Directory.Exists(SaveConfig.UserArchiveDirectoryPath))
                {
                    Directory.CreateDirectory(SaveConfig.UserArchiveDirectoryPath);
                }

                foreach (var groupPair in _cache)
                {
                    var group = groupPair.Value;
                    if (group.IsDirty || isForce)
                    {
                        group.Save();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Save failed: {e}");
            }
            finally
            {
                _isSaving = false;
            }
        }

        /// <summary>
        /// 异步保存数据
        /// </summary>
        public static async Task SaveAsync(bool isForce = false)
        {
            try
            {
                if (!Directory.Exists(SaveConfig.UserArchiveDirectoryPath))
                {
                    Directory.CreateDirectory(SaveConfig.UserArchiveDirectoryPath);
                }

                // 收集所有需要保存的脏数据组
                var saveTasks = new List<Task>();
                foreach (var groupPair in _cache)
                {
                    var group = groupPair.Value;
                    if (group.IsDirty || isForce)
                    {
                        saveTasks.Add(group.SaveAsync());
                    }
                }

                await Task.WhenAll(saveTasks);
            }
            catch (Exception e)
            {
                Debug.LogError($"Save failed: {e}");
            }
            finally
            {
                _isSaving = false;
            }
        }

        /// <summary>
        /// 同步加载所有存档数据
        /// </summary>
        public static void LoadAll()
        {
            try
            {
                if (!Directory.Exists(SaveConfig.UserArchiveDirectoryPath))
                {
                    return;
                }

                var saveFiles = Directory.GetFiles(SaveConfig.UserArchiveDirectoryPath, $"*{SAVE_FILE_EXTENSION}");
                
                foreach (var file in saveFiles)
                {
                    var groupName = Path.GetFileNameWithoutExtension(file);
                    
                    // 创建或获取组数据
                    if (!_cache.TryGetValue(groupName, out var groupData))
                    {
                        groupData = new KvSaveDataGroup(groupName);
                        _cache[groupName] = groupData;
                    }
                    
                    // 同步加载数据
                    groupData.LoadFromDisk();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"LoadAll failed: {e}");
            }
        }

        /// <summary>
        /// 异步加载所有存档数据
        /// </summary>
        public static async Task LoadAllAsync(Action complete = null)
        {
            if (!Directory.Exists(SaveConfig.UserArchiveDirectoryPath))
            {
                complete?.Invoke();
                return;
            }

            var saveFiles = Directory.GetFiles(SaveConfig.UserArchiveDirectoryPath, $"*{SAVE_FILE_EXTENSION}");
            var loadTasks = new List<Task>();

            foreach (var file in saveFiles)
            {
                var groupName = Path.GetFileNameWithoutExtension(file);
                var groupData = new KvSaveDataGroup(groupName);
                groupData.LoadFromDisk();
            }

            await Task.WhenAll(loadTasks);
            complete?.Invoke();
        }

        /// <summary>
        /// 加载指定组的数据
        /// </summary>
        public static async Task LoadGroupAsync(string groupName)
        {
            var filePath = SaveConfig.GetGroupFilePath(groupName);

            if (!File.Exists(filePath))
            {
                return;
            }

            try
            {
                if (!File.Exists(filePath))
                    return;

                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (CryptoStream csDecrypt = new CryptoStream(fs, AESEncryptor, CryptoStreamMode.Read))
                using (StreamReader reader = new StreamReader(csDecrypt))
                {
                    // 读取文件内容
                    while (reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        if (line == null || line.StartsWith("#EndOfGroup#"))
                        {
                            break; // 结束标记
                        }

                        // 解析数据行
                        var parts = line.Split(new[] { ':' }, 2);
                        if (parts.Length == 2)
                        {
                            var key = parts[0].Trim();
                            var value = parts[1].Trim();

                            // 将数据存入缓存
                            SetValue(key, value, groupName);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load group {groupName}: {e}");
            }
        }

        /// <summary>
        /// 删除指定组的存档
        /// </summary>
        public static void DeleteGroup(string groupName)
        {
            if (_cache.ContainsKey(groupName))
            {
                _cache.Remove(groupName);
            }

            var filePath = SaveConfig.GetGroupFilePath(groupName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        /// <summary>
        /// 清除所有缓存数据
        /// </summary>
        public static void ClearCache()
        {
            _cache.Clear();
        }
    }
}
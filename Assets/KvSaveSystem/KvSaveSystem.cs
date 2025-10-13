using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace KVSaveSystem
{
    /// <summary>
    /// 键值对存档系统
    /// </summary>
    public static class KvSaveSystem
    {
        private const string DEFAULT_GROUP_NAME = "Default";

        private const string EMPTY_STRING = "";

        private static Dictionary<string, KvSaveDataGroup> _cache = new();

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
        /// 设置存档数据（缓存）
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
        /// 获取存档数据（缓存）
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
        /// 同步保存数据（持久化）
        /// </summary>
        /// <param name="isForce"></param>
        public static void Save(bool isForce = false)
        {
            foreach (var groupPair in _cache)
            {
                var group = groupPair.Value;
                if (group.IsDirty || isForce)
                {
                    group.Save();
                }
            }
        }

        /// <summary>
        /// 异步保存数据（持久化）
        /// </summary>
        public static async Task SaveAsync(bool isForce = false)
        {
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

            Debug.Log($"Starting async save for {saveTasks.Count} groups.");
            await Task.WhenAll(saveTasks);
            await Task.Delay(1000);
            Debug.Log("All async save operations completed.");
        }

        /// <summary>
        /// 同步加载所有存档数据
        /// </summary>
        public static void LoadAll(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            var saveFiles = Directory.GetFiles(directoryPath, $"*{SaveConfig.SAVE_FILE_EXTENSION}");

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

        /// <summary>
        /// 异步加载所有存档数据
        /// </summary>
        public static async Task LoadAllAsync(string directoryPath, Action complete = null)
        {
            if (!Directory.Exists(directoryPath))
            {
                complete?.Invoke();
                return;
            }

            var saveFiles = Directory.GetFiles(directoryPath, $"*{SaveConfig.SAVE_FILE_EXTENSION}");
            
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
        public static void LoadGroup(string groupName)
        {
            var filePath = SaveConfig.GetGroupFilePath(groupName);

            if (!File.Exists(filePath))
                return;

            var groupData = new KvSaveDataGroup(groupName);
            groupData.LoadFromDisk();
            _cache[groupName] = groupData;
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

#if UNITY_EDITOR
        public static void PrintSaveCacheData()
        {
            string log = $"--- Cache Data Detail ---\n";
            foreach (var kv in _cache)
            {
                var group = kv.Value;
                log += $"Group: {group.GroupName}, IsDirty: {group.IsDirty}, Data Count: {group.DataDic.Count}\n";
                foreach (var dataKvp in group.DataDic)
                {
                    log += $"  Key: {dataKvp.Key}, Type: {dataKvp.Value.GetType().Name}, Value: {dataKvp.Value}\n";
                }
            }
            Debug.Log(log);
        }
#endif
    }
}
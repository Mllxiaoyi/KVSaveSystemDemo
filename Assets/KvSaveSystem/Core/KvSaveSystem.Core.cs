using System.Collections.Generic;
using System.IO;
using KVSaveSystem;

/// <summary>
/// 键值对存档系统
/// </summary>
public partial class KvSaveSystem
{
    private const string DEFAULT_GROUP_NAME = "Default";

    private const string EMPTY_STRING = "";

    private static Dictionary<string, KvSaveDataGroup> _cache = new();

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
            IArchiveSetting archiveSetting = SaveArchiveSettingProvider.Current.GetArchiveSetting(groupName, true);
            if (archiveSetting != null && archiveSetting.IsLazyLoad)
            {
                //懒加载
                groupData = new KvSaveDataGroup(groupName);
                groupData.Load();
                _cache[groupName] = groupData;
                return groupData.GetData(key, defaultValue);
            }
            return defaultValue;
        }

        return groupData.GetData(key, defaultValue);
    }

    /// <summary>
    /// 同步保存数据（持久化）
    /// </summary>
    /// <param name="isForce">强制保存所有组</param>
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
    /// 异步保存数据（持久化），除非特殊情况，否则建议使用 SaveAsync
    /// </summary>
    public static void SaveAsyncInternal(bool isForce = false)
    {
        foreach (var groupPair in _cache)
        {
            var group = groupPair.Value;
            if (group.IsDirty || isForce)
            {
                group.SaveAsync();
            }
        }
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

        var saveFiles = Directory.GetFiles(directoryPath, $"*{SaveSystemConst.SAVE_FILE_EXTENSION}");

        foreach (var file in saveFiles)
        {
            var groupName = Path.GetFileNameWithoutExtension(file);

            // 检查是否为懒加载组，如果是则跳过
            IArchiveSetting archiveSetting = SaveArchiveSettingProvider.Current.GetArchiveSetting(groupName, true);
            if (archiveSetting != null && archiveSetting.IsLazyLoad)
            {
                continue;
            }

            // 创建或获取组数据
            if (!_cache.TryGetValue(groupName, out var groupData))
            {
                groupData = new KvSaveDataGroup(groupName);
                _cache[groupName] = groupData;
            }

            // 同步加载数据
            groupData.Load();
        }
    }

    /// <summary>
    /// 删除指定组的存档
    /// </summary>
    public static void DeleteGroup(string groupName)
    {
        _cache.Remove(groupName);

        var filePath = SaveSystemConst.GetGroupFilePath(groupName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    /// <summary>
    /// 获取指定组
    /// </summary>
    /// <param name="groupName"></param>
    /// <returns></returns>
    public static KvSaveDataGroup GetGroup(string groupName)
    {
        if (!_cache.TryGetValue(groupName, out var groupData))
        {
            // 检查是否为懒加载组，如果是则按需加载
            IArchiveSetting archiveSetting = SaveArchiveSettingProvider.Current.GetArchiveSetting(groupName, true);
            if (archiveSetting != null && archiveSetting.IsLazyLoad)
            {
                groupData = new KvSaveDataGroup(groupName);
                groupData.Load();
                _cache[groupName] = groupData;
                return groupData;
            }
        }

        return _cache.GetValueOrDefault(groupName);
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
                log += $"  Key: {dataKvp.Key}, Type: {dataKvp.Value.GetType().Name}";
                if (dataKvp.Value is KvSaveDataObj<int> intData)
                {
                    log += $", Value: {intData.Value}\n";
                }
                else if (dataKvp.Value is KvSaveDataObj<string> strData)
                {
                    log += $", Value: {strData.Value}\n";
                }
                else if (dataKvp.Value is KvSaveDataObj<float> floatData)
                {
                    log += $", Value: {floatData.Value}\n";
                }
                else
                {
                    // 出错了
                    log += ", Value: [Unsupported Type]\n";
                }
            }
        }

        UnityEngine.Debug.Log(log);
    }
#endif
}
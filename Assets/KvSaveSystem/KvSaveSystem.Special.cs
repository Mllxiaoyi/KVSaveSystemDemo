using System.Text;
using UnityEngine;
using KVSaveSystem;
using UnityEditor;
//TODO using BaseFramework.Config;
//TODO using Ninja3.Networking;

/// <summary>
/// 键值对存档系统 - 忍三 功能扩展
/// </summary>
public partial class KvSaveSystem
{
    public static bool isOpenDoubleWrite = true;
    /// <summary>
    /// 双读双写开关
    /// </summary>
    public static bool isOpenDoubleCheck = true;
    
    private static StringBuilder _tmpStringBuidler = new StringBuilder();
    
    public static void SetString(string key, string value, string groupName = EMPTY_STRING)
    {
        groupName = GetKeyGroupName(key, groupName);
        SetValue(key, value, groupName);
        if (isOpenDoubleWrite)
            PlayerPrefs.SetString(key, value);
    }

    public static string GetString(string key, string defaultValue, string groupName = EMPTY_STRING)
    {
        groupName = GetKeyGroupName(key, groupName);
        string result = GetValue(key, defaultValue, groupName);
        if (isOpenDoubleCheck)
        {
            string playerPrefValue = PlayerPrefs.GetString(key, defaultValue);
            if (playerPrefValue != result)
            {
                // 双读不一致，上报日志
                Debug.LogWarning($"[KvSaveSystem] 双读不一致，Key: {key}, 存档值: {result}, PlayerPrefs值: {playerPrefValue}");
                SetString(key, playerPrefValue, groupName);
                return playerPrefValue;
            }
        }
        return result;
    }

    public static void SetInt(string key, int value, string groupName = EMPTY_STRING)
    {
        groupName = GetKeyGroupName(key, groupName);
        SetValue(key, value, groupName);
        if (isOpenDoubleWrite)
            PlayerPrefs.SetInt(key, value);
    }

    public static int GetInt(string key, int defaultValue, string groupName = EMPTY_STRING)
    {
        groupName = GetKeyGroupName(key, groupName);
        int result = GetValue(key, defaultValue, groupName);
        if (isOpenDoubleCheck)
        {
            int playerPrefValue = PlayerPrefs.GetInt(key, defaultValue);
            if (playerPrefValue != result)
            {
                // 双读不一致，上报日志
                Debug.LogWarning($"[KvSaveSystem] 双读不一致，Key: {key}, 存档值: {result}, PlayerPrefs值: {playerPrefValue}");
                SetInt(key, playerPrefValue, groupName);
                return playerPrefValue;
            }
        }
        return result;
    }

    public static void SetFloat(string key, float value, string groupName = EMPTY_STRING)
    {
        groupName = GetKeyGroupName(key, groupName);
        SetValue(key, value, groupName);
        if (isOpenDoubleWrite)
            PlayerPrefs.SetFloat(key, value);
    }

    public static float GetFloat(string key, float defaultValue, string groupName = EMPTY_STRING)
    {
        groupName = GetKeyGroupName(key, groupName);
        float result = GetValue(key, defaultValue, groupName);
        if (isOpenDoubleCheck)
        {
            float playerPrefValue = PlayerPrefs.GetFloat(key, defaultValue);
            if (Mathf.Approximately(playerPrefValue, result))
            {
                // 双读不一致，上报日志
                Debug.LogWarning($"[KvSaveSystem] 双读不一致，Key: {key}, 存档值: {result}, PlayerPrefs值: {playerPrefValue}");
                SetFloat(key, playerPrefValue, groupName);
                return playerPrefValue;
            }
        }
        return result;
    }
    
    public static string GetKeyGroupName(string key, string inputGroupName)
    {
        if (inputGroupName != EMPTY_STRING)
            return inputGroupName;

        return DEFAULT_GROUP_NAME;
    }
    
    public static string GetKeyAccord2User(string key)
    {
        _tmpStringBuidler.Length = 0;
        //TODO _tmpStringBuidler.AppendFormat("{0}@{1}", key, NetworkingUserToken.uid);
        return _tmpStringBuidler.ToString();
    }
    
    public static string GetStringAccord2User(string key, string defaultValue, string groupName = EMPTY_STRING)
    {
        string userKey = GetKeyAccord2User(key);
        return GetString(userKey, defaultValue.ToString(), groupName);
    }
    
    public static void SetStringAccord2User(string key, string value,  string groupName = EMPTY_STRING)
    {
        string userKey = GetKeyAccord2User(key);
        SetString(userKey, value, groupName);
    }
    
    public static int GetIntAccord2User(string key, int defaultValue, string groupName = EMPTY_STRING)
    {
        string userKey = GetKeyAccord2User(key);
        return GetInt(userKey, defaultValue, groupName);
    }
    
    public static void SetIntAccord2User(string key, int value,  string groupName = EMPTY_STRING)
    {
        string userKey = GetKeyAccord2User(key);
        SetInt(userKey, value, groupName);
    }
    
    public static float GetFloatAccord2User(string key, float defaultValue, string groupName = EMPTY_STRING)
    {
        string userKey = GetKeyAccord2User(key);
        return GetFloat(userKey, defaultValue, groupName);
    }
    
    public static void SetFloatAccord2User(string key, float value,  string groupName = EMPTY_STRING)
    {
        string userKey = GetKeyAccord2User(key);
        SetFloat(userKey, value, groupName);
    }
}
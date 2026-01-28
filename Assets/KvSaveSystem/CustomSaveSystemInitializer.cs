using UnityEditor;
using UnityEngine;

namespace KVSaveSystem
{
    /// <summary>
    /// 在项目初始化时注册自定义配置提供者
    /// </summary>
    public static class CustomSaveSystemInitializer
    {
        private const string SAVE_ARCHIVE_SETTING_ASSET_PATH = "Assets/KvSaveSystem/Setting/SaveArchiveSettingSO.asset";
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            var customProvider = LoadOrCreateInstance<SaveArchiveSettingSO>(SAVE_ARCHIVE_SETTING_ASSET_PATH);
            SaveArchiveSettingProvider.SetProvider(customProvider);
            Debug.Log("已注册自定义存档配置提供者");
        }
        
        private static T LoadOrCreateInstance<T>(string assetPath) where T : ScriptableObject
        {
#if UNITY_EDITOR
            var instance = AssetDatabase.LoadMainAssetAtPath(assetPath) as T;
            if (instance == null)
            {
                instance = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(instance, assetPath);
                AssetDatabase.SaveAssets();
            }
            return instance;
#else
        var instance = InGameResourceFactory.Instance.LoadAsset<T>(assetPath);
        if (instance == null)
        {
            BaseFramework.Log.Error($"错误，找不到 {nameof(T)}！");
            instance = ScriptableObject.CreateInstance<T>();
        }
        return instance;
#endif
        }
    }
}
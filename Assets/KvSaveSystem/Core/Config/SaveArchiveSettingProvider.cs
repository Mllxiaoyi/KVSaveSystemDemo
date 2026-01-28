using UnityEngine;

namespace KVSaveSystem
{
    /// <summary>
    /// 存档配置提供者管理器
    /// 允许不同项目注入自定义的配置提供者实现
    /// </summary>
    public static class SaveArchiveSettingProvider
    {
        private static ISaveArchiveSettingProvider _provider;
        private static readonly object _providerLock = new object();

        /// <summary>
        /// 当前的配置提供者
        /// </summary>
        public static ISaveArchiveSettingProvider Current
        {
            get
            {
                if (_provider == null)
                {
                    lock (_providerLock)
                    {
                        if (_provider == null)
                        {
                            _provider = ScriptableObject.CreateInstance<SaveArchiveSettingSO>();
                        }
                    }
                }
                return _provider;
            }
        }

        /// <summary>
        /// 设置自定义的配置提供者
        /// 不同项目可以调用此方法来注入自己的配置加载逻辑
        /// </summary>
        /// <param name="provider">自定义的配置提供者</param>
        public static void SetProvider(ISaveArchiveSettingProvider provider)
        {
            lock (_providerLock)
            {
                _provider = provider;
            }
        }
    }
}
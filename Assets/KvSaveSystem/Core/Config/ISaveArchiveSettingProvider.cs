namespace KVSaveSystem
{
    /// <summary>
    /// 存档配置提供者接口
    /// 不同项目可以实现此接口来提供自定义的配置加载逻辑
    /// </summary>
    public interface ISaveArchiveSettingProvider
    {
        /// <summary>
        /// 是否开启加密
        /// </summary>
        bool EnableEncrypt { get; }

        /// <summary>
        /// 获取指定组的存档配置
        /// </summary>
        /// <param name="groupName">组名</param>
        /// <param name="onlySpecial">是否只获取特殊配置（不使用默认配置）</param>
        /// <returns>存档配置，如果找不到且 onlySpecial 为 true 则返回 null</returns>
        IArchiveSetting GetArchiveSetting(string groupName, bool onlySpecial = false);

        /// <summary>
        /// 是否开启双写（设置值时同样设置 PlayerPrefs 的值）
        /// </summary>
        bool IsOpenDoubleWrite { get; }

        /// <summary>
        /// 初始化
        /// </summary>
        void Init();
    }
}
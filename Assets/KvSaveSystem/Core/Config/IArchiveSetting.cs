namespace KVSaveSystem
{
    /// <summary>
    /// 存档配置类接口
    /// </summary>
    public interface IArchiveSetting
    {
        /// <summary>
        /// 数据档案操作类型
        /// </summary>
        ArchiveOperationType ArchiveOperationMode { get; }
        
        /// <summary>
        /// 是否只能同步保存
        /// </summary>
        bool IsForceSaveSync{ get; }
        
        /// <summary>
        /// 是用户登录后的存档吗
        /// </summary>
        bool IsUserArchive { get; }

        /// <summary>
        /// 是懒加载吗
        /// </summary>
        bool IsLazyLoad { get; }
    }
}


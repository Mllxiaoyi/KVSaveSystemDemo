using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace KVSaveSystem
{
    [Serializable]
    public class KvSaveDataGroup : IDisposable
    {
        public string GroupName
        {
            get;
            private set;
        }
        
        public IDictionary<string, ISaveDataObj> DataDic
        {
            get;
            private set;
        }
        
        public bool IsDirty
        {
            get;
            private set;
        }
        
        public bool IsSaving
        {
            get;
            private set;
        }
        
        public bool IsLoading
        {
            get;
            private set;
        }
        
        public IArchiveSetting ArchiveSetting
        {
            get;
            private set;
        }
        
        // 缓存的取消令牌源，用于管理该组的异步操作
        private CancellationTokenSource _cancellationTokenSource;
        
        public KvSaveDataGroup(string groupName)
        {
            GroupName = groupName;
            DataDic = new Dictionary<string, ISaveDataObj>();
            IsDirty = true;
            IsSaving = false;
            IsLoading = false;
            ArchiveSetting = ArchiveSettingConfigSO.GetArchiveSetting(groupName);
            _cancellationTokenSource = new CancellationTokenSource();
        }
    
        public void SetData<T>(string key, T value)
        {
            if (!DataDic.TryGetValue(key, out var cachedDataObj))
            {
                DataDic[key] = KvSaveDataFactory.GetSaveDataObj(value);
                IsDirty = true;
            }
            else if (cachedDataObj is KvSaveDataObj<T> cachedObj)
            {
                // 类型匹配，检查值是否相同，值不相同才更新
                if (!cachedObj.Equals(value))
                {
                    cachedObj.Value = value;
                    IsDirty = true;
                }
            }
            else
            {
                // 类型不匹配，创建新的
                DataDic[key] = KvSaveDataFactory.GetSaveDataObj(value);
                IsDirty = true;
            }
        }
    
        public T GetData<T>(string key, T defaultValue = default(T))
        {
            if (DataDic.TryGetValue(key, out var cachedDataObj) && 
                cachedDataObj is KvSaveDataObj<T> cachedData)
            {
                return cachedData.Value;
            }
            return defaultValue;
        }
        
        public void Clear()
        {
            DataDic.Clear();
            IsDirty = true;
        }

        public void Save()
        {
            try
            {
                switch (ArchiveSetting.ArchiveOperationMode)
                {
                    case ArchiveOperationType.JsonFile:
                        JsonFileArchiveOperation.SaveToDisk(this);
                        break;
                    case ArchiveOperationType.ZeroFormatterFile:
                        ZeroFormatterFileArchiveOperation.SaveToDisk(this);
                        break;
                    default:
                        break;
                }
            
                IsDirty = false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save group {GroupName}: {e}");
            }
        }
        
        public async Task SaveAsync()
        {
            // 取消之前的保存操作，避免并发
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }
            
            _cancellationTokenSource = new CancellationTokenSource();
            
            try
            {
                IsSaving = true;
                
                switch (ArchiveSetting.ArchiveOperationMode)
                {
                    case ArchiveOperationType.JsonFile:
                        // TODO: 实现 JsonFileArchiveOperation.SaveToDiskAsync
                        break;
                    case ArchiveOperationType.ZeroFormatterFile:
                        await ZeroFormatterFileArchiveOperation.SaveToDiskAsync(this, _cancellationTokenSource.Token);
                        break;
                    default:
                        break;
                }
            
                IsDirty = false;
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"Save operation for group {GroupName} was cancelled");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save group {GroupName}: {e}");
            }
            finally
            {
                IsSaving = false;
            }
        }
        
        public void LoadFromDisk()
        {
            var groupName = GroupName;
            try
            {
                switch (ArchiveSetting.ArchiveOperationMode)
                {
                    case ArchiveOperationType.JsonFile:
                        //JsonFileArchiveOperation.LoadFromDisk(groupName);
                        break;
                    case ArchiveOperationType.ZeroFormatterFile:
                        ZeroFormatterFileArchiveOperation.LoadFromDisk(groupName);
                        break;
                    default:
                        break;
                }
            
                IsDirty = false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load group {groupName}: {e}");
            }
            finally
            {
                
            }
        }
        
        /// <summary>
        /// 取消当前的异步保存操作
        /// </summary>
        public void CancelSave()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                Debug.Log($"Cancelled save operation for group {GroupName}");
            }
        }
        
        /// <summary>
        /// 获取当前的取消令牌
        /// </summary>
        public CancellationToken GetCancellationToken()
        {
            return _cancellationTokenSource?.Token ?? CancellationToken.None;
        }
        
        /// <summary>
        /// 检查是否有进行中的保存操作被请求取消
        /// </summary>
        public bool IsCancellationRequested => _cancellationTokenSource?.IsCancellationRequested ?? false;
        
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }
}
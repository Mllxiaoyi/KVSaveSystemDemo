using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

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

        // 线程安全锁，保护关键数据
        private readonly object _saveLock = new object();

        // 保存操作的性能统计
        private long _lastSaveTimeMs;
        private DateTime _lastSaveTime;
        
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
            // 线程安全检查
            lock (_saveLock)
            {
                // 如果当前没有脏数据，不需要保存
                if (!IsDirty)
                {
                    UnityEngine.Debug.Log($"Group {GroupName} is already up to date, skipping save");
                    return;
                }

                // 如果已经在保存中，取消之前的保存操作
                if (IsSaving)
                {
                    UnityEngine.Debug.Log($"Group {GroupName} is already saving, cancelling previous operation");
                    _cancellationTokenSource?.Cancel();
                }
            }

            // 取消之前的保存操作，避免并发
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }

            _cancellationTokenSource = new CancellationTokenSource();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                lock (_saveLock)
                {
                    IsSaving = true;
                }

                UnityEngine.Debug.Log($"Starting async save for group {GroupName} on background thread");

                // 使用 Task.Run 在后台线程执行保存操作
                await Task.Run(async () =>
                {
                    // 在后台线程中执行，避免阻塞主线程
                    switch (ArchiveSetting.ArchiveOperationMode)
                    {
                        case ArchiveOperationType.JsonFile:
                            // SaveJsonFileAsync(_cancellationTokenSource.Token);
                            break;
                        case ArchiveOperationType.ZeroFormatterFile:
                            await ZeroFormatterFileArchiveOperation.SaveToDiskAsync(this, _cancellationTokenSource.Token);
                            break;
                        default:
                            UnityEngine.Debug.LogWarning($"Unsupported archive operation mode: {ArchiveSetting.ArchiveOperationMode}");
                            break;
                    }
                }, _cancellationTokenSource.Token);

                stopwatch.Stop();

                // 线程安全更新状态
                lock (_saveLock)
                {
                    IsDirty = false;
                    _lastSaveTimeMs = stopwatch.ElapsedMilliseconds;
                    _lastSaveTime = DateTime.Now;
                }

                UnityEngine.Debug.Log($"Successfully saved group {GroupName} in {stopwatch.ElapsedMilliseconds}ms on background thread");
            }
            catch (OperationCanceledException)
            {
                UnityEngine.Debug.Log($"Save operation for group {GroupName} was cancelled after {stopwatch.ElapsedMilliseconds}ms");
            }
            catch (Exception e)
            {
                stopwatch.Stop();
                UnityEngine.Debug.LogError($"Failed to save group {GroupName} after {stopwatch.ElapsedMilliseconds}ms: {e}");
                throw; // 重新抛出异常，让调用者处理
            }
            finally
            {
                lock (_saveLock)
                {
                    IsSaving = false;
                }
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
        /// 获取上次保存操作的耗时（毫秒）
        /// </summary>
        public long LastSaveTimeMs
        {
            get
            {
                lock (_saveLock)
                {
                    return _lastSaveTimeMs;
                }
            }
        }

        /// <summary>
        /// 获取上次保存的时间
        /// </summary>
        public DateTime LastSaveTime
        {
            get
            {
                lock (_saveLock)
                {
                    return _lastSaveTime;
                }
            }
        }

        /// <summary>
        /// 获取保存性能统计信息
        /// </summary>
        public string GetPerformanceInfo()
        {
            lock (_saveLock)
            {
                if (_lastSaveTime == default)
                {
                    return $"Group '{GroupName}': No save operations completed yet";
                }

                var timeSinceLastSave = DateTime.Now - _lastSaveTime;
                return $"Group '{GroupName}': Last save {_lastSaveTimeMs}ms ago at {_lastSaveTime:HH:mm:ss} " +
                       $"({timeSinceLastSave.TotalSeconds:F1}s ago), " +
                       $"IsDirty: {IsDirty}, IsSaving: {IsSaving}";
            }
        }
        
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
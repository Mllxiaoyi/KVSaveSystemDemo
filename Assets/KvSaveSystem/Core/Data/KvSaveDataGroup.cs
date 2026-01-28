using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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

        public string FilePath
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
            DataDic = new ConcurrentDictionary<string, ISaveDataObj>();
            IsDirty = true;
            IsSaving = false;
            IsLoading = false;
            ArchiveSetting = SaveArchiveSettingProvider.Current.GetArchiveSetting(groupName);
            FilePath = SaveSystemConst.GetGroupFilePath(groupName, ArchiveSetting);
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
                FileArchiveOperation.SaveToDisk(this);
                IsDirty = false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save group {GroupName}: {e}");
            }
        }

        public async Task SaveAsync()
        {
            if (ArchiveSetting.IsForceSaveSync)
            {
                Save();
                return;
            }
            
            var stopwatch = Stopwatch.StartNew();
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            try
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = cancelTokenSource;
                IsSaving = true;
                SaveSystemLog.Performance($"开始保存，组名：{GroupName}");
                
                await Task.Run(
                    async () => { await FileArchiveOperation.SaveToDiskAsync(this, cancelTokenSource.Token); },
                    cancelTokenSource.Token);

                stopwatch.Stop();
                IsDirty = false;
                SaveSystemLog.Performance($"保存完成，组名：{GroupName}，耗时：{stopwatch.ElapsedMilliseconds}ms");
            }
            catch (OperationCanceledException)
            {
                SaveSystemLog.Info($"保存任务被取消，组名：{GroupName}");
            }
            catch (Exception e)
            {
                SaveSystemLog.Error($"保存失败，组名：{GroupName}，Exception: {e}");
            }
            finally
            {
                stopwatch.Stop();
                if (_cancellationTokenSource == cancelTokenSource)
                    _cancellationTokenSource = null;
                cancelTokenSource?.Dispose();
                IsSaving = false;
            }
        }

        public void Load()
        {
            var groupName = GroupName;
            var isLazyLoad = ArchiveSetting != null && ArchiveSetting.IsLazyLoad;
            var stopwatch = isLazyLoad ? Stopwatch.StartNew() : null;

            try
            {
                if (isLazyLoad)
                {
                    SaveSystemLog.Performance($"开始懒加载组数据：{groupName}");
                }
                else
                {
                    SaveSystemLog.Performance($"开始加载组数据：{groupName}");
                }

                FileArchiveOperation.LoadFromDisk(this, FilePath);
                IsDirty = false;

                if (isLazyLoad && stopwatch != null)
                {
                    stopwatch.Stop();
                    SaveSystemLog.Performance($"完成懒加载组数据：{groupName}，耗时：{stopwatch.ElapsedMilliseconds}ms");
                }
                else
                {
                    SaveSystemLog.Performance($"完成加载组数据：{groupName}");
                }
            }
            catch (Exception e)
            {
                stopwatch?.Stop();
                SaveSystemLog.Error($"加载组数据失败：{groupName}，异常：{e.Message}");
                throw; // 重新抛出异常，让调用方决定如何处理
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
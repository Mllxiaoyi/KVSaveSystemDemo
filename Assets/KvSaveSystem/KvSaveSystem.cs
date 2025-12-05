using UnityEngine;
using Sirenix.OdinInspector;

public partial class KvSaveSystem : MonoBehaviour
{
    [SerializeField] 
    private SaveConfig _saveConfig;

    /// <summary>
    /// 是否开启自动保存（定时刷盘）
    /// </summary>
    [SerializeField] 
    private bool _enableAutoSave;
    
    /// <summary>
    /// 定时刷盘时间间隔（秒）
    /// </summary>
    [SerializeField] 
    [ShowIf("@_enableAutoSave == true")]
    private float _flushInterval = 5f;

    private float _timer;

    private bool _needSaveThisFrame;

    private void Start()
    {
        ResetTimer();
        _needSaveThisFrame = false;
    }

    private void LateUpdate()
    {
        if (_enableAutoSave)
        {
            _timer -= Time.deltaTime;

            if (_timer <= 0f)
            {
                _needSaveThisFrame = true;
            }
        }

        if (_needSaveThisFrame)
        {
            ResetTimer();
            SaveAsyncInternal();
        }
    }

    public void SaveAsync()
    {
        _needSaveThisFrame = true;
    }

    public void ResetTimer()
    {
        _timer = _flushInterval;
    }
}
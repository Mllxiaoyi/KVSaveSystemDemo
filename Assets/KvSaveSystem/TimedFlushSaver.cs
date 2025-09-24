using KVSaveSystem;
using UnityEngine;

public class TimedFlushSaver : MonoBehaviour
{
    [SerializeField] 
    private float _flushInterval = 5f;

    private float _timer;
    
    private void Start()
    {
        _timer = _flushInterval;
    }

    private void Update()
    {
        _timer -= Time.deltaTime;

        if (_timer <= 0f)
        {
            KvSaveSystem.SaveAsync();
            _timer = _flushInterval; // 重置定时器
        }
    }

    public void ResetTimer()
    {
        _timer = _flushInterval;
    }

    public void ForceFlush()
    {
        KvSaveSystem.SaveAsync();
        ResetTimer();
    }
}

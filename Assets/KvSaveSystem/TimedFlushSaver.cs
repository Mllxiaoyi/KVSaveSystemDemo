using System.Collections;
using System.Collections.Generic;
using KVSaveSystem;
using UnityEngine;

public class TimedFlushSaver : MonoBehaviour
{
    [SerializeField] private float flushInterval = 5f;
    
    private void Start()
    {
        StartCoroutine(TimedFlushCoroutine(flushInterval));
    }

    IEnumerator TimedFlushCoroutine(float interval)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            KvSaveSystem.SaveAsync();
        }
    }
}

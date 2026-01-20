using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineTest : MonoBehaviour
{
    private Coroutine ie;
    // Start is called before the first frame update
    void Start()
    {
        ie = StartCoroutine(TestCoroutine());
        // 停止协程
        StopCoroutine(ie);
    }

    // Update is called once per frame
    void Update()
    {
        // 检查 ie 协程是否被停止
        if (ie == null)
        {
            Debug.Log("Coroutine has been stopped");
        }
    }
    
    IEnumerator TestCoroutine()
    {
        Debug.Log("Coroutine started");
        yield return new WaitForSeconds(2);
        Debug.Log("Coroutine ended after 2 seconds");
    }
}

using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class TryCatchTest : MonoBehaviour
{
    [Button]
    private void DoTryCatchTest()
    {
        try
        {
            ErrorFunc();
        }
        catch (Exception e)
        {
            Debug.Log("e");
        }
        finally
        {
            Debug.Log("finally");
        }
        
        Debug.Log("After TryCatch");
    }

    private void ErrorFunc()
    {
        try
        {
            int a = 1;
            int b = a / 0;
        }
        catch (OperationCanceledException e)
        {
            Debug.Log("ErrorFunc e");
        }
        finally
        {
            Debug.Log("ErrorFunc finally");
        }
        
        Debug.Log("ErrorFunc After TryCatch");
    }
}

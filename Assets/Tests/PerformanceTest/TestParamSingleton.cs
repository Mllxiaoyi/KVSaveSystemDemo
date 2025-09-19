using UnityEngine;
using UnityEngine.UI;

public class TestParamSingleton : MonoBehaviour
{
    private static TestParamSingleton _instance;
    public static TestParamSingleton Instance => _instance;

    [SerializeField] 
    private InputField testCountInput;
    [SerializeField] 
    private InputField testRepeatTimesInput;

    public static int TestCount
    {
        get
        {
            if (_instance == null)
                return 0;
            
            if (_instance.testCountInput) 
                return int.Parse(_instance.testCountInput.text);
            
            return 0;
        }
    }
    
    public static int RepeatTimes
    {
        get
        {
            if (_instance == null)
                return 0;
            
            if (_instance.testRepeatTimesInput) 
                return int.Parse(_instance.testRepeatTimesInput.text);
            
            return 0;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

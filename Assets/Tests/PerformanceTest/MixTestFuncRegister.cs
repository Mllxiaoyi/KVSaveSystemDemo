using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MixTestFuncRegister : MonoBehaviour
{
    [SerializeField] private InputField _testCountInput;
    [SerializeField] private InputField _testRepeatTimesInput;

    public int TestCount
    {
        get
        {
            if (_testCountInput)
                return int.Parse(_testCountInput.text);

            return 0;
        }
    }

    public int RepeatTimes
    {
        get
        {
            if (_testRepeatTimesInput)
                return int.Parse(_testRepeatTimesInput.text);

            return 0;
        }
    }


    [SerializeField] private GameObject _uiGroupTemplate;

    [SerializeField] private GameObject _buttonTemplate;

    [SerializeField] private Transform _content;

    private GameObject _currentGroup;

    [SerializeField] private IOSerializePlayerPrefsFirstTest ioSerializePlayerPrefsFirstTest;
    [SerializeField] private SerializeDeserializeSpeedTest serializeDeserializeSpeedTest;
    [SerializeField] private GetSetByCacheSpeedTest getSetByCacheSpeedTest;
    [SerializeField] private CryptSpeedTest cryptSpeedTest;

    void Start()
    {
        _uiGroupTemplate.SetActive(false);
        _buttonTemplate.SetActive(false);

        // IOSerializePlayerPrefsFirstTest 组
        AddGroup("IOSerialize 测试");
        AddGroupFunc("PlayerPrefs、I/O+XML、Json 和 Dictionary写入速度", () =>
            ioSerializePlayerPrefsFirstTest.DoFirstSimpleWriteSpeedTest(TestCount, RepeatTimes));
        AddGroupFunc("XML写入分段耗时测试", () =>
            ioSerializePlayerPrefsFirstTest.DoWriteSpeedTestXML(TestCount, RepeatTimes));
        AddGroupFunc("Json写入分段耗时测试", () =>
            ioSerializePlayerPrefsFirstTest.DoWriteSpeedTestJson(TestCount, RepeatTimes));

        // SerializeDeserializeSpeedTest 组
        AddGroup("序列化性能测试");
        AddGroupFunc("XML 序列化", () =>
            serializeDeserializeSpeedTest.TestXmlSerialization(TestCount, RepeatTimes));
        AddGroupFunc("Binary 序列化", () =>
            serializeDeserializeSpeedTest.TestBinarySerialization(TestCount, RepeatTimes));
        AddGroupFunc("JSON 序列化", () =>
            serializeDeserializeSpeedTest.TestJsonSerialization(TestCount, RepeatTimes));
        AddGroupFunc("ZeroFormatter 序列化", () =>
            serializeDeserializeSpeedTest.TestZeroFormatterSerialization(TestCount, RepeatTimes));
        AddGroupFunc("MemoryPack 序列化", () =>
            serializeDeserializeSpeedTest.TestMemoryPackSerialization(TestCount, RepeatTimes));
        AddGroupFunc("Nino 序列化", () =>
            serializeDeserializeSpeedTest.TestNinoSerialization(TestCount, RepeatTimes));

        // GetSetByCacheSpeedTest 组
        AddGroup("缓存读取测试");
        AddGroupFunc("简单字典读取", () =>
            getSetByCacheSpeedTest.TestSimpleDict(TestCount, RepeatTimes));
        AddGroupFunc("ZeroFormatter 字典读取", () =>
            getSetByCacheSpeedTest.TestZeroFormatterDict(TestCount, RepeatTimes));
        AddGroupFunc("MemoryPack 字典读取", () =>
            getSetByCacheSpeedTest.TestMemoryPackDict(TestCount, RepeatTimes));
        AddGroupFunc("NinoSerializer 字典读取", () =>
            getSetByCacheSpeedTest.TestNinoSerializerDict(TestCount, RepeatTimes));

        // CryptSpeedTest 组
        AddGroup("🔒加密性能测试");
        AddGroupFunc("XOR 混淆性能", () =>
            cryptSpeedTest.TestXorPerformance(TestCount, RepeatTimes));
        AddGroupFunc("AES 加密性能", () =>
            cryptSpeedTest.TestAesPerformance(TestCount, RepeatTimes));
        AddGroupFunc("XOR vs AES 性能对比", () =>
            cryptSpeedTest.CompareXorVsAes(TestCount, RepeatTimes));
        AddGroupFunc("XOR 流式 vs 整体处理对比", () =>
            cryptSpeedTest.CompareXorStreamVsBatch(TestCount, RepeatTimes));
        AddGroupFunc("文件读写性能对比", () =>
            cryptSpeedTest.CompareFileIOPerformance(TestCount, RepeatTimes));
    }

    public void AddGroup(string groupName)
    {
        GameObject newGroup = Instantiate(_uiGroupTemplate, _content);
        newGroup.transform.Find("Title").gameObject.GetComponent<Text>().text = groupName;
        newGroup.SetActive(true);
        _currentGroup = newGroup;
    }

    public void AddGroupFunc(string funcName, UnityAction func)
    {
        if (_currentGroup == null)
        {
            Debug.LogError("请先添加组");
            return;
        }

        GameObject newButton = Instantiate(_buttonTemplate, _currentGroup.transform);
        newButton.transform.Find("Text").gameObject.GetComponent<Text>().text = funcName;
        newButton.GetComponent<Button>().onClick.AddListener(func);
        newButton.SetActive(true);
    }
}
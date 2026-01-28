# SaveArchiveSettingProvider 使用指南

## 概述

`SaveArchiveSettingProvider` 是一个提供者模式的实现，允许不同项目自定义存档配置的加载逻辑，解决了 `KvSaveSystem` 与 `SaveArchiveSettingSO` 的强耦合问题。

## 架构设计

### 接口定义
- `ISaveArchiveSettingProvider`: 存档配置提供者接口
- `SaveArchiveSettingProvider`: 静态管理器，管理当前使用的提供者
- `SaveArchiveSettingSO`: 默认实现，保持向后兼容

### 解耦效果
- `KvSaveSystem` 不再直接依赖 `SaveArchiveSettingSO`
- 通过 `SaveArchiveSettingProvider.Current` 获取配置
- 不同项目可以注入自定义实现

## 使用方式

### 1. 默认使用（无需修改现有代码）

```csharp
// KvSaveSystem 会自动使用 SaveArchiveSettingSO.Instance 作为默认提供者
// 现有项目无需任何修改即可继续工作
```

### 2. 自定义提供者实现

```csharp
public class MyCustomProvider : ISaveArchiveSettingProvider
{
    public bool EnableEncrypt => GetEncryptFromMySource();
    public bool IsOpenDoubleWrite => GetDoubleWriteFromMySource();

    public IArchiveSetting GetArchiveSetting(string groupName, bool onlySpecial = false)
    {
        // 从自定义来源（JSON、数据库、远程服务等）加载配置
        return LoadSettingFromMySource(groupName, onlySpecial);
    }

    public void Init()
    {
        // 初始化逻辑
    }
}
```

### 3. 注册自定义提供者

```csharp
// 在游戏启动时注册
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
public static void RegisterCustomProvider()
{
    var customProvider = new MyCustomProvider();
    SaveArchiveSettingProvider.SetProvider(customProvider);
}
```

## 应用场景

### 1. 不同平台使用不同配置
```csharp
#if UNITY_MOBILE
    SaveArchiveSettingProvider.SetProvider(new MobileOptimizedProvider());
#elif UNITY_STANDALONE
    SaveArchiveSettingProvider.SetProvider(new DesktopProvider());
#endif
```

### 2. 从远程服务器加载配置
```csharp
public class RemoteConfigProvider : ISaveArchiveSettingProvider
{
    public async void Init()
    {
        // 从服务器异步加载配置
        var config = await LoadConfigFromServer();
        ApplyConfig(config);
    }
}
```

### 3. 动态配置切换
```csharp
// 运行时切换配置（如测试环境 vs 生产环境）
if (isTestMode)
{
    SaveArchiveSettingProvider.SetProvider(new TestModeProvider());
}
```

### 4. 项目特定的配置逻辑
```csharp
public class ProjectSpecificProvider : ISaveArchiveSettingProvider
{
    public IArchiveSetting GetArchiveSetting(string groupName, bool onlySpecial = false)
    {
        // 根据项目业务逻辑返回不同配置
        if (groupName.StartsWith("Critical"))
        {
            return new ArchiveSetting(ArchiveOperationType.Nino, isForceSaveSync: true);
        }
        return defaultSetting;
    }
}
```

## 迁移指南

### 对于现有项目
- **无需修改任何代码**，系统会自动使用默认提供者
- 现有的 `SaveArchiveSettingSO` 资源文件继续有效

### 对于需要自定义的项目
1. 实现 `ISaveArchiveSettingProvider` 接口
2. 在适当时机调用 `SaveArchiveSettingProvider.SetProvider()`
3. 可选：移除对 `SaveArchiveSettingSO` 的直接依赖

## 注意事项

1. **线程安全**: `SaveArchiveSettingProvider` 使用了线程安全的单例模式
2. **初始化时机**: 建议在 `RuntimeInitializeOnLoadMethod` 中注册自定义提供者
3. **向后兼容**: 默认行为与原来完全一致，不会影响现有项目
4. **性能**: 提供者模式的性能开销极小，不会影响运行时性能

## 示例代码

参考 `CustomSaveArchiveSettingProvider.cs` 中的完整示例实现。
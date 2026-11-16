# Unity 多平台 SDK 接入框架

一套面向 Unity 的多渠道 SDK 接入层，用**统一的 C# 接口**封装 Android、iOS、PC、WebGL 四端的登录、支付、账号、广告与数据上报能力。游戏层只依赖 `SDKHelper`，切换渠道时只需替换 `SDKConfig` 下的配置，无需改动业务代码。

- Unity 版本：**2022.3.62f2**（[ProjectVersion.txt](ProjectSettings/ProjectVersion.txt)）
- 支持平台：Android / iOS / Windows(PC) / WebGL，编辑器内自动降级为通用实现

## 特性

| 能力 | 说明 |
|---|---|
| 统一接口 | 23 个接口覆盖初始化、登录、登出、切换账号、支付、广告、账号中心、上报、元数据查询 |
| 平台隔离 | `#if UNITY_ANDROID` / `UNITY_IOS` / `UNITY_WEBGL` / `UNITY_STANDALONE` 编译隔离，互不干扰 |
| 回调统一 | 原生侧只回调一条 JSON 消息，C# 侧解析后分发给注册的委托 |
| 配置驱动 | 渠道文件放在 `SDKConfig/`，打包前自动镜像到 `Assets/Plugins`，不进版本库的中间态 |
| 一键自测 | `SDKTestPanel` 纯代码构建的调试面板，打任意平台的包都能用同一套按钮验收 |

## 目录结构

```text
Assets/
├─ Scripts/
│  ├─ SDK/
│  │  ├─ SDKHelper.cs              # 单例入口：平台实现挂载、消息分发、对外 API
│  │  └─ SdkHelper/
│  │     ├─ SDKParent.cs           # 抽象基类，定义全部可重写接口（默认空实现）
│  │     ├─ AndroidSDKParent.cs    # Android：AndroidJavaClass 反射调用 Java 层
│  │     ├─ IOSSDKParent.cs        # iOS：[DllImport("__Internal")] 调用 ObjC 桥接
│  │     ├─ PCSDKParent.cs         # PC：直接 C# 调用（含窗口嵌入等 user32 API）
│  │     ├─ WebGLSDKParent.cs      # WebGL：jslib 桥接
│  │     └─ CommonSDKHelper.cs     # 未接 SDK / 编辑器环境的兜底实现
│  └─ SDKTest/
│     └─ SDKTestPanel.cs           # 接入演示与自测面板
├─ Editor/
│  ├─ SDKConfigImporter.cs         # 打包前把 SDKConfig 模板同步到 Assets/Plugins
│  └─ iOSBuildPostprocessor.cs     # 导出 Xcode 工程后覆盖 Info.plist、配置链接参数
├─ Addones/LitJson/                # JSON 库（第三方）
├─ Plugins/                        # 各平台原生插件（由 SDKConfig 同步生成）
└─ Scenes/                         # SampleScene、SDKTest 演示场景

SDKConfig/                         # 渠道配置模板（版本库内维护，按平台分目录）
├─ Android/                        # AndroidManifest.xml、mainTemplate.gradle、Java 源码、libs
├─ iOS/                            # Info.plist、setting.json、ObjC 桥接源码
└─ Webgl_All/                      # jslib、WebGL 模板（index.html、样式、JS 桥接）
```

## 架构

```text
游戏代码
   ↓  只与这一层交互
SDKHelper (MonoBehaviour 单例, DontDestroyOnLoad)
   ↓  Awake 时按编译宏挂载唯一的平台实现
SDKParent (抽象接口, 全部 virtual 空实现)
   ↓
Android / iOS / PC / WebGL / Common
   ↓  原生回调 OnMessage("event::json")
SDKHelper 解析 → MSG_LOGIN_SUCCESS / MSG_PAY_SUCCESS / ... → 业务委托
```

### 快速接入

1. 在启动场景中挂载 `SDKHelper`（或由首个场景的对象持有）。
2. 注册回调并调用接口：

```csharp
void Start()
{
    SDKHelper.instance.SetLoginHandler(
        json => Debug.Log("登录成功: " + json),
        msg  => Debug.Log("登录失败: " + msg));

    SDKHelper.instance.Login();
}
```

### 回调事件

`SDKHelper` 中的消息常量即原生层可回传的事件名：

`loginSuccess`、`loginFail`、`loginCancel`、`logout`、`paySuccess`、`payFail`、`onSwitchAccount`、`onSdkExit`、`ADCallBack`、`sdkLog`

### 数据上报类型

`SubmitExtraData(data, type)` 的 `type` 取值：`1` 选择服务器、`2` 创建角色、`3` 进入游戏、`4` 等级提升、`5` 退出游戏。

### 实现新渠道

1. 在 `Assets/Scripts/SDK/SdkHelper/` 下新建 `XXXSDKParent : SDKParent`，按需 `override` 接口。
2. 在 `SDKHelper.Awake()` 的对应平台宏分支里替换为 `XXXSDKParent`。
3. 把渠道配置放到 `SDKConfig/` 对应目录，并在 `SDKConfigImporter` 的 `Rules` 中补充拷贝规则。

## 渠道配置（SDKConfig）

`SDKConfig/` 是**配置源**，`Assets/Plugins/`、`Assets/WebGLTemplates/` 中的对应文件是**打包时的产物**，由编辑器脚本自动镜像，不要手工修改产物。

菜单 **Tools → SDKConfig**：

| 菜单项 | 作用 |
|---|---|
| 同步当前平台 | 按当前 Build Target 执行同步 |
| 同步 Android / 同步 iOS / 同步 WebGL | 指定平台同步，便于提前检查 |
| 清理已同步文件 | 依据 `ProjectSettings/SDKConfigSync.manifest` 删除上次同步的全部文件 |
| 打包前自动同步 | 开关：Build / Build And Run 时自动同步（默认开启） |

同步流程是幂等的：先清理上次同步记录的文件 → 移除其它平台的同名模板 → 拷贝当前平台文件。切换 Build Target 时也会自动同步。

**iOS** 额外由 `iOSBuildPostprocessor` 在导出 Xcode 工程后处理，`SDKConfig/iOS/setting.json` 支持以下字段（均可缺省）：

| 字段 | 作用 |
|---|---|
| `libs` | → `OTHER_LDFLAGS -lxxx`（`.a` / `.tbd` / `.dylib`） |
| `frameworks` | → Frameworks Build Phase |
| `linker_flags` | → `OTHER_LDFLAGS`（同时作用于两个 target） |
| `compiler_flags` | → `SDKConfig/iOS/plugins` 下 `.m` / `.mm` 的编译参数 |
| `build_setting` | → 逐项 `SetBuildProperty` |
| `plist` | 使用的 Info.plist 路径，缺省为同级 `Info.plist` |

## 构建

1. 用 Unity 2022.3.62f2 打开工程。
2. **File → Build Settings** 选择目标平台并 Switch Platform（会自动同步该平台配置）。
3. 打包前确认 **Tools → SDKConfig → 打包前自动同步** 已勾选。
4. Build。产物输出到 `output/`，其中**已编译好的可测试包会纳入版本库**（见 [output/README.md](output/README.md)）。

各平台前置条件：

- **Android**：启用 `Custom Main Manifest` / `Custom Main Gradle Template` / `Custom Gradle Properties Template`，配置才会生效；JDK、SDK 路径在 Preferences 中设置。
- **iOS**：需 macOS + Xcode，导出后由后处理自动写入 `Info.plist` 与链接参数。
- **WebGL**：Player Settings 的 Resolution and Presentation 中选择 `Webgl_All` 模板。
- **PC**：直接 `UNITY_STANDALONE` 构建，SDK 逻辑为原生 C#。

## 测试

打开 `Assets/Scenes/SDKTest.unity`，运行后即可使用自测面板：左侧为各接口按钮，右侧实时显示回调 JSON 与调用日志。打真机包后使用同一面板即可验证渠道接入。

### 免打包直接体验

仓库内已附编译好的可测试包，无需配置 Unity/iOS/Android 环境：

| 平台 | 文件 | 用法 |
|---|---|---|
| Android | `output/and/sdk.apk`（22 MB） | `adb install -r output/and/sdk.apk` |
| Windows | `output/pc/SDK.exe` | 双击运行（需整个 `pc/` 目录） |

包内为默认桥接实现，回调为模拟数据，仅供联调，详见 [output/README.md](output/README.md)。

## 版本库约定

`.gitignore` 已排除 Unity 自动生成的内容，克隆后用 Unity 打开会自动重建：

- 忽略：`Library/`、`Temp/`、`Logs/`、`obj/`、`UserSettings/`、`.vs/`、`*.csproj`、`*.sln`、`*.apk`（`output/` 除外）等
- 提交：`Assets/`（**含 `.meta`**）、`Packages/`、`ProjectSettings/`、`SDKConfig/`、`output/`（可测试构建包）

> 注意：`Assets/**` 的 `.meta` 文件必须一并提交，否则场景与资源引用会全部丢失。

## 第三方

- [LitJson](Assets/Addones/LitJson)（JSON 序列化）
- `SDKConfig/` 内的渠道桥接 JS 与配置模板由对应渠道方（如 4399、QQ 空间）提供，版权归渠道方所有。

### 未包含在仓库中的文件

以下渠道方保密文件不纳入本仓库（已在 `.gitignore` 中排除），克隆后需自行向渠道方获取并放回原位，否则 WebGL 的 4399 / QQ 空间渠道无法构建：

```text
SDKConfig/Webgl_All/template/js/4399/            # 整个目录（4399SDK.js / 4399SDKHelper.js / jquery.min.js）
SDKConfig/Webgl_All/template/js/qqZone/qqZoneSDKHelper.js
```

> 运行时 `dynamic_loader.js` 会加载 `js/4399/4399SDKHelper.js` 与 `js/qqZone/qqZoneSDKHelper.js`，缺失时这两个渠道报 404，其余平台不受影响。

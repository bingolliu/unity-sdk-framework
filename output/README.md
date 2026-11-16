# 可测试构建包

已编译好的、可直接运行验证 SDK 接口的构建产物，随仓库提供，克隆后无需本地打包即可试跑。

| 平台 | 路径 | 体积 | 用法 |
|---|---|---|---|
| Android | [and/sdk.apk](and/sdk.apk) | 22.01 MB | `adb install -r output/and/sdk.apk` |
| Windows (PC) | [pc/SDK.exe](pc/SDK.exe) | 651 KB（另需同目录依赖） | 双击运行 |

> PC 版依赖同目录的 `UnityPlayer.dll`、`SDK_Data/`、`MonoBleedingEdge/`、`UnityCrashHandler64.exe`，**必须整个 `pc/` 目录一起拷走**，单独复制 exe 无法启动。

## 构建信息

| 项 | 值 |
|---|---|
| Unity 版本 | 2022.3.62f2 |
| 脚本后端 | Mono（PC 版含 `MonoBleedingEdge/`） |
| 应用名 / 公司 | SDK / DefaultCompany |
| 启动场景 | `Assets/Scenes/SDKTest.unity`（Build Settings 第 0 个），其后为 `SampleScene.unity` |
| Android minSdkVersion | 22 |
| 构建时间 | 2026-11-16 |

## 验证方式

两个平台启动后都会进入 **SDKTest 自测面板**：左侧按钮调用 `SDKHelper` 的各接口，右侧实时显示回调 JSON 与调用日志（`sdkLog`）。

按面板上对应的按钮即可验证登录、登出、切换账号、支付、广告、上报等链路是否连通。

## 重要说明

- 包内是**默认桥接实现**（`com.defComp.game.sdk`），**未接入 4399 / QQ 等真实渠道**。登录、支付等回调返回的是模拟数据，仅用于验证 Unity 与原生层之间的调用链路。
- **不要用于生产或分发给玩家**，仅供联调自测。
- 4399 / QQ 渠道的桥接 JS 因授权原因不在仓库中（见[根目录 README](../README.md#未包含在仓库中的文件)），补齐前无法打出这两个 WebGL 渠道包。
- 本目录内容由重新打包生成，请勿手工修改；更新请重新 Build 后覆盖。

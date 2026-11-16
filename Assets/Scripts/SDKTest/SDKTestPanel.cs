using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// SDK 接入演示 / 自测面板。
///
/// 界面完全由代码构建，不依赖预制体：把本脚本挂到任意场景物体上，
/// 运行后左侧是 SDK 接口按钮，右侧是回调与返回值日志。
/// 打不同平台的包后，用同一套按钮即可验证各渠道的接入是否正常。
/// </summary>
public class SDKTestPanel : MonoBehaviour
{
    private const string PAY_JSON =
        "{\"cpOrderId\":\"10001\",\"itemId\":1,\"itemName\":\"元宝\",\"itemDes\":\"60元宝\",\"price\":6,\"serverId\":1,\"callbackUrl\":\"\",\"extension\":\"\"}";

    private const string ROLE_JSON =
        "{\"serverID\":1,\"serverName\":\"测试服\",\"roleID\":\"10001\",\"roleName\":\"测试角色\",\"roleLevel\":10,\"vip\":\"0\",\"power\":1000}";

    private const string PLAYER_JSON =
        "{\"roleID\":\"10001\",\"roleName\":\"测试角色\",\"roleLevel\":10,\"serverID\":1,\"serverName\":\"测试服\"}";

    private const float ButtonWidth = 264f;
    private const float ButtonHeight = 44f;
    private const float ButtonStep = 54f;

    private Font _font;
    private RectTransform _root;
    private ScrollRect _logScroll;
    private Text _logText;
    private Text _infoText;
    private readonly List<string> _lines = new List<string>();

    private void Start()
    {
        EnsureSdkHelper();
        _font = LoadFont();
        BuildUI();
        RegisterSdkCallbacks();
        Log("测试面板已启动，平台: " + Application.platform);
        LogSdkInfo();
    }

    // ---------------------------------------------------------------- 初始化

    private static void EnsureSdkHelper()
    {
        if (SDKHelper.instance != null)
        {
            return;
        }

        // 场景里没有 SDKHelper 时自动创建，Awake 里会按平台挂上对应的 SDKParent 实现
        GameObject go = new GameObject("SDKHelper");
        go.AddComponent<SDKHelper>();
        DontDestroyOnLoad(go);
    }

    private static Font LoadFont()
    {
        Font font = null;
        try
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        catch (Exception)
        {
            // Unity 2022.1 及更早版本没有 LegacyRuntime.ttf
        }

        if (font == null)
        {
            try
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            catch (Exception)
            {
                // 忽略，下面兜底
            }
        }

        if (font == null)
        {
            font = Font.CreateDynamicFontFromOSFont("Arial", 18);
        }

        return font;
    }

    private void RegisterSdkCallbacks()
    {
        SDKHelper sdk = SDKHelper.instance;
        if (sdk == null)
        {
            Log("[错误] SDKHelper 未初始化");
            return;
        }

        sdk.SetLoginHandler(
            data => Log("[回调] loginSuccess: " + data),
            data => Log("[回调] loginFail: " + data));
        sdk.SetLoginCancel(data => Log("[回调] loginCancel: " + data));
        sdk.SetCommonMsgHandler(data => Log("[回调] commonMsg: " + data));
        sdk.SetSdkLogHandler(data => Log("[SDK] " + data));
    }

    // ---------------------------------------------------------------- 界面

    private void BuildUI()
    {
        GameObject canvasGo = new GameObject("SDKTestCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);

        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        _root = canvasGo.GetComponent<RectTransform>();

        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemGo = new GameObject("SDKTestEventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystemGo.transform.SetParent(transform, false);
        }

        RectTransform background = CreateRect("Background", _root, Vector2.zero, Vector2.zero);
        background.anchorMin = Vector2.zero;
        background.anchorMax = Vector2.one;
        background.offsetMin = Vector2.zero;
        background.offsetMax = Vector2.zero;
        Image backgroundImage = background.gameObject.AddComponent<Image>();
        backgroundImage.color = new Color(0.07f, 0.08f, 0.1f, 1f);

        CreateLabel("Title", "SDK 接入演示 / 自测面板", new Vector2(20f, -14f), new Vector2(900f, 40f), 30,
            TextAnchor.MiddleLeft, new Color(1f, 0.85f, 0.4f));
        _infoText = CreateLabel("Info", "", new Vector2(20f, -56f), new Vector2(1600f, 28f), 18,
            TextAnchor.MiddleLeft, new Color(0.7f, 0.85f, 1f));

        CreateLabel("SectionSdk", "SDK 方法", new Vector2(20f, -94f), new Vector2(400f, 26f), 20,
            TextAnchor.MiddleLeft, new Color(0.55f, 1f, 0.55f));

        float row = -132f;

        // 第一列：账号 / 支付 / 其它
        CreateButton("登录 Login", 20f, row, OnLogin);
        CreateButton("自动登录 AutoLogin", 20f, row -= ButtonStep, OnAutoLogin);
        CreateButton("登出 LogOut", 20f, row -= ButtonStep, OnLogOut);
        CreateButton("切换账号 SwitchLogin", 20f, row -= ButtonStep, OnSwitchLogin);
        CreateButton("支付 Pay", 20f, row -= ButtonStep, OnPay);
        CreateButton("广告 CallAD", 20f, row -= ButtonStep, OnCallAD);
        CreateButton("个人中心 ShowAccountCenter", 20f, row -= ButtonStep, OnShowAccountCenter);
        CreateButton("退出流程 DoExit", 20f, row -= ButtonStep, OnDoExit);
        CreateButton("设置玩家信息 SetPlayerInfo", 20f, row -= ButtonStep, OnSetPlayerInfo);
        CreateButton("查询 SDK 信息", 20f, row -= ButtonStep, OnQueryInfo);

        // 第二列：数据上报 / 其它信息读取
        row = -132f;
        CreateButton("上报·选择服务器", 294f, row, () => OnSubmitExtraData(SDKHelper.TYPE_SELECT_SERVER));
        CreateButton("上报·创建角色", 294f, row -= ButtonStep, () => OnSubmitExtraData(SDKHelper.TYPE_CREATE_ROLE));
        CreateButton("上报·进入游戏", 294f, row -= ButtonStep, () => OnSubmitExtraData(SDKHelper.TYPE_ENTER_GAME));
        CreateButton("上报·等级提升", 294f, row -= ButtonStep, () => OnSubmitExtraData(SDKHelper.TYPE_LEVEL_UP));
        CreateButton("上报·退出游戏", 294f, row -= ButtonStep, () => OnSubmitExtraData(SDKHelper.TYPE_EXIT_GAME));
        CreateButton("GetMeta(PrivacyURL)", 294f, row -= ButtonStep, () => OnGetMeta("PrivacyURL"));
        CreateButton("GetMeta(PrivacyDefault)", 294f, row -= ButtonStep, () => OnGetMeta("PrivacyDefault"));
        CreateButton("CallStringMethod 示例", 294f, row -= ButtonStep, OnCallStringMethod);
        CreateButton("退出游戏 Quit()", 294f, row -= ButtonStep, OnQuitGame);
        CreateButton("模拟 onSdkExit", 294f, row -= ButtonStep, OnSimulateSdkExit);

        // 模拟原生回调：不接渠道时也能演示整条回调链路
        CreateLabel("SectionSim", "模拟原生回调（cmd + \"::\" + content）", new Vector2(20f, -700f), new Vector2(700f, 26f), 20,
            TextAnchor.MiddleLeft, new Color(1f, 0.75f, 0.55f));

        row = -738f;
        CreateButton("模拟 loginSuccess", 20f, row, () => Simulate(SDKHelper.MSG_LOGIN_SUCCESS, "{\"uid\":\"10001\",\"token\":\"test_token\"}"));
        CreateButton("模拟 loginFail", 20f, row -= ButtonStep, () => Simulate(SDKHelper.MSG_LOGIN_FAIL, "test fail"));
        CreateButton("模拟 loginCancel", 20f, row -= ButtonStep, () => Simulate(SDKHelper.MSG_LOGIN_CANCEL, ""));
        CreateButton("模拟 paySuccess", 20f, row -= ButtonStep, () => Simulate(SDKHelper.MSG_PAY_SUCCESS, "{\"orderId\":\"10001\"}"));

        row = -738f;
        CreateButton("模拟 payFail", 294f, row, () => Simulate(SDKHelper.MSG_PAY_FAIL, "test fail"));
        CreateButton("模拟 onSwitchAccount", 294f, row -= ButtonStep, () => Simulate(SDKHelper.MSG_SWITCH_ACCOUNT, ""));
        CreateButton("模拟 ADCallBack", 294f, row -= ButtonStep, () => Simulate(SDKHelper.MSG_AD_CALLBACK, "1"));
        CreateButton("清空日志", 294f, row -= ButtonStep, ClearLog);

        BuildLogPanel();
    }

    private void BuildLogPanel()
    {
        RectTransform logRoot = CreateRect("Log", _root, new Vector2(580f, -14f), new Vector2(1320f, 970f));
        Image logBackground = logRoot.gameObject.AddComponent<Image>();
        logBackground.color = new Color(0f, 0f, 0f, 0.55f);

        _logScroll = logRoot.gameObject.AddComponent<ScrollRect>();

        RectTransform viewport = CreateRect("Viewport", logRoot, Vector2.zero, Vector2.zero);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(10f, 10f);
        viewport.offsetMax = new Vector2(-10f, -10f);
        viewport.gameObject.AddComponent<RectMask2D>();

        RectTransform content = CreateRect("Content", viewport, Vector2.zero, new Vector2(0f, 100f));
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0f, 1f);
        content.anchoredPosition = Vector2.zero;

        _logText = content.gameObject.AddComponent<Text>();
        _logText.font = _font;
        _logText.fontSize = 18;
        _logText.alignment = TextAnchor.UpperLeft;
        _logText.color = new Color(0.9f, 0.95f, 1f);
        _logText.horizontalOverflow = HorizontalWrapMode.Wrap;
        _logText.verticalOverflow = VerticalWrapMode.Overflow;
        _logText.supportRichText = false;

        ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        _logScroll.viewport = viewport;
        _logScroll.content = content;
        _logScroll.horizontal = false;
        _logScroll.vertical = true;
        _logScroll.movementType = ScrollRect.MovementType.Clamped;
        _logScroll.scrollSensitivity = 40f;
    }

    private RectTransform CreateRect(string name, Transform parent, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rect = (RectTransform)go.transform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return rect;
    }

    private Text CreateLabel(string name, string content, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor anchor, Color color)
    {
        RectTransform rect = CreateRect(name, _root, anchoredPosition, size);
        Text text = rect.gameObject.AddComponent<Text>();
        text.font = _font;
        text.fontSize = fontSize;
        text.alignment = anchor;
        text.color = color;
        text.text = content;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.supportRichText = false;
        return text;
    }

    private Button CreateButton(string label, float x, float y, UnityAction onClick)
    {
        RectTransform rect = CreateRect("Btn_" + label, _root, new Vector2(x, y), new Vector2(ButtonWidth, ButtonHeight));

        Image image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(0.16f, 0.35f, 0.58f, 1f);

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        RectTransform textRect = CreateRect("Text", rect, Vector2.zero, Vector2.zero);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text text = textRect.gameObject.AddComponent<Text>();
        text.font = _font;
        text.fontSize = 18;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = label;
        text.supportRichText = false;
        return button;
    }

    // ---------------------------------------------------------------- SDK 调用

    private SDKParent GetSdkParent()
    {
        SDKHelper sdk = SDKHelper.instance;
        if (sdk == null)
        {
            Log("[错误] SDKHelper 未初始化");
            return null;
        }

        if (sdk.sdkParent == null)
        {
            Log("[错误] SDKParent 未初始化，检查 SDKHelper.Awake 的平台分支");
            return null;
        }

        return sdk.sdkParent;
    }

    private void OnLogin()
    {
        Log("调用 Login()");
        SDKHelper sdk = SDKHelper.instance;
        if (sdk == null)
        {
            Log("[错误] SDKHelper 未初始化");
            return;
        }

        sdk.Login();
    }

    private void OnAutoLogin()
    {
        SDKParent parent = GetSdkParent();
        if (parent == null)
        {
            return;
        }

        Log("调用 AutoLogin()");
        parent.AutoLogin();
    }

    private void OnLogOut()
    {
        SDKHelper sdk = SDKHelper.instance;
        if (sdk == null)
        {
            Log("[错误] SDKHelper 未初始化");
            return;
        }

        Log("调用 LogOut()");
        sdk.LogOut();
    }

    private void OnSwitchLogin()
    {
        SDKHelper sdk = SDKHelper.instance;
        if (sdk == null)
        {
            Log("[错误] SDKHelper 未初始化");
            return;
        }

        Log("调用 SwitchLogin()");
        sdk.SwitchLogin(data => Log("[回调] onSwitchAccount: " + data));
    }

    private void OnPay()
    {
        SDKHelper sdk = SDKHelper.instance;
        if (sdk == null)
        {
            Log("[错误] SDKHelper 未初始化");
            return;
        }

        Log("调用 Pay() 订单: " + PAY_JSON);
        sdk.Pay(PAY_JSON,
            data => Log("[回调] paySuccess: " + data),
            data => Log("[回调] payFail: " + data));
    }

    private void OnCallAD()
    {
        SDKHelper sdk = SDKHelper.instance;
        if (sdk == null)
        {
            Log("[错误] SDKHelper 未初始化");
            return;
        }

        Log("调用 CallAD()");
        sdk.CallAD(data => Log("[回调] ADCallBack: " + data));
    }

    private void OnShowAccountCenter()
    {
        SDKHelper sdk = SDKHelper.instance;
        if (sdk == null)
        {
            Log("[错误] SDKHelper 未初始化");
            return;
        }

        Log("调用 ShowAccountCenter()");
        sdk.ShowAccountCenter();
    }

    private void OnDoExit()
    {
        SDKParent parent = GetSdkParent();
        if (parent == null)
        {
            return;
        }

        Log("调用 DoExit()");
        parent.DoExit();
    }

    private void OnSetPlayerInfo()
    {
        SDKHelper sdk = SDKHelper.instance;
        if (sdk == null)
        {
            Log("[错误] SDKHelper 未初始化");
            return;
        }

        Log("调用 SetPlayerInfo(): " + PLAYER_JSON);
        sdk.SetPlayerInfo(PLAYER_JSON);
    }

    private void OnSubmitExtraData(int type)
    {
        SDKHelper sdk = SDKHelper.instance;
        if (sdk == null)
        {
            Log("[错误] SDKHelper 未初始化");
            return;
        }

        Log("调用 SubmitExtraData(type=" + type + "): " + ROLE_JSON);
        sdk.SubmitExtraData(ROLE_JSON, type);
    }

    private void OnGetMeta(string key)
    {
        SDKHelper sdk = SDKHelper.instance;
        if (sdk == null)
        {
            Log("[错误] SDKHelper 未初始化");
            return;
        }

        string value = sdk.GetMeta(key);
        Log("GetMeta(\"" + key + "\") = " + (value ?? "null"));
    }

    private void OnCallStringMethod()
    {
        SDKHelper sdk = SDKHelper.instance;
        if (sdk == null)
        {
            Log("[错误] SDKHelper 未初始化");
            return;
        }

        const string className = "com.defComp.game.plugins.MainActivity";
        const string methodName = "GetSubChannelID";
        Log("调用 CallStringMethod(\"" + className + "\", \"" + methodName + "\")");
        string result = sdk.CallStringMethod(className, methodName, "");
        Log("CallStringMethod 返回: " + (result ?? "null"));
    }

    private void OnQueryInfo()
    {
        LogSdkInfo();
    }

    /// <summary>直接退出游戏（Editor 下停止 Play 模式）</summary>
    private void OnQuitGame()
    {
        Log("退出游戏：Application.Quit()" + (Application.isEditor ? "（Editor 下停止 Play）" : ""));
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>模拟渠道点击退出确认后的 onSdkExit 回调（走 SDKHelper 的退出流程）</summary>
    private void OnSimulateSdkExit()
    {
        Simulate(SDKHelper.MSG_SDK_EXIT, "");
    }

    private void LogSdkInfo()
    {
        SDKHelper sdk = SDKHelper.instance;
        if (sdk == null)
        {
            Log("[错误] SDKHelper 未初始化");
            return;
        }

        string parentName = sdk.sdkParent == null ? "null" : sdk.sdkParent.GetType().Name;
        if (_infoText != null)
        {
            _infoText.text = "平台: " + Application.platform + "    SDKHelper: SDKHelper    SDKParent: " + parentName;
        }

        Log("--- SDK 信息 ---");
        Log("SDKParent 实现: " + parentName);
        Log("GetSDKVersionCode(): " + sdk.GetSDKVersionCode());
        Log("GetSDKToken(): " + sdk.GetSDKToken());
        Log("GetUserId(): " + sdk.GetUserId());
        Log("GetUserName(): " + sdk.GetUserName());
        Log("GetSdkExtension(): " + sdk.GetSdkExtension());
        Log("GetSubChannelId(): " + sdk.GetSubChannelId());
        Log("GetCurChannelSubName(): " + sdk.GetCurChannelSubName());
        Log("GetGameStr(): " + sdk.GetGameStr());
        Log("GetMeta(\"PrivacyDefault\"): " + sdk.GetMeta("PrivacyDefault"));
    }

    private void Simulate(string cmd, string content)
    {
        SDKHelper sdk = SDKHelper.instance;
        if (sdk == null)
        {
            Log("[错误] SDKHelper 未初始化");
            return;
        }

        string message = cmd + SDKHelper.MSG_GAP + content;
        Log("模拟原生回调: " + message);
        sdk.OnMessage(message);
    }

    // ---------------------------------------------------------------- 日志

    private void Log(string message)
    {
        string line = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message;
        _lines.Add(line);
        if (_lines.Count > 200)
        {
            _lines.RemoveAt(0);
        }

        Debug.Log("[SDKTest] " + message);

        if (_logText == null)
        {
            return;
        }

        _logText.text = string.Join("\n", _lines.ToArray());
        Canvas.ForceUpdateCanvases();
        if (_logScroll != null)
        {
            _logScroll.verticalNormalizedPosition = 0f;
        }
    }

    private void ClearLog()
    {
        _lines.Clear();
        if (_logText != null)
        {
            _logText.text = "";
        }

        Log("日志已清空");
    }
}

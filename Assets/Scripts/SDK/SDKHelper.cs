using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using LitJson;

public class SDKHelper : MonoBehaviour
{
    public const string MSG_LOGIN_SUCCESS = "loginSuccess";
    public const string MSG_LOGIN_FAIL = "loginFail";
    public const string MSG_LOGIN_CANCEL = "loginCancel";
    public const string MSG_LOGOUT = "logout";
    public const string MSG_PAY_SUCCESS = "paySuccess";
    public const string MSG_PAY_FAIL = "payFail";
    public const string MSG_SWITCH_ACCOUNT = "onSwitchAccount";
    public const string MSG_SDK_EXIT = "onSdkExit";
    public const string MSG_AD_CALLBACK = "ADCallBack";
    /// <summary>平台实现打印日志用，内容会直接输出到 SDKTestPanel</summary>
    public const string MSG_LOG = "sdkLog";
    public const string MSG_GAP = "::";

    public const int TYPE_SELECT_SERVER = 1; // 选择服务器
    public const int TYPE_CREATE_ROLE = 2; // 创建角色 public static
    public const int TYPE_ENTER_GAME = 3; // 进入游戏
    public const int TYPE_LEVEL_UP = 4; // 等级提升
    public const int TYPE_EXIT_GAME = 5; // 退出游戏

    private Action<string> _adCallBack;
    private Action<string> _msgLoginSuccess;
    private Action<string> _msgLoginFail;
    private Action<string> _msgLoginCancel;
    private Action<string> _paySuccess;
    private Action<string> _payFail;
    private Action<string> _switchAccount;
    private Action<string> _msgCommonCB;
    private Action<string> _sdkLog;

    private static SDKHelper _instance;
    public static SDKHelper instance { get { return _instance; } }

    private SDKParent _sdkParent;
    public SDKParent sdkParent { get { return _sdkParent; } }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);
#if UNITY_EDITOR
            _sdkParent = this.gameObject.AddComponent<CommonSDKHelper>();
#elif UNITY_ANDROID
            _sdkParent = this.gameObject.AddComponent<AndroidSDKParent>();
#elif UNITY_IOS
            _sdkParent = this.gameObject.AddComponent<IOSSDKParent>();
#elif UNITY_WEBGL
            _sdkParent = this.gameObject.AddComponent<WebGLSDKParent>();
#elif UNITY_STANDALONE
            // pc的sdk直接根据平台在这里继承PCSDKParent 后重写里面的方法就可以了,因为pc的sdk的真实调用就是用的C#
            _sdkParent = this.gameObject.AddComponent<PCSDKParent>();
#endif
            // 兜底：没有匹配到任何平台实现时使用通用 helper
            if (_sdkParent == null)
            {
                _sdkParent = this.gameObject.AddComponent<CommonSDKHelper>();
            }
        }
        else
        {
            Destroy(this);
        }
    }

    public void SetLoginHandler(Action<string> loginSuccess, Action<string> loginFail)
    {
        _msgLoginSuccess = loginSuccess;
        _msgLoginFail = loginFail;
    }

    public void SetLoginCancel(Action<string> loginCancel)
    {
        _msgLoginCancel = loginCancel;
    }

    public void SetCommonMsgHandler(Action<string> commonMsg)
    {
        _msgCommonCB = commonMsg;
    }

    /// <summary>注册 SDK 日志回调，SDKTestPanel 用它把各平台实现的日志显示在界面上</summary>
    public void SetSdkLogHandler(Action<string> handler)
    {
        _sdkLog = handler;
    }

    /// <summary>
    /// 各平台实现调用它打日志：会同时输出到 Console 和已注册的日志回调（SDKTestPanel）。
    /// 原生侧也可以通过 OnMessage("sdkLog::xxx") 走到这里。
    /// </summary>
    public static void LogSdk(string message)
    {
        if (_instance != null)
        {
            _instance._OnSdkLog(message);
            return;
        }

        Debug.Log("[SDK] " + message);
    }
    #region call sdk function
    public void Login()
    {
        _sdkParent.Login();
    }

    public void LogOut()
    {
        _sdkParent.LogOut();
    }

    public string GetSDKVersionCode()
    {
        return _sdkParent.GetSDKVersionCode();
    }

    public string GetSDKToken()
    {
        return _sdkParent.GetSDKToken();
    }

    public string GetUserId()
    {
        return _sdkParent.GetUserId();
    }

    public string GetUserName()
    {
        return _sdkParent.GetUserName();
    }

    public string GetGameStr()
    {
        return _sdkParent.GetGameStr();
    }

    public void SubmitExtraData(string data, int type)
    {
        _sdkParent.SubmitExtraData(data, type);
    }

    public void SetPlayerInfo(string data)
    {
        _sdkParent.SetPlayerInfo(data);
    }

    public void CallAD(Action<string> action)
    {
        _adCallBack = action;
        _sdkParent.CallAD();
    }

    public void Pay(string payData, Action<string> paySuccessCallBack, Action<string> payFailCallBack)
    {
        _paySuccess = paySuccessCallBack;
        _payFail = payFailCallBack;
        _sdkParent.Pay(payData);
    }

    public void SwitchLogin(Action<string> action)
    {
        _switchAccount = action;
        _sdkParent.SwitchAccount();
    }

    public string GetSdkExtension()
    {
        return _sdkParent.GetSdkExtension();
    }

    public int GetSubChannelId()
    {
        return _sdkParent.GetSubChannelId();
    }

    public string GetCurChannelSubName()
    {
        return _sdkParent.GetCurChannelSubName();
    }

    public virtual string GetMeta(string key)
    {
        return _sdkParent.GetMeta(key);
    }

    public string CallStringMethod(string className, string methodName, string json)
    {
        return _sdkParent.CallStringMethod(className, methodName, json);
    }

    public void ShowAccountCenter()
    {
        _sdkParent.ShowAccountCenter();
    }
    #endregion call sdk function

    #region sdk callback
    public void OnMessage(string message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            string[] msgs = System.Text.RegularExpressions.Regex.Split(message, MSG_GAP);
            if (msgs.Length > 0)
            {
                string content = msgs.Length > 1 ? msgs[1] : "";
                string cmd = msgs[0];
                switch (cmd)
                {
                    case MSG_LOGIN_SUCCESS:
                        _LoginSuccess(content);
                        break;
                    case MSG_LOGIN_CANCEL:
                        _LoginCancel(content);
                        break;
                    case MSG_LOGIN_FAIL:
                        _LoginFail(content);
                        break;
                    case MSG_PAY_SUCCESS:
                        _PaySuccess(content);
                        break;
                    case MSG_PAY_FAIL:
                        _PayFail(content);
                        break;
                    case MSG_LOGOUT:
                    case MSG_SWITCH_ACCOUNT:
                        _SwitchAccount(content);
                        break;
                    case MSG_SDK_EXIT:
                        _OnMsgSdkExit();
                        break;
                    case MSG_AD_CALLBACK:
                        _AdCallBack(content);
                        break;
                    case MSG_LOG:
                        _OnSdkLog(content);
                        break;
                    default:
                        Debug.Log(msgs[0]);
                        break;
                }
            }
        }
    }

    /// <summary>SDK 日志落地：输出到 Console，并转发给已注册的日志回调</summary>
    public void _OnSdkLog(string data)
    {
        Debug.Log("[SDK] " + data);
        if (_sdkLog != null)
        {
            _sdkLog(data);
        }
    }

    public void _SwitchAccount(string data)
    {        if (_switchAccount != null)
        {
            _switchAccount(data);
            _switchAccount = null;
        }
    }

    private void _OnMsgSdkExit()
    {
        _OnSdkLog("onSdkExit，退出游戏");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// 有Sdk回调调用
    /// </summary>
    /// <param name="data"></param>
    public void _PaySuccess(string data)
    {
        if (_paySuccess != null)
            _paySuccess(data);
    }

    /// <summary>
    /// 有Sdk回调调用
    /// </summary>
    /// <param name="data"></param>
    public void _PayFail(string data)
    {
        if (_payFail != null)
            _payFail(data);
    }

    /// <summary>
    /// 原生 SDK 可直接按方法名回调的支付成功入口
    /// </summary>
    public void PaySuccess(string data)
    {
        _PaySuccess(data);
    }

    /// <summary>
    /// 原生 SDK 可直接按方法名回调的支付失败入口
    /// </summary>
    public void PayFaild(string data)
    {
        _PayFail(data);
    }

    /// <summary>
    /// 原生 SDK 可直接按方法名回调的通用消息入口
    /// </summary>
    public void OnMsgCommonCB(string data)
    {
        if (_msgCommonCB != null)
            _msgCommonCB(data);
    }


    public void _LoginSuccess(string data)
    {
        if (_msgLoginSuccess != null)
        {
            _msgLoginSuccess(data);
        }
    }
    public void _LoginFail(string data)
    {
        if (_msgLoginFail != null)
        {
            _msgLoginFail(data);
        }
    }

    public void _LoginCancel(string data)
    {
        if (_msgLoginCancel != null)
        {
            _msgLoginCancel(data);
        }
    }

    void _AdCallBack(string data)
    {
        if (_adCallBack != null)
            _adCallBack(data);
    }
    #endregion sdk callback
}

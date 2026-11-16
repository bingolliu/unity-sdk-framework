using UnityEngine;
using System.Runtime.InteropServices;

public class IOSSDKParent : SDKParent
{
#if UNITY_IOS && !UNITY_EDITOR

    #region native interface
    // 方法名 / 参数 / 返回值必须与 SDKConfig/iOS/plugins/IOSSdkAppController.mm 中的 extern "C" 函数一致
    [DllImport("__Internal")]
    private static extern void _InitCallBackGameObject(string callBackObjectName, string callBackMethod);

    [DllImport("__Internal")]
    private static extern void _Login();

    [DllImport("__Internal")]
    private static extern void _AutoLogin();

    [DllImport("__Internal")]
    private static extern void _SwitchLogin();

    [DllImport("__Internal")]
    private static extern void _LogOut();

    [DllImport("__Internal")]
    private static extern void _DoExit();

    [DllImport("__Internal")]
    private static extern void _Pay(string data);

    [DllImport("__Internal")]
    private static extern void _SetPlayerInfo(string data);

    [DllImport("__Internal")]
    private static extern void _SubmitExtraData(string data, int type);

    [DllImport("__Internal")]
    private static extern void _ShowAccountCenter();

    [DllImport("__Internal")]
    private static extern void _CallAD();

    [DllImport("__Internal")]
    private static extern string _CallStringMethod(string className, string methodName, string json);

    [DllImport("__Internal")]
    private static extern string _GetSDKVersionCode();

    [DllImport("__Internal")]
    private static extern string _GetSDKToken();

    [DllImport("__Internal")]
    private static extern string _GetUserId();

    [DllImport("__Internal")]
    private static extern string _GetSdkUserName();

    [DllImport("__Internal")]
    private static extern string _GetSdkExtension();

    [DllImport("__Internal")]
    private static extern string _GetCurChannelSubName();

    [DllImport("__Internal")]
    private static extern string _GetGameStr();

    [DllImport("__Internal")]
    private static extern string _GetMeta(string key);

    [DllImport("__Internal")]
    private static extern int _GetSubChannelId();
    #endregion native interface

    public override void Init(string callBackObjectName)
    {
        _InitCallBackGameObject(callBackObjectName, "OnMessage");
    }

    public override void Login()
    {
        _Login();
    }

    public override void AutoLogin()
    {
        _AutoLogin();
    }

    public override void SwitchAccount()
    {
        _SwitchLogin();
    }

    public override void LogOut()
    {
        _LogOut();
    }

    public override void DoExit()
    {
        _DoExit();
    }

    public override void Pay(string payData)
    {
        _Pay(payData);
    }

    public override void SetPlayerInfo(string data)
    {
        _SetPlayerInfo(data);
    }

    public override void SubmitExtraData(string data, int type)
    {
        _SubmitExtraData(data, type);
    }

    public override void ShowAccountCenter()
    {
        _ShowAccountCenter();
    }

    public override void CallAD()
    {
        _CallAD();
    }

    public override string CallStringMethod(string className, string methodName, string json)
    {
        return _CallStringMethod(className, methodName, json);
    }

    public override string GetSDKVersionCode()
    {
        return _GetSDKVersionCode();
    }

    public override string GetSDKToken()
    {
        return _GetSDKToken();
    }

    public override string GetUserId()
    {
        return _GetUserId();
    }

    public override string GetUserName()
    {
        return _GetSdkUserName();
    }

    public override string GetSdkExtension()
    {
        return _GetSdkExtension();
    }

    public override string GetCurChannelSubName()
    {
        return _GetCurChannelSubName();
    }

    public override string GetGameStr()
    {
        return _GetGameStr();
    }

    public override string GetMeta(string key)
    {
        return _GetMeta(key);
    }

    public override int GetSubChannelId()
    {
        return _GetSubChannelId();
    }

#endif
}
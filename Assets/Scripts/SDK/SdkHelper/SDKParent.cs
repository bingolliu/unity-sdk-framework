using UnityEngine;
using System;
using System.IO;

public class SDKParent : MonoBehaviour
{
    /// <summary>统一打日志：带上具体实现类名，输出到 Console 和 SDKTestPanel</summary>
    protected void LogCall(string message)
    {
        SDKHelper.LogSdk(GetType().Name + "." + message);
    }

    protected void _OnMessage(string message)
    {
        SDKHelper.instance.OnMessage(message);
    }

    protected virtual void Awake()
    {
        LogCall("Awake()");
        DontDestroyOnLoad(this);
        Init(gameObject.name);
    }

    protected virtual void Start()
    {
    }

    public virtual void Init(string callBackObjectName)
    {
        LogCall("Init(" + callBackObjectName + ")");
    }

    public virtual void AutoLogin()
    {
        LogCall("AutoLogin()");
    }

    public virtual void Login()
    {
        LogCall("Login()");
    }

    public virtual void DoExit()
    {
        LogCall("DoExit()");
    }

    public virtual void LogOut()
    {
        LogCall("LogOut()");
    }

    //sdk logout
    public virtual void LogOutCallback()
    {
        LogCall("LogOutCallback()");
    }

    public virtual string GetSDKVersionCode()
    {
        return "";
    }

    public virtual string GetSDKToken()
    {
        return "";
    }

    public virtual string GetUserId()
    {
        return "";
    }

    public virtual string GetUserName()
    {
        return "";
    }

    public virtual void SetPlayerInfo(string data)
    {
        LogCall("SetPlayerInfo(" + data + ")");
    }

    public virtual void SubmitExtraData(string data, int type)
    {
        LogCall("SubmitExtraData(type=" + type + ", data=" + data + ")");
    }

    public virtual void Pay(string mmaiData)
    {
        LogCall("Pay(" + mmaiData + ")");
    }

    public virtual void SwitchAccount()
    {
        LogCall("SwitchAccount()");
    }

    public virtual string GetGameStr()
    {
        return "";
    }

    public virtual string GetSdkExtension()
    {
        return "";
    }

    public virtual int GetSubChannelId()
    {
        return 0;
    }

    public virtual string GetCurChannelSubName()
    {
        return "";
    }

    public virtual string CallStringMethod(string className, string methodName, string json)
    {
        LogCall("CallStringMethod(" + className + "." + methodName + ")");
        return "{\"exist\":\"false\"}";
    }

    public virtual string ReqCommonMsg(string key)
    {
        LogCall("ReqCommonMsg(" + key + ")");
        return "";
    }

    public virtual void ShowAccountCenter()
    {
        LogCall("ShowAccountCenter()");
    }

    public virtual string GetMeta(string key)
    {
        return "";
    }

    public virtual void CallAD()
    {
        LogCall("CallAD()");
    }
}
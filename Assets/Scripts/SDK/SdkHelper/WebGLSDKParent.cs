using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


public class WebGLSDKParent : SDKParent
{
#if UNITY_WEBGL && !UNITY_EDITOR

    Dictionary<string, string> @params = new Dictionary<string, string>();

    [DllImport("__Internal")]
    private static extern void InitCallBackGameObject(string callBackObjectName, string callBackMethod);

    [DllImport("__Internal")]
    private static extern void InitSdk();
    [DllImport("__Internal")]
    private static extern string UrlParmString();

    [DllImport("__Internal")]
    private static extern string _Pay(string payData);

    [DllImport("__Internal")]
    private static extern string _ReqCommonMsg(string data);

    [DllImport("__Internal")]
    private static extern void SubmitData(string data, int type);

    [DllImport("__Internal")]
    private static extern int _GetSubChannelId();


    private string UrlMsg = string.Empty;

    public override void Init(string callBackObjectName)
    {
        LogCall("Init(" + callBackObjectName + ")");
        // 把 Unity 回调对象告诉 jslib，js 侧才能把日志 / 回调送回 SDKHelper.OnMessage
        InitCallBackGameObject(callBackObjectName, "OnMessage");
    }

    protected override void Start()
    {
        @params.Clear();
        try
        {
            UrlMsg = UrlParmString();
        }
        catch (System.Exception e)
        {
            UrlMsg = "[catch]" + e.Message;
            Debug.LogError(UrlMsg);
            return;
        }
        // 解析 url 参数
        UrlMsg = UrlMsg.Replace("?", "");
        Debug.Log("url 参数: " + UrlMsg);
        string[] strings = UrlMsg.Split('&');
        foreach (string p in strings)
        {
            string[] kv = p.Split('=');
            @params.Add(kv[0], kv[1]);
        }
        InitSdk();
    }

    
    public override void AutoLogin()
    {
        if (string.IsNullOrEmpty(GetSDKToken()))
        {
            Debug.LogWarning("WebGLSDKParent.AutoLogin() Fail");
            SDKHelper.instance.OnMessage(SDKHelper.MSG_LOGIN_FAIL + SDKHelper.MSG_GAP);
        }
        else
        {
            Debug.LogWarning("WebGLSDKParent.AutoLogin() Success");
            SDKHelper.instance.OnMessage(SDKHelper.MSG_LOGIN_SUCCESS + SDKHelper.MSG_GAP);
        }
    }

    public override string GetMeta(string key)
    {
        if (@params.ContainsKey(key))
        {
            var v = @params[key];
            Debug.LogWarning("WebGLSDKParent.GetMeta = "+ key+" v = "+v);
            return @params[key];
        }
        return "";
    }

    public void SetMeta(string kv)
    {
        Debug.LogWarning("WebGLSDKParent.SetMeta()  " + kv);
        var temp = kv.Split('~');
        if (temp.Length >= 2 )
        {
            @params[temp[0]] = temp[1];
        }
    }

    public override string GetSDKToken()
    {
        if (@params.ContainsKey("token"))
        {
            return @params["token"];
        }
        return "";
    }

    public override string GetUserId()
    {
        if (@params.ContainsKey("uid"))
        {
            return @params["uid"];
        }
        return "";
    }
    
    public override void Pay(string payData)
    {
        Debug.Log("WebGLSDKHelper  Pay(string payData) : " + payData);
        _Pay(payData);
    }

    public override void SubmitExtraData(string data, int type)
    {
        Debug.Log("WebGLSDKHelper   SubmitExtraData(string data, int type) : " + data);
        SubmitData(data, type);
    }

    public override int GetSubChannelId()
    {
        
        int id = _GetSubChannelId();
        Debug.Log("WebGLSDKParent     GetSubChannelId : " + id);
        return id;
    }

    public override string ReqCommonMsg(string key)
    {
        return _ReqCommonMsg(key);
    }
    void OnDestroy()
    {
        @params.Clear();
    }
#endif
}

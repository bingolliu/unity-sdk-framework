using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;
using LitJson;

public class AndroidSDKParent : SDKParent
{

#if UNITY_ANDROID

    public AndroidJavaObject javaObject
    {
        get
        {
            if (null == _javaObject)
            {
                using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    _javaObject = jc.GetStatic<AndroidJavaObject>("currentActivity");
                }
            }

            return _javaObject;
        }
    }

    private AndroidJavaObject _javaObject;


    public override void Init(string callBackObjectName)
    {
        Debug.Log("AndroidJavaObject.Init() callBackObjectName:" + callBackObjectName);
        try
        {
            javaObject.Call("initCallBackGameObject", callBackObjectName, "OnMessage");
        }
        catch (Exception e)
        {
            Debug.LogError("AndroidSDKParent.initCallBackGameObject() e.Message:" + e.Message);
        }
    }

    public override void SetPlayerInfo(string data)
    {
        try
        {
            javaObject.Call("SetPlayerInfo", data);
        }
        catch (Exception e)
        {
            Debug.LogError("AndroidSDKParent.SetPlayerInfo() e.Message:" + e.Message);
        }
    }

    public override string CallStringMethod(string className, string methodName, string json)
    {
        try
        {
            return javaObject.Call<string>("callStringMethod", className, methodName, json);
        }
        catch (Exception e)
        {
            Debug.LogError("AndroidSDKParent.CallStringMethod() e.Message:" + e.Message);
        }
        return string.Empty;

    }


    public override void LogOut()
    {
        try
        {
            javaObject.Call("logout");
        }
        catch (Exception e)
        {
            Debug.LogError("AndroidSDKParent.LogOut() e.Message:" + e.Message);
        }
    }

    public override void Login()
    {
        try
        {
            javaObject.Call("login");
        }
        catch (Exception e)
        {
            Debug.LogError("AndroidSDKParent.Login() e.Message:" + e.Message);
        }
    }

    public override void SwitchAccount()
    {
        try
        {
            javaObject.Call("switchLogin");
        }
        catch (Exception e)
        {
            Debug.LogError("AndroidSDKParent.SwitchAccount() e.Message:" + e.Message);
        }
    }

    public override void DoExit()
    {
        try
        {
            javaObject.Call("exit");
        }
        catch (Exception e)
        {
            Debug.LogError("AndroidSDKParent.DoExit() e.Message:" + e.Message);
        }
    }

    public override void CallAD()
    {
        try
        {
            javaObject.Call("CallAD");
        }
        catch (Exception e)
        {
            Debug.LogError("AndroidSDKParent.CallAD() e.Message:" + e.Message);
        }
    }

    public override string GetSDKVersionCode()
    {
        try
        {
            return javaObject.Call<string>("GetSDKVersionCode");
        }
        catch (Exception e)
        {
            Debug.LogError("AndroidSDKParent.GetSDKVersionCode() e.Message:" + e.Message);
        }
        return string.Empty;
    }

    public override string GetSDKToken()
    {
        try
        {
            return javaObject.Call<string>("GetSDKToken");
        }
        catch (Exception e)
        {
            Debug.LogError("AndroidSDKParent.GetSDKToken() e.Message:" + e.Message);
        }
        return string.Empty;
    }

    public override void SubmitExtraData(string data, int type)
    {
        try
        {
            javaObject.Call("SubmitExtraData", data, type);
        }
        catch (Exception e)
        {
            Debug.LogError("AndroidSDKParent.SubmitExtraData() e.Message:" + e.Message);
        }
    }

    public override string GetUserId()
    {
        try
        {
            return javaObject.Call<string>("GetUserId");
        }
        catch (Exception e)
        {
            Debug.LogError("AndroidSDKParent.GetUserId() e.Message:" + e.Message);
        }
        return string.Empty;
    }

    public override string GetUserName()
    {
        try
        {
            return javaObject.Call<string>("GetUserName");
        }
        catch (Exception e)
        {
            Debug.LogError("AndroidSDKParent.GetSdkUserName() e.Message:" + e.Message);
        }
        return string.Empty;
    }

    public override void ShowAccountCenter()
    {
        try
        {
            javaObject.Call("ShowAccountCenter");
        }
        catch (Exception e)
        {
            Debug.LogError("AndroidSDKParent.ShowAccountCenter() e.Message:" + e.Message);
        }
    }

    public override string GetSdkExtension()
    {
        try
        {
            return javaObject.Call<string>("GetSdkExtension");
        }
        catch (Exception e)
        {
            Debug.LogError("AndroidSDKParent.GetSdkExtension() e.Message:" + e.Message);
        }
        return string.Empty;
    }

    public override int GetSubChannelId()
    {
        try
        {
            return javaObject.Call<int>("GetSubChannelID");
        }
        catch (Exception e)
        {
            Debug.LogError("AndroidSDKParent.GetSubChannelID() e.Message:" + e.Message);
        }
        return 0;
    }

    public override string GetCurChannelSubName()
    {
        try
        {
            return javaObject.Call<string>("GetSubChannelSubName");
        }
        catch (Exception e)
        {
            Debug.LogError("AndroidSDKParent.GetCurChannelSubName() e.Message:" + e.Message);
        }
        return string.Empty;
    }

    public override string GetMeta(string key)
    {
        try
        {
            return javaObject.Call<string>("GetMetaString", key);
        }
        catch (Exception e)
        {
            Debug.LogError("AndroidSDKParent.GetMeta() e.Message:" + e.Message);
        }
        return string.Empty;
    }

    public override void Pay(string mmaiData)
    {
        try
        {
            javaObject.Call("Pay", mmaiData);
        }
        catch (Exception e)
        {
            Debug.LogError("AndroidSDKParent.Pay() e.Message:" + e.Message);
        }
    }

#endif

}

package com.defComp.game.plugins;

import android.content.Intent;
import android.content.res.Configuration;
import android.os.Bundle;
import android.util.Log;

import com.defComp.game.sdk.CommonSdkHelper;
import com.defComp.game.sdk.IBaseSdkHelper;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

import org.json.JSONException;
import org.json.JSONObject;

import java.lang.reflect.Method;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;

/**
 * Unity 与渠道 SDK 之间的桥接层（模板）。
 *
 * 调用链：
 *   C# AndroidSDKParent --AndroidJavaObject.Call--> MainActivity --IBaseSdkHelper--> 渠道 SDK
 *   渠道 SDK --回调--> IBaseSdkHelper 实现 --sendMessage--> C# SDKHelper.OnMessage
 *
 * 注意：C# 通过 AndroidJavaObject.Call 调用的方法，其方法名 / 参数 / 返回值必须与
 *       Assets/Scripts/SDK/SdkHelper/AndroidSDKParent.cs 中的调用保持一致。
 *
 * 接入新渠道的步骤：
 *   1. 复制 com.defComp.game.sdk.CommonSdkHelper，改名为渠道 Helper，按 TODO 填充；
 *   2. 把 InitSdk() 里的 new CommonSdkHelper() 换成渠道 Helper；
 *   3. 按渠道要求补充 AndroidManifest.xml 的权限 / activity / meta-data 以及 libs 下的 jar。
 */
public class MainActivity extends UnityPlayerActivity
{
    private static final String TAG = "SdkBridge";

    private IBaseSdkHelper _sdkHelper;
    private String _gameObjectName = "";
    private String _callBackMethod = "OnMessage";

    // ------------------------------------------------------------------ lifecycle

    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        InitSdk();
        _sdkHelper.onCreate(savedInstanceState);
    }

    @Override
    protected void onStart()
    {
        super.onStart();
        _sdkHelper.onStart();
    }

    @Override
    protected void onResume()
    {
        super.onResume();
        _sdkHelper.onResume();
    }

    @Override
    protected void onPause()
    {
        super.onPause();
        _sdkHelper.onPause();
    }

    @Override
    protected void onStop()
    {
        super.onStop();
        _sdkHelper.onStop();
    }

    @Override
    protected void onRestart()
    {
        super.onRestart();
        _sdkHelper.onRestart();
    }

    @Override
    protected void onDestroy()
    {
        super.onDestroy();
        _sdkHelper.onDestroy();
    }

    @Override
    public void onNewIntent(Intent intent)
    {
        super.onNewIntent(intent);
        _sdkHelper.onNewIntent(intent);
    }

    @Override
    public void onConfigurationChanged(Configuration newConfig)
    {
        super.onConfigurationChanged(newConfig);
        _sdkHelper.onConfigurationChanged(newConfig);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data)
    {
        super.onActivityResult(requestCode, resultCode, data);
        _sdkHelper.onActivityResult(requestCode, resultCode, data);
    }

    @Override
    public void onRequestPermissionsResult(int requestCode, String[] permissions, int[] grantResults)
    {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        _sdkHelper.onRequestPermissionsResult(requestCode, permissions, grantResults);
    }

    // ------------------------------------------------------------------ init

    /** 创建渠道 SDK 实现，接入新渠道时只改这里 */
    public void InitSdk()
    {
        _sdkHelper = new CommonSdkHelper();
        _sdkHelper.SetActivity(this);
        _sdkHelper.InitSdk();
    }

    /**
     * C# SDKHelper.Awake -> SDKParent.Init 调用，记录消息回调的 Unity 对象与方法名。
     */
    public void initCallBackGameObject(String gameObjectName, String callBackMethod)
    {
        _gameObjectName = gameObjectName == null ? "" : gameObjectName;
        _callBackMethod = (callBackMethod == null || callBackMethod.length() == 0) ? "OnMessage" : callBackMethod;
        Log.i(TAG, "initCallBackGameObject " + _gameObjectName + " " + _callBackMethod);
    }

    /**
     * 渠道 SDK 通过它把消息回传给 Unity，消息格式： cmd + "::" + content
     * 具体命令见 com.defComp.game.sdk.CommonSdkHelper 里的 MSG_* 常量（与 C# SDKHelper 一致）。
     */
    public void sendMessage(String message)
    {
        if (_gameObjectName == null || _gameObjectName.length() == 0)
        {
            Log.w(TAG, "callback object is empty, drop message: " + message);
            return;
        }
        UnityPlayer.UnitySendMessage(_gameObjectName, _callBackMethod, message == null ? "" : message);
    }

    // ------------------------------------------------------------------ sdk function

    public void login()
    {
        Log.i(TAG, "call login()");
        _sdkHelper.login();
    }

    public void switchLogin()
    {
        Log.i(TAG, "call switchLogin()");
        _sdkHelper.switchLogin();
    }

    public void logout()
    {
        Log.i(TAG, "call logout()");
        _sdkHelper.logout();
    }

    public void exit()
    {
        Log.i(TAG, "call exit()");
        _sdkHelper.exit();
    }

    public void ShowAccountCenter()
    {
        Log.i(TAG, "call ShowAccountCenter()");
        _sdkHelper.ShowAccountCenter();
    }

    public void Pay(String data) throws JSONException
    {
        Log.i(TAG, "call Pay(" + data + ")");
        _sdkHelper.Pay(data);
    }

    public void SetPlayerInfo(String data)
    {
        Log.i(TAG, "call SetPlayerInfo(" + data + ")");
        _sdkHelper.SetPlayerInfo(data);
    }

    public void SubmitExtraData(String data, int type) throws JSONException
    {
        Log.i(TAG, "call SubmitExtraData(type=" + type + ", data=" + data + ")");
        _sdkHelper.SubmitExtraData(data, type);
    }

    public void CallAD()
    {
        Log.i(TAG, "call CallAD()");
        _sdkHelper.CallAD();
    }

    // ------------------------------------------------------------------ getter

    public String GetSDKVersionCode()
    {
        String value = _sdkHelper.GetSDKVersionCode();
        Log.i(TAG, "call GetSDKVersionCode() -> " + value);
        return value;
    }

    public String GetSDKToken()
    {
        String value = _sdkHelper.GetSDKToken();
        Log.i(TAG, "call GetSDKToken() -> " + value);
        return value;
    }

    public String GetUserId()
    {
        String value = _sdkHelper.GetUserId();
        Log.i(TAG, "call GetUserId() -> " + value);
        return value;
    }

    public String GetUserName()
    {
        String value = _sdkHelper.GetUserName();
        Log.i(TAG, "call GetUserName() -> " + value);
        return value;
    }

    public String GetSdkExtension()
    {
        String value = _sdkHelper.GetSdkExtension();
        Log.i(TAG, "call GetSdkExtension() -> " + value);
        return value;
    }

    public int GetSubChannelID()
    {
        int value = _sdkHelper.GetSubChannelID();
        Log.i(TAG, "call GetSubChannelID() -> " + value);
        return value;
    }

    public String GetSubChannelSubName()
    {
        String value = _sdkHelper.GetSubChannelSubName();
        Log.i(TAG, "call GetSubChannelSubName() -> " + value);
        return value;
    }

    public String GetMetaString(String key)
    {
        String value = _sdkHelper.GetMetaString(key);
        Log.i(TAG, "call GetMetaString(" + key + ") -> " + value);
        return value;
    }
    // ------------------------------------------------------------------ escape hatch

    /**
     * 通用反射调用：调用渠道 SDK 中任意类的无参方法或单字符串参数方法，供 C# CallStringMethod 使用。
     * 返回值固定为 JSON：成功 {"exist":"true","result":"..."}，失败 {"exist":"false"}。
     * 渠道 SDK 的 UI 操作一般要求在主线程执行，所以这里切到 UI 线程并等待结果。
     */
    public String callStringMethod(final String className, final String methodName, final String json)
    {
        final JSONObject result = new JSONObject();
        try
        {
            result.put("exist", "false");
        }
        catch (JSONException e)
        {
            Log.e(TAG, "callStringMethod build result failed", e);
        }

        final CountDownLatch latch = new CountDownLatch(1);
        runOnUiThread(new Runnable()
        {
            @Override
            public void run()
            {
                try
                {
                    Class<?> clazz = Class.forName(className);
                    Object instance = clazz.newInstance();
                    Method method;
                    Object returnValue;
                    if (json == null || json.length() == 0)
                    {
                        method = clazz.getMethod(methodName);
                        returnValue = method.invoke(instance);
                    }
                    else
                    {
                        method = clazz.getMethod(methodName, String.class);
                        returnValue = method.invoke(instance, json);
                    }

                    result.put("exist", "true");
                    if (method.getReturnType() != void.class && returnValue != null)
                    {
                        result.put("result", String.valueOf(returnValue));
                    }
                }
                catch (Exception e)
                {
                    Log.w(TAG, "callStringMethod " + className + "." + methodName + " failed", e);
                }
                finally
                {
                    latch.countDown();
                }
            }
        });

        try
        {
            latch.await(3, TimeUnit.SECONDS);
        }
        catch (InterruptedException e)
        {
            Thread.currentThread().interrupt();
            Log.w(TAG, "callStringMethod wait interrupted");
        }

        return result.toString();
    }
}
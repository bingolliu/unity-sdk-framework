package com.defComp.game.sdk;

import android.content.Context;
import android.content.Intent;
import android.content.res.Configuration;
import android.os.Bundle;
import android.util.Log;

import com.defComp.game.plugins.MainActivity;

import org.json.JSONException;

/**
 * 未接入渠道 SDK 时的默认实现，同时作为接入新渠道的模板。
 *
 * 接入步骤：
 *   1. 复制本文件，改名为 {渠道名}SdkHelper；
 *   2. 按方法上的 TODO 调用渠道 SDK，并把结果通过 sendMessage(...) 回传给 Unity；
 *   3. 在 MainActivity.InitSdk() 中把 new CommonSdkHelper() 换成新的实现。
 */
public class CommonSdkHelper implements IBaseSdkHelper
{
    protected static final String TAG = "CommonSdkHelper";

    // 以下常量必须与 C# SDKHelper 中的常量保持一致，消息格式： cmd + MSG_GAP + content

    public static final String MSG_GAP = "::";
    public static final String MSG_LOGIN_SUCCESS = "loginSuccess";
    public static final String MSG_LOGIN_FAIL = "loginFail";
    public static final String MSG_LOGIN_CANCEL = "loginCancel";
    public static final String MSG_LOGOUT = "logout";
    public static final String MSG_PAY_SUCCESS = "paySuccess";
    public static final String MSG_PAY_FAIL = "payFail";
    public static final String MSG_SWITCH_ACCOUNT = "onSwitchAccount";
    public static final String MSG_SDK_EXIT = "onSdkExit";
    public static final String MSG_AD_CALLBACK = "ADCallBack";
    public static final String MSG_LOG = "sdkLog";

    public static final int TYPE_SELECT_SERVER = 1; // 选择服务器
    public static final int TYPE_CREATE_ROLE = 2;   // 创建角色
    public static final int TYPE_ENTER_GAME = 3;    // 进入游戏
    public static final int TYPE_LEVEL_UP = 4;      // 等级提升
    public static final int TYPE_EXIT_GAME = 5;     // 退出游戏

    protected MainActivity _activity;

    @Override
    public void SetActivity(Context activity)
    {
        _activity = (MainActivity) activity;
    }

    @Override
    public void InitSdk()
    {
        log("InitSdk()");
        // TODO: 渠道 SDK 初始化，例如 XXXSdk.getInstance().init(_activity, appId, appKey);
    }

    // ---------------------------------------------------------------- 生命周期
    // 渠道 SDK 如果提供对应的生命周期接口，在这里透传

    @Override
    public void onCreate(Bundle savedInstanceState)
    {
    }

    @Override
    public void onStart()
    {
    }

    @Override
    public void onResume()
    {
    }

    @Override
    public void onPause()
    {
    }

    @Override
    public void onStop()
    {
    }

    @Override
    public void onRestart()
    {
    }

    @Override
    public void onDestroy()
    {
    }

    @Override
    public void onNewIntent(Intent newIntent)
    {
    }

    @Override
    public void onConfigurationChanged(Configuration newConfig)
    {
    }

    @Override
    public void onActivityResult(int requestCode, int resultCode, Intent data)
    {
    }

    @Override
    public void onRequestPermissionsResult(int requestCode, String[] permissions, int[] grantResults)
    {
    }

    // ---------------------------------------------------------------- 账号

    @Override
    public void login()
    {
        log("login() 未接入渠道 SDK，等待接入");
        // TODO: 调渠道登录，登录成功后回传：
        // sendMessage(MSG_LOGIN_SUCCESS, "{\"uid\":\"\",\"token\":\"\"}");
        // 失败： sendMessage(MSG_LOGIN_FAIL, errMsg);
        // 取消： sendMessage(MSG_LOGIN_CANCEL, "");

    }

    @Override
    public void switchLogin()
    {
        log("switchLogin()");
        // TODO: 渠道支持切换账号时调用；若渠道用登出回调来切换，则 logout() 即可
    }

    @Override
    public void logout()
    {
        log("logout()");
        // TODO: 渠道登出，成功后回传 sendMessage(MSG_SWITCH_ACCOUNT, "");
    }

    @Override
    public void exit()
    {
        log("exit()");
        // TODO: 渠道退出（弹退出确认框），确认退出后回传 sendMessage(MSG_SDK_EXIT, "");
        // 若渠道没有退出流程，可以直接：_activity.finish(); android.os.Process.killProcess(android.os.Process.myPid());
    }

    @Override
    public void ShowAccountCenter()
    {
        log("ShowAccountCenter()");
        // TODO: 渠道个人中心 / 用户中心
    }

    // ---------------------------------------------------------------- 支付与数据上报

    @Override
    public void Pay(String data) throws JSONException
    {
        log("Pay(" + data + ")");
        // data 是 C# 侧传入的订单 JSON 字符串，按渠道要求解析成参数后拉起支付
        // 支付成功回传： sendMessage(MSG_PAY_SUCCESS, "{\"orderId\":\"\"}");
        // 支付失败回传： sendMessage(MSG_PAY_FAIL, errMsg);

    }

    @Override
    public void SetPlayerInfo(String data)
    {
        log("SetPlayerInfo(" + data + ")");
        // TODO: 玩家信息上报，部分渠道用于防沉迷 / 风控
    }

    @Override
    public void SubmitExtraData(String data, int type) throws JSONException
    {
        log("SubmitExtraData(type=" + type + ", data=" + data + ")");
        // TODO: 角色数据上报，type 见 TYPE_* 常量

    }

    // ---------------------------------------------------------------- 广告

    @Override
    public void CallAD()
    {
        log("CallAD()");
        // TODO: 拉起激励视频广告，播放结束后回传 sendMessage(MSG_AD_CALLBACK, "1"/"0");
    }

    // ---------------------------------------------------------------- 信息读取

    @Override
    public String GetSDKVersionCode()
    {
        return "";
    }

    @Override
    public String GetSDKToken()
    {
        return "";
    }

    @Override
    public String GetUserId()
    {
        return "";
    }

    @Override
    public String GetUserName()
    {
        return "";
    }

    @Override
    public String GetSdkExtension()
    {
        return "";
    }

    @Override
    public int GetSubChannelID()
    {
        return 0;
    }

    @Override
    public String GetSubChannelSubName()
    {
        return "";
    }

    @Override
    public String GetMetaString(String key)
    {

        return "";
    }

    // ---------------------------------------------------------------- 工具方法

    /**
     * 打日志：同时输出到 logcat 和 Unity 的 SDKTestPanel（走 sdkLog 消息通道）
     */
    protected void log(String message)
    {
        Log.i(TAG, message);
        sendMessage(MSG_LOG, message);
    }

    /**
     * 把消息回传给 Unity 的 SDKHelper.OnMessage，消息格式固定为 cmd + "::" + content
     */
    protected void sendMessage(String cmd, String content)
    {
        if (_activity == null)
        {
            Log.w(TAG, "activity 未注入，丢弃消息: " + cmd);
            return;
        }
        _activity.sendMessage(cmd + MSG_GAP + (content == null ? "" : content));
    }
}
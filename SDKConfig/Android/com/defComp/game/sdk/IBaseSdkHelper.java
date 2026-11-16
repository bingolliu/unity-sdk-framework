package com.defComp.game.sdk;

import android.content.Context;
import android.content.Intent;
import android.content.res.Configuration;
import android.os.Bundle;

import org.json.JSONException;

/**
 * 渠道 SDK 接入接口（模板）。
 *
 * MainActivity 只面向本接口，接入不同渠道时提供不同的实现类即可，
 * 不用改动 Unity 桥接层，也不会影响其它渠道。
 * 方法与 C# AndroidSDKParent.cs 一一对应。
 */
public interface IBaseSdkHelper
{
    /** Unity 桥接层初始化时注入 Activity（通常是 MainActivity） */
    void SetActivity(Context activity);

    /** 渠道 SDK 初始化 */
    void InitSdk();

    // ---------------------------------------------------------------- Activity 生命周期透传
    void onCreate(Bundle savedInstanceState);

    void onStart();

    void onResume();

    void onPause();

    void onStop();

    void onRestart();

    void onDestroy();

    void onNewIntent(Intent newIntent);

    void onConfigurationChanged(Configuration newConfig);

    void onActivityResult(int requestCode, int resultCode, Intent data);

    void onRequestPermissionsResult(int requestCode, String[] permissions, int[] grantResults);

    // ---------------------------------------------------------------- 账号
    void login();

    /** 切换账号 / 重新登录，部分渠道等同于 logout + login */
    void switchLogin();

    void logout();

    void exit();

    void ShowAccountCenter();

    // ---------------------------------------------------------------- 支付与数据上报
    void Pay(String data) throws JSONException;

    /** 设置玩家信息，部分渠道用于风控 / 防沉迷 */
    void SetPlayerInfo(String data);

    /**
     * 角色数据上报，type 取值见 CommonSdkHelper.TYPE_* （与 C# SDKHelper.TYPE_* 一致）
     */
    void SubmitExtraData(String data, int type) throws JSONException;

    // ---------------------------------------------------------------- 广告
    void CallAD();

    // ---------------------------------------------------------------- 信息读取
    String GetSDKVersionCode();

    String GetSDKToken();

    String GetUserId();

    String GetUserName();

    String GetSdkExtension();

    int GetSubChannelID();

    String GetSubChannelSubName();

    String GetMetaString(String key);
}
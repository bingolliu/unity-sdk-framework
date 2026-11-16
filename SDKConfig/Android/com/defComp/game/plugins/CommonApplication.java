package com.defComp.game.plugins;

import android.app.Application;

/**
 * 自定义 Application（模板）。
 *
 * AndroidManifest.xml 中 android:name 指向本类。
 * 部分渠道 SDK 需要在 Application 阶段做预初始化、多进程判断或获取 ApplicationContext，
 * 有需要时在 onCreate 中补充；没有要求则保持空实现即可。
 */
public class CommonApplication extends Application
{
    @Override
    public void onCreate()
    {
        super.onCreate();
        // TODO: 例如 XXXSdk.preInit(this);
    }
}
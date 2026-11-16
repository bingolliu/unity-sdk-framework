#import "CommonIOSSdkHelper.h"
#import "SdkCommonFun.h"

/**
 * 默认实现（全部空实现），接入新渠道时：
 *   1. 直接在本文件里重写 IOSSdkHelper 的接口方法，按方法上的 TODO 调用渠道 SDK；
 *   2. 结果通过 SendSdkMessage(@"cmd::content") 回传给 Unity；
 *   3. IOSSdkAppController 的 createSdkHelper 默认返回本实现，无需改动。
 *
 * 若同一工程需要同时保留多套渠道实现，再复制 .h / .m 为新类，并只改 createSdkHelper 的返回值。
 */
@implementation CommonIOSSdkHelper

#pragma mark - 日志

/** 打日志：同时输出到 Xcode 控制台和 Unity 的 SDKTestPanel（走 sdkLog 消息通道） */
- (void)log:(NSString *)message
{
    NSLog(@"[CommonIOSSdkHelper] %@", message);
    SendSdkMessage([NSString stringWithFormat:@"%@%@%@", SDK_MSG_LOG, SDK_MSG_GAP, message]);
}

#pragma mark - 初始化

- (void)initSdk
{
    [self log:@"initSdk() 未接入渠道 SDK"];
    // TODO: 渠道 SDK 初始化，例如 [[XXXSDK sharedInstance] initWithAppId:@"..." appKey:@"..."];
}

#pragma mark - 账号

- (void)autoLogin
{
    [self log:@"autoLogin()"];
    // TODO: 渠道自动登录，成功后回传：
    // SendSdkMessage([NSString stringWithFormat:@"%@%@{\"uid\":\"\",\"token\":\"\"}", SDK_MSG_LOGIN_SUCCESS, SDK_MSG_GAP]);
    // 失败：SendSdkMessage([NSString stringWithFormat:@"%@%@%@", SDK_MSG_LOGIN_FAIL, SDK_MSG_GAP, errMsg]);
}

- (void)login
{
    [self log:@"login()"];
    // TODO: 渠道手动登录（部分渠道没有单独的登录入口，只在 autoLogin 里处理）
}

- (void)switchLogin
{
    [self log:@"switchLogin()"];
    // TODO: 切换账号
}

- (void)logout
{
    [self log:@"logout()"];
    // TODO: 渠道登出，成功后回传 SendSdkMessage([NSString stringWithFormat:@"%@%@", SDK_MSG_SWITCH_ACCOUNT, SDK_MSG_GAP]);
}

- (void)exitApp
{
    [self log:@"exitApp()"];
    // TODO: 渠道退出流程，确认后回传：
    // SendSdkMessage([NSString stringWithFormat:@"%@%@", SDK_MSG_SDK_EXIT, SDK_MSG_GAP]);
}

- (void)showAccountCenter
{
    [self log:@"showAccountCenter()"];
    // TODO: 渠道个人中心
}

#pragma mark - 支付与数据上报

- (void)payWithData:(NSString *)payData
{
    [self log:[NSString stringWithFormat:@"payWithData(%@)", payData]];
    // payData 是 C# 侧传入的订单 JSON 字符串，按渠道要求解析后拉起支付
    // 成功：SendSdkMessage([NSString stringWithFormat:@"%@%@{\"orderId\":\"\"}", SDK_MSG_PAY_SUCCESS, SDK_MSG_GAP]);
    // 失败：SendSdkMessage([NSString stringWithFormat:@"%@%@%@", SDK_MSG_PAY_FAIL, SDK_MSG_GAP, errMsg]);
}

- (void)setPlayerInfo:(NSString *)data
{
    [self log:[NSString stringWithFormat:@"setPlayerInfo(%@)", data]];
    // TODO: 玩家信息上报
}

- (void)submitExtraData:(NSString *)data type:(int)type
{
    [self log:[NSString stringWithFormat:@"submitExtraData(type=%d, data=%@)", type, data]];
    // TODO: 角色数据上报，type 见 SDK_TYPE_*
}

#pragma mark - 广告

- (void)callAD
{
    [self log:@"callAD()"];
    // TODO: 拉起激励视频，播放结束后回传 SendSdkMessage(@"ADCallBack::1")
}

#pragma mark - 信息读取

- (NSString *)getSDKVersionCode
{
    return @"";
}

- (NSString *)getSDKToken
{
    return @"";
}

- (NSString *)getUserId
{
    return @"";
}

- (NSString *)getUserName
{
    return @"";
}

- (NSString *)getSdkExtension
{
    return @"";
}

- (NSString *)getCurChannelSubName
{
    return @"";
}

- (NSString *)getGameStr
{
    return @"";
}

- (NSString *)getMeta:(NSString *)key
{
    // 未接入渠道时给游戏层一份默认配置，接入渠道后从渠道配置或 Info.plist 中读取
    if ([key isEqualToString:@"PrivacyDefault"])
    {
        return @"yes";
    }
    if ([key isEqualToString:@"PrivacyURL"])
    {
        return @"http://res-dtry.oclkj.com/def/privacy/privacy.txt";
    }
    return @"";
}

- (NSString *)callStringMethod:(NSString *)className method:(NSString *)methodName json:(NSString *)json
{
    [self log:[NSString stringWithFormat:@"callStringMethod(%@.%@)", className, methodName]];
    // 反射调用渠道 SDK 的方法，默认返回统一的“不存在”结构，与 Android 侧保持一致
    return @"{\"exist\":\"false\"}";
}

- (int)getSubChannelId
{
    // TODO: 从 Info.plist 的 SubChannelID 读取，例如：
    // return [[[NSBundle mainBundle] objectForInfoDictionaryKey:@"SubChannelID"] intValue];
    return 0;
}

@end
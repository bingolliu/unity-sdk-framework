#ifndef IOSSdkHelper_h
#define IOSSdkHelper_h

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

// ---------------------------------------------------------------------------
// 消息常量：必须与 C# SDKHelper 中的常量保持一致，消息格式 cmd + "::" + content
// ---------------------------------------------------------------------------
#define SDK_MSG_GAP             @"::"
#define SDK_MSG_LOGIN_SUCCESS   @"loginSuccess"
#define SDK_MSG_LOGIN_FAIL      @"loginFail"
#define SDK_MSG_LOGIN_CANCEL    @"loginCancel"
#define SDK_MSG_LOGOUT          @"logout"
#define SDK_MSG_PAY_SUCCESS     @"paySuccess"
#define SDK_MSG_PAY_FAIL        @"payFail"
#define SDK_MSG_SWITCH_ACCOUNT  @"onSwitchAccount"
#define SDK_MSG_SDK_EXIT        @"onSdkExit"
#define SDK_MSG_AD_CALLBACK     @"ADCallBack"
#define SDK_MSG_LOG             @"sdkLog"

// 角色数据上报类型，与 C# SDKHelper.TYPE_* 保持一致
#define SDK_TYPE_SELECT_SERVER  1
#define SDK_TYPE_CREATE_ROLE    2
#define SDK_TYPE_ENTER_GAME     3
#define SDK_TYPE_LEVEL_UP       4
#define SDK_TYPE_EXIT_GAME      5

/**
 * 渠道 SDK 接入协议（模板）。
 *
 * 接入步骤：
 *   1. 复制 CommonIOSSdkHelper.h / CommonIOSSdkHelper.m，改类名，按 TODO 调用渠道 SDK；
 *   2. 渠道回调里通过 SendSdkMessage(@"loginSuccess::{\"uid\":\"\"}") 把结果回传给 Unity；
 *   3. 把 IOSSdkAppController 的 createSdkHelper 换成渠道实现。
 *
 * 本协议的方法与 C# IOSSDKParent.cs 一一对应。
 */
@protocol IOSSdkHelper <NSObject>

@required
- (void)initSdk;

- (void)autoLogin;

- (void)login;

/** 切换账号，部分渠道等同于 logout + login */
- (void)switchLogin;

- (void)logout;

- (void)exitApp;

- (void)payWithData:(NSString *)payData;

/** 玩家信息上报，部分渠道用于风控 / 防沉迷 */
- (void)setPlayerInfo:(NSString *)data;

/** 角色数据上报，type 见 SDK_TYPE_* */
- (void)submitExtraData:(NSString *)data type:(int)type;

- (void)showAccountCenter;

- (void)callAD;

- (NSString *)getSDKVersionCode;

- (NSString *)getSDKToken;

- (NSString *)getUserId;

- (NSString *)getUserName;

- (NSString *)getSdkExtension;

- (NSString *)getCurChannelSubName;

- (NSString *)getGameStr;

- (NSString *)getMeta:(NSString *)key;

/** 通用反射调用：调用渠道 SDK 里任意类的无参 / 单字符串参数方法，返回 JSON 字符串 */
- (NSString *)callStringMethod:(NSString *)className method:(NSString *)methodName json:(NSString *)json;

- (int)getSubChannelId;

@optional
/** 渠道 SDK 需要启动参数、生命周期或 URL Scheme 回调时实现 */
- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions;

- (void)applicationDidBecomeActive:(UIApplication *)application;

- (void)applicationWillResignActive:(UIApplication *)application;

- (BOOL)application:(UIApplication *)app openURL:(NSURL *)url options:(NSDictionary<UIApplicationOpenURLOptionsKey, id> *)options;

@end

#ifdef __cplusplus
extern "C" {
#endif

/** 由桥接层（IOSSdkAppController.mm）实现，渠道实现通过它把消息回传给 Unity 的 SDKHelper.OnMessage */
extern void SendSdkMessage(NSString *message);

#ifdef __cplusplus
}
#endif

#endif /* IOSSdkHelper_h */
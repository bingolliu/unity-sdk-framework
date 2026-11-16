#import "UnityAppController.h"
#import "SdkCommonFun.h"
#import "IOSSdkHelper.h"
#import "CommonIOSSdkHelper.h"

/*
 * Unity 与渠道 SDK 之间的桥接层（iOS 模板）。
 *
 * 调用链：
 *   C# IOSSDKParent --[DllImport("__Internal")]--> 本文件 extern "C" 函数 --> IOSSdkHelper 实现 --> 渠道 SDK
 *   渠道 SDK --回调--> IOSSdkHelper 实现 --SendSdkMessage--> C# SDKHelper.OnMessage
 *
 * 接入新渠道时只需要：
 *   1. 在 CommonIOSSdkHelper 里重写 IOSSdkHelper 的接口方法（createSdkHelper 默认返回它）；
 *   2. 渠道需要并存多套实现时，才新建渠道实现并改下面的 createSdkHelper 返回值；
 *   3. 本文件可整体复制并改名，注意同时修改 IMPL_APP_CONTROLLER_SUBCLASS。
 *
 * 注意：本文件中的 C 函数名 / 参数 / 返回值必须与 Assets/Scripts/SDK/SdkHelper/IOSSDKParent.cs
 *       中的 DllImport 保持一致。
 */

@class IOSSdkAppController;

// Unity 回调目标，由 C# SDKParent.Init -> _InitCallBackGameObject 传入
static NSString *sCallBackObjectName = nil;
static NSString *sCallBackMethod = @"OnMessage";

// 由 -init 赋值，供下面的 C 函数访问
static IOSSdkAppController *sAppController = nil;

/** 渠道实现调用它把消息回传给 Unity，消息格式： cmd + "::" + content */
void SendSdkMessage(NSString *message)
{
    NSString *msg = (message != nil) ? message : @"";
    if (sCallBackObjectName == nil || sCallBackObjectName.length == 0)
    {
        NSLog(@"[SdkBridge] Unity 回调对象尚未设置, 丢弃消息: %@", msg);
        return;
    }
    UnitySendMessage([sCallBackObjectName UTF8String], [sCallBackMethod UTF8String], [msg UTF8String]);
}

@interface IOSSdkAppController : UnityAppController
{
    id<IOSSdkHelper> _sdkHelper;
}

- (id<IOSSdkHelper>)createSdkHelper;

- (id<IOSSdkHelper>)sdkHelper;

@end

@implementation IOSSdkAppController

- (instancetype)init
{
    self = [super init];
    if (self)
    {
        sAppController = self;
        _sdkHelper = [self createSdkHelper];
    }
    return self;
}

/** 接入新渠道时改这里：返回渠道实现，未接入渠道时用默认空实现 */
- (id<IOSSdkHelper>)createSdkHelper
{
    return [[CommonIOSSdkHelper alloc] init];
}

- (id<IOSSdkHelper>)sdkHelper
{
    return _sdkHelper;
}

#pragma mark - app lifecycle

- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions
{
    BOOL result = [super application:application didFinishLaunchingWithOptions:launchOptions];

    [_sdkHelper initSdk];
    if ([_sdkHelper respondsToSelector:@selector(application:didFinishLaunchingWithOptions:)])
    {
        [_sdkHelper application:application didFinishLaunchingWithOptions:launchOptions];
    }
    return result;
}

- (void)applicationDidBecomeActive:(UIApplication *)application
{
    [super applicationDidBecomeActive:application];
    if ([_sdkHelper respondsToSelector:@selector(applicationDidBecomeActive:)])
    {
        [_sdkHelper applicationDidBecomeActive:application];
    }
}

- (void)applicationWillResignActive:(UIApplication *)application
{
    [super applicationWillResignActive:application];
    if ([_sdkHelper respondsToSelector:@selector(applicationWillResignActive:)])
    {
        [_sdkHelper applicationWillResignActive:application];
    }
}

- (BOOL)application:(UIApplication *)app openURL:(NSURL *)url options:(NSDictionary<UIApplicationOpenURLOptionsKey, id> *)options
{
    if ([_sdkHelper respondsToSelector:@selector(application:openURL:options:)])
    {
        // 渠道处理了 URL Scheme 就不再交给 Unity
        return [_sdkHelper application:app openURL:url options:options];
    }
    return [super application:app openURL:url options:options];
}

@end

#if defined(__cplusplus)
extern "C" {
#endif

#pragma mark - 初始化

void _InitCallBackGameObject(const char *callBackObjectName, const char *callBackMethod)
{
    NSLog(@"[SdkBridge] InitCallBackGameObject()");
    sCallBackObjectName = [CreateNSString(callBackObjectName) copy];
    if (callBackMethod != NULL && strlen(callBackMethod) > 0)
    {
        sCallBackMethod = [CreateNSString(callBackMethod) copy];
    }
    NSLog(@"[SdkBridge] InitCallBackGameObject %@ %@", sCallBackObjectName, sCallBackMethod);
}

#pragma mark - 账号

void _Login()
{
    NSLog(@"[SdkBridge] Login()");
    [[sAppController sdkHelper] login];
}

void _AutoLogin()
{
    NSLog(@"[SdkBridge] AutoLogin()");
    [[sAppController sdkHelper] autoLogin];
}

void _SwitchLogin()
{
    NSLog(@"[SdkBridge] SwitchLogin()");
    [[sAppController sdkHelper] switchLogin];
}

void _LogOut()
{
    NSLog(@"[SdkBridge] LogOut()");
    [[sAppController sdkHelper] logout];
}

void _ShowAccountCenter()
{
    NSLog(@"[SdkBridge] ShowAccountCenter()");
    [[sAppController sdkHelper] showAccountCenter];
}

void _DoExit()
{
    NSLog(@"[SdkBridge] DoExit()");
    [[sAppController sdkHelper] exitApp];
}

#pragma mark - 支付与数据上报

void _Pay(const char *data)
{
    NSLog(@"[SdkBridge] Pay()");
    [[sAppController sdkHelper] payWithData:CreateNSString(data)];
}

void _SetPlayerInfo(const char *data)
{
    NSLog(@"[SdkBridge] SetPlayerInfo()");
    [[sAppController sdkHelper] setPlayerInfo:CreateNSString(data)];
}

void _SubmitExtraData(const char *data, int type)
{
    NSLog(@"[SdkBridge] SubmitExtraData()");
    [[sAppController sdkHelper] submitExtraData:CreateNSString(data) type:type];
}

#pragma mark - 广告

void _CallAD()
{
    NSLog(@"[SdkBridge] CallAD()");
    [[sAppController sdkHelper] callAD];
}

#pragma mark - 通用调用

const char *_CallStringMethod(const char *className, const char *methodName, const char *json)
{
    return _makeStringCopyFromNSString([[sAppController sdkHelper] callStringMethod:CreateNSString(className)
                                                                              method:CreateNSString(methodName)
                                                                                json:CreateNSString(json)]);
}

#pragma mark - 信息读取

const char *_GetSDKVersionCode()
{
    return _makeStringCopyFromNSString([[sAppController sdkHelper] getSDKVersionCode]);
}

const char *_GetSDKToken()
{
    return _makeStringCopyFromNSString([[sAppController sdkHelper] getSDKToken]);
}

const char *_GetUserId()
{
    return _makeStringCopyFromNSString([[sAppController sdkHelper] getUserId]);
}

const char *_GetSdkUserName()
{
    return _makeStringCopyFromNSString([[sAppController sdkHelper] getUserName]);
}

const char *_GetSdkExtension()
{
    return _makeStringCopyFromNSString([[sAppController sdkHelper] getSdkExtension]);
}

const char *_GetCurChannelSubName()
{
    return _makeStringCopyFromNSString([[sAppController sdkHelper] getCurChannelSubName]);
}

const char *_GetGameStr()
{
    return _makeStringCopyFromNSString([[sAppController sdkHelper] getGameStr]);
}

const char *_GetMeta(const char *key)
{
    return _makeStringCopyFromNSString([[sAppController sdkHelper] getMeta:CreateNSString(key)]);
}

int _GetSubChannelId()
{
    NSLog(@"[SdkBridge] GetSubChannelId()");
    return [[sAppController sdkHelper] getSubChannelId];
}

#if defined(__cplusplus)
}
#endif

IMPL_APP_CONTROLLER_SUBCLASS(IOSSdkAppController)
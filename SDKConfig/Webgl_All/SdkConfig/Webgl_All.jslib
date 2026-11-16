// WebGL 平台 SDK 桥接（模板）。
// C# WebGLSDKParent 通过 DllImport 调用这里的方法；这里再调用页面里的 SDKHelper.* 完成渠道接入。
// 日志通道：SendSdkLog -> Unity SDKHelper.OnMessage("sdkLog::...") -> SDKTestPanel

var UnitySdkRuntime = {
    callBackObject: "",
    callBackMethod: "OnMessage"
};

function SendSdkLog(message) {
    console.log("[Sdk] " + message);
    if (typeof unityInstance === "undefined" || !unityInstance || !UnitySdkRuntime.callBackObject) {
        return;
    }
    unityInstance.SendMessage(UnitySdkRuntime.callBackObject, UnitySdkRuntime.callBackMethod, "sdkLog::" + message);
}

// 让页面里的渠道 JS（如 askjSDKHelper.js）也能打日志到 Unity
if (typeof window !== "undefined") {
    window.SendSdkLog = SendSdkLog;
}

var Webgl = {
    // objectName: string, methodName: string
    InitCallBackGameObject: function (objectName, methodName) {
        UnitySdkRuntime.callBackObject = UTF8ToString(objectName);
        UnitySdkRuntime.callBackMethod = UTF8ToString(methodName);
        SendSdkLog("InitCallBackGameObject " + UnitySdkRuntime.callBackObject + "." + UnitySdkRuntime.callBackMethod);
    },

    // pass a string to unity needs call stringToUTF8 
    UrlParmString: function()
    {
        var returnStr = window.location.search;
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        return buffer;
    },

    InitSdk:function()
    {
        SendSdkLog("InitSdk()");
        SDKHelper.InitializeSdk();
    },

    // data -> jsonStr
    Pay : function(data)
    {
        var dataJson = JSON.parse(UTF8ToString(data));
        SendSdkLog("Pay(" + JSON.stringify(dataJson) + ")");
        SDKHelper.SDKPay(dataJson);
    },

    // data -> string
    // return string
    ReqCommonMsg : function(data)
    {
        var message = UTF8ToString(data);
        SendSdkLog("ReqCommonMsg(" + message + ")");
        // get a string from unity needs call UTF8ToString
        return SDKHelper.SDKReqCommonMsg(message);
    },

    // data -> string
    // return string
    CertifiedAdult: function(data)
    {
        var message = UTF8ToString(data);
        SendSdkLog("CertifiedAdult(" + message + ")");
        // get a string from unity needs call UTF8ToString
        return SDKHelper.SDKCertifiedAdult(message)
    },

    // data -> jsonStr, type -> int
    SubmitData : function(data, type)
    {
        var dataJson = JSON.parse(UTF8ToString(data));
        SendSdkLog("SubmitData(type=" + type + ", data=" + JSON.stringify(dataJson) + ")");
        SDKHelper.SDKSubmitData(dataJson, type)
    },

    // return int
    GetSubChannelID : function()
    {
        var subChannelId = SDKHelper.SDKGetSubChannelID();
        SendSdkLog("GetSubChannelID() -> " + subChannelId);
        return subChannelId;
    },

    // return int 
    // Determine SDKHelper based on the return value
    GetPlatformTag : function()
    {
        var tag = parseInt(mASKJLoader.GetUrlParameter("mPlatformTag"));
        SendSdkLog("GetPlatformTag() -> " + tag);
        return tag;
    },

};
 
mergeInto(LibraryManager.library, Webgl);
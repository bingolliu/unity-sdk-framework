// WebGL 渠道桥接模板：接入新渠道时按本文件重写 SDKHelper.* 的方法。
// jslib（Webgl_All.jslib）负责调用这些方法；日志走 window.SendSdkLog，回调走 unityInstance.SendMessage。

var SDKHelper = SDKHelper || {};

// 打日志：优先走 jslib 暴露的 SendSdkLog（会显示在 Unity 的 SDKTestPanel 上），否则只输出到浏览器控制台
SDKHelper.log = function (message) {
    if (typeof window.SendSdkLog === "function") {
        window.SendSdkLog(message);
    } else {
        console.log("[SDK] " + message);
    }
};

SDKHelper.InitializeSdk = function () {
    SDKHelper.log("InitializeSdk() 未接入渠道 SDK");
};

SDKHelper.SDKPay = function (jsonData) {
    SDKHelper.log("SDKPay(" + JSON.stringify(jsonData) + ")");
};

SDKHelper.SDKReqCommonMsg = function (str) {
    SDKHelper.log("SDKReqCommonMsg(" + str + ")");
    return "";
};

SDKHelper.CertifiedAdult = function (str) {
    SDKHelper.log("CertifiedAdult(" + str + ")");
    return "";
};

SDKHelper.SDKSubmitData = function (jsonData, type) {
    SDKHelper.log("SDKSubmitData(type=" + type + ", data=" + JSON.stringify(jsonData) + ")");
};

SDKHelper.SDKGetSubChannelID = function () {
    SDKHelper.log("SDKGetSubChannelID()");
    return 99999
};
window.SDKHelper = SDKHelper;
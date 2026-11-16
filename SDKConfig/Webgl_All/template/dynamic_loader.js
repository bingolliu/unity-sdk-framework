// dynamic_loader.js

var mASKJLoader = {};

mASKJLoader.GetUrlParameter = function (key) {
    const queryString = window.location.search;
    const urlParams = new URLSearchParams(queryString);
    return urlParams.get(key);
}

mASKJLoader.EncodedStr = function (str) {
    var utf8Bytes = new TextEncoder().encode(str);
    var encodedStr = btoa(String.fromCharCode.apply(null, utf8Bytes));
    return encodedStr
}

mASKJLoader.Unicode2Ch = function (str) {
    if (!str) {
        return;
    }
    var len = 1;
    var result = '';
    for (var i = 0; i < str.length; i = i + len) {
        len = 1;
        var temp = str.charAt(i);
        if (temp == '\\') {
            if (str.charAt(i + 1) == 'u') {
                var unicode = str.substr((i + 2), 4);
                result += String.fromCharCode(parseInt(unicode, 16).toString(10));
                len = 6;
            } else {
                result += temp;
            }
        } else {
            result += temp;
        }
    }
    return result;
}

mASKJLoader.LoadScript = function (url, callback) {
    var script = document.createElement('script');
    script.type = 'text/javascript';
    script.src = url;
    script.async = false; // 确保脚本是同步加载的

    // 确保脚本加载完成后调用回调函数
    script.onload = function () {
        if (typeof callback === 'function') {
            callback();
        }
    };

    document.head.appendChild(script);
}

mASKJLoader.LoadHtml = function (url, elementId) {
    fetch(url)
        .then(response => response.text())
        .then(data => {
            document.getElementById(elementId).innerHTML = data;
        })
        .catch(error => console.error('Error loading ', url, ": ", error));
}

mASKJLoader.CheckBrowser = function () {
    // 获取浏览器 userAgent
    var ua = navigator.userAgent;
    var isIE = ua.indexOf("MSIE") > -1;
    if (isIE) {
        return false;
    }
    // ie 11
    if (!!window.ActiveXObject || "ActiveXObject" in window) {
        return false;
    }
    return true;
}

mASKJLoader.Load = function () {
    var supportedBrowser = this.CheckBrowser()
    if (!supportedBrowser) {
        window.alert("当前浏览器不支持");
    }

    var platfTag = parseInt(this.GetUrlParameter("mPlatformTag"))
    switch (platfTag) {
        case 69: // flash
            if (!supportedBrowser) {
                window.location = "https://game.flash.cn/zsj";
            }
            this.LoadScript("js/flash/flashSDKHelper.js");
            break
        case 74: // wan360
            this.LoadScript("js/wan360/wan360SDKHelper.js");
            break
        case 75: // fusion360
            if (!supportedBrowser){
                const platform = mASKJLoader.GetUrlParameter('platform')
                if ((platform.indexOf("apps") > -1))
                {
                    var obj = {
                        type:'playGame',
                        msg:'0'
                    }
                    window.parent.postMessage(obj, '*')
                    window.location= "https://pc.game.iqiyi.com/downloadpca";
                }
                else
                {
                    const back_url = mASKJLoader.GetUrlParameter("back_url")
                    window.location= back_url;
                }
            }
            this.LoadScript("js/fusion/fusion360SDKHelper.js");
            break
        case 97: // 602
            if (!supportedBrowser) {
                window.location = "https://zsj.602.com/html/94851.html";
            }
            this.LoadScript("js/602/602SDKHelper.js");
            break
        case 108: // qq 空间
            this.LoadScript("js/qqZone/qqZoneSDKHelper.js");
            break
        case 109: // 迅玩
            this.LoadScript("js/xunwan/xunwanSDKHelper.js");
            break
        case 115: // 西游
            this.LoadScript("js/xiyou/xiyouSDKHelper.js");
            break
        case 116: // 阅文
            this.LoadScript("js/yuewen/yuewenSDKHelper.js");
            break
        case 117: // 第一游戏
            this.LoadScript("js/diyi/diyiSDKHelper.js");
            break
        case 118: // 百度            
            this.LoadScript("js/baidu/baiduSDKHelper.js");
            break
        case 119: // 联想
            this.LoadScript("js/lenovo/lenovoSDKHelper.js");
            break
        case 123: // ku25
            this.LoadScript("js/ku25/ku25SDKHelper.js");
            break
        case 124: // feihuo
            this.LoadScript("js/feihuo/feihuoSDKHelper.js");
            break
        case 125: // duoyou
            this.LoadScript("js/duoyou/duoyouSDKHelper.js");
            break
        case 126: // 52GG
            this.LoadScript("js/gg52/gg52SDKHelper.js");
            break
        case 127: // 武侠谷
            this.LoadScript("js/wuxiagu/wuxiaguSDKHelper.js");
            break
        case 128: // 4游戏
            this.LoadScript("js/game4/game4SDKHelper.js")
            break
        case 129: // 4399
            this.LoadScript("js/4399/4399SDKHelper.js")
            break
        case 130: // 妖豆
            this.LoadScript("js/yaodou/yaodouSDKHelper.js")
            break
        case 131: // 4游戏页游
            this.LoadScript("js/game4/game4WebSDKHelper.js")
            break
        case 132: // 52gg页游
            this.LoadScript("js/gg52/gg52WebSDKHelper.js")
            break
        case 2001: // 海外超游
            this.LoadScript("js/overseas/overseasSDKHelper.js")
            break
        default:
            console.warn('default platfTag')
            this.LoadScript("js/askj/askjSDKHelper.js")
            return
    }
}

window.mASKJLoader = mASKJLoader
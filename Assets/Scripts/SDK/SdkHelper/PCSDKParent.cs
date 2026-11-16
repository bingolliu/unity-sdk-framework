using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
using System.Text;
using System.Diagnostics;

public class PCSDKParent : SDKParent
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }


    //用于查找窗口句柄的Unity窗口类的名称
    protected const string UNITY_WND_CLASSNAME = "askjhtj";
    protected static IntPtr unityHWnd;
    public delegate bool WNDENUMPROC(IntPtr hwnd, uint lParam);


    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool EnumWindows(WNDENUMPROC lpEnumFunc, uint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetParent(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetAncestor(IntPtr hWnd, uint gaFlags);

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, ref uint lpdwProcessId);

    [DllImport("user32.dll")]
    public static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int nMaxCount);

    [DllImport("User32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern int MessageBox(IntPtr handle, String message, String title, int type);

    [DllImport("kernel32.dll")]
    public static extern void SetLastError(uint dwErrCode);

    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", EntryPoint = "GetWindowRect")]
    public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hwnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    public static extern bool MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool bRepaint);


    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

    [DllImport("user32")]
    public static extern int SetForegroundWindow(IntPtr hwnd);

    [DllImport("user32.dll ", EntryPoint = "SetParent")]
    public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32")]
    public static extern bool UpdateWindow(IntPtr hwnd);

    public static IntPtr GetProcessWnd()
    {
        IntPtr ptrWnd = IntPtr.Zero;
        uint pid = (uint)Process.GetCurrentProcess().Id;  // 当前进程 ID

        bool bResult = EnumWindows(new WNDENUMPROC(delegate (IntPtr hwnd, uint lParam)
        {
            uint id = 0;

            //if (GetParent(hwnd) == IntPtr.Zero)
            {
                GetWindowThreadProcessId(hwnd, ref id);
                if (id == lParam)
                {
                    ptrWnd = hwnd;   // 把句柄缓存起来
                    SetLastError(0);    // 设置无错误
                    return false;   // 返回 false 以终止枚举窗口
                }
            }

            return true;

        }), pid);


        return (!bResult && Marshal.GetLastWin32Error() == 0) ? ptrWnd : IntPtr.Zero;
    }


    public override void Login()
    {
        LogCall("Login()");
    }

    public override void DoExit()
    {
        LogCall("DoExit()");
    }

    public override void LogOut()
    {
        LogCall("LogOut()");
    }

    public override void SwitchAccount()
    {
        LogCall("SwitchAccount()");
    }

    public override void ShowAccountCenter()
    {
        LogCall("ShowAccountCenter()");
    }

    public override void CallAD()
    {
        LogCall("CallAD()");
    }

    public override void Pay(string payData)
    {
        LogCall("Pay(" + payData + ")");
    }

    public override void SetPlayerInfo(string data)
    {
        LogCall("SetPlayerInfo(" + data + ")");
    }

    public override void SubmitExtraData(string data, int type)
    {
        LogCall("SubmitExtraData(type=" + type + ", data=" + data + ")");
    }

    public override string GetSDKVersionCode()
    {
        LogCall("GetSDKVersionCode()");
        return "";
    }

    public override string GetSDKToken()
    {
        LogCall("GetSDKToken()");
        return "";
    }

    public override string GetUserId()
    {
        LogCall("GetUserId()");
        return "";
    }

    public override string GetUserName()
    {
        LogCall("GetUserName()");
        return "";
    }

    public override string GetGameStr()
    {
        LogCall("GetGameStr()");
        return "";
    }

    public override string GetSdkExtension()
    {
        LogCall("GetSdkExtension()");
        return "";
    }

    public override int GetSubChannelId()
    {
        LogCall("GetSubChannelId()");
        return 0;
    }

    public override string GetCurChannelSubName()
    {
        LogCall("GetCurChannelSubName()");
        return "";
    }

    public override string GetMeta(string key)
    {
        LogCall("GetMeta(" + key + ")");
        return "";
    }

    public override string CallStringMethod(string className, string methodName, string json)
    {
        LogCall("CallStringMethod(" + className + "." + methodName + ", " + json + ")");
        return "{\"exist\":\"false\"}";
    }
}
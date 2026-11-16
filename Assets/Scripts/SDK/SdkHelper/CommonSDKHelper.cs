using UnityEngine;
using System.Collections;

/// <summary>
/// 未接sdk的版本使用这一个helper
/// </summary>
public class CommonSDKHelper : SDKParent
{
    public override string GetMeta(string key)
    {
        string ret = "";
        switch (key)
        {
            case "PrivacyDefault":
                ret = "yes";
                break;
            case "PrivacyURL":
                ret = "http://res-dtry.oclkj.com/def/privacy/privacy.txt";
                break;
            default:

                break;
        }
        return ret;
    }

    public override int GetSubChannelId()
    {
        return 0;
    }
}

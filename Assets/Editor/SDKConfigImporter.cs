using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// 打包时按目标平台把 SDKConfig 下的模板文件同步到 Assets/Plugins。
///
/// 流程：
///   1. 删除上一次同步留下的全部文件（记录在 ProjectSettings/SDKConfigSync.manifest）；
///   2. 删除其它平台的同名模板文件，避免多平台文件混在一起；
///   3. 拷贝当前平台的文件（拷贝前逐个删除旧文件）。
///
/// 同步是幂等的，可以重复执行；手工在菜单里执行一次就能看到效果，
/// 打包（Build / Build And Run）时会自动执行。
///
/// 说明：这些模板只包含 Java / ObjC / jslib / gradle / manifest，
///       不包含 .cs，所以打包过程中刷新资源不会触发脚本重新编译。
/// </summary>
public class SDKConfigImporter : IPreprocessBuildWithReport, IActiveBuildTargetChanged
{
    private const string ConfigFolderName = "SDKConfig";
    private const string ManifestRelativePath = "ProjectSettings/SDKConfigSync.manifest";
    private const string AutoSyncPrefKey = "SDKConfig.AutoSyncOnBuild";

    private const string MenuRoot = "Tools/SDKConfig/";
    private const string MenuAutoSync = MenuRoot + "打包前自动同步";

    // 打包回调尽量排在前面，保证其它打包处理能用到拷贝进来的插件
    public int callbackOrder
    {
        get { return -1000; }
    }

    /// <summary>一条同步规则：把 SDKConfig 下的源（文件或目录）镜像到工程内的目标路径</summary>
    private class CopyRule
    {
        public readonly string source;
        public readonly string destination;

        public CopyRule(string source, string destination)
        {
            this.source = source;
            this.destination = destination;
        }
    }

    private static Dictionary<BuildTarget, CopyRule[]> rules;

    private static Dictionary<BuildTarget, CopyRule[]> Rules
    {
        get
        {
            if (rules != null)
            {
                return rules;
            }

            rules = new Dictionary<BuildTarget, CopyRule[]>
            {
                {
                    BuildTarget.Android, new[]
                    {
                        new CopyRule("Android/AndroidManifest.xml", "Assets/Plugins/Android/AndroidManifest.xml"),
                        new CopyRule("Android/com", "Assets/Plugins/Android/com"),
                        new CopyRule("Android/libs", "Assets/Plugins/Android/libs"),
                        // 下面两个只有勾选 Custom Main Gradle Template / Custom Gradle Properties Template 时才会生效，
                        // 文件存在时 Unity 会自动使用它们
                        new CopyRule("Android/mainTemplate.gradle", "Assets/Plugins/Android/mainTemplate.gradle"),
                        new CopyRule("Android/gradleTemplate.properties", "Assets/Plugins/Android/gradleTemplate.properties")
                    }
                },
                {
                    BuildTarget.iOS, new[]
                    {
                        // .h / .mm 放在 Plugins/iOS 会被 Unity 自动加入 Xcode 工程
                        new CopyRule("iOS/plugins", "Assets/Plugins/iOS")
                    }
                },
                {
                    BuildTarget.WebGL, new[]
                    {
                        new CopyRule("Webgl_All/SdkConfig/Webgl_All.jslib", "Assets/Plugins/WebGL/Webgl_All.jslib"),
                        // WebGL 渠道还需要自定义模板页（index.html / loader / 渠道 SDK js），一起同步到模板目录
                        new CopyRule("Webgl_All/template", "Assets/WebGLTemplates/Webgl_All")
                    }
                }
            };

            return rules;
        }
    }

    // ---------------------------------------------------------------- 打包回调

    /// <summary>切换 Build Target 时顺手同步一次，避免打包前 Plugins 里还是上一个平台的文件</summary>
    public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
    {
        if (!EditorPrefs.GetBool(AutoSyncPrefKey, true))
        {
            return;
        }

        Sync(newTarget);
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        if (!EditorPrefs.GetBool(AutoSyncPrefKey, true))
        {
            Debug.Log("[SDKConfig] 打包前自动同步已关闭，跳过");
            return;
        }

        BuildTarget target = report.summary.platform;
        if (!Sync(target))
        {
            throw new BuildFailedException("[SDKConfig] 平台模板同步失败，已中止打包（详见上方错误日志）");
        }
    }

    // ---------------------------------------------------------------- 菜单

    [MenuItem(MenuRoot + "同步当前平台", false, 100)]
    private static void SyncActivePlatform()
    {
        Sync(EditorUserBuildSettings.activeBuildTarget);
    }

    [MenuItem(MenuRoot + "同步 Android", false, 101)]
    private static void SyncAndroid()
    {
        Sync(BuildTarget.Android);
    }

    [MenuItem(MenuRoot + "同步 iOS", false, 102)]
    private static void SyncIOS()
    {
        Sync(BuildTarget.iOS);
    }

    [MenuItem(MenuRoot + "同步 WebGL", false, 103)]
    private static void SyncWebGL()
    {
        Sync(BuildTarget.WebGL);
    }

    [MenuItem(MenuRoot + "清理已同步文件", false, 150)]
    private static void ClearSyncedFiles()
    {
        string projectRoot = GetProjectRoot();
        RemoveTracked(projectRoot);

        foreach (KeyValuePair<BuildTarget, CopyRule[]> pair in Rules)
        {
            RemovePlatformFiles(projectRoot, pair.Value);
        }

        AssetDatabase.Refresh();
        Debug.Log("[SDKConfig] 已清理同步过的插件文件");
    }

    [MenuItem(MenuAutoSync, true)]
    private static bool ToggleAutoSyncValidate()
    {
        Menu.SetChecked(MenuAutoSync, EditorPrefs.GetBool(AutoSyncPrefKey, true));
        return true;
    }

    [MenuItem(MenuAutoSync, false, 200)]
    private static void ToggleAutoSync()
    {
        bool value = !EditorPrefs.GetBool(AutoSyncPrefKey, true);
        EditorPrefs.SetBool(AutoSyncPrefKey, value);
        Debug.Log("[SDKConfig] 打包前自动同步: " + (value ? "开启" : "关闭"));
    }

    // ---------------------------------------------------------------- 同步实现

    /// <summary>把指定平台的模板同步到工程内，成功返回 true</summary>
    public static bool Sync(BuildTarget target)
    {
        string projectRoot = GetProjectRoot();
        string configRoot = Path.Combine(projectRoot, ConfigFolderName);

        if (!Directory.Exists(configRoot))
        {
            Debug.LogError("[SDKConfig] 找不到配置目录: " + configRoot);
            return false;
        }

        List<string> tracked = new List<string>();

        try
        {
            // 1. 清掉上一次同步的内容
            RemoveTracked(projectRoot);

            // 2. 清掉其它平台的同名文件，保证 Plugins 里只有当前平台的东西
            foreach (KeyValuePair<BuildTarget, CopyRule[]> pair in Rules)
            {
                if (pair.Key == target)
                {
                    continue;
                }

                RemovePlatformFiles(projectRoot, pair.Value);
            }

            CopyRule[] current;
            if (!Rules.TryGetValue(target, out current))
            {
                Debug.Log("[SDKConfig] " + target + " 平台不需要同步原生插件（纯 C# 接入）");
                WriteManifest(projectRoot, tracked);
                AssetDatabase.Refresh();
                return true;
            }

            // 3. 拷贝当前平台（内部逐个文件先删后拷）
            int count = 0;
            foreach (CopyRule rule in current)
            {
                string sourceFull = Path.Combine(configRoot, rule.source);
                string destFull = Path.Combine(projectRoot, rule.destination);

                if (!File.Exists(sourceFull) && !Directory.Exists(sourceFull))
                {
                    // 目录规则允许不存在（例如渠道没有 jar 依赖），文件规则缺失才算异常
                    if (File.Exists(sourceFull) || Path.HasExtension(rule.source))
                    {
                        Debug.LogWarning("[SDKConfig] 源文件不存在，跳过: " + rule.source);
                    }

                    continue;
                }

                count += CopyEntry(sourceFull, destFull, projectRoot, tracked);
            }

            WriteManifest(projectRoot, tracked);
            AssetDatabase.Refresh();

            Debug.Log("[SDKConfig] 已为 " + target + " 同步 " + count + " 个文件到 Assets/Plugins");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("[SDKConfig] 同步 " + target + " 失败: " + e);
            return false;
        }
    }

    /// <summary>删除某一平台规则对应的目标文件（只删模板里存在的同名条目，不影响其它插件）</summary>
    private static void RemovePlatformFiles(string projectRoot, CopyRule[] platformRules)
    {
        string configRoot = Path.Combine(projectRoot, ConfigFolderName);

        foreach (CopyRule rule in platformRules)
        {
            RemoveMirror(Path.Combine(configRoot, rule.source), Path.Combine(projectRoot, rule.destination));
        }
    }

    private static void RemoveMirror(string sourceFull, string destFull)
    {
        if (File.Exists(sourceFull))
        {
            if (File.Exists(destFull))
            {
                File.Delete(destFull);
            }
            return;
        }

        if (!Directory.Exists(sourceFull) || !Directory.Exists(destFull))
        {
            return;
        }

        foreach (string file in Directory.GetFiles(sourceFull))
        {
            if (IsIgnoredFile(file))
            {
                continue;
            }

            string dest = Path.Combine(destFull, Path.GetFileName(file));
            if (File.Exists(dest))
            {
                File.Delete(dest);
            }
        }

        foreach (string dir in Directory.GetDirectories(sourceFull))
        {
            RemoveMirror(dir, Path.Combine(destFull, Path.GetFileName(dir)));
        }
    }

    /// <summary>递归拷贝，返回拷贝的文件数；已存在的目标文件会先删除</summary>
    private static int CopyEntry(string sourceFull, string destFull, string projectRoot, List<string> tracked)
    {
        if (File.Exists(sourceFull))
        {
            string destDir = Path.GetDirectoryName(destFull);
            if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            if (File.Exists(destFull))
            {
                File.Delete(destFull);
            }

            File.Copy(sourceFull, destFull, true);
            tracked.Add(ToRelativePath(projectRoot, destFull));
            return 1;
        }

        if (!Directory.Exists(sourceFull))
        {
            return 0;
        }

        if (!Directory.Exists(destFull))
        {
            Directory.CreateDirectory(destFull);
        }

        int count = 0;
        foreach (string file in Directory.GetFiles(sourceFull))
        {
            if (IsIgnoredFile(file))
            {
                continue;
            }

            count += CopyEntry(file, Path.Combine(destFull, Path.GetFileName(file)), projectRoot, tracked);
        }

        foreach (string dir in Directory.GetDirectories(sourceFull))
        {
            count += CopyEntry(dir, Path.Combine(destFull, Path.GetFileName(dir)), projectRoot, tracked);
        }

        return count;
    }

    // ---------------------------------------------------------------- 同步清单

    private static void RemoveTracked(string projectRoot)
    {
        string manifest = Path.Combine(projectRoot, ManifestRelativePath);
        if (!File.Exists(manifest))
        {
            return;
        }

        string[] entries = File.ReadAllLines(manifest);

        // 倒序删除，保证先删文件/子目录
        for (int i = entries.Length - 1; i >= 0; i--)
        {
            if (string.IsNullOrEmpty(entries[i]))
            {
                continue;
            }

            string full = Path.Combine(projectRoot, entries[i]);
            try
            {
                if (File.Exists(full))
                {
                    File.Delete(full);
                }
                else if (Directory.Exists(full))
                {
                    Directory.Delete(full, true);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[SDKConfig] 删除旧文件失败: " + full + " " + e.Message);
            }
        }

        File.Delete(manifest);
    }

    private static void WriteManifest(string projectRoot, List<string> tracked)
    {
        string manifest = Path.Combine(projectRoot, ManifestRelativePath);
        string dir = Path.GetDirectoryName(manifest);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllLines(manifest, tracked.ToArray());
    }

    // ---------------------------------------------------------------- 工具

    private static bool IsIgnoredFile(string path)
    {
        string name = Path.GetFileName(path);
        return name.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)
               || name.StartsWith(".", StringComparison.Ordinal);
    }

    private static string GetProjectRoot()
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
    }

    private static string ToRelativePath(string projectRoot, string fullPath)
    {
        return fullPath.Substring(projectRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace('\\', '/');
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using LitJson;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEngine;

/// <summary>
/// iOS 导出 Xcode 工程后的处理（配合 SDKConfigImporter 使用）。
///
/// 做两件事：
///   1. 用 SDKConfig/iOS 下的 Info.plist 覆盖导出工程里的 Info.plist；
///   2. 按 SDKConfig/iOS/setting.json 配置导出的 Xcode 工程：
///        libs            → OTHER_LDFLAGS -lxxx（.a / .tbd / .dylib）
///        frameworks      → Frameworks Build Phase（.framework）
///        linker_flags    → OTHER_LDFLAGS（Unity-iPhone 与 UnityFramework 两个 target）
///        compiler_flags  → SDKConfig/iOS/plugins 下 .m / .mm 的 COMPILER_FLAGS
///        build_setting   → 逐项 SetBuildProperty（两个 target）
///        plist           → Info.plist 相对 setting.json 的路径，缺省为同级 Info.plist
///
/// setting.json 示例见 SDKConfig/iOS/setting.json，字段可缺省。
/// </summary>
public class iOSBuildPostprocessor : IPostprocessBuildWithReport
{
    private const string ConfigFolder = "SDKConfig/iOS";
    private const string SettingFileName = "setting.json";
    private const string PluginsFolder = "plugins";
    private const string DefaultPlistName = "Info.plist";

    /// <summary>排在其它后处理之后，保证拿到的工程是最终状态</summary>
    public int callbackOrder
    {
        get { return 100; }
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.iOS)
        {
            return;
        }

        try
        {
            Process(report.summary.outputPath);
        }
        catch (Exception e)
        {
            // 打包已经结束，这里只报错，不中断 Unity
            Debug.LogError("[SDKConfig] iOS 后处理失败: " + e);
        }
    }

    private static void Process(string xcodePath)
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string configDir = Path.Combine(projectRoot, ConfigFolder);

        if (!Directory.Exists(configDir))
        {
            Debug.LogWarning("[SDKConfig] 未找到 iOS 配置目录，跳过后处理: " + configDir);
            return;
        }

        if (!Directory.Exists(xcodePath))
        {
            Debug.LogError("[SDKConfig] 导出的 Xcode 工程不存在: " + xcodePath);
            return;
        }

        Setting setting = Setting.Load(Path.Combine(configDir, SettingFileName));

        // 1. Info.plist 覆盖
        CopyInfoPlist(configDir, xcodePath, setting.plist);

        // 2. Xcode 工程设置
        string pbxProjectPath = PBXProject.GetPBXProjectPath(xcodePath);
        if (!File.Exists(pbxProjectPath))
        {
            Debug.LogError("[SDKConfig] 找不到 project.pbxproj: " + pbxProjectPath);
            return;
        }

        PBXProject project = new PBXProject();
        project.ReadFromFile(pbxProjectPath);

        string mainTarget = project.GetUnityMainTargetGuid();
        string frameworkTarget = project.GetUnityFrameworkTargetGuid();

        foreach (string framework in setting.frameworks)
        {
            AddFramework(project, frameworkTarget, framework);
        }

        foreach (string lib in setting.libs)
        {
            AddLibrary(project, frameworkTarget, lib);
        }

        foreach (string flag in setting.linkerFlags)
        {
            if (string.IsNullOrEmpty(flag))
            {
                continue;
            }

            // Unity 的代码都在 UnityFramework 里，主 target 也一起加，保证两者链接一致
            project.AddBuildProperty(frameworkTarget, "OTHER_LDFLAGS", flag);
            project.AddBuildProperty(mainTarget, "OTHER_LDFLAGS", flag);
        }

        foreach (KeyValuePair<string, string> pair in setting.buildSettings)
        {
            project.SetBuildProperty(frameworkTarget, pair.Key, pair.Value);
            project.SetBuildProperty(mainTarget, pair.Key, pair.Value);
            Debug.Log("[SDKConfig] BUILD_SETTING " + pair.Key + " = " + pair.Value);
        }

        if (setting.compilerFlags.Count > 0)
        {
            ApplyCompilerFlags(project, frameworkTarget, xcodePath, pbxProjectPath, Path.Combine(configDir, PluginsFolder), setting.compilerFlags);
        }

        File.WriteAllText(pbxProjectPath, project.WriteToString());
        Debug.Log("[SDKConfig] 已按 setting.json 更新 Xcode 工程: " + pbxProjectPath);
    }

    // ---------------------------------------------------------------- Info.plist

    private static void CopyInfoPlist(string configDir, string xcodePath, string plistRelative)
    {
        string source = ResolvePlistPath(configDir, plistRelative);
        if (source == null)
        {
            Debug.LogWarning("[SDKConfig] 未找到 Info.plist 模板，跳过覆盖");
            return;
        }

        if (!ValidatePlist(source))
        {
            Debug.LogError("[SDKConfig] Info.plist 模板格式非法，已跳过覆盖: " + source);
            return;
        }

        string destination = Path.Combine(xcodePath, DefaultPlistName);
        File.Copy(source, destination, true);
        Debug.Log("[SDKConfig] 已用 " + ToRelative(configDir, source) + " 覆盖导出的 " + destination);
    }

    private static string ResolvePlistPath(string configDir, string plistRelative)
    {
        if (!string.IsNullOrEmpty(plistRelative))
        {
            string candidate = Path.Combine(configDir, plistRelative.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(candidate))
            {
                return candidate;
            }

            Debug.LogWarning("[SDKConfig] setting.json 的 plist 路径不存在: " + plistRelative + "，改用默认 " + DefaultPlistName);
        }

        string fallback = Path.Combine(configDir, DefaultPlistName);
        return File.Exists(fallback) ? fallback : null;
    }

    private static bool ValidatePlist(string path)
    {
        try
        {
            PlistDocument document = new PlistDocument();
            document.ReadFromFile(path);
            return document.root != null;
        }
        catch (Exception e)
        {
            Debug.LogError("[SDKConfig] 解析 Info.plist 失败: " + path + " " + e.Message);
            return false;
        }
    }

    // ---------------------------------------------------------------- 库与框架

    private static void AddFramework(PBXProject project, string targetGuid, string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        if (name.EndsWith(".framework", StringComparison.OrdinalIgnoreCase))
        {
            project.AddFrameworkToProject(targetGuid, name, false);
            Debug.Log("[SDKConfig] framework += " + name);
            return;
        }

        AddLibrary(project, targetGuid, name);
    }

    private static void AddLibrary(PBXProject project, string targetGuid, string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        string lower = name.ToLowerInvariant();
        bool isStaticOrDynamic = lower.EndsWith(".a") || lower.EndsWith(".tbd") || lower.EndsWith(".dylib");
        if (!isStaticOrDynamic)
        {
            Debug.LogWarning("[SDKConfig] 未识别的库类型，已跳过: " + name);
            return;
        }

        // libz.tbd -> -lz / libc++.tbd -> -lc++ / libGCloudVoice.a -> -lGCloudVoice
        string libName = Path.GetFileNameWithoutExtension(name);
        if (libName.StartsWith("lib", StringComparison.Ordinal))
        {
            libName = libName.Substring(3);
        }

        project.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", "-l" + libName);
        Debug.Log("[SDKConfig] OTHER_LDFLAGS += -l" + libName + "  (" + name + ")");
    }

    // ---------------------------------------------------------------- 编译参数

    private static void ApplyCompilerFlags(PBXProject project, string targetGuid, string xcodePath, string pbxProjectPath, string pluginsDir, List<string> flags)
    {
        if (!Directory.Exists(pluginsDir))
        {
            Debug.LogWarning("[SDKConfig] 找不到插件源码目录: " + pluginsDir);
            return;
        }

        string flagLine = string.Join(" ", flags.ToArray());

        foreach (string source in Directory.GetFiles(pluginsDir))
        {
            string extension = Path.GetExtension(source).ToLowerInvariant();
            if (extension != ".m" && extension != ".mm")
            {
                continue;
            }

            string fileName = Path.GetFileName(source);
            string fileGuid = FindFileGuid(project, pbxProjectPath, xcodePath, fileName);
            if (string.IsNullOrEmpty(fileGuid))
            {
                Debug.LogWarning("[SDKConfig] 导出的工程里找不到源文件，跳过编译参数设置: " + fileName);
                continue;
            }

            try
            {
                project.SetCompileFlagsForFile(targetGuid, fileGuid, new List<string>(flags));
                Debug.Log("[SDKConfig] " + fileName + " COMPILER_FLAGS = " + flagLine);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[SDKConfig] 设置 " + fileName + " 编译参数失败: " + e.Message);
            }        }
    }

    private static string FindFileGuid(PBXProject project, string pbxProjectPath, string xcodePath, string fileName)
    {
        string[] hits;
        try
        {
            hits = Directory.GetFiles(xcodePath, fileName, SearchOption.AllDirectories);
        }
        catch (Exception)
        {
            hits = new string[0];
        }

        // 1. 先按工程内相对路径 / 真实路径查（Unity 提供的 API）
        foreach (string hit in hits)
        {
            string relative = hit.Substring(xcodePath.Length)
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Replace('\\', '/');

            string guid = project.FindFileGuidByProjectPath(relative);
            if (!string.IsNullOrEmpty(guid))
            {
                return guid;
            }

            guid = project.FindFileGuidByRealPath(hit);
            if (!string.IsNullOrEmpty(guid))
            {
                return guid;
            }
        }

        // 2. 兜底：直接从 pbxproj 里找文件名对应的 PBXFileReference GUID
        return FindFileGuidInProjectFile(pbxProjectPath, fileName);
    }

    /// <summary>
    /// 从 project.pbxproj 文本里找 PBXFileReference 的 GUID。
    /// Unity 生成的文件引用是单行的，形如：
    ///   ABCDEF...01234567 /* Foo.mm */ = {isa = PBXFileReference; ... path = Libraries/Plugins/iOS/Foo.mm; ... };
    /// </summary>
    private static string FindFileGuidInProjectFile(string pbxProjectPath, string fileName)
    {
        if (!File.Exists(pbxProjectPath))
        {
            return null;
        }

        string[] lines = File.ReadAllLines(pbxProjectPath);
        foreach (string line in lines)
        {
            if (line.IndexOf("isa = PBXFileReference", StringComparison.Ordinal) < 0)
            {
                continue;
            }

            if (line.IndexOf(fileName, StringComparison.Ordinal) < 0)
            {
                continue;
            }

            Match match = Regex.Match(line, "([0-9A-Fa-f]{24,32})");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
        }

        return null;
    }

    private static string ToRelative(string root, string full)
    {
        if (full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            return full.Substring(root.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace('\\', '/');
        }

        return full;
    }

    // ---------------------------------------------------------------- setting.json

    /// <summary>setting.json 解析结果，字段都可以缺省</summary>
    private class Setting
    {
        public readonly List<string> libs = new List<string>();
        public readonly List<string> frameworks = new List<string>();
        public readonly List<string> compilerFlags = new List<string>();
        public readonly List<string> linkerFlags = new List<string>();
        public readonly Dictionary<string, string> buildSettings = new Dictionary<string, string>();
        public string plist = "";

        public static Setting Load(string path)
        {
            Setting setting = new Setting();
            if (!File.Exists(path))
            {
                Debug.LogWarning("[SDKConfig] 未找到 setting.json，仅做 Info.plist 覆盖: " + path);
                return setting;
            }

            try
            {
                JsonData root = JsonMapper.ToObject(File.ReadAllText(path));

                ReadArray(root, "libs", setting.libs);
                ReadArray(root, "frameworks", setting.frameworks);
                ReadArray(root, "compiler_flags", setting.compilerFlags);
                ReadArray(root, "linker_flags", setting.linkerFlags);

                JsonData plist = GetField(root, "plist");
                if (plist != null && plist.IsString)
                {
                    setting.plist = (string)plist;
                }

                JsonData buildSetting = GetField(root, "build_setting");
                if (buildSetting != null && buildSetting.IsObject)
                {
                    IDictionary dict = buildSetting as IDictionary;
                    if (dict != null)
                    {
                        foreach (object key in dict.Keys)
                        {
                            string name = key as string;
                            if (name == null)
                            {
                                continue;
                            }

                            JsonData value = dict[key] as JsonData;
                            if (value != null)
                            {
                                setting.buildSettings[name] = value.IsString ? (string)value : value.ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[SDKConfig] 解析 setting.json 失败: " + path + " " + e.Message);
            }

            return setting;
        }

        private static void ReadArray(JsonData root, string key, List<string> target)
        {
            JsonData array = GetField(root, key);
            if (array == null)
            {
                return;
            }

            IList list = array as IList;
            if (list == null)
            {
                return;
            }

            foreach (object item in list)
            {
                JsonData value = item as JsonData;
                if (value != null && value.IsString)
                {
                    string text = (string)value;
                    if (!string.IsNullOrEmpty(text))
                    {
                        target.Add(text);
                    }
                }
            }
        }

        private static JsonData GetField(JsonData root, string key)
        {
            if (root == null)
            {
                return null;
            }

            IDictionary dict = root as IDictionary;
            if (dict == null || !dict.Contains(key))
            {
                return null;
            }

            return dict[key] as JsonData;
        }
    }
}

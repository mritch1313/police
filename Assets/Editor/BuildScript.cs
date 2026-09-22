using UnityEngine;
using UnityEditor;
using System.Linq;

public class BuildScript
{
    [MenuItem("PoliceChase/Build Android APK")]
    public static void BuildAndroid()
    {
        BuildAndroidInternal();
    }

    public static void BuildAndroidInternal()
    {
        Debug.Log("[BuildScript] Starting Android build...");

        // Ensure scenes
        string[] scenes = new string[] { "Assets/Scenes/Main.unity" };
        foreach (var s in scenes)
        {
            if (!System.IO.File.Exists(s))
            {
                Debug.LogError($"[BuildScript] Scene not found: {s}");
                EditorApplication.Exit(1);
                return;
            }
        }

        // Set target
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        // Player settings
        PlayerSettings.companyName = "PoliceChaseStudio";
        PlayerSettings.productName = "PoliceChase";
        PlayerSettings.bundleVersion = "1.0.0";
        PlayerSettings.Android.bundleVersionCode = 1;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.defaultScreenOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;
        PlayerSettings.useOSAutorotation = false;
        PlayerSettings.colorSpace = ColorSpace.Linear;
        PlayerSettings.gpuSkinning = true;
        PlayerSettings.MTRendering = true;
        PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.Auto;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        // URP
        var urpAssetPath = "Assets/Settings/Rendering/PoliceChaseURP.asset";
        var urpAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset>(urpAssetPath);
        if (urpAsset != null)
        {
            UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline = urpAsset;
            QualitySettings.renderPipeline = urpAsset;
        }

        // Build
        string buildPath = "Builds/PoliceChase.apk";
        System.IO.Directory.CreateDirectory("Builds");

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = scenes;
        options.locationPathName = buildPath;
        options.target = BuildTarget.Android;
        options.options = BuildOptions.None;

        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"[BuildScript] Build succeeded: {buildPath} size={report.summary.totalSize}");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"[BuildScript] Build failed: {report.summary.result}");
            // Log errors
            foreach (var step in report.steps)
            {
                foreach (var msg in step.messages)
                {
                    Debug.LogError($"[Build] {msg.type}: {msg.content}");
                }
            }
            EditorApplication.Exit(1);
        }
    }

    // For CI verification
    public static void VerifyProject()
    {
        Debug.Log("[BuildScript] Verifying project...");

        // Check scripts compile
        var allScripts = AssetDatabase.FindAssets("t:Script");
        Debug.Log($"[Verify] Found {allScripts.Length} scripts");

        // Check main scene
        if (!System.IO.File.Exists("Assets/Scenes/Main.unity"))
        {
            Debug.LogError("[Verify] Main scene missing");
            EditorApplication.Exit(1);
            return;
        }

        // Check URP asset
        if (!System.IO.File.Exists("Assets/Settings/Rendering/PoliceChaseURP.asset"))
        {
            Debug.LogError("[Verify] URP asset missing");
            EditorApplication.Exit(1);
            return;
        }

        Debug.Log("[Verify] Project verification passed");
        EditorApplication.Exit(0);
    }
}

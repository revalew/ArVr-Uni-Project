using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

public class UnityBuilder
{
    [MenuItem("Build/Build Android")]
    public static void BuildAndroid()
    {
        // Get filename based on project name
        string appName = Application.productName;
        
        // Define output path
        string buildPath = Path.Combine("build", "Android");
        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }
        
        // Configure build settings
        PlayerSettings.Android.keystorePass = System.Environment.GetEnvironmentVariable("KEYSTORE_PASS") ?? "";
        PlayerSettings.Android.keyaliasPass = System.Environment.GetEnvironmentVariable("KEY_ALIAS_PASS") ?? "";
        
        // Enable development build and script debugging for non-production builds
        bool isDevelopmentBuild = EditorUserBuildSettings.development;
        
        // Set build options
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = GetEnabledScenes(),
            locationPathName = Path.Combine(buildPath, appName + ".apk"),
            target = BuildTarget.Android,
            options = isDevelopmentBuild 
                ? BuildOptions.Development | BuildOptions.AllowDebugging
                : BuildOptions.None
        };
        
        // Build
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }

    [MenuItem("Build/Build iOS")]
    public static void BuildiOS()
    {
        // Get filename based on project name
        string appName = Application.productName;
        
        // Define output path
        string buildPath = Path.Combine("build", "iOS");
        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }
        
        // Enable development build and script debugging for non-production builds
        bool isDevelopmentBuild = EditorUserBuildSettings.development;
        
        // Set build options
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = GetEnabledScenes(),
            locationPathName = buildPath,
            target = BuildTarget.iOS,
            options = isDevelopmentBuild 
                ? BuildOptions.Development | BuildOptions.AllowDebugging
                : BuildOptions.None
        };
        
        // Build
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }

    // Helper method to get all enabled scenes from build settings
    private static string[] GetEnabledScenes()
    {
        return (from scene in EditorBuildSettings.scenes
                where scene.enabled
                select scene.path).ToArray();
    }
}
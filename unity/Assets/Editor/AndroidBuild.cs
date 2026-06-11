using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace DoenerEmpire.EditorTools
{
    public static class AndroidBuild
    {
        public static void BuildApk()
        {
            string outputPath = Path.GetFullPath(Path.Combine(
                Application.dataPath,
                "..",
                "..",
                "build",
                "Android",
                "doener-empire-unity.apk"));

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? "build/Android");

            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            EditorUserBuildSettings.androidBuildType = AndroidBuildType.Release;
            PlayerSettings.Android.useCustomKeystore = false;

            BuildPlayerOptions options = new()
            {
                scenes = new[] { "Assets/Scenes/CityMap.unity" },
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None,
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException($"Android APK build failed: {report.summary.result}");
            }

            Debug.Log($"Android APK build completed: {outputPath}");
        }
    }
}

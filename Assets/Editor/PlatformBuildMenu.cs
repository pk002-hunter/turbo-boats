using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace TurboBoats.Editor
{
    public static class PlatformBuildMenu
    {
        [MenuItem("Turbo Boats/Build/WebGL")]
        public static void BuildWebGL()
        {
            Build(BuildTargetGroup.WebGL, BuildTarget.WebGL, "WebGL", null, BuildOptions.None);
        }

        [MenuItem("Turbo Boats/Build/Android Development APK")]
        public static void BuildAndroidDevelopment()
        {
            Build(BuildTargetGroup.Android, BuildTarget.Android, "Android", "TurboBoats.apk", BuildOptions.Development);
        }

        [MenuItem("Turbo Boats/Build/iOS Development Project")]
        public static void BuildIOSDevelopment()
        {
            Build(BuildTargetGroup.iOS, BuildTarget.iOS, "iOS", null, BuildOptions.Development);
        }

        private static void Build(BuildTargetGroup group, BuildTarget target, string folder, string fileName, BuildOptions options)
        {
            if (!BuildPipeline.IsBuildTargetSupported(group, target))
            {
                throw new BuildFailedException(
                    $"The {target} build module is not installed in this Unity Editor. Install it with Unity Hub and try again.");
            }

            EditorBuildSettingsScene[] enabledScenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .ToArray();
            string[] scenePaths = enabledScenes.Select(scene => scene.path).ToArray();
            if (scenePaths.Length == 0)
                throw new BuildFailedException("No enabled scenes are listed in Build Settings.");

            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(group, target))
                throw new BuildFailedException($"Unity could not switch the active build target to {target}.");

            string buildsDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds"));
            Directory.CreateDirectory(buildsDirectory);
            string outputPath = Path.Combine(buildsDirectory, folder);
            if (!string.IsNullOrEmpty(fileName))
            {
                Directory.CreateDirectory(outputPath);
                outputPath = Path.Combine(outputPath, fileName);
            }

            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenePaths,
                locationPathName = outputPath,
                target = target,
                targetGroup = group,
                options = options
            });

            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException($"{target} build finished with status {report.summary.result}.");

            Debug.Log($"{target} build completed: {outputPath} ({report.summary.totalSize / (1024 * 1024)} MB)");
        }
    }
}

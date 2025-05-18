using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Editor.Generators
{
    public class AudioMixerConstantsGenerator : AssetPostprocessor
    {
        private const string PathToMixersFolder = "Assets/_Project/Source/Audio/Mixers";
        private const string PathToOutput = "Generated";
        private const string OutputClassName = "AudioResources";

        private static bool _pendingGenerate;
        public const string AutoGeneratePrefKey = "AudioMixerConstantsGenerator_AutoGenerate";

        [MenuItem("Tools/Audio Mixer Generator/Generate Audio Mixer Class")]
        public static void GenerateAudioMixerConstants()
        {
            GenerateClass();
        }

        private static void GenerateGroupsForMixer(string filePath, StringBuilder classBuilder, string indent)
        {
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                if (line.Contains("m_Name:"))
                {
                    var name = line.Split(':')[1].Trim();
                    if (name.StartsWith("m_") || string.IsNullOrEmpty(name)) continue;
                    classBuilder.AppendLine($"{indent}public const string Group_{name} = \"{name}\";");
                }
            }
        }

        private static void GenerateClass()
        {
            var mixersPath = Path.Combine(Application.dataPath, "../", PathToMixersFolder);
            if (!Directory.Exists(mixersPath))
            {
                Debug.LogError("Mixers folder not found at path: " + mixersPath);
                return;
            }

            var outputDirectory = Path.Combine(Application.dataPath, PathToOutput);
            var outputPath = Path.Combine(outputDirectory,           $"{OutputClassName}.cs");

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            var namespaces = new HashSet<string> { "UnityEngine", "UnityEngine.Audio" };
            var classBuilder = new StringBuilder();
            classBuilder.AppendLine("// This file is auto-generated. Do not modify manually.");
            classBuilder.AppendLine();

            classBuilder.AppendLine($"public static class {OutputClassName}");
            classBuilder.AppendLine("{");

            GenerateMixerClasses(mixersPath, classBuilder, "    ", namespaces);

            classBuilder.AppendLine("}");

            var finalBuilder = new StringBuilder();
            foreach (var ns in namespaces.OrderBy(n => n))
            {
                finalBuilder.AppendLine($"using {ns};");
            }

            finalBuilder.AppendLine();
            finalBuilder.Append(classBuilder.ToString());

            File.WriteAllText(outputPath, finalBuilder.ToString());
            AssetDatabase.Refresh();
            Debug.Log($"Generated {OutputClassName} class at: {outputPath}");
        }

        private static void GenerateMixerClasses(string folderPath, StringBuilder classBuilder, string indent,
            HashSet<string> namespaces)
        {
            var mixerFiles = Directory.GetFiles(folderPath, "*.mixer", SearchOption.AllDirectories);

            foreach (var mixerFile in mixerFiles)
            {
                var mixerName = Path.GetFileNameWithoutExtension(mixerFile);
                var exposedParameters = ParseExposedParameters(File.ReadAllText(mixerFile));

                classBuilder.AppendLine($"{indent}public static class {mixerName}Mixer");
                classBuilder.AppendLine($"{indent}{{");

                classBuilder.AppendLine($"{indent}    // Exposed Parameters");
                foreach (var param in exposedParameters)
                {
                    classBuilder.AppendLine($"{indent}    public const string {param} = \"{param}\";");
                }

                classBuilder.AppendLine($"{indent}    // Audio Mixer Groups");
                GenerateGroupsForMixer(mixerFile, classBuilder, indent + "    ");

                classBuilder.AppendLine($"{indent}}}");
            }
        }

        private static List<string> ParseExposedParameters(string content)
        {
            var exposedParams = new List<string>();
            var exposedParamRegex = new Regex(@"- guid:\s*\w+\s*name:\s*(\w+)", RegexOptions.Compiled);
            foreach (Match match in exposedParamRegex.Matches(content))
            {
                exposedParams.Add(match.Groups[1].Value);
            }

            return exposedParams;
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (EditorPrefs.GetBool(AutoGeneratePrefKey, false))
            {
                ScheduleClassGeneration();
            }
        }

        private static void ScheduleClassGeneration()
        {
            if (_pendingGenerate) return;
            _pendingGenerate = true;
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isCompiling)
                {
                    EditorApplication.delayCall += OnRecompileCompleted;
                }
                else
                {
                    OnRecompileCompleted();
                }
            };
        }

        private static void OnRecompileCompleted()
        {
            if (EditorApplication.isCompiling) return;
            _pendingGenerate = false;
            GenerateClass();
        }
    }
}
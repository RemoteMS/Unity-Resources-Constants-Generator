using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Editor.Generators
{
    public class ResourceClassGenerator : AssetPostprocessor
    {
        private const string PathToResourcesFolder = "Resources";
        private const string PathToOutput = "Generated";
        private const string OutputClassName = "GameResources";

        private static bool _pendingGenerate;

        public const string AutoGeneratePrefKey = "ResourceClassGenerator_AutoGenerate";

        [MenuItem("Tools/Resource Generator/Generate Resource Class")]
        public static void GenerateResourceClass()
        {
            GenerateClass();
        }

        private static void GenerateClass()
        {
            var resourcesPath = Path.Combine(Application.dataPath, PathToResourcesFolder);
            if (!Directory.Exists(resourcesPath))
            {
                Debug.LogError("Resources folder not found at path: " + resourcesPath);
                return;
            }

            var outputDirectory = Path.Combine(Application.dataPath, PathToOutput);
            var outputPath = Path.Combine(outputDirectory,           $"{OutputClassName}.cs");

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            var namespaces = new HashSet<string> { "UnityEngine", "UnityEngine.UIElements" };

            var classBuilder = new StringBuilder();
            classBuilder.AppendLine("// This file is auto-generated. Do not modify manually.");
            classBuilder.AppendLine();

            classBuilder.AppendLine($"public static class {OutputClassName}");
            classBuilder.AppendLine("{");

            GenerateClassForFolder(resourcesPath, classBuilder, "    ", namespaces);

            classBuilder.AppendLine("}");

            var finalBuilder = new StringBuilder();
            foreach (var ns in namespaces.OrderBy(n => n))
            {
                finalBuilder.AppendLine($"using {ns};");
            }

            finalBuilder.AppendLine();
            finalBuilder.Append(classBuilder.ToString());

            var isNewFile = !File.Exists(outputPath);
            File.WriteAllText(outputPath, finalBuilder.ToString());

            if (!isNewFile) return;

            AssetDatabase.Refresh();
            Debug.Log($"Generated {OutputClassName} class at: {outputPath}");
        }

        private static void GenerateClassForFolder(string folderPath, StringBuilder classBuilder, string indent,
            HashSet<string> namespaces)
        {
            var subFolders = Directory.GetDirectories(folderPath);
            var files = Directory.GetFiles(folderPath);

            foreach (string subFolder in subFolders)
            {
                var folderName = EscapeToValidIdentifier(Path.GetFileName(subFolder));
                classBuilder.AppendLine($"{indent}public static class {folderName}");
                classBuilder.AppendLine($"{indent}{{");
                GenerateClassForFolder(subFolder, classBuilder, indent + "    ", namespaces);
                classBuilder.AppendLine($"{indent}}}");
            }

            foreach (string file in files)
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                if (string.IsNullOrEmpty(fileNameWithoutExtension)) continue;

                var fileName = EscapeToValidIdentifier(fileNameWithoutExtension);
                var relativePath = GetRelativePath(file);

                var assetType = GetAssetType(file, relativePath, namespaces);
                if (!string.IsNullOrEmpty(assetType))
                {
                    classBuilder.AppendLine(
                        $"{indent}public static {assetType} {fileName} => Resources.Load<{assetType}>(\"{relativePath}\");");
                }
            }
        }

        private static string GetAssetType(string filePath, string relativePath, HashSet<string> namespaces)
        {
            var fileExtension = Path.GetExtension(filePath).ToLower();

            switch (fileExtension)
            {
                case ".prefab":
                    var componentType = GetFirstMonoBehaviourType(relativePath, namespaces);
                    return string.IsNullOrEmpty(componentType) ? "GameObject" : componentType;
                case ".txt":
                case ".json":
                case ".xml":
                    return "TextAsset";
                case ".mp3":
                case ".wav":
                case ".ogg":
                    return "AudioClip";
                case ".png":
                case ".jpg":
                case ".jpeg":
                    return "Texture2D";
                case ".mat":
                    return "Material";
                case ".shader":
                    return "Shader";
                case ".uss":
                    return "StyleSheet";
                case ".asset":
                    return GetScriptableObjectType(relativePath, namespaces);
                default:
                    return null;
            }
        }

        private static string GetScriptableObjectType(string relativePath, HashSet<string> namespaces)
        {
            var fullPath = $"Assets/{PathToResourcesFolder}/" + relativePath + ".asset";
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(fullPath);

            if (asset == null)
            {
                Debug.LogError($"ScriptableObject not found: <color=red>{fullPath}</color>");
                return null;
            }

            var type = asset.GetType();
            if (!string.IsNullOrEmpty(type.Namespace))
            {
                namespaces.Add(type.Namespace);
            }

            return type.Name;
        }

        private static string GetFirstMonoBehaviourType(string relativePath, HashSet<string> namespaces)
        {
            var fullPath = $"Assets/{PathToResourcesFolder}/" + relativePath + ".prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
            if (prefab == null)
            {
                Debug.LogError($"Prefab not found: <color=red>{fullPath}</color>");
                return null;
            }

            var components = prefab.GetComponents<Component>();
            foreach (var component in components)
            {
                if (component == null || component is not MonoBehaviour) continue;

                var type = component.GetType();
                if (!string.IsNullOrEmpty(type.Namespace))
                {
                    namespaces.Add(type.Namespace);
                }

                return type.Name;
            }

            return null;
        }

        private static string EscapeToValidIdentifier(string name)
        {
            var validName = new StringBuilder();
            foreach (var c in name)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    validName.Append(c);
                }
                else
                {
                    validName.Append('_');
                }
            }

            if (char.IsDigit(validName[0]))
            {
                validName.Insert(0, '_');
            }

            var finalName = validName.ToString();
            if (IsCSharpKeyword(finalName))
            {
                finalName = $"@{finalName}";
            }

            return finalName;
        }

        private static bool IsCSharpKeyword(string word)
        {
            string[] keywords =
            {
                "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const",
                "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit",
                "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in",
                "int", "interface", "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator",
                "out", "override", "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte",
                "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw",
                "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void",
                "volatile", "while"
            };

            return ArrayUtility.Contains(keywords, word);
        }

        private static string GetRelativePath(string fullPath)
        {
            var index = fullPath.IndexOf("Resources", StringComparison.Ordinal);
            if (index == -1)
            {
                Debug.LogError($"Could not find 'Resources' in path: {fullPath}");
                return null;
            }

            var relativePath = fullPath.Substring(index + "Resources/".Length);
            return relativePath.Replace("\\", "/").Replace(Path.GetExtension(fullPath), "");
        }

        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
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

    public static class ResourceClassGeneratorMenu
    {
        private const string MenuName = "Tools/Resource Generator/Auto-generate on Asset Changes";

        [MenuItem(MenuName)]
        private static void ToggleAutoGenerate()
        {
            var autoGenerate = !EditorPrefs.GetBool(ResourceClassGenerator.AutoGeneratePrefKey, true);
            EditorPrefs.SetBool(ResourceClassGenerator.AutoGeneratePrefKey, autoGenerate);
            Debug.Log("Auto-generate on Asset Changes set to: " + autoGenerate);
        }

        [MenuItem(MenuName, true)]
        private static bool ToggleAutoGenerateValidate()
        {
            Menu.SetChecked(MenuName, EditorPrefs.GetBool(ResourceClassGenerator.AutoGeneratePrefKey, true));
            return true;
        }
    }
}
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Editor.Generators
{
    public class SceneConstantsGenerator : AssetPostprocessor
    {
        private const string PathToScenes = "Assets/Scenes";
        private const string PathToOutput = "Generated";
        private const string OutputClassName = "Scenes";

        [MenuItem("Tools/Scene Generator/Generate Scene Constants")]
        public static void GenerateSceneConstants()
        {
            GenerateClass();
        }

        private static void GenerateClass()
        {
            var outputDirectory = Path.Combine(Application.dataPath, PathToOutput);
            var outputPath = Path.Combine(outputDirectory, $"{OutputClassName}.cs");

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            var sceneFiles = Directory.GetFiles(PathToScenes, "*.unity", SearchOption.AllDirectories);
            var sceneNames = sceneFiles.Select(Path.GetFileNameWithoutExtension).ToList();

            var classBuilder = new StringBuilder();
            classBuilder.AppendLine("// This file is auto-generated. Do not modify manually.");
            classBuilder.AppendLine();
            classBuilder.AppendLine("public static class Scenes");
            classBuilder.AppendLine("{");

            foreach (var scene in sceneNames)
            {
                var validName = EscapeToValidIdentifier(scene);
                classBuilder.AppendLine($"    public const string {validName} = \"{scene}\";");
            }

            classBuilder.AppendLine("}");

            File.WriteAllText(outputPath, classBuilder.ToString());
            AssetDatabase.Refresh();
            Debug.Log($"Generated {OutputClassName} class at: {outputPath}");
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

            return validName.ToString();
        }
    }
}

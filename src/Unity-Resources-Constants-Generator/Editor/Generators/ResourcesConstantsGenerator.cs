using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor.Generators
{
    public static class ResourcesConstantsGenerator
    {
        private const string OutputPath = "Assets/Generated/ResourcesKeys.cs";

        [MenuItem("Tools/Generate Resources Keys")]
        public static void Generate()
        {
            var allResourceFolders = Directory
                .GetDirectories(Application.dataPath, "Resources", SearchOption.AllDirectories)
                .Select(dir => Path.GetFullPath(dir))
                .ToList();

            if (allResourceFolders.Count == 0)
            {
                Debug.LogWarning("No Resources folders found in the project.");
                return;
            }

            var keys = new HashSet<(string path, string formattedKey)>();

            foreach (var resourceFolder in allResourceFolders)
            {
                var resourceFiles = Directory.GetFiles(resourceFolder, "*.*", SearchOption.AllDirectories)
                    .Where(file => !file.EndsWith(".meta"))
                    .Select(file =>
                    {
                        string relativePath = Path.GetRelativePath(resourceFolder, file);
                        string pathWithoutExtension = Path.ChangeExtension(relativePath, null);
                        string formattedKey = FormatKey(Path.GetFileName(pathWithoutExtension));
                        return (path: pathWithoutExtension, formattedKey);
                    })
                    .ToList();

                foreach (var key in resourceFiles)
                {
                    keys.Add(key);
                }
            }

            if (keys.Count == 0)
            {
                Debug.LogWarning("No valid resource files found in any Resources folders.");
                return;
            }

            var classContent = GenerateClass(keys);

            var directory = Path.GetDirectoryName(OutputPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory ?? throw new InvalidOperationException("Wrong path directory"));
            }

            File.WriteAllText(OutputPath, classContent);
            AssetDatabase.Refresh();

            Debug.Log(
                $"ResourcesKeys.cs was generated at {OutputPath} with {keys.Count} keys from {allResourceFolders.Count} Resources folders");
        }

        private static string GenerateClass(IEnumerable<(string path, string formattedKey)> keys)
        {
            var root = new ClassNode("ResourcesConstants");

            foreach (var (path, formattedKey) in keys.OrderBy(k => k.path))
            {
                var parts = path.Split('/', '\\');
                var currentNode = root;
                
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    string folderName = FormatKey(parts[i]);
                    currentNode = currentNode.GetOrAddChild(folderName);
                }

                
                string keyValue = path.Replace("\\", "/");
                currentNode.constants.Add($"    public const string {formattedKey} = \"{keyValue}\";");
            }

            return root.ToString();
        }

        private static string FormatKey(string key)
        {
            return key.Replace(" ", "_")
                .Replace("-",  "_")
                .Replace(".",  "_")
                .Replace("/",  "_")
                .Replace("\\", "_")
                .Replace("[",  "_")
                .Replace("]",  "_")
                .Replace("&",  "_And_");
        }

        private class ClassNode
        {
            public string name { get; }
            public List<string> constants { get; } = new();
            public Dictionary<string, ClassNode> children { get; } = new();

            public ClassNode(string name)
            {
                this.name = name;
            }

            public ClassNode GetOrAddChild(string name)
            {
                if (!children.TryGetValue(name, out var child))
                {
                    child = new ClassNode(name);
                    children[name] = child;
                }

                return child;
            }

            public override string ToString()
            {
                var lines = new List<string>();
                lines.Add($"public static class {name}");
                lines.Add("{");

                foreach (var constant in constants)
                {
                    lines.Add(constant);
                }

                foreach (var child in children.Values.OrderBy(c => c.name))
                {
                    if (constants.Count > 0 && child == children.Values.First())
                    {
                        lines.Add("");
                    }

                    lines.AddRange(child.ToString().Split('\n').Select(l => "    " + l));
                }

                lines.Add("}");
                return string.Join("\n", lines);
            }
        }
    }
}
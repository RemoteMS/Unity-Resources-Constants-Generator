using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Editor.Generators
{
    public class ResourcesConstantsGeneratorWindow : EditorWindow
    {
        private string _outputPath;
        private string _fileExtensions;
        private string _className;
        private bool _includeSubfolders;
        private bool _autoGenerateOnChange;
        private bool _openWindowOnFileAdd;
        private Vector2 _scrollPosition;

        public static class DefaultValues
        {
            public const bool AutoGenerateOnChange = false;
            public const bool OpenWindowOnFileAdd = true;
        }

        private const string WindowTitle = "Resources Constants Generator";
        private static readonly Vector2 MinWindowSize = new Vector2(400, 300);

        [MenuItem(itemName: "Tools/Resources Constants Generator/Settings...", priority = 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<ResourcesConstantsGeneratorWindow>(WindowTitle);
            window.minSize = MinWindowSize;
        }

        private void OnEnable()
        {
            _outputPath = EditorPrefs.GetString(EditorPrefsConstants.OutputPath,
                ResourcesConstantsGenerator.DefaultValues.OutputPath);
            _fileExtensions = EditorPrefs.GetString(EditorPrefsConstants.FileExtensions,
                ResourcesConstantsGenerator.DefaultValues.FileExtensions);
            _className = EditorPrefs.GetString(EditorPrefsConstants.ClassName,
                ResourcesConstantsGenerator.DefaultValues.ClassName);
            _includeSubfolders = EditorPrefs.GetBool(EditorPrefsConstants.IncludeSubfolders,
                ResourcesConstantsGenerator.DefaultValues.IncludeSubfolders);
            _autoGenerateOnChange = EditorPrefs.GetBool(EditorPrefsConstants.AutoGenerateOnChange,
                DefaultValues.AutoGenerateOnChange);
            _openWindowOnFileAdd = EditorPrefs.GetBool(EditorPrefsConstants.OpenWindowOnFileAdd,
                DefaultValues.OpenWindowOnFileAdd);

            Debug.Log(
                $"OnEnable: AutoGenerateOnChange={_autoGenerateOnChange}, OpenWindowOnFileAdd={_openWindowOnFileAdd}");
        }

        private void OnDisable()
        {
            EditorPrefs.SetString(EditorPrefsConstants.OutputPath,     _outputPath);
            EditorPrefs.SetString(EditorPrefsConstants.FileExtensions, _fileExtensions);
            EditorPrefs.SetString(EditorPrefsConstants.ClassName,      _className);
            EditorPrefs.SetBool(EditorPrefsConstants.IncludeSubfolders,    _includeSubfolders);
            EditorPrefs.SetBool(EditorPrefsConstants.AutoGenerateOnChange, _autoGenerateOnChange);
            EditorPrefs.SetBool(EditorPrefsConstants.OpenWindowOnFileAdd,  _openWindowOnFileAdd);

            Debug.Log(
                $"OnDisable: AutoGenerateOnChange={_autoGenerateOnChange}, OpenWindowOnFileAdd={_openWindowOnFileAdd}");
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            GUILayout.Label("Resources Constants Generator Settings", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.Label("Output Path", EditorStyles.label);
            EditorGUILayout.BeginHorizontal();
            _outputPath = EditorGUILayout.TextField(_outputPath);
            if (GUILayout.Button("Browse", GUILayout.Width(80)))
            {
                var folder = System.IO.Path.GetDirectoryName(_outputPath);
                var defaultFolder = string.IsNullOrEmpty(folder) ? "Assets" : folder;
                var selectedPath =
                    EditorUtility.SaveFilePanel("Select Output Path", defaultFolder, "ResourcesKeys", "cs");

                if (!string.IsNullOrEmpty(selectedPath))
                {
                    _outputPath = FileUtil.GetProjectRelativePath(selectedPath);
                }
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label("Class Name", EditorStyles.label);
            _className = EditorGUILayout.TextField(_className);

            if (string.IsNullOrWhiteSpace(_className))
                EditorGUILayout.HelpBox("Enter the name of the root class (e.g., ResourcesConstants).",
                    MessageType.Warning);

            GUILayout.Space(10);
            GUILayout.Label("File Extensions (comma-separated)", EditorStyles.label);
            _fileExtensions = EditorGUILayout.TextField(_fileExtensions);

            if (string.IsNullOrWhiteSpace(_fileExtensions))
                EditorGUILayout.HelpBox("Example: *.prefab,*.asset,*.png,*.jpg", MessageType.Warning);

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            _includeSubfolders = EditorGUILayout.Toggle("", _includeSubfolders, GUILayout.Width(20));
            GUILayout.Label("Include Subfolders", GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _autoGenerateOnChange = EditorGUILayout.Toggle("", _autoGenerateOnChange, GUILayout.Width(20));
            GUILayout.Label("Auto Generate on Change", GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _openWindowOnFileAdd = EditorGUILayout.Toggle("", _openWindowOnFileAdd, GUILayout.Width(20));
            GUILayout.Label("Open Window on File Add", GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Now"))
            {
                GenerateConstants();
            }

            if (GUILayout.Button("Reset Settings"))
            {
                ResetSettings();
                Repaint();
            }

            if (GUILayout.Button("Default Settings"))
            {
                SetDefaultSettings();
                Repaint();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();
        }

        private void GenerateConstants()
        {
            if (string.IsNullOrWhiteSpace(_className))
            {
                Debug.LogError("Class name cannot be empty. Using default name: " +
                               ResourcesConstantsGenerator.DefaultValues.ClassName);
                _className = ResourcesConstantsGenerator.DefaultValues.ClassName;
            }

            ResourcesConstantsGenerator.Generate(_outputPath, _fileExtensions, _includeSubfolders, _className);
        }

        private void ResetSettings()
        {
            _outputPath = EditorPrefs.GetString(EditorPrefsConstants.OutputPath,
                ResourcesConstantsGenerator.DefaultValues.OutputPath);
            _fileExtensions = EditorPrefs.GetString(EditorPrefsConstants.FileExtensions,
                ResourcesConstantsGenerator.DefaultValues.FileExtensions);
            _className = EditorPrefs.GetString(EditorPrefsConstants.ClassName,
                ResourcesConstantsGenerator.DefaultValues.ClassName);
            _includeSubfolders = EditorPrefs.GetBool(EditorPrefsConstants.IncludeSubfolders,
                ResourcesConstantsGenerator.DefaultValues.IncludeSubfolders);
            _autoGenerateOnChange = EditorPrefs.GetBool(EditorPrefsConstants.AutoGenerateOnChange,
                DefaultValues.AutoGenerateOnChange);
            _openWindowOnFileAdd = EditorPrefs.GetBool(EditorPrefsConstants.OpenWindowOnFileAdd,
                DefaultValues.OpenWindowOnFileAdd);

            Debug.Log(
                $"ResetSettings: AutoGenerateOnChange={_autoGenerateOnChange}, OpenWindowOnFileAdd={_openWindowOnFileAdd}");
        }

        private void SetDefaultSettings()
        {
            _outputPath = ResourcesConstantsGenerator.DefaultValues.OutputPath;
            _fileExtensions = ResourcesConstantsGenerator.DefaultValues.FileExtensions;
            _className = ResourcesConstantsGenerator.DefaultValues.ClassName;
            _includeSubfolders = ResourcesConstantsGenerator.DefaultValues.IncludeSubfolders;
            _autoGenerateOnChange = DefaultValues.AutoGenerateOnChange;
            _openWindowOnFileAdd = DefaultValues.OpenWindowOnFileAdd;

            EditorPrefs.SetString(EditorPrefsConstants.OutputPath,     _outputPath);
            EditorPrefs.SetString(EditorPrefsConstants.FileExtensions, _fileExtensions);
            EditorPrefs.SetString(EditorPrefsConstants.ClassName,      _className);
            EditorPrefs.SetBool(EditorPrefsConstants.IncludeSubfolders,    _includeSubfolders);
            EditorPrefs.SetBool(EditorPrefsConstants.AutoGenerateOnChange, _autoGenerateOnChange);
            EditorPrefs.SetBool(EditorPrefsConstants.OpenWindowOnFileAdd,  _openWindowOnFileAdd);

            Debug.Log("Settings reset to default values.");
            Debug.Log(
                $"SetDefaultSettings: AutoGenerateOnChange={_autoGenerateOnChange}, OpenWindowOnFileAdd={_openWindowOnFileAdd}");
        }

        public bool AutoGenerateOnChange => _autoGenerateOnChange;
        public string OutputPath => _outputPath;

        public string[] FileExtensions => _fileExtensions.Split(',')
            .Select(ext => ext.Trim())
            .Where(ext => !string.IsNullOrEmpty(ext))
            .ToArray();

        public bool IncludeSubfolders => _includeSubfolders;
        public bool OpenWindowOnFileAdd => _openWindowOnFileAdd;

        public static ResourcesConstantsGeneratorWindow GetWindow(bool openIfNotExists)
        {
            var window = EditorWindow.GetWindow<ResourcesConstantsGeneratorWindow>(WindowTitle, false);
            if (openIfNotExists && window == null)
            {
                window = EditorWindow.GetWindow<ResourcesConstantsGeneratorWindow>(WindowTitle);
            }

            return window;
        }
    }

    public class ResourcesAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            var autoGenerateOnChange = EditorPrefs.GetBool(
                EditorPrefsConstants.AutoGenerateOnChange,
                ResourcesConstantsGeneratorWindow.DefaultValues.AutoGenerateOnChange);
            var openWindowOnFileAdd = EditorPrefs.GetBool(
                EditorPrefsConstants.OpenWindowOnFileAdd,
                ResourcesConstantsGeneratorWindow.DefaultValues.OpenWindowOnFileAdd);
            var outputPath = EditorPrefs.GetString(
                EditorPrefsConstants.OutputPath,
                ResourcesConstantsGenerator.DefaultValues.OutputPath);
            var fileExtensions = EditorPrefs.GetString(
                EditorPrefsConstants.FileExtensions,
                ResourcesConstantsGenerator.DefaultValues.FileExtensions);
            var className = EditorPrefs.GetString(
                EditorPrefsConstants.ClassName,
                ResourcesConstantsGenerator.DefaultValues.ClassName);
            var includeSubfolders = EditorPrefs.GetBool(
                EditorPrefsConstants.IncludeSubfolders,
                ResourcesConstantsGenerator.DefaultValues.IncludeSubfolders);

            Debug.Log(
                $"OnPostprocessAllAssets: AutoGenerateOnChange={autoGenerateOnChange}, OpenWindowOnFileAdd={openWindowOnFileAdd}");

            // Check the changes in the Resources folders
            var hasChangesInResources = false;

            // Checking added files
            foreach (var asset in importedAssets)
            {
                if (IsInResourcesFolder(asset) && !asset.EndsWith(".meta") && !asset.Contains("Generated"))
                {
                    Debug.Log($"Asset added in Resources: {asset}");
                    hasChangesInResources = true;

                    if (openWindowOnFileAdd)
                    {
                        Debug.Log($"Opening window due to OpenWindowOnFileAdd being true. (Asset: {asset})");
                        var window = ResourcesConstantsGeneratorWindow.GetWindow(true);
                        if (window != null)
                        {
                            window.Show();
                            window.Focus();
                        }
                        else
                        {
                            Debug.Log("Failed to get window instance for OpenWindowOnFileAdd.");
                        }
                    }
                    else
                    {
                        Debug.Log($"Window opening skipped due to OpenWindowOnFileAdd being false. (Asset: {asset})");
                    }
                }
            }

            // Checking deleted files
            foreach (var asset in deletedAssets)
            {
                if (IsInResourcesFolder(asset) && !asset.EndsWith(".meta") && !asset.Contains("Generated"))
                {
                    Debug.Log($"Asset deleted in Resources: {asset}");
                    hasChangesInResources = true;
                }
            }

            // Checking migrated files
            for (var i = 0; i < movedAssets.Length; i++)
            {
                var movedTo = movedAssets[i];
                var movedFrom = movedFromAssetPaths[i];

                var wasInResources = IsInResourcesFolder(movedFrom);
                var isInResources = IsInResourcesFolder(movedTo);

                if ((wasInResources || isInResources) && !movedTo.EndsWith(".meta") && !movedTo.Contains("Generated"))
                {
                    Debug.Log($"Asset moved: {movedFrom} -> {movedTo}");
                    hasChangesInResources = true;

                    if (isInResources && openWindowOnFileAdd)
                    {
                        Debug.Log($"Opening window due to OpenWindowOnFileAdd being true. (Moved to: {movedTo})");
                        var window = ResourcesConstantsGeneratorWindow.GetWindow(true);
                        if (window != null)
                        {
                            window.Show();
                            window.Focus();
                        }
                        else
                        {
                            Debug.Log("Failed to get window instance for OpenWindowOnFileAdd.");
                        }
                    }
                    else if (isInResources)
                    {
                        Debug.Log(
                            $"Window opening skipped due to OpenWindowOnFileAdd being false. (Moved to: {movedTo})");
                    }
                }
            }

            //  If changes have been made to the Resources folder and autogeneration is enabled, generate the file
            if (hasChangesInResources && autoGenerateOnChange)
            {
                Debug.Log("Changes detected in Resources folder. Generating constants...");
                ResourcesConstantsGenerator.Generate(outputPath, fileExtensions, includeSubfolders, className);
            }
        }

        private static bool IsInResourcesFolder(string assetPath)
        {
            // Convert the path to the same format (replace \ with /)
            assetPath = assetPath.Replace("\\", "/");

            // Check if the path contains the Resources folder
            var parts = assetPath.Split('/');
            for (var i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "Resources")
                {
                    return true;
                }
            }

            return false;
        }
    }
}
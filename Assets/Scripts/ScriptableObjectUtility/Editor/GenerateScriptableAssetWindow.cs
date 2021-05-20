using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BordlessFramework.Utility
{
    public class GenerateScriptableAssetWindow : EditorWindow
    {
        private const string Title = "Generate Scriptable Asset";
        private string generateScriptableAssetPathText = $"{nameof(generateScriptableAssetPathText)}";
        private List<string> scriptableClassNames = new List<string>();
        private int index;
        private string generatePath;

        private void OnEnable()
        {
            generatePath = EditorPrefs.GetString(generateScriptableAssetPathText, "Assets/");
            LoadAssembly();
        }

        [MenuItem(BordlessFrameworkMenu.MenuName + "/" + BordlessFrameworkMenu.Editor + "/generate custom scriptable asset", priority = BordlessFrameworkMenu.EditorPriority)]
        public static void GenerateCustomScriptableAsset()
        {
            GenerateScriptableAssetWindow window = GetWindow<GenerateScriptableAssetWindow>();
            window.titleContent.text = Title;
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            index = EditorGUILayout.Popup(index, scriptableClassNames.ToArray());
            var selectedClassName = scriptableClassNames.Count > 0 ? scriptableClassNames[index] : "no available ScriptableObject";
            if (GUILayout.Button(scriptableClassNames.Count > 0 ? "Generate" : "no available ScriptableObject"))
            {
                if (scriptableClassNames.Count == 0) { return; }
                CreateScriptableObjectAsset(generatePath, selectedClassName);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(position.height - EditorGUIUtility.singleLineHeight * (1f + 1f + 0.5f));

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("select generate directory"))
            {
                generatePath = EditorUtility.OpenFolderPanel("select generate directory", EditorPrefs.GetString(generateScriptableAssetPathText, "Assets/"), "");
                if (!string.IsNullOrEmpty(generatePath)) { EditorPrefs.SetString(generateScriptableAssetPathText, generatePath); }
            }
            GUILayout.TextField(generatePath);
            //GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();

        }

        private void CreateScriptableObjectAsset(string generatePath, string selectedClassName)
        {
            var path = $"{generatePath}/{selectedClassName}.asset";
            ScriptableObject asset = CreateInstance(selectedClassName);
            CreateAsset(asset, path);
        }

        /// <summary>
        /// 在指定路径创建资源文件
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="path"></param>
        public static void CreateAsset(Object asset, string path)
        {
            if (!asset) { throw new System.Exception("save asset failed, asset is null"); return; }
            // ensure the parent directory of path exist
            if (!System.IO.File.Exists(path)) { System.IO.Directory.CreateDirectory(System.IO.Directory.GetParent(path).FullName); }
            // cut off path, or CreateAsset will fail
            path = path.Substring(path.IndexOf("Assets/"));
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(asset);
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }

        private void LoadAssembly()
        {
            // 收集所有 CustomScriptableObject 类型
            var assembly = Assembly.GetAssembly(typeof(CustomScriptableObject));
            var types = assembly.GetTypes();

            scriptableClassNames.Clear();
            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                // 用 CustomScriptableObject 而不是 ScriptableObject，因为有的类如EditorWindow也继承自它，这样就会显示很多没用 的类型
                if (type.IsSubclassOf(typeof(CustomScriptableObject))) { scriptableClassNames.Add(type.Name); }
            }
        }
    }
}
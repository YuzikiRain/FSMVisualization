using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BordlessFramework.Utility
{
    public class FSMGraphWindow : EditorWindow
    {
        private FSMGraphView graphView;
        private FSMData currentFSMAsset;

        [MenuItem("BordlessFramework" + "/可视化状态机")]
        private static FSMGraphWindow OpenFSMGraphWindow()
        {
            var window = GetWindow<FSMGraphWindow>();
            window.titleContent.text = "可视化状态机";
            return window;
        }

        [OnOpenAsset(1)]
        public static bool OpenFSMGraphWindow(int instanceID, int line)
        {
            var window = OpenFSMGraphWindow();
            var asset = EditorUtility.InstanceIDToObject(instanceID) as FSMData;
            if (asset) window.Load(asset);
            return true;
        }

        private void OnEnable()
        {
            ConstructGraphView();
            GenerateToolbar();
            LoadRecentFSM();
        }

        /// <summary>
        /// 创建GraphView
        /// </summary>
        private void ConstructGraphView()
        {
            // 默认EditorWindow里只有一个GraphView
            graphView = new FSMGraphView() { name = "Dialogue Graph" };
            // 拉伸到和EditorWindow相同大小
            graphView.StretchToParentSize();
            graphView.rootVisualElement = rootVisualElement;
            // 添加到root
            rootVisualElement.Add(graphView);

        }

        /// <summary>
        /// 创建工具条
        /// </summary>
        private void GenerateToolbar()
        {
            var toolBar = new Toolbar();

            var toolbarMenu = new ToolbarMenu();
            toolbarMenu.text = "file";
            toolbarMenu.menu.AppendAction("load", _ => OpenLoadPanel());
            toolBar.Add(toolbarMenu);

            var saveButton = new Button(Save) { text = "save" };
            toolBar.Add(saveButton);

            toolBar.Add(new ObjectField());

            rootVisualElement.Add(toolBar);
        }

        private string OpenSavePanel()
        {
            var path = EditorUtility.SaveFilePanelInProject("save FSMAsset", "New FSMAsset", "asset", string.Empty);
            //path = AssetDatabase.GenerateUniqueAssetPath(path);
            if (path == null) return path;
            EditorPrefs.SetString("RecentFSMAssetPath", path);
            return path;
        }

        private void Save()
        {
            // 新建的FSMAsset，还未保存
            if (!currentFSMAsset)
            {
                currentFSMAsset = CreateInstance<FSMData>();
                var path = OpenSavePanel();
                AssetDatabase.CreateAsset(currentFSMAsset, path);
                AssetDatabase.Refresh();
            }
            currentFSMAsset.NodeDatas = new List<FSMStateNodeData>();
            graphView.nodes.ForEach(tempNode =>
            {
                FSMStateNode node = tempNode as FSMStateNode;
                var fsmStateNodeData = new FSMStateNodeData();
                fsmStateNodeData.GUID = node.viewDataKey;
                fsmStateNodeData.Position = node.GetPosition().position;

                currentFSMAsset.PortDatas = new List<FSMPortData>();
                int portIndex = 0;
                foreach (Port port in node.outputContainer.Children())
                {
                    var portData = new FSMPortData();
                    var edge = port.connections.First();
                    portData.FromNodeGUID = node.viewDataKey;
                    portData.ToNodeGUID = edge.input.node.viewDataKey;
                    portData.Index = portIndex;
                    portData.ConditionGUID = node.PortIndexToConditions[portIndex];
                    portIndex++;

                    currentFSMAsset.PortDatas.Add(portData);
                };

                currentFSMAsset.NodeDatas.Add(fsmStateNodeData);
            });

            EditorUtility.SetDirty(currentFSMAsset);
            AssetDatabase.SaveAssets();
        }

        private void OpenLoadPanel()
        {
            var defaultPath = EditorPrefs.GetString("RecentFSMAssetPath");
            var path = EditorUtility.OpenFilePanel("load FSMAsset", defaultPath, "asset");
            if (path == null) return;
            path = FileUtil.GetProjectRelativePath(path);
            var asset = AssetDatabase.LoadAssetAtPath<FSMData>(path);

            Load(asset);
        }

        private void Load(FSMData asset)
        {
            if (!asset) return;
            currentFSMAsset = asset;
            if (currentFSMAsset.NodeDatas == null || currentFSMAsset.NodeDatas.Count == 0) return;

            // 清除Node和Edge
            foreach (var edge in graphView.edges.ToList()) graphView.RemoveElement(edge);
            foreach (var node in graphView.nodes.ToList()) graphView.RemoveElement(node);

            foreach (var data in currentFSMAsset.NodeDatas)
            {
                var node = graphView.AddNode(data.Position);
                node.viewDataKey = data.GUID;
                node.SetPosition(new Rect(data.Position, new Vector2(200f, 150f)));
            }

            if (currentFSMAsset.PortDatas == null || currentFSMAsset.PortDatas.Count == 0) return;
            foreach (var portData in currentFSMAsset.PortDatas)
            {
                FSMStateNode fromNode = null;
                FSMStateNode toNode = null;
                foreach (FSMStateNode tempNode in graphView.nodes.ToList())
                {
                    if (tempNode.viewDataKey == portData.FromNodeGUID) fromNode = tempNode;
                    if (tempNode.viewDataKey == portData.ToNodeGUID) toNode = tempNode;
                }
                if (fromNode == null || toNode == null) continue;
                var fromPort = fromNode.AddCondition();
                fromNode.SetCondition(portData.Index, fromPort, portData.ConditionGUID);
                graphView.Add(fromPort.ConnectTo(toNode.InputPort));
            };
        }

        private void LoadRecentFSM()
        {
            var path = EditorPrefs.GetString("RecentFSMAssetPath");
            if (path == null) return;
            var asset = AssetDatabase.LoadAssetAtPath<FSMData>(path);

            Load(asset);
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(graphView);
        }

    }
}
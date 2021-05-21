using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BordlessFramework.Utility
{
    public class FSMGraphView : GraphView
    {
        public VisualElement rootVisualElement;
        internal static List<FSMVisualState> AvailableStates = new List<FSMVisualState>();
        internal static List<FSMVisualCondition> AvailableConditions = new List<FSMVisualCondition>();
        public List<FSMPortData> PortDatas = new List<FSMPortData>();
        private readonly Vector2 defaultNodeSize = new Vector2(x: 150, y: 200);

        public FSMGraphView()
        {
            // 设置缩放
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            // 
            this.AddManipulator(new ContentDragger());
            // 
            this.AddManipulator(new SelectionDragger());
            // 添加矩形选择框工具
            this.AddManipulator(new RectangleSelector());

            // 初始化可用State
            InitAvailableState();
            // 初始化可用Condition
            InitAvailableCondition();
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            ports.ForEach((port) =>
            {
                if (startPort != port && startPort.node != port.node)
                    compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (evt.target is GraphView)
            {
                Vector2 graphMousePosition = contentViewContainer.WorldToLocal(Event.current.mousePosition);
                evt.menu.AppendAction("Add State", dropdownMenuAction => AddNode(graphMousePosition));
                evt.menu.AppendSeparator();
            }

            if (evt.target is GraphView || evt.target is Node || evt.target is Group)
            {
                evt.menu.AppendAction("Cut", delegate
                 {
                     CutSelectionCallback();
                 }, (DropdownMenuAction a) => canCutSelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }

            if (evt.target is GraphView || evt.target is Node || evt.target is Group)
            {
                evt.menu.AppendAction("Copy", delegate
                {
                    CopySelectionCallback();
                }, (DropdownMenuAction a) => canCopySelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }

            if (evt.target is GraphView)
            {
                evt.menu.AppendAction("Paste", delegate
                {
                    PasteCallback();
                }, (DropdownMenuAction a) => canPaste ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }

            if (evt.target is GraphView || evt.target is Node || evt.target is Group || evt.target is Edge)
            {
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("Delete", delegate
                {
                    DeleteSelectionCallback(AskUser.DontAskUser);
                }, (DropdownMenuAction a) => canDeleteSelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }

            if (evt.target is GraphView || evt.target is Node || evt.target is Group)
            {
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("Duplicate", delegate
                {
                    DuplicateSelectionCallback();
                }, (DropdownMenuAction a) => canDuplicateSelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                evt.menu.AppendSeparator();
            }
        }

        private static void InitAvailableCondition()
        {
            foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(FSMVisualCondition)}"))
            {
                AvailableConditions.Add(AssetDatabase.LoadAssetAtPath<FSMVisualCondition>(AssetDatabase.GUIDToAssetPath(guid)));
            }
        }

        private static void InitAvailableState()
        {
            foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(FSMVisualState)}"))
            {
                AvailableStates.Add(AssetDatabase.LoadAssetAtPath<FSMVisualState>(AssetDatabase.GUIDToAssetPath(guid)));
            }
        }

        /// <summary>
        /// 添加节点
        /// </summary>
        public FSMStateNode AddNode()
        {
            return AddNode(Vector2.zero);
        }

        /// <summary>
        /// 添加节点
        /// </summary>
        public FSMStateNode AddNode(Vector2 position)
        {
            var node = new FSMStateNode(this);

            node.SetPosition(new Rect(position, defaultNodeSize));
            node.RefreshExpandedState();
            node.RefreshPorts();

            AddElement(node);
            return node;
        }


    }
}
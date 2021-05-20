using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BordlessFramework.Utility
{
    public class FSMStateNode : Node
    {
        public Port InputPort;
        public Dictionary<int, string> PortIndexToConditions = new Dictionary<int, string>();
        private FSMGraphView fsmGraphView;
        private Button selectStateButton;

        public FSMStateNode(FSMGraphView fsmGraphView)
        {
            this.fsmGraphView = fsmGraphView;
            //title = "title";
            titleContainer.RemoveAt(0);

            selectStateButton = new Button(SelectState) { text = "Select State" };
            titleContainer.Insert(0, selectStateButton);

            inputContainer.Add(InputPort = AddInputPort());
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Add Condition", _ => AddCondition());
            evt.menu.AppendSeparator();

            base.BuildContextualMenu(evt);
        }

        private void SelectState()
        {
            var searchWindow = ScriptableObject.CreateInstance<StateSearchWindow>();
            searchWindow.Init(this, FSMGraphView.StateTypes);
            SearchWindow.Open(new SearchWindowContext(), searchWindow);
        }

        public void SetState(Type type)
        {
            selectStateButton.text = type.Name;
        }

        public Port AddCondition()
        {
            var outputPort = InstantiateOutputPort();
            outputContainer.Add(outputPort);

            RefreshExpandedState();
            RefreshPorts();

            return outputPort;
        }

        public Port InsertCondition(int index)
        {
            var outputPort = InstantiateOutputPort();
            outputContainer.Insert(index, outputPort);

            RefreshExpandedState();
            RefreshPorts();

            return outputPort;
        }

        private Port InstantiateOutputPort()
        {
            var outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            outputPort.portName = string.Empty;

            var selectConditionButton = new Button(() =>
            {
                var searchWindow = ScriptableObject.CreateInstance<ConditionSearchWindow>();
                searchWindow.Init(this, FSMGraphView.AvailableConditions, outputPort, outputContainer.childCount - 1);
                SearchWindow.Open(new SearchWindowContext(), searchWindow);
            })
            { text = "Select Condition", name = "Select Condition" };
            selectConditionButton.style.backgroundColor = new StyleColor(Color.clear);
            outputPort.contentContainer.Add(selectConditionButton);

            // Delete button
            Button deleteButton = new Button(() =>
            {
                outputPort.RemoveFromHierarchy();
                outputPort.DisconnectAll();
                foreach (var edge in outputPort.connections)
                {
                    edge.input.Disconnect(edge);
                    fsmGraphView.Remove(edge);
                }
            })
            { text = "x", };
            outputPort.contentContainer.Add(deleteButton);

            return outputPort;
        }

        private Port AddInputPort()
        {
            var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            inputPort.portName = string.Empty;
            outputContainer.Add(inputPort);

            RefreshExpandedState();
            RefreshPorts();

            return inputPort;
        }

        public void SetCondition(int portIndex, Port port, FSMCondition condition)
        {
            Button button = port.contentContainer.Query<Button>("Select Condition");
            button.text = condition.GetType().Name;
            PortIndexToConditions[portIndex] = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(condition));
        }

        public void SetCondition(int portIndex, Port port, string conditionGUID)
        {
            Button button = port.contentContainer.Query<Button>("Select Condition");
            var conditionAsset = AssetDatabase.LoadAssetAtPath<FSMCondition>(AssetDatabase.GUIDToAssetPath(conditionGUID));
            button.text = conditionAsset.GetType().Name;
            PortIndexToConditions[portIndex] = conditionGUID;
        }

    }
}
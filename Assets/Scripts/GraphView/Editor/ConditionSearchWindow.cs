using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BordlessFramework.Utility
{
    public class ConditionSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private FSMStateNode fsmNode;
        private Port selectedPort;
        private int selectedPortIndex;
        private List<FSMCondition> conditions = new List<FSMCondition>();

        public void Init(FSMStateNode node, List<FSMCondition> conditions, Port port, int selectedPortIndex)
        {
            fsmNode = node;
            this.selectedPort = port;
            this.selectedPortIndex = selectedPortIndex;
            this.conditions = conditions;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>();
            searchTreeEntries.Add(new SearchTreeGroupEntry(new GUIContent("Condition")));
            foreach (var condition in conditions)
            {
                searchTreeEntries.Add(new SearchTreeEntry(new GUIContent(condition.GetType().Name)) { level = 1, userData = condition });
            }

            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            fsmNode.SetCondition(selectedPortIndex, selectedPort, searchTreeEntry.userData as FSMCondition);
            return true;
        }

    }
}
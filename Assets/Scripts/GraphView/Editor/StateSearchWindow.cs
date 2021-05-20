using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BordlessFramework.Utility
{
    public class StateSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private FSMStateNode fsmNode;
        private List<Type> stateTypes = new List<Type>();

        public void Init(FSMStateNode node, List<Type> stateTypes)
        {
            fsmNode = node;
            this.stateTypes = stateTypes;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>();
            searchTreeEntries.Add(new SearchTreeGroupEntry(new GUIContent("State")));
            foreach (var type in stateTypes)
            {
                searchTreeEntries.Add(new SearchTreeEntry(new GUIContent(type.Name)) { level = 1, userData = type });
            }

            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            fsmNode.SetState(searchTreeEntry.userData as Type);
            return true;
        }

    }
}
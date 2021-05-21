using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BordlessFramework.Utility
{
    public class StateSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private FSMStateNode fsmNode;
        private List<FSMVisualState> states = new List<FSMVisualState>();

        public void Init(FSMStateNode node, List<FSMVisualState> stateTypes)
        {
            fsmNode = node;
            this.states = stateTypes;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>();
            searchTreeEntries.Add(new SearchTreeGroupEntry(new GUIContent("State")));
            foreach (var state in states)
            {
                searchTreeEntries.Add(new SearchTreeEntry(new GUIContent(state.GetType().Name)) { level = 1, userData = state });
            }

            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            fsmNode.SetState(searchTreeEntry.userData as FSMVisualState);
            return true;
        }

    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
{

    private DialogueGraphView _graphView;
    private EditorWindow _window;

    public void Init(EditorWindow window, DialogueGraphView graphView)
    {

        _window = window;
        _graphView = graphView;
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        var tree = new List<SearchTreeEntry>
        {
            new SearchTreeGroupEntry(new GUIContent(text: "Create Elements"), level: 0),
            new SearchTreeGroupEntry(new GUIContent(text: "Dialogue Node"), level: 1),
            new SearchTreeEntry(new GUIContent(text: "    Dialogue Node"))
            {
                userData = new DialogueNode(),level = 2
            },
        };
        return tree;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        var worldMousePosition = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent,
            context.screenMousePosition - _window.position.position);
        var localMousePosition = _graphView.contentContainer.WorldToLocal(worldMousePosition);
        switch (SearchTreeEntry.userData)
        {
            case DialogueNode dialogueNode:
                _graphView.CreateNode("Dialogue Node", "", "", localMousePosition);
                return true;
            default:
                return false;
        }
    }
}

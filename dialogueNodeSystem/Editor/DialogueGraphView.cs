using System; 
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;


public class DialogueGraphView : GraphView
{
    public readonly Vector2 defaultNodeSize = new Vector2(x: 150, y: 200);

    public char fix = 't';

    public Blackboard Blackboard;
    public List<ExposedProperty> ExposedProperties = new List<ExposedProperty>();

    private NodeSearchWindow _searchWindow;

    public DialogueGraphView(EditorWindow editorWindow)
    {
        styleSheets.Add(Resources.Load<StyleSheet>(path: "DialogueGraph"));
        SetupZoom(ContentZoomer.DefaultMinScale,ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var grid = new GridBackground();
        Insert(index: 0, grid);
        grid.StretchToParentSize();

        AddElement(GenerateEntryPointNode());
        AddSearchWindow(editorWindow);


    }

    private void AddSearchWindow(EditorWindow editorWindow)
    {
        _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
        _searchWindow.Init(editorWindow,this);
        nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
    }

    public void AddPropertyToBlackBoard(ExposedProperty exposedProperty)
    {
        var localPropertyName = exposedProperty.PropertyName;
        var localPropertyValue = exposedProperty.PropertyValue;

        while (ExposedProperties.Any(x => x.PropertyName == localPropertyName))
            localPropertyName = $"{localPropertyName}"+ $"{1}";


        var property = new ExposedProperty();
        property.PropertyName = localPropertyName;
        property.PropertyValue = localPropertyValue;
        ExposedProperties.Add(property);

        var container = new VisualElement();
        var blackboardField = new BlackboardField { text = property.PropertyName, typeText = "string property" };
        container.Add(blackboardField);


        var propertyValueTextField = new TextField(label: "Value:")
        {
            value = localPropertyValue
        };
        propertyValueTextField.RegisterValueChangedCallback(evt =>
        {
            var changingPropertyIndex = ExposedProperties.FindIndex(x => x.PropertyName == property.PropertyName);
            ExposedProperties[changingPropertyIndex].PropertyValue = evt.newValue;
        });

        var blackBoardValueRow = new BlackboardRow(blackboardField,propertyValueTextField);
        container.Add(blackBoardValueRow);

        Blackboard.Add(container);
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();
        var startPortView = startPort;

        ports.ForEach(funcCall: (port) =>
        {
            var portView = port;
            if (startPort != port && startPort.node != port.node)
                compatiblePorts.Add(port);
        });

        return compatiblePorts;
    }


    private Port GeneratePort(DialogueNode node, Direction portDirection,Port.Capacity capacity = Port.Capacity.Single)
    {
        return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
    }


    private DialogueNode GenerateEntryPointNode()
    {
        var node = new DialogueNode
        {
            title = "START",
            GUID = Guid.NewGuid().ToString(),
            DialogueID = "",
            DialogueChar = "",
            DialogueText = "ENTRYPOINT",
            EntryPoint = true
        };

        var generatedPort = GeneratePort(node, Direction.Output);
        generatedPort.portName = "Next";
        node.outputContainer.Add(generatedPort);


        node.capabilities &= ~Capabilities.Movable;
        node.capabilities &= ~Capabilities.Deletable;

        node.RefreshExpandedState();
        node.RefreshPorts();

        node.SetPosition(new Rect(x: 100, y: 200, width: 100, height: 150));
        return node;
    }

    public void CreateNode(string nodeName, string nodeID, string nodeChar, Vector2 position)
    {
        AddElement(CreateDialogueNode(nodeName, nodeID, nodeChar, position));
    }


    public DialogueNode CreateDialogueNode(string nodeName, string nodeID, string nodeChar, Vector3 position)
    {
        var dialogueNode = new DialogueNode
        {
            title = nodeName,
            DialogueID = nodeID,
            DialogueChar = nodeChar,
            DialogueText = nodeName,
            GUID = Guid.NewGuid().ToString()

        };

        var inputPort = GeneratePort(dialogueNode, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";
        dialogueNode.inputContainer.Add(inputPort);

        dialogueNode.styleSheets.Add(Resources.Load<StyleSheet>(path: "Node"));

        var button = new Button(clickEvent: () => { AddChoicePort(dialogueNode); });
        button.text = "Add Choice";
        dialogueNode.titleContainer.Add(button);

        var DialogueNodeID = new TextField(label: "DialogueID", maxLength: 15000, multiline: false, isPasswordField: false, maskChar: fix); 
        DialogueNodeID.RegisterValueChangedCallback(evt =>
        {
            dialogueNode.DialogueID = evt.newValue;
            dialogueNode.title = evt.newValue;

        });

        var DialogueNodeChar = new TextField(label: "Charapter", maxLength: 15000, multiline: false, isPasswordField: false, maskChar: fix); ;
        DialogueNodeChar.RegisterValueChangedCallback(evt =>
        {
            dialogueNode.DialogueChar = evt.newValue;

        });

        var textField = new TextField(label: string.Empty, maxLength: 15000, multiline: true, isPasswordField: false, maskChar: fix); 
        textField.RegisterValueChangedCallback(evt =>
        {
            //dialogueNode.DialogueText = evt.newValue;
            dialogueNode.DialogueText = evt.newValue;
            //dialogueNode.title = evt.newValue;

        });

        textField.SetValueWithoutNotify(dialogueNode.DialogueText);
        DialogueNodeID.SetValueWithoutNotify(dialogueNode.DialogueID);
        DialogueNodeChar.SetValueWithoutNotify(dialogueNode.DialogueChar);

        // Added special id for Dialogue Node
        dialogueNode.mainContainer.Add(DialogueNodeID);

        // Added charapter textbox in Node system
        dialogueNode.mainContainer.Add(DialogueNodeChar);

        // Added Dialogue field in Node system
        dialogueNode.mainContainer.Add(textField);

        dialogueNode.RefreshExpandedState();
        dialogueNode.RefreshPorts();
        dialogueNode.SetPosition(new Rect(position, defaultNodeSize));

        return dialogueNode;
    }

    public void AddChoicePort (DialogueNode dialogueNode, string overriddenPortName = "")
    {
        var generatedPort = GeneratePort(dialogueNode, Direction.Output);


        var oldLabel = generatedPort.contentContainer.Q<Label>(name: "type");
        generatedPort.contentContainer.Remove(oldLabel);



        var outputPortCount = dialogueNode.outputContainer.Query(name: "connector").ToList().Count;
        generatedPort.portName = $"Choice {outputPortCount}";

        //var choicePortName = string.IsNullOrEmpty(overriddenPortName) ? $"Choice { outputPortCount + 1}" : overriddenPortName;
        var choicePortName = string.IsNullOrEmpty(overriddenPortName) ? $"{ outputPortCount + 1}:        " : overriddenPortName;

        var textFiled = new TextField
        {
            name = string.Empty,
            value = choicePortName
        };
        textFiled.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);
        generatedPort.contentContainer.Add(new Label(text: " "));
        generatedPort.contentContainer.Add(textFiled);
        var deleteButton = new Button(() => RemovePort(dialogueNode, generatedPort))
        {
            text = "X"
        };
        generatedPort.contentContainer.Add(deleteButton);

        generatedPort.portName = choicePortName;
        
        dialogueNode.outputContainer.Add(generatedPort);
        dialogueNode.RefreshPorts();
        dialogueNode.RefreshExpandedState();
    }

    private void RemovePort(DialogueNode dialogueNode, Port generatedPort)
    {
        var targetEdge = edges.ToList()
            .Where(x => x.output.portName == generatedPort.portName && x.output.node == generatedPort.node);

        if (targetEdge.Any())
        {
            var edge = targetEdge.First();
            edge.input.Disconnect(edge);
            RemoveElement(targetEdge.First());
        }


        dialogueNode.outputContainer.Remove(generatedPort);
        dialogueNode.RefreshPorts();
        dialogueNode.RefreshExpandedState();
    }
}

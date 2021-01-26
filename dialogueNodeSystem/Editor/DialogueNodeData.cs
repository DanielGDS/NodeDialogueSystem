using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueNodeData 
{
    public string NodeGUID;
    public string DialogueID;
    [Multiline] public string DialogueChar;
    [TextArea(10, 3)] public string DialogueText;
    public Vector2 Position;
}

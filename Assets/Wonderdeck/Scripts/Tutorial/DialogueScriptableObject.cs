using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New DialogueScriptableObject", menuName = "Wonderdeck/Dialogue/[DIALOGUE] New Dialogue")]
public class DialogueScriptableObject : ScriptableObject
{
    public int TutorialStepIndex;
    public List<string> DialogueList = new List<string>();
}

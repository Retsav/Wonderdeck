using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioConfig", menuName = "Wonderdeck/AudioConfig")]
public class AudioConfig : ScriptableObject
{
    public List<string> lilyVoiceLinesStandPaths;
    public List<string> lilyVoiceLinesHitPaths;
    public List<string> callumVoiceLinesStandPaths;
    public List<string> callumVoiceLinesHitPaths;
    public List<string> cardSwooshPaths;

    public string dialogueBoxPath;
    public string uiClickPath;
    public string useItemPath;
    public string openInventoryPath;
    public string tokenPlaced;
    public string standDrawPath;
    public string winPath;
}

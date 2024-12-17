using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "DeckConfig", menuName = "Wonderdeck/BlackjackConfig")]
public class BlackjackConfig : ScriptableObject
{
    public int cardsToDeal = 2;
    public int baseScoreThreshold = 21;
}

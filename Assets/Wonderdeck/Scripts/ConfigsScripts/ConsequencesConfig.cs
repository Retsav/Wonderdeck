using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;


[CreateAssetMenu(fileName = "ConsequencesConfig", menuName = "Wonderdeck/ConsequencesConfig")]
public class ConsequencesConfig : ScriptableObject
{
    public List<ConsequenceData> consequenceDatas = new List<ConsequenceData>();
}



[Serializable]
public class ConsequenceData
{
    public string consequenceID = Guid.NewGuid().ToString();
    public int healthPercentage;
    [SerializeReference] public BaseConsequence consequence;
}

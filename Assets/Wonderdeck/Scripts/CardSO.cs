using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "Card", menuName = "Wonderdeck/Card")]
public class CardSO : ScriptableObject
{
    [FormerlySerializedAs("cardId")] public string CardId; 
    public event EventHandler<string> NameChangedEvent;
    
    public Sprite CardFace;
    public Sprite CardBack;
    

    public List<ScriptableObject> DrawCardEffects = new List<ScriptableObject>();
    public List<ScriptableObject> PlayCardEffects = new List<ScriptableObject>();
    public List<ScriptableObject> DiscardCardEffects = new List<ScriptableObject>();

    public string cardFacePath;
    public string cardBackPath;


    
    private void OnEnable()
    {
        if (CardFace == null) CardFace = Resources.Load<Sprite>("FaceCard");
        if (CardBack == null) CardBack = Resources.Load<Sprite>("BackCard");
    }

    public void OnNameChanged(string newName) => NameChangedEvent?.Invoke(this, newName);
}

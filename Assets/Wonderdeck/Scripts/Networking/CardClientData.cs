using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CardClientData
{
    public string CardName { get; private set; }
    public string CardID { get; private set; }
    public Sprite CardFace { get; private set; }
    public Sprite CardBack { get; private set; }
    
    public CardClientData() {}
    
    public CardClientData(string cardName, string cardID, Sprite cardFace, Sprite cardBack)
    {
        CardName = cardName;
        CardID = cardID;
        CardFace = cardFace;
        CardBack = cardBack;
    }
}

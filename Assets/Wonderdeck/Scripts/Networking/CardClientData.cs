using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Serializing;
using UnityEngine;

public class CardClientData
{
    public string CardName { get; private set; }
    public string CardID { get; private set; }
    public string CardFaceSpritePath { get; private set; }
    public string CardBackSpritePath { get; private set; }
    
    public CardClientData() {}
    
    public CardClientData(string cardName, string cardID, string cardFace, string cardBack)
    {
        CardName = cardName;
        CardID = cardID;
        CardFaceSpritePath = cardFace;
        CardBackSpritePath = cardBack;
    }
}

public static class CardClientDataSerializer
{
    public static void WriteCardClientData(this Writer writer, CardClientData data)
    {
        writer.WriteString(data.CardName);
        writer.WriteString(data.CardID);
        writer.WriteString(data.CardFaceSpritePath);
        writer.WriteString(data.CardBackSpritePath);
    }

    public static CardClientData ReadCardClientData(this Reader reader)
    {
        CardClientData data = new CardClientData(reader.ReadString(), reader.ReadString(), reader.ReadString(), reader.ReadString());
        return data;
    }

}

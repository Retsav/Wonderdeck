using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Serializing;
using UnityEngine;

public class CardClientData
{
    public bool IsHidden { get; private set; }
    public PlayerType Owner { get; private set; }
    public string CardName { get; private set; }
    public string CardID { get; private set; }
    public string CardFaceSpritePath { get; private set; }
    public string CardBackSpritePath { get; private set; }
    
    public CardClientData(PlayerType owner)
    {
        IsHidden = true;
        Owner = owner;
        CardName = null;
        CardID = Guid.NewGuid().ToString();
        CardFaceSpritePath = null;
        CardBackSpritePath = null;
    }
    
    public CardClientData(bool isHidden, string cardName, string cardID, string cardFace, string cardBack, PlayerType owner)
    {
        IsHidden = isHidden;
        CardName = cardName;
        Owner = owner;
        CardID = cardID;
        CardFaceSpritePath = cardFace;
        CardBackSpritePath = cardBack;
    }
    
}

public static class CardClientDataSerializer
{
    public static void WriteCardClientData(this Writer writer, CardClientData data)
    {
        writer.WriteBoolean(data.IsHidden);
        writer.WriteString(data.CardName);
        writer.WriteString(data.CardID);
        writer.WriteString(data.CardFaceSpritePath);
        writer.WriteString(data.CardBackSpritePath);
        writer.Write(data.Owner);
    }

    public static CardClientData ReadCardClientData(this Reader reader)
    {
        CardClientData data = new CardClientData(reader.ReadBoolean(), reader.ReadString(), reader.ReadString(), reader.ReadString(), reader.ReadString(), reader.Read<PlayerType>());
        return data;
    }

}

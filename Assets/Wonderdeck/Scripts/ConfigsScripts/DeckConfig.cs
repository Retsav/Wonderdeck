using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "DeckConfig", menuName = "Wonderdeck/DeckConfig")]
public class DeckConfig : ScriptableObject
{
    public List<Card> cards = new List<Card>();
}

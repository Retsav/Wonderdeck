using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "ItemConfig", menuName = "Wonderdeck/ItemConfig")]
public class ItemConfig : ScriptableObject
{
    public List<CardSO> itemCards = new List<CardSO>();
    public List<CardSO> consequenceItemCardsFirstTier = new List<CardSO>();
    public List<CardSO> consequenceItemCardsSecondTier = new List<CardSO>();
    public List<CardSO> consequenceItemCardsThirdTier = new List<CardSO>();
}

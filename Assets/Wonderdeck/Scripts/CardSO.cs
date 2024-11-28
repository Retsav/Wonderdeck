using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Card", menuName = "Wonderdeck/Card")]
public class CardSO : ScriptableObject
{
    public Sprite CardFace;
    public float CardValue;
    
    [SerializeReference]
    public List<ICardEffect> DrawCardEffects = new List<ICardEffect>();
    [SerializeReference]
    public List<ICardEffect> PlayCardEffects = new List<ICardEffect>();
    [SerializeReference]
    public List<ICardEffect> DiscardCardEffects = new List<ICardEffect>();
}

using UnityEngine;
using Zenject;


[CreateAssetMenu(fileName = "New AddValueEffect", menuName = "Wonderdeck/Card Effects/[CARD EFFECT] Add Value")]
public class AddValueEffect : ScriptableObject, ICardEffect
{
    public float cardValue = 1;

    
    public void OnExecute(PlayerType playerType)
    {

            
        
    }
    
}

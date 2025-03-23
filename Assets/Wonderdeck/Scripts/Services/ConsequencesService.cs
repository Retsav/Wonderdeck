using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsequencesService : IConsequencesService
{
    public List<string> AppliedConsequenceIDsFirstPlayer { get; set; } = new();
    public List<string> AppliedConsequenceIDsSecondPlayer { get; set; } = new();
    
    public event EventHandler<ConsequenceAppliedEventArgs> ConsequenceAppliedEvent;
    public void OnConsequenceApplied(BaseConsequence consequence, string consequenceID, PlayerType playerType) => ConsequenceAppliedEvent?.Invoke(this, new ConsequenceAppliedEventArgs(consequence, consequenceID, playerType));
}

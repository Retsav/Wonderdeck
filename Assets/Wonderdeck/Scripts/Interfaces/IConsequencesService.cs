using System;
using System.Collections.Generic;

public interface IConsequencesService
{
    public List<string> AppliedConsequenceIDsFirstPlayer { get; set; }
    public List<string> AppliedConsequenceIDsSecondPlayer { get; set; }
    public event EventHandler<ConsequenceAppliedEventArgs> ConsequenceAppliedEvent;
    public void OnConsequenceApplied(BaseConsequence consequence, string consequenceID, PlayerType playerType);
    public int GetHighestConsequenceTierOnPlayer(PlayerType playerType);
    public ConsequenceData GetConsequenceData(string id);
}



public class ConsequenceAppliedEventArgs : EventArgs
{
    public BaseConsequence Consequence;
    public string ConsequenceID;
    public PlayerType PlayerType;

    public ConsequenceAppliedEventArgs(BaseConsequence consequence, string consequenceID, PlayerType playerType)
    {
        Consequence = consequence;
        ConsequenceID = consequenceID;
        PlayerType = playerType;
    }
}

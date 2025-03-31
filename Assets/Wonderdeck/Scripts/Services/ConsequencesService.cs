using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsequencesService : IConsequencesService
{
    public List<string> AppliedConsequenceIDsFirstPlayer { get; set; } = new();
    public List<string> AppliedConsequenceIDsSecondPlayer { get; set; } = new();

    private ConsequencesConfig _consequencesConfig;
    
    public event EventHandler<ConsequenceAppliedEventArgs> ConsequenceAppliedEvent;
    public void OnConsequenceApplied(BaseConsequence consequence, string consequenceID, PlayerType playerType) => ConsequenceAppliedEvent?.Invoke(this, new ConsequenceAppliedEventArgs(consequence, consequenceID, playerType));
    public int GetHighestConsequenceTierOnPlayer(PlayerType playerType)
    {
        var listToCheck = playerType == PlayerType.Player1
            ? AppliedConsequenceIDsFirstPlayer
            : AppliedConsequenceIDsSecondPlayer;
        var highestTierFound = -1;
        for (int i = 0; i < listToCheck.Count; i++)
        {
            var id = listToCheck[i];
            var data = GetConsequenceData(id);
            if (data.consequenceTier > highestTierFound)
                highestTierFound = data.consequenceTier;
        }
        return highestTierFound;
    }

    public ConsequenceData GetConsequenceData(string id)
    {
        _consequencesConfig ??= DebugConfigLoader.Instance.GetConfig<ConsequencesConfig>();
        if (_consequencesConfig == null)
        {
            Debug.LogError("Consequences config in GetConsequencesData is null");
            return null;
        }

        var consequenceDatas = _consequencesConfig.consequenceDatas;
        for (int i = 0; i < consequenceDatas.Count; i++)
        {
            var consequence = consequenceDatas[i];
            if (consequence.consequenceID != id)
                continue;
            return consequence;
        }
        Debug.LogError("Consequence not found!");
        return null;
    }
}

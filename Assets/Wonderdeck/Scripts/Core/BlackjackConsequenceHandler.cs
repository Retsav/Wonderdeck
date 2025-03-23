using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Zenject;

public class BlackjackConsequenceHandler : NetworkBehaviour
{

    private ConsequencesConfig _consequencesConfig;
    
    private IConsequencesService _consequencesService;
    private IHealthService _healthService;

    [Inject]
    private void ResolveDependencies(IConsequencesService consequencesService, IHealthService healthService)
    {
        _consequencesService = consequencesService;
        _healthService = healthService;
    }
    
    
    public override void OnStartClient()
    {
        if (!NetworkManager.ClientManager.Connection.IsHost)
            return;
        _consequencesConfig = DebugConfigLoader.Instance.GetConfig<ConsequencesConfig>();
        _healthService.DamageAppliedEvent += ApplyConsequence;
    }

    private void ApplyConsequence(object sender, DamageAppliedEventArgs e)
    {
  
        int maxHealth = _healthService.MaxHealth;
    

        int firstPlayerHealth = _healthService.FirstPlayerHealth;
        int secondPlayerHealth = _healthService.SecondPlayerHealth;

        for (var index = 0; index < _consequencesConfig.consequenceDatas.Count; index++)
        {
            var consequenceData = _consequencesConfig.consequenceDatas[index];
            if (_consequencesService.AppliedConsequenceIDsFirstPlayer.Contains(consequenceData.consequenceID))
                continue;
            int thresholdHealth = (maxHealth * consequenceData.healthPercentage) / 100;
            if (firstPlayerHealth <= thresholdHealth)
            {
                _consequencesService.OnConsequenceApplied(consequenceData.consequence, consequenceData.consequenceID, PlayerType.Player1);
                _consequencesService.AppliedConsequenceIDsFirstPlayer.Add(consequenceData.consequenceID);
            }
        }


        for (var index = 0; index < _consequencesConfig.consequenceDatas.Count; index++)
        {
            var consequenceData = _consequencesConfig.consequenceDatas[index];
            int thresholdHealth = (maxHealth * consequenceData.healthPercentage) / 100;
            if (secondPlayerHealth <= thresholdHealth)
            {
                _consequencesService.OnConsequenceApplied(consequenceData.consequence, consequenceData.consequenceID, PlayerType.Player2);
                _consequencesService.AppliedConsequenceIDsSecondPlayer.Add(consequenceData.consequenceID);
            }
        }
    }
}

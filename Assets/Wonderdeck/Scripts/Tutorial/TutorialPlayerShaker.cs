using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Zenject;

public class TutorialPlayerShaker : MonoBehaviour
{
    private ITutorialService _tutorialService;
    
    [Inject]
    private void ResolveDependencies(ITutorialService tutorialService)
    {
        _tutorialService = tutorialService;
    }
    
    public void ShakePlayer()
    {
        transform.DOShakePosition(3f).OnComplete(() =>
        {
            if (_tutorialService.TutorialStep is 6 or 12 or 18)
            {
                _tutorialService.IncreaseTutorialStep();
            }
        });
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class BlackjackCardVisual : NetworkBehaviour
{
    [SerializeField] private Transform firstPlayerCardsSpawnPoint;
    [SerializeField] private Transform secondPlayerCardsSpawnPoint;
    [SerializeField] private GameObject cardPrefab;
    

    [SerializeField] private float cardSpacing = .3f;

    private IBlackjackService _blackjackService;

    [Inject]
    private void ResolveSingleDependencies(IBlackjackService blackjackService)
    {
        _blackjackService = blackjackService;
    }

    public override void OnStartClient()
    {
        _blackjackService.CardsUpdated += OnCardsUpdated;
    }


    private void OnCardsUpdated(object sender, CardsDataUpdatedEventArgs e)
    {
        switch (e.PlayerType)
        {
            case PlayerType.Player1:
                for (int i = 0; i < e.Cards.Count; i++)
                {
                    var cardVisual = Instantiate(cardPrefab, firstPlayerCardsSpawnPoint.position, Quaternion.identity);
                    cardVisual.transform.DOMoveX(-0.4f + i * cardSpacing, 0.3f);
                }
                break;
            case PlayerType.Player2:
                for (int i = 0; i < e.Cards.Count; i++)
                {
                    var cardVisual = Instantiate(cardPrefab, secondPlayerCardsSpawnPoint.position, Quaternion.identity);
                    cardVisual.transform.DOMoveX(-0.4f + i * cardSpacing, 0.3f);
                }
                break;
        }
    }

    private void OnDestroy()
    {
        _blackjackService.CardsUpdated -= OnCardsUpdated;
    }
}

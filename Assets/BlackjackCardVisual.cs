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

    List<CardVisual> spawnedCardVisuals = new List<CardVisual>();
    private IBlackjackService _blackjackService;


    private int firstPlayerSpawnedCardsCount = 0;
    private int secondPlayerSpawnedCardsCount = 0;

    private MaterialPropertyBlock _mpb;

    [Inject]
    private void ResolveSingleDependencies(IBlackjackService blackjackService)
    {
        _blackjackService = blackjackService;
    }

    public override void OnStartClient()
    {
        _blackjackService.CardsUpdated += OnCardsUpdated;
        _mpb = new MaterialPropertyBlock();
    }


    private void OnCardsUpdated(object sender, CardsDataUpdatedEventArgs e)
    {
        switch (e.PlayerType)
        {
            case PlayerType.Player1:
                SpawnAndPositionCards(e.Cards, ref firstPlayerSpawnedCardsCount, firstPlayerCardsSpawnPoint, -0.4f, PlayerType.Player1);
                break;
            case PlayerType.Player2:
                SpawnAndPositionCards(e.Cards, ref secondPlayerSpawnedCardsCount, secondPlayerCardsSpawnPoint, -0.4f, PlayerType.Player2);
                break;
        }
    }
    
    private void SpawnAndPositionCards(List<CardClientData> cards, ref int spawnedCardsCount, Transform spawnPoint, float initialPositionOffset, PlayerType playerType)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (i < spawnedCardsCount) continue;

            var cardVisualPrefab = Instantiate(cardPrefab, spawnPoint.position, NetworkManager.ClientManager.Connection.IsHost ? Quaternion.identity : Quaternion.Euler(new Vector3(0f, 180f, 0f)));
            if (cardVisualPrefab.TryGetComponent(out CardVisual cardVisual))
            {
                var texture = Resources.Load<Texture>($"{cards[i].CardFaceSpritePath}");
                if (texture != null)
                {
                    _mpb.SetTexture("_BaseMap", texture);
                    cardVisual.cardMeshRenderer.SetPropertyBlock(_mpb);
                }
            }

            spawnedCardsCount++;
            cardVisualPrefab.transform.DOMoveX(initialPositionOffset + i * cardSpacing, 0.3f);
        }
    }

    private void OnDestroy()
    {
        _blackjackService.CardsUpdated -= OnCardsUpdated;
    }
}

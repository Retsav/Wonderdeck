using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using TMPro;
using UnityEngine;
using Zenject;

public class BlackjackScoring : NetworkBehaviour
{
    [SerializeField] private GameObject firstPlayerScoreObject;
    [SerializeField] private GameObject secondPlayerScoreObject;
    
    [SerializeField] private TextMeshProUGUI playerOneScoreLabel;
    [SerializeField] private TextMeshProUGUI playerSecondScoreLabel;
    
    
    private IBlackjackService _blackjackService;
    private INetworkingService _networkingService;

    private PlayerType _playerType;
    private bool _hasHiddenCard;

    [Inject]
    private void ResolveDependencies(IBlackjackService blackjackService, INetworkingService networkingService)
    {
        _blackjackService = blackjackService;
        _networkingService = networkingService;
    }
    
    private void OnDestroy() => Unsubscribe();
    private void OnDisable() => Unsubscribe();
    public override void OnStopClient() => Unsubscribe();

    private void Unsubscribe()
    {
        _blackjackService.CardVisualRequested  -= OnVisualRequested;
        _blackjackService.RoundEnd -= OnRoundEnd;
        _blackjackService.ScoreThresholdChanged -= ScoreThresholdChanged;
        _blackjackService.CardPlayed -= OnCardPlayed;
        _blackjackService.CardsUpdated -= OnCardsUpdated;
    }

    public override void OnStartClient()
    {
        _playerType = NetworkManager.ClientManager.Connection.IsHost ? PlayerType.Player1 : PlayerType.Player2;
        _blackjackService.CardVisualRequested += OnVisualRequested;
        _blackjackService.RoundEnd += OnRoundEnd;
        _blackjackService.ScoreThresholdChanged += ScoreThresholdChanged;
        _blackjackService.CardsUpdated += OnCardsUpdated;
        _blackjackService.CardPlayed += OnCardPlayed;
        if (_networkingService.GetPlayerType(NetworkManager.ClientManager.Connection) != PlayerType.Player2) return;
        firstPlayerScoreObject.transform.Rotate(new Vector3(0f, 180f, 0f));
        secondPlayerScoreObject.transform.Rotate(new Vector3(0f, 180f, 0f));

    }

    private void OnCardsUpdated(object sender, CardsDataUpdatedEventArgs e) => RefreshScoresFromServer();

    private void OnCardPlayed(object sender, CardPlayedEventArgs e) => RefreshScoresFromServer();

    private void ScoreThresholdChanged() => OnVisualRequested(null, null);


    private void OnRoundEnd(object sender, EventArgs e)
    {
        playerOneScoreLabel.text = $"{0}/21";
        playerSecondScoreLabel.text = $"{0}/21";
    }

    private void OnVisualRequested(object sender, CardVisualRequestedEventArgs e)
    {
        RefreshScoresFromServer();
    }

    private void RefreshScoresFromServer()
    {
        GetScoreServerRpc(_blackjackService.LocalFirstPlayerCards, NetworkManager.ClientManager.Connection, PlayerType.Player1, _playerType);
        GetScoreServerRpc(_blackjackService.LocalSecondPlayerCards, NetworkManager.ClientManager.Connection, PlayerType.Player2, _playerType);
    }

    [ServerRpc(RequireOwnership = false)]
    private void GetScoreServerRpc(List<CardClientData> cardClientDataList, NetworkConnection conn, PlayerType playerType, PlayerType currentPlayer)
    {
        float score = 0;
        _hasHiddenCard = false;
        foreach (var card in cardClientDataList)
        {
            if (card.IsHidden && card.Owner != currentPlayer)
            {
                _hasHiddenCard = true;
                continue;
            }
            var cardSO = _blackjackService.GetCardByID(card.CardID);
            if (cardSO == null) continue;
            foreach (var cardEffect in cardSO.DrawCardEffects)
                if (cardEffect is AddValueEffectSO effect) score += effect.cardValue;
        }
        UpdateScoringObserverRpc(playerType, conn, score, _blackjackService.CurrentScoreThreshold, _hasHiddenCard);
    }
    

    [ObserversRpc]
    private void UpdateScoringObserverRpc(PlayerType player,  NetworkConnection conn, float score, int threshold, bool hasHiddenCard)
    {
        if (conn != NetworkManager.ClientManager.Connection)
            return;
        string result = "";
        switch (player)
        {
            case PlayerType.Player1:
                if (hasHiddenCard)
                    result = score <= 0 ? $"?/{threshold}" : $"{score}+?/{threshold}";
                else
                    result = $"{score}/{threshold}";
                playerOneScoreLabel.text = result;
                break;
            case PlayerType.Player2:
                if (hasHiddenCard)
                    result = score <= 0 ? $"?/{threshold}" : $"{score}+?/{threshold}";
                else
                    result = $"{score}/{threshold}";
                playerSecondScoreLabel.text = result;
                break;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using EasyTextEffects;
using FishNet.Connection;
using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class DamageCalculatorUI : NetworkBehaviour
{
    [SerializeField] private GameObject firstPlayerDamageCalculatorGameObject;
    [SerializeField] private GameObject secondPlayerDamageCalculatorGameObject;
    [SerializeField] private TextMeshProUGUI firstPlayerDamageLabel;
    [SerializeField] private TextMeshProUGUI secondPlayerDamageLabel;
    [SerializeField] private TextEffect firstTextEffect;
    [SerializeField] private TextEffect secondTextEffect;
    
    
    private IBlackjackService _blackjackService;
    private IHealthService _healthService;
    private IPostProcessingService _postProcessingService;
    private Color _orginalTextColor;

    private PlayerType _playerType;
    private bool _hasHiddenCard;

    [Inject]
    private void ResolveDependencies(IBlackjackService blackjackService, IHealthService healthService, IPostProcessingService postProcessingService)
    {
        _blackjackService = blackjackService;
        _healthService = healthService;
        _postProcessingService = postProcessingService;
    }
    
    private void OnDestroy() => Unsubscribe();
    private void OnDisable() => Unsubscribe();
    public override void OnStopClient() => Unsubscribe();

    private void Unsubscribe()
    {
        _blackjackService.RefreshScoreEvent -= OnRefreshScore;
        _blackjackService.CardVisualRequested  -= OnVisualRequested;
        _blackjackService.RoundEnd -= OnRoundEnd;
        _blackjackService.ScoreThresholdChanged -= ScoreThresholdChanged;
        _blackjackService.CardPlayed -= OnCardPlayed;
        _blackjackService.CardsUpdatedObserverEvent -= OnCardsUpdated;
        _blackjackService.CardEffectsResolved -= OnCardsResolved;
        _postProcessingService.activateTextChange -= ActivateTextChange;
    }

    public override void OnStartClient()
    {
        _playerType = ClientManager.Connection.IsHost ? PlayerType.Player1 : PlayerType.Player2;
        _orginalTextColor = firstPlayerDamageLabel.color;
        _blackjackService.RefreshScoreEvent += OnRefreshScore;
        _blackjackService.CardVisualRequested += OnVisualRequested;
        _blackjackService.RoundEnd += OnRoundEnd;
        _blackjackService.ScoreThresholdChanged += ScoreThresholdChanged;
        _blackjackService.CardsUpdatedObserverEvent += OnCardsUpdated;
        _blackjackService.CardPlayed += OnCardPlayed;
        _blackjackService.CardEffectsResolved += OnCardsResolved;
        _postProcessingService.activateTextChange += ActivateTextChange;
        if (_playerType != PlayerType.Player2) return;
        firstPlayerDamageCalculatorGameObject.transform.Rotate(new Vector3(0f, 180f, 0f));
        secondPlayerDamageCalculatorGameObject.transform.Rotate(new Vector3(0f, 180f, 0f));
    }

    private void ActivateTextChange(object sender, EventArgs e)
    {
        firstTextEffect.enabled = true;
        secondTextEffect.enabled = true;
        firstTextEffect.Refresh();
        secondTextEffect.Refresh();
    }

    private void OnRefreshScore(object sender, EventArgs e) => RefreshScoresFromServer();

    private void OnCardsResolved(object sender, EventArgs e) => RefreshScoresFromServer();

    private void OnCardsUpdated(object sender, CardsDataUpdatedEventArgs e) => RefreshScoresFromServer();

    private void OnCardPlayed(object sender, CardPlayedEventArgs e) => RefreshScoresFromServer();

    private void ScoreThresholdChanged() => OnVisualRequested(null, null);
    
    private void OnVisualRequested(object sender, CardVisualRequestedEventArgs e) => RefreshScoresFromServer();


    private void OnRoundEnd(object sender, EventArgs e)
    {
        firstPlayerDamageLabel.text = $"0";
        secondPlayerDamageLabel.text = $"0";
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
            if (card.IsHidden && string.IsNullOrEmpty(card.CardName))
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
        var damage = (int)Math.Abs(score - threshold);
        var damageModifier = player == PlayerType.Player1 
            ? _healthService.FirstPlayerDamageModifier 
            : _healthService.SecondPlayerDamageModifier;
        var label = player == PlayerType.Player1 
            ? firstPlayerDamageLabel 
            : secondPlayerDamageLabel;
        if (damageModifier > 0)
        {
            damage += damageModifier;
            label.DOColor(Color.red, 0.3f);
        } else if (damageModifier < 0)
        {
            damage += damageModifier;
            damage = Math.Max(damage, 0);
            label.DOColor(Color.green, 0.3f);
        }
        else
            label.DOColor(_orginalTextColor, 0.3f);
        var damageText = damage.ToString();
        if (hasHiddenCard)
            damageText += "/?";
        label.text = damageText;
    }
}

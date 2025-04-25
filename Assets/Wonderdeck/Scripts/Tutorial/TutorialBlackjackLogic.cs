using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TutorialBlackjackLogic : MonoBehaviour
{
    [SerializeField] private TutorialHandler tutorialHandler;
    [SerializeField] private TutorialCardResolver tutorialCardResolver;
    [SerializeField] private TutorialPlayerHealthVisuals tutorialPlayerHealthVisuals;
    [SerializeField] private TutorialInventoryLogic _tutorialInventoryLogic;
    
    
    
    
    [SerializeField] private TutorialAnimationController playerAnimationController;
    [SerializeField] private TutorialAnimationController arbiterAnimationController;

    [SerializeField] private TutorialPlayerShaker playerShaker;
    [SerializeField] private TutorialPlayerShaker arbiterShaker;
    
    
    
    
    public List<CardSO> FirstPlayerDeck = new List<CardSO>();
    public List<CardSO> ArbiterDeck = new List<CardSO>();
    public int Threshold = 21;


    private IBlackjackService _blackjackService;
    private ITutorialService _tutorialService;
    private ISelectModeService _selectModeService;

    [Inject]
    private void ResolveDependencies(IBlackjackService blackjackService, ITutorialService tutorialService, ISelectModeService selectModeService)
    {
        _blackjackService = blackjackService;
        _tutorialService = tutorialService;
        _selectModeService = selectModeService;
    }
    
    
    private void Start()
    {
        _tutorialService.PlayerDrawRequested += HandleTutorialDraw;
        _tutorialService.PlayerStandRequested += HandleTutorialStand;
        _tutorialService.DialogueFinished += OnDialogueFinished;
        _blackjackService.RequestCardSwap += OnSwapCardRequested;
        _selectModeService.RequestSelectionEffectExecution += RequestSelectionEffectExecution;
        _blackjackService.RevealCardsEvent += RevealCardsEffectPlayed;
    }

    private void OnDestroy()
    {
        _tutorialService.PlayerDrawRequested -= HandleTutorialDraw;
        _tutorialService.PlayerStandRequested -= HandleTutorialStand;
        _tutorialService.DialogueFinished -= OnDialogueFinished;
        _blackjackService.RequestCardSwap -= OnSwapCardRequested;
        _selectModeService.RequestSelectionEffectExecution -= RequestSelectionEffectExecution;
        _blackjackService.RevealCardsEvent -= RevealCardsEffectPlayed;
    }

    private void RevealCardsEffectPlayed(object sender, RevealCardsEventArgs e)
    {
        switch (_tutorialService.TutorialStep)
        {
            case 21:
                _tutorialService.HasHiddenCard = false;
                tutorialCardResolver.RevealHiddenCards();
                _tutorialService.IncreaseTutorialStep();
                break;
        }
    }

    private void RequestSelectionEffectExecution(object sender, SelectionEffectExecutionEventArgs e)
    {
        switch (_tutorialService.TutorialStep)
        {
            case 16:
                _tutorialInventoryLogic.DestroyPersistentItems();
                _tutorialService.IncreaseTutorialStep();
                break;
        }
    }

    private void OnSwapCardRequested(object sender, EventArgs e)
    {
        switch (_tutorialService.TutorialStep)
        {
            case 9:
                tutorialCardResolver.DestroyCard("97bb4f3d-dc5a-45bd-8fe3-e4ba26d7e014", PlayerType.Player2); //Clubs_9
                tutorialCardResolver.DestroyCard("0630eb72-5be7-45a2-a2ed-af5275841c7f", PlayerType.Player1);  //Diamonds_2
                ArbiterDeck.Remove(_blackjackService.GetCardByID("97bb4f3d-dc5a-45bd-8fe3-e4ba26d7e014"));
                FirstPlayerDeck.Remove(_blackjackService.GetCardByID("0630eb72-5be7-45a2-a2ed-af5275841c7f"));
                DealCard(PlayerType.Player1, "97bb4f3d-dc5a-45bd-8fe3-e4ba26d7e014");
                DealCard(PlayerType.Player2, "0630eb72-5be7-45a2-a2ed-af5275841c7f");
                _tutorialService.IncreaseTutorialStep();
                break;
        }
    }

    private void HandleTutorialStand(object sender, EventArgs e)
    {
        switch (_tutorialService.TutorialStep)
        {
            case 4:
                playerAnimationController.DrawCardAnimation();
                _tutorialService.IncreaseTutorialStep();
                break;
            case 10:
                playerAnimationController.DrawCardAnimation();
                _tutorialService.IncreaseTutorialStep();
                break;
            case 17:
                playerAnimationController.DrawCardAnimation();
                _tutorialService.IncreaseTutorialStep();
                break;
        }
    }

    private void OnDialogueFinished(object sender, int e)
    {
        switch (_tutorialService.TutorialStep)
        {
            case 3:
                DealCard(PlayerType.Player2, "76877bb4-1626-4a1e-bc78-7f990a5d2e2c"); //Spades_6
                arbiterAnimationController.DrawCardAnimation();
                _tutorialService.IncreaseTutorialStep();
                break;
            case 5:
                arbiterAnimationController.DrawCardAnimation();
                _tutorialService.IncreaseTutorialStep();
                break;
            case 7:
                DealCard(PlayerType.Player2, "77010eed-b0fc-437b-b3f7-1cf311c7eb75"); //Diamonds_10
                DealCard(PlayerType.Player2, "97bb4f3d-dc5a-45bd-8fe3-e4ba26d7e014"); //Clubs_9
                DealCard(PlayerType.Player1, "fdf0802d-0718-46b9-a1bd-df9f2df27855"); //Clubs_J
                DealCard(PlayerType.Player1, "0630eb72-5be7-45a2-a2ed-af5275841c7f"); //Diamonds_2
                _tutorialService.IncreaseTutorialStep();
                break;
            case 8:
                _tutorialInventoryLogic.DealItem(PlayerType.Player1, "0baf8ff8-247b-4fea-a73a-32244749757c"); //SwapCards
                _tutorialService.IncreaseTutorialStep();
                break;
            case 13:
                DealCard(PlayerType.Player1, "fdf0802d-0718-46b9-a1bd-df9f2df27855"); //Clubs_J
                DealCard(PlayerType.Player1, "97bb4f3d-dc5a-45bd-8fe3-e4ba26d7e014"); //Clubs_9
                DealCard(PlayerType.Player2, "4a92257e-925b-438b-a316-4b31ca8532ce"); //Spades_A
                DealCard(PlayerType.Player2, "2b38f055-eab3-4b65-ac26-1901eaf96f45"); //Hearts_A
                DealCard(PlayerType.Player2, "e7baff6f-673f-44a6-ac7d-b37403a25ac8"); //Diamonds_A
                DealCard(PlayerType.Player2, "69de797e-967a-453d-b68a-00460e88ae19"); //Clubs_A
                _tutorialInventoryLogic.DealItem(PlayerType.Player2, "ed29d6c1-8f51-4af1-8f54-d7ee0524af57");
                _tutorialService.IncreaseTutorialStep();
                break;
            case 14:
                _tutorialInventoryLogic.ArbiterPlayItem(_tutorialService.SecondPlayerItems[0]);
                _tutorialService.IncreaseTutorialStep();
                break;
            case 15:
                DealCard(PlayerType.Player2, "76877bb4-1626-4a1e-bc78-7f990a5d2e2c"); //Spades_6
                arbiterAnimationController.DrawCardAnimation();
                _tutorialService.IncreaseTutorialStep();
                break;
            case 20:
                _tutorialService.HasHiddenCard = true;
                DealHiddenCard(PlayerType.Player2, "76877bb4-1626-4a1e-bc78-7f990a5d2e2c"); //Spades_6
                arbiterAnimationController.DrawCardAnimation();
                _tutorialService.IncreaseTutorialStep();
                break;
        }       
        
    }

    private void HandleTutorialDraw(object sender, EventArgs e)
    {
        switch (_tutorialService.TutorialStep)
        {
            case 2:
                DealCard(PlayerType.Player1, "ed7989a3-445c-469e-a617-08649e42318d"); //Spades_2
                playerAnimationController.DrawCardAnimation();
                _tutorialService.IncreaseTutorialStep();
                break;
            /*case 10:
                DealCard(PlayerType.Player1, "ed7989a3-445c-469e-a617-08649e42318d"); 
                playerAnimationController.DrawCardAnimation();
                _tutorialService.IncreaseTutorialStep();
                break;*/
        }
    }


    public void OnTutorialStepConfirmed(int step)
    {
        switch (step)
        {
            case 1:
                DealCard(PlayerType.Player1, "81fbfa00-72cc-46db-95f2-9e69a67daa3f"); //Diamond_K
                DealCard(PlayerType.Player1, "88ee9bde-ca7f-48a5-9f65-0453e8a35d93"); //Spades_9
                DealCard(PlayerType.Player2, "56029a92-f071-48b4-b54d-d79acb422bda");
                DealCard(PlayerType.Player2, "77010eed-b0fc-437b-b3f7-1cf311c7eb75");
                _tutorialService.IncreaseTutorialStep();
                break;
            case 6:
                arbiterShaker.ShakePlayer();
                tutorialCardResolver.DestroyAllCards();
                tutorialPlayerHealthVisuals.SetHealth(97, PlayerType.Player2);
                FirstPlayerDeck.Clear();
                ArbiterDeck.Clear();
                break;
            case 11:
                DealCard(PlayerType.Player2, "2b38f055-eab3-4b65-ac26-1901eaf96f45"); //HEARTS_A
                arbiterAnimationController.DrawCardAnimation();
                _tutorialService.IncreaseTutorialStep();
                break;
            case 12:
                arbiterAnimationController.DrawCardAnimation();
                arbiterShaker.ShakePlayer();
                tutorialCardResolver.DestroyAllCards();
                tutorialPlayerHealthVisuals.SetHealth(95, PlayerType.Player2);
                FirstPlayerDeck.Clear();
                ArbiterDeck.Clear();
                _tutorialService.FirstPlayerItems.Clear();
                break;
            case 16:
                _tutorialInventoryLogic.DealItem(PlayerType.Player1, "f81ddc16-e3a3-4ffb-a4c4-5bbd9ea5d9dd");
                break;
            case 18:
                arbiterAnimationController.DrawCardAnimation();
                arbiterShaker.ShakePlayer();
                tutorialCardResolver.DestroyAllCards();
                tutorialPlayerHealthVisuals.SetHealth(66, PlayerType.Player2);
                FirstPlayerDeck.Clear();
                ArbiterDeck.Clear();
                _tutorialService.FirstPlayerItems.Clear();
                break;
            case 19:
                DealCard(PlayerType.Player1, "81fbfa00-72cc-46db-95f2-9e69a67daa3f"); //Diamond_K
                DealCard(PlayerType.Player1, "88ee9bde-ca7f-48a5-9f65-0453e8a35d93"); //Spades_9
                DealCard(PlayerType.Player2, "56029a92-f071-48b4-b54d-d79acb422bda");
                DealCard(PlayerType.Player2, "77010eed-b0fc-437b-b3f7-1cf311c7eb75");
                _tutorialService.IncreaseTutorialStep();
                break;
            case 21:
                _tutorialInventoryLogic.DealItem(PlayerType.Player1, "1c92a2e6-1679-4adb-bec1-2266315505d2"); //RevealCard
                break;
        }
    }


    public void DealCard(PlayerType playerType, string cardID)
    {
        var card = _blackjackService.GetCardByID(cardID);
        if (card == null)
            return;
        var playerCards = (playerType == PlayerType.Player1) ? FirstPlayerDeck : ArbiterDeck;
        playerCards.Add(card);
        tutorialCardResolver.ResolveCard(card, playerType, PlayType.Draw);
    }
    
    public void DealHiddenCard(PlayerType playerType, string cardID)
    {
        var card = _blackjackService.GetCardByID(cardID);
        if (card == null)
            return;
        var playerCards = (playerType == PlayerType.Player1) ? FirstPlayerDeck : ArbiterDeck;
        playerCards.Add(card);
        tutorialCardResolver.SpawnCardHidden(card);
    }
    
    
}



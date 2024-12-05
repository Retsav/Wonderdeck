using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackjackService : IBlackjackService
{
    public event EventHandler<CardsDataUpdatedEventArgs> CardsUpdated;
    public void OnCardsUpdated(CardsDataUpdatedEventArgs args) => CardsUpdated?.Invoke(this, args);
}

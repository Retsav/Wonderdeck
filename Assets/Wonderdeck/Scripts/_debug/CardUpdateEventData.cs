

public struct CardUpdateEventData 
{
    public CardClientData cardClientData;
    public PlayerType cardOwner;
    public PlayerType cardReceiver;


    public CardUpdateEventData(CardClientData clientData, PlayerType owner, PlayerType receiver)
    {
        cardClientData = clientData;
        cardOwner = owner;
        cardReceiver = receiver;
    }
}



public struct CardUpdateEventData 
{
    public CardClientData cardClientData;
    public PlayerType cardOwner;
    public PlayerType cardReceiver;
    public bool activateParticles;


    public CardUpdateEventData(CardClientData clientData, PlayerType owner, PlayerType receiver, bool activateParticles = false)
    {
        cardClientData = clientData;
        cardOwner = owner;
        cardReceiver = receiver;
        this.activateParticles = activateParticles;
    }
}


public class DrawValueCardEffect : ICardEffect
{
    private readonly DrawValueCardEffectSO _data;
    private readonly IBlackjackService _blackjackService;
    
    public DrawValueCardEffect(DrawValueCardEffectSO data, IBlackjackService blackjackService)
    {
        _data = data;
        _blackjackService = blackjackService;
    }
    
    public void OnExecute(PlayerType playerType) => _blackjackService.OnGetCardWithSpecificValue(new GetCardWithSpecificValueEventArgs(_data.valueToDraw, playerType));
}

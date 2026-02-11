


[System.Serializable]
public class PayloadCardData
{
    public RPCAction action = RPCAction.PLAY_CARD;
    public int cardId;
}



public enum RPCAction
{ 
    PLAY_CARD,
    HAND_CARD
}
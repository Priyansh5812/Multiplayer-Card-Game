using Fusion;
using UnityEngine;

[CreateAssetMenu(fileName = "CardRegistry", menuName = "Scriptable Objects/CardRegistry")]
public class CardRegistry : ScriptableObject
{
    public CardData[] cards;

    [ContextMenu("Gene")]
    public void GenerateRandomCards()
    {
        cards = new CardData[12];

        for (short i = 0; i < 12; i++)
        {
            cards[i] = new CardData
            {
                id = i,
                Name = $"Card_{i}",
            };
            cards[i].Cost = (short)Random.Range(1, 5);
            cards[i].Power = (short)Mathf.Clamp(Random.Range(cards[i].Cost - 1, cards[i].Cost + 2), 0, Mathf.Infinity);
        }
    }
}


[System.Serializable]
public struct CardData
{
    public int id;
    public string Name;
    public short Cost;
    public short Power;
    public CardState cardState;
    public AbilityData CardAbility;
}

[System.Serializable]

public struct NetCardData : INetworkStruct
{
    public int deckIndex;
    public CardState cardState;

    public void InitializeFromCardData(int deckIndex , CardData data)
    { 
        this.deckIndex = deckIndex;
        cardState = data.cardState;
    }

    public void SetState(CardState state) => this.cardState = state;
}



[System.Serializable]
public struct AbilityData
{
    public Ability type;
    public short value;
}

public enum Ability
{   
    None,
    GainPoints,
    StealPoints,
    DoublePower,
    DrawExtraCard,
    DiscardOpponentRandomCard,
    DestroyOpponentCardInPlay 
}

public enum CardState
{ 
    DECK,
    HAND,
    PLAYED,
    GRAVE
}
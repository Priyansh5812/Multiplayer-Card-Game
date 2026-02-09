using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : NetworkBehaviour
{
    [SerializeField] CardRegistry cardRegistry;

    [Networked , Capacity(12)]
    NetworkArray<NetCardData> netDeck => default;
    private List<int> P1_hand = new();
    
    [Networked, Capacity(3)]
    NetworkArray<int> P2_hand => default;

    [Networked , Capacity(12)]
    NetworkArray<int> P1_Play => default;
    [Networked , Capacity(12)]
    NetworkArray<int> P2_Play => default;

    [Networked]
    int P1_DeckIndex
    {
        get; set;
    } = 0;
   
    [Networked]
    int P2_DeckIndex
    {
        get; set;
    } = 1;


    public override void Spawned()
    {
        base.Spawned();
        EventManager.OnGameLogicEstablished?.Invoke(this);
    }

    public void InitializeCardLogic()
    {
        InitializeDeck();
        InitializeHands();
    }

    private void InitializeDeck()
    {
        if (!Runner.IsServer)
            return;

        for (int i = 0; i < cardRegistry.cards.Length; i++)
        {
            CardData card = cardRegistry.cards[i];
            NetCardData nCard = default;
            nCard.InitializeFromCardData(i , card);
            netDeck.Set(i, nCard);
        }
    }

    private void InitializeHands()
    {
        if (!Runner.IsServer)
            return;

        // Host Hand Init
        for (int i = 0; i < 3; i++)
        {
            P1_hand.Add(netDeck[P1_DeckIndex].deckIndex);
            netDeck[P1_DeckIndex].SetState(CardState.HAND);
            P1_DeckIndex += 2;
            Debug.Log(P1_hand[i]);
        }

        for (int i = 0; i < 3; i++)
        {
            P2_hand.Set(i , netDeck[P2_DeckIndex].deckIndex);
            netDeck[P2_DeckIndex].SetState(CardState.HAND);
            P2_DeckIndex += 2;
            Debug.Log(P2_hand[i]);
        }

    }


    public IEnumerable<int> GetHand()
    { 
        return Runner.GameMode == GameMode.Host ? P1_hand : P2_hand;
    }

    public (bool , CardData) GetCardData(int cardDeckIndex)
    {
        if (cardDeckIndex < 0 || cardDeckIndex >= cardRegistry.cards.Length)
            return (false, default);

        return (true, cardRegistry.cards[cardDeckIndex]);
    }
}

using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : NetworkBehaviour
{
    [SerializeField] CardRegistry cardRegistry;

    private List<CardData> deck = new();
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
            deck.Add(card);
        }       
    }

    private void InitializeHands()
    {
        if (!Runner.IsServer)
            return;

        // Host Hand Init
        for (int i = 0; i < 3; i++)
        {
            P1_hand.Add(deck[P1_DeckIndex].id);
            deck[P1_DeckIndex].SetState(CardState.HAND);
            P1_DeckIndex += 2;
            Debug.Log(P1_hand[i]);
        }

        for (int i = 0; i < 3; i++)
        {
            P2_hand.Set(i, deck[P2_DeckIndex].id);
            deck[P2_DeckIndex].SetState(CardState.HAND);
            P2_DeckIndex += 2;
            Debug.Log(P2_hand[i]);
        }

    }

    public IEnumerable<int> GetHand()
    { 
        return Runner.GameMode == GameMode.Host ? P1_hand : P2_hand;
    }
}

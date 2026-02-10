using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLogic : NetworkBehaviour
{
    [Networked, Capacity(3)]
    NetworkArray<int> PlayerHand => default;

    [Networked, Capacity(3)]
    NetworkArray<int> PlayerPlay=> default;

    private List<int> predictedPlayerHand;
    private List<int> predictedPlayerPlay;

    [Networked]
    public GameLogic gameLogic
    {
        get; set;
    }

    [Networked]
    public int deckStartingIndex
    { get; set; } = 0;


    public void Initialize(GameLogic logic , int deckStartingIndex)
    { 
        this.gameLogic = logic;
        this.deckStartingIndex = deckStartingIndex;
    }

    public override void Spawned()
    {
        this.gameObject.name = $"Player_{Object.InputAuthority.PlayerId}";
        Debug.Log($"From {this.gameObject.name} Is Logic NULL : {this.gameLogic == null}");
        if (HasInputAuthority)
            EventManager.OnPlayerLogicEstablished.Invoke(this);
        InitListeners();
    }

    private void OnEnable()
    {
        InitListeners();
    }

    private void InitListeners()
    {
        if (Runner == null)
            return;

        if (HasInputAuthority)
        {
            EventManager.OnDealCard.AddListener(DealCard);
        }

        if (Runner.IsServer)
        { 
            EventManager.InitializeHands.AddListener(InitializeHands);
        }
    }

    private void DealCard(int cardID)
    {
       
    }


    private void InitializeHands(int deckIndexIncrement)
    {
        if (!Runner.IsServer)
            return;

        Debug.Log($"Updating Hands at {this.gameObject.name}");

        for (int i = 0; i < 3; i++)
        {
            PlayerHand.Set(i, gameLogic.GetNetCardDataAt(deckStartingIndex).Item2.deckIndex);
            gameLogic.SetCardState(deckStartingIndex, CardState.HAND);
            deckStartingIndex += deckIndexIncrement;
        }

        string str = string.Empty;
        foreach (var i in PlayerHand)
        {
            str += i + " ";
        }

        Debug.Log(str);
    }

    public IEnumerable<int> GetHand() => PlayerHand;

    public (bool, CardData) GetCardData(int index) => gameLogic.GetCardData(index);

    private void DeInitListeners()
    {
        if (Runner == null)
            return;

        Debug.Log($"DeInit from {this.gameObject.name}");

        if (HasInputAuthority)
        {
            EventManager.OnDealCard.RemoveListener(DealCard);
        }

        if (Runner.IsServer)
        {
            EventManager.InitializeHands.RemoveListener(InitializeHands);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {   
        base.Despawned(runner, hasState);
        DeInitListeners();
    }

    private void OnDisable()
    {
        DeInitListeners();
    }
}

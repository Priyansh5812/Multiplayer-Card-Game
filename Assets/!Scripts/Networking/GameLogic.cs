using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;
using System.Threading.Tasks;

public class GameLogic : NetworkBehaviour
{
    [SerializeField] CardRegistry cardRegistry;

    [Networked , Capacity(12)]
    NetworkArray<NetCardData> netDeck => default;

    [Networked]
    TickTimer roundTimer
    { get; set; }

    [Networked]
    int roundNo
    {
        get; set;
    } = 1;

    [Networked]
    int maxCost
    {
        get; set;
    } = 1;


    public override void Spawned()
    {
        base.Spawned();
        EventManager.OnGameLogicEstablished?.Invoke(this);
    }

    private void OnEnable()
    {
        InitListeners();    
    }

    private void InitListeners()
    {
        EventManager.OnRoundTimerEnded.AddListener(RoundTimerEnded);
    }   

    public void InitializeCardLogic()
    {
        InitializeDeck();
    }

    private void InitiateRoundTimer()
    {
        roundTimer = TickTimer.CreateFromSeconds(Runner, 30.0f);
        RPC_StartRoundTimerCallbackRoutine();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer, InvokeLocal = true, TickAligned = true)]
    private void RPC_StartRoundTimerCallbackRoutine()
    { 
        EventManager.OnRoundUpdated.Invoke(roundNo);
        StartCoroutine(RoundTimerCallbackUpdation());
    }

    IEnumerator RoundTimerCallbackUpdation()
    {
        EventManager.OnRoundTimerStarted.Invoke();
        Debug.Log("Round Timer Started");
        while (!roundTimer.Expired(Runner))
        {
            EventManager.OnRoundTimerUpdated.Invoke(roundTimer.RemainingTime(Runner).Value);
            yield return null;
        }

        EventManager.OnRoundTimerUpdated.Invoke(0);
        Debug.Log("Round Timer Ended");
        EventManager.OnRoundTimerEnded.Invoke();
    }

    private void RoundTimerEnded()
    {
        Debug.Log("Round Ended");

        if (Runner.GameMode == GameMode.Host)
            Server_RoundTimerEnded();
        else
            Client_RoundTimerEnded();
    }

    private async void Server_RoundTimerEnded()
    {
        Client_RoundTimerEnded(); // Host is also a Client

        if (roundNo > Constants.MaxRounds)
        {
            // TODO: End Game
            Debug.Log("Game Ended");
        }
        else
        {
            Debug.Log("Delay Initiated");
            await Task.Delay(1000);
            Debug.Log("Delay Ended");
            roundNo++;
            InitiateRoundTimer();
        }
    }

    private void Client_RoundTimerEnded()
    { 
        
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

    public (bool , NetCardData) GetNetCardDataAt(int index)
    {
        if (index < 0 || index >= netDeck.Length)
            return (false, default);

        return (true, netDeck[index]);
    }

    public bool SetCardState(int index , CardState state)
    {
        if (index < 0 || index >= netDeck.Length)
            return false;

        Debug.Log("Req Card State : "+state.ToString());
        var deckData = netDeck[index];
        deckData.SetState(state);
        netDeck.Set(index , deckData);
        Debug.Log(netDeck[index].cardState);
        return true;
    }

    public IEnumerable<int> GetHand()
    { 
        return Runner.GameMode == GameMode.Host ? null : null;
    }

    public (bool , CardData) GetCardData(int cardDeckIndex)
    {
        if (cardDeckIndex < 0 || cardDeckIndex >= cardRegistry.cards.Length)
            return (false, default);

        return (true, cardRegistry.cards[cardDeckIndex]);
    }


    [Rpc(RpcSources.StateAuthority , RpcTargets.All , InvokeLocal = true, TickAligned = true)]
    public void RPC_InitiateGameStart()
    {
        Debug.Log("Game Started");
        roundNo = 1;
        //EventManager.UpdateHandsAndPlayView.Invoke();
        EventManager.OnStartGame.Invoke();
        InitiateRoundTimer();
    }

    private void DeInitListeners()
    {
        EventManager.OnRoundTimerEnded.RemoveListener(RoundTimerEnded);
    }





    private void OnDisable()
    {
        DeInitListeners();
    }
}

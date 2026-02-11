using Fusion;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Linq;
public class PlayerLogic : NetworkBehaviour
{
    [Networked, Capacity(3)]
    NetworkArray<int> PlayerHand => default;

    [Networked, Capacity(12)]
    NetworkArray<int> PlayerPlay=> default;

    private List<int> predictedPlayerHand = new();
    private List<int> predictedPlayerPlay = new();

    [Networked]
    public GameLogic gameLogic
    {
        get; set;
    }


    private CardView selectedCard = null;

    [Networked]
    public int deckStartingIndex
    { get; set; } = 0;

    [Networked]
    public int currCost
    {
        get; set;
    } = 1;

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
            EventManager.SetCardSelection.AddListener(SetCardSelection);
            EventManager.OnSelectedCardPlayOrHand.AddListener(OnCardPlayOrHand);
        }

        if (Runner.IsServer)
        { 
            EventManager.InitializeHands.AddListener(InitializeHands);
        }
    }



    private void InitializeHands(int deckIndexIncrement)
    {
        if (!Runner.IsServer)
            return;

        Debug.Log($"Updating Hands at {this.gameObject.name}");


        int tempDeckStartingIndex = this.deckStartingIndex;

        for (int i = 0; i < 3; i++)
        {
            PlayerHand.Set(i, gameLogic.GetNetCardDataAt(tempDeckStartingIndex).Item2.deckIndex);
            gameLogic.SetCardState(tempDeckStartingIndex, CardState.HAND);
            tempDeckStartingIndex += deckIndexIncrement;
        }

        for (int i = 0; i < PlayerPlay.Length; i++)
        {
            PlayerPlay.Set(i, -1);
        }

        RPC_UpdatePredictionArrays();
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority, InvokeLocal = true, TickAligned = false)]
    public void RPC_UpdatePredictionArrays()
    {
        Debug.Log($"Updated Prediction hand and play in {this.gameObject.name}");
        predictedPlayerHand.Clear();
        predictedPlayerPlay.Clear();
        predictedPlayerHand.AddRange(PlayerHand);
        predictedPlayerPlay.AddRange(PlayerPlay);
        EventManager.UpdateHandsAndPlayView.Invoke();
        EventManager.UpdateCardSelectionView.Invoke(selectedCard != null);
    }



    private void SetCardSelection(CardView cardView)
    {
        var cardData = gameLogic.GetNetCardDataAt(cardView.id);

        if (!cardData.Item1 || CardState.DECK == cardData.Item2.cardState || CardState.GRAVE == cardData.Item2.cardState)
            return;

        if (selectedCard == null)
        {
            selectedCard = cardView;
            selectedCard.IsSelected = true;
        }
        else if (selectedCard == cardView)
        {
            selectedCard.IsSelected = false;
            selectedCard = null;
        }
        else 
        {
            selectedCard.IsSelected = false;
            selectedCard = cardView;
            selectedCard.IsSelected = true;
        }

        EventManager.UpdateCardSelectionView.Invoke(selectedCard != null);
    }


    private void OnCardPlayOrHand()
    {
        var cardData = gameLogic.GetNetCardDataAt(selectedCard.id);

        Debug.Log($"{cardData.Item1} : {cardData.Item2.cardState}");

        if (!cardData.Item1 || CardState.DECK == cardData.Item2.cardState || CardState.GRAVE == cardData.Item2.cardState)
            return;

        bool playAction = false;

        if (cardData.Item2.cardState == CardState.PLAYED)
        {
            if (!predictedPlayerPlay.Contains(selectedCard.id))
                return;

            predictedPlayerPlay.Remove(selectedCard.id);
            predictedPlayerHand.Add(selectedCard.id);
        }

        else if (cardData.Item2.cardState == CardState.HAND)
        { 
            if (!predictedPlayerHand.Contains(selectedCard.id))
            {
                return;
            }

            playAction = true;
            predictedPlayerHand.Remove(selectedCard.id);
            predictedPlayerPlay.Add(selectedCard.id);
        }

        Debug.Log("Valid to predict");

        PayloadCardData data = new PayloadCardData()
        {
            action = playAction ? RPCAction.PLAY_CARD : RPCAction.HAND_CARD,
            cardId = selectedCard.id,
        };

        Debug.Log("Reseted Card Selection State");
        EventManager.UpdateCardSelectionView.Invoke(false);
        Debug.Log("Updated Cards View");
        EventManager.UpdateHandsAndPlayView.Invoke();
        selectedCard = null;
        RPC_SendMessage(JsonConvert.SerializeObject(data));
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SendMessage(string json)
    {
        Debug.Log($"Recieved JSON : {json}");

        try
        {   
            JObject obj = JObject.Parse(json);

            switch ((RPCAction)obj["action"].Value<int>())
            {
                case RPCAction.PLAY_CARD:
                    ValidateCardPlayReq(JsonConvert.DeserializeObject<PayloadCardData>(json));
                    break;
                case RPCAction.HAND_CARD:
                    ValidateCardHandReq(JsonConvert.DeserializeObject<PayloadCardData>(json));
                    break;
                default:
                    Debug.LogError("Went into Default");
                    return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }


    private void ValidateCardPlayReq(PayloadCardData data)
    {
        var cardData = gameLogic.GetNetCardDataAt(data.cardId);

        if (!cardData.Item1 || cardData.Item2.cardState != CardState.HAND)
            return;

        int index = -1;
        for (int i = 0; i < PlayerHand.Length; i++)
        {
            if (PlayerHand.Get(i) == data.cardId)
            {
                PlayerHand.Set(i, -1);
                index = i;
                break;
            }
        }

        if (index != -1)
        {
            gameLogic.SetCardState(data.cardId, CardState.PLAYED);
            for (int i = 0; i < PlayerPlay.Length; i++)
            {
                if (PlayerPlay[i] == -1)
                {
                    PlayerPlay.Set(i, data.cardId);
                    break;
                }
            }
        }
        else 
        {
            RPC_UpdatePredictionArrays();
        }

        string str = string.Empty;
        foreach (var i in PlayerHand)
        {
            str += i + " ";
        }
        Debug.Log("New Hand\n"+str);
         str = string.Empty;
        foreach (var i in PlayerPlay)
        {
            str += i + " ";
        }
        Debug.Log("New Play\n"+str);

    }

    private void ValidateCardHandReq(PayloadCardData data)
    {
        var cardData = gameLogic.GetNetCardDataAt(data.cardId);

        if (!cardData.Item1 || cardData.Item2.cardState != CardState.PLAYED)
            return;

        int index = -1;
        for (int i = 0; i < PlayerPlay.Length; i++)
        {
            if (PlayerPlay.Get(i) == data.cardId)
            {
                PlayerPlay.Set(i, -1);
                index = i;
                break;
            }
        }

        if (index != -1)
        {
            gameLogic.SetCardState(data.cardId, CardState.HAND);
            for (int i = 0; i < PlayerHand.Length; i++)
            {
                if (PlayerHand[i] == -1)
                {
                    PlayerHand.Set(i, data.cardId);
                    break;
                }
            }
        }
        else
        {
            RPC_UpdatePredictionArrays();
        }

        string str = string.Empty;
        foreach (var i in PlayerHand)
        {
            str += i + " ";
        }
        Debug.Log("New Hand\n" + str);
        str = string.Empty;
        foreach (var i in PlayerPlay)
        {
            str += i + " ";
        }
        Debug.Log("New Play\n" + str);
    }

    
    public List<int> GetHand() => predictedPlayerHand;
    public List<int> GetPlay() => predictedPlayerPlay;

    public (bool, CardData) GetCardData(int index) => gameLogic.GetCardData(index);

    private void DeInitListeners()
    {
        if (Runner == null)
            return;

        Debug.Log($"DeInit from {this.gameObject.name}");

        if (HasInputAuthority)
        {
            EventManager.OnSelectedCardPlayOrHand.RemoveListener(OnCardPlayOrHand);
            EventManager.SetCardSelection.RemoveListener(SetCardSelection);
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

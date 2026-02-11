using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GameplayController
{
    private GameplayView view;
    private PlayerLogic logic;
    private List<CardView> handCards = new();
    private List<CardView> playCards = new();
    private StringBuilder timerString;
    private StringBuilder roundText;
    public GameplayController(GameplayView view) 
    {
        this.view = view;
        timerString = new StringBuilder();
        roundText = new StringBuilder();
        EventManager.OnPlayerLogicEstablished.AddListener(InitializePlayerLogic);
    }
   

    private void InitializePlayerLogic(PlayerLogic logic)
    {
        Debug.Log($"Player Logic Initialized at Gameplay View {logic == null} , {logic.gameObject.name}");
        this.logic = logic;
    }


    public void UpdateHandPlayView()
    {
        foreach (var i in handCards)
        {
            i.Dispose();
        }

        foreach (var i in playCards)
        {
            i.Dispose();
        }

        handCards.Clear();
        playCards.Clear();

        List<int> hand = logic.GetHand();
        foreach (var i in hand)
        {
            var res = logic.GetCardData(i);
            if (!res.Item1)
            {
                Debug.LogError($"Deck Index not in range with {i}");
                continue;
            }

            AddCardToHand(res.Item2);
        }

        List<int> play = logic.GetPlay();
        foreach (var i in play)
        {
            if (i < 0) // Avoiding values which indicates "No Card" {-1}
                continue;

            var res = logic.GetCardData(i);
            if (!res.Item1)
            {
                Debug.LogError($"Deck Index not in range with {i}");
                continue;
            }

            AddCardToPlay(res.Item2);
        }
    }

    public void UpdateRoundTimerDisplay(float remainingTime)
    { 
        timerString.Clear();
        timerString.Append(Mathf.Ceil(remainingTime));
        view.UpdateRoundTimerText(timerString);
    }

    public void UpdateRoundStat(int roundNo)
    {
        roundText.Clear();
        roundText.Append($"Round - {roundNo}");
        view.UpdateRoundStat(roundText);
    }

    private void AddCardToHand(CardData data)
    {
        CardView view = EventManager.GetCard.Invoke(data);
        handCards.Add(view);
        this.view.AddCardToHand(view);
    }

    private void AddCardToPlay(CardData data)
    {
        CardView view = EventManager.GetCard.Invoke(data);
        playCards.Add(view);
        this.view.AddCardToPlay(view);
    }

    public void InitiateCardSelection()
    {
        EventManager.OnSelectedCardPlayOrHand.Invoke();
        view.SetCardUnselectedState();
    }

}
using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GameplayController
{
    private GameplayView view;
    private PlayerLogic logic;
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


    public void UpdateHand()
    {
        IEnumerable<int> hand = logic.GetHand();
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
        this.view.AddCardToHand(view);
    }

}
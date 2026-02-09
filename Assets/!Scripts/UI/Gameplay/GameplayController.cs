using System.Collections.Generic;
using UnityEngine;

public class GameplayController
{
    private GameplayView view;
    private GameLogic logic;
    public GameplayController(GameplayView view) 
    {
        this.view = view;
        EventManager.OnGameLogicEstablished.AddListener(InitializeGameLogic);
    }
   

    private void InitializeGameLogic(GameLogic logic)
    {
        Debug.Log($"Game Logic Initialized at Gameplay View {logic == null}");
        this.logic = logic;
        InitializeHand();
    }

    private void InitializeHand()
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

    private void AddCardToHand(CardData data)
    {
        CardView view = EventManager.GetCard.Invoke(data);
        this.view.AddCardToHand(view);
    }

}
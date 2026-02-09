using System;
using System.Collections;
using UnityEngine;

public class GameplayView : IMonoState
{

    private ViewStateController viewController;
    private GameplayController controller;
    private GameLogic logic;
    private GameplayViewData data;
    public GameplayView(ViewStateController viewController, GameplayViewData data)
    { 
        this.viewController = viewController;
        this.data = data;
        EventManager.OnGameLogicEstablished.AddListener(InitializeGameLogic);
        controller = new(this);
    }


    public bool IsAlreadyTriggered
    {
        get; private set;
    } = false;

    

    public void OnEnable(Action OnEnableCompleted = null)
    {   
        data.cgMain.alpha = 1.0f;
        data.cgMain.interactable = data.cgMain.blocksRaycasts = true;
        OnEnableCompleted?.Invoke();
    }

    public void Start(Action OnStartCompleted = null)
    {   
        OnStartCompleted?.Invoke();
    }

    private void InitializeGameLogic(GameLogic logic)
    {
        Debug.Log($"Game Logic Initialized at Gameplay View {logic == null}");
        this.logic = logic;
        EventManager.OnGameLogicEstablished.RemoveListener(InitializeGameLogic);
        CheckHand();
    }

    private void CheckHand()
    {
        var hand = logic.GetHand();

        string str = string.Empty;
        foreach (var i in hand)
        {
            str += i.ToString() + "\n";
        }

        data.debugText?.SetText(str);
    }

    public void OnDisable(Action OnDisableCompleted = null)
    {
        OnDisableCompleted?.Invoke();
    }

}

[System.Serializable]
public class GameplayViewData
{
    public CanvasGroup cgMain;
    public TMPro.TextMeshProUGUI debugText;   
}
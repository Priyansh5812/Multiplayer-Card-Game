using System;
using System.Text;
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
        InitListeners();
        OnEnableCompleted?.Invoke();
    }

    private void InitListeners()
    {
        EventManager.OnStartGame.AddListener(controller.UpdateHand);
        EventManager.OnRoundTimerUpdated.AddListener(controller.UpdateRoundTimerDisplay);
        EventManager.OnRoundUpdated.AddListener(controller.UpdateRoundStat);
    }

    public void Start(Action OnStartCompleted = null)
    {   
        OnStartCompleted?.Invoke();
    }

    public void AddCardToHand(CardView cardView)
    {
        cardView.transform.SetParent(data.handParent);
        cardView.transform.localScale = Vector3.one;
        cardView.transform.localPosition = Vector3.zero;
    }

    public void UpdateRoundTimerText(StringBuilder str)
    {
        data.timerText?.SetText(str);
    }

    public void UpdateRoundStat(StringBuilder str)
    {
        data.roundText?.SetText(str);
    }

    private void DeInitListeners()
    {
        EventManager.OnStartGame.RemoveListener(controller.UpdateHand);
        EventManager.OnRoundTimerUpdated.RemoveListener(controller.UpdateRoundTimerDisplay);
        EventManager.OnRoundUpdated.RemoveListener(controller.UpdateRoundStat);
    }

    public void OnDisable(Action OnDisableCompleted = null)
    {
        DeInitListeners();
        OnDisableCompleted?.Invoke();
    }
}

[System.Serializable]
public class GameplayViewData
{
    public CanvasGroup cgMain;
    public RectTransform handParent;
    public TMPro.TextMeshProUGUI timerText;
    public TMPro.TextMeshProUGUI roundText;
}
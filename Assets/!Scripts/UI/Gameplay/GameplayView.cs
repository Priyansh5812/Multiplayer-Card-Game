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

    public void AddCardToHand(CardView cardView)
    {
        cardView.transform.SetParent(data.handParent);
        cardView.transform.localScale = Vector3.one;
        cardView.transform.localPosition = Vector3.zero;
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
    public RectTransform handParent;
}
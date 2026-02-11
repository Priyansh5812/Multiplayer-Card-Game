using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

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
        EventManager.UpdateCardSelectionView.AddListener(UpdateCardSelectionState);
        EventManager.UpdateHandsAndPlayView.AddListener(controller.UpdateHandPlayView);
        EventManager.OnRoundTimerUpdated.AddListener(controller.UpdateRoundTimerDisplay);
        EventManager.OnRoundUpdated.AddListener(controller.UpdateRoundStat);
        data.btn_select.onClick.AddListener(controller.InitiateCardSelection);
    }

    public void Start(Action OnStartCompleted = null)
    {   
        IsAlreadyTriggered = true;
        PrepareStartup();
        OnStartCompleted?.Invoke();
    }

    private void PrepareStartup()
    {
        SetCardUnselectedState();
    }

    public void AddCardToHand(CardView cardView)
    {
        cardView.transform.SetParent(data.handParent);
        cardView.transform.localScale = Vector3.one;
        cardView.transform.localPosition = Vector3.zero;
    }
    public void AddCardToPlay(CardView cardView)
    {
        cardView.transform.SetParent(data.PlayParent);
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
        EventManager.UpdateCardSelectionView.RemoveListener(UpdateCardSelectionState);
        EventManager.UpdateHandsAndPlayView.AddListener(controller.UpdateHandPlayView);
        EventManager.OnRoundTimerUpdated.RemoveListener(controller.UpdateRoundTimerDisplay);
        EventManager.OnRoundUpdated.RemoveListener(controller.UpdateRoundStat);
        data.btn_select.onClick.RemoveListener(controller.InitiateCardSelection);
    }

    public void OnDisable(Action OnDisableCompleted = null)
    {
        DeInitListeners();
        OnDisableCompleted?.Invoke();
    }

    private void UpdateCardSelectionState(bool isSelected)
    {
        if (isSelected)
            SetCardSelectedState();
        else
            SetCardUnselectedState();
    }

    public void SetCardUnselectedState()
    {
        data.btn_select.gameObject.SetActive(false);
    }

    public void SetCardSelectedState()
    { 
        data.btn_select.gameObject.SetActive(true);
    }
}

[System.Serializable]
public class GameplayViewData
{
    public CanvasGroup cgMain;
    public RectTransform handParent;
    public RectTransform PlayParent;
    public TMPro.TextMeshProUGUI timerText;
    public TMPro.TextMeshProUGUI roundText;
    public TMPro.TextMeshProUGUI RemainingCostText;
    public TMPro.TextMeshProUGUI ScoreText;
    public Button btn_forfeit;
    public Button btn_endTurn;
    public Button btn_select;
}
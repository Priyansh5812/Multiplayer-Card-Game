using System;
using System.Threading.Tasks;
using UnityEngine;

public class MenuView : IMonoState
{   
    public bool IsAlreadyTriggered
    {
        get;
        private set;
    }

    private ViewStateController viewStateController;
    private MenuData data;

    public MenuView(ViewStateController viewController , MenuData lobbyData)
    { 
       viewStateController = viewController; 
       data = lobbyData;
    }


    public void OnEnable(Action OnEnableCompleted = null)
    {
        InitListeners();
        OnEnableCompleted?.Invoke();
    }

    private void InitListeners()
    {
        data.startAsClient.onClick.AddListener(StartAsClient);
        data.startAsHost.onClick.AddListener(StartAsHost);
    }


    private void StartAsClient()
    {
        var netInstance = viewStateController.GetNetworkCallbackDispatcher();
        netInstance.RegisterCallbacks(OnSuccess, OnFailed);
        data.cgMain.interactable = false;
        data.statusText?.SetText(data.ConnectingTxt);
        netInstance.StartGameAsClient();
    }

    private void StartAsHost()
    { 
        var netInstance = viewStateController.GetNetworkCallbackDispatcher();
        netInstance.RegisterCallbacks(OnSuccess, OnFailed);
        data.cgMain.interactable = false;
        data.statusText?.SetText(data.ConnectingTxt);
        netInstance.StartGameAsHost();
    }


    private async void OnSuccess()
    {
        data.statusText?.SetText(data.ConnectionSuccess);
        viewStateController.InitiateStateChange(typeof(GameplayView));
    }

    private void OnFailed(string str)
    {
        data.statusText?.SetText(str);
        data.cgMain.interactable = true;
    }

    public void Start(Action OnStartCompleted = null)
    {
        IsAlreadyTriggered = true;
        OnStartCompleted?.Invoke();
    }

    private void DeinitListeners()
    {
        data.startAsClient.onClick.RemoveListener(StartAsClient);
        data.startAsHost.onClick.RemoveListener(StartAsHost);
    }

    public void OnDisable(Action OnDisableCompleted = null)
    {
        data.cgMain.alpha = 0.0f;
        data.cgMain.interactable = data.cgMain.blocksRaycasts = false;
        DeinitListeners();
        OnDisableCompleted?.Invoke();
    }
}



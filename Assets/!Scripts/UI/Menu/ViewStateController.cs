using System.Collections.Generic;
using UnityEngine;
using System;
public class ViewStateController : MonoBehaviour
{
    private readonly Dictionary<Type, IMonoState> stateReg = new();
    private bool isChangingState;
    IMonoState currentState;
    [SerializeField] NetworkCallbackDispatcher netPrefab;
    [SerializeField] MenuData menuData;
    [SerializeField] GameplayViewData gameplayViewData;
    private NetworkCallbackDispatcher netInstance;
    private void Start()
    {
        ConstructMenuStates();
        InitiateStateChange(typeof(MenuView));
    }

    private void ConstructMenuStates()
    {
        stateReg.Add(typeof(MenuView), new MenuView(this , menuData));
        stateReg.Add(typeof(GameplayView), new GameplayView(this, gameplayViewData));
    }


    public NetworkCallbackDispatcher GetNetworkCallbackDispatcher()
    {
        if (netInstance == null)
            netInstance = Instantiate(netPrefab , null);

        return netInstance;
    }


    public void InitiateStateChange(Type newStateType)
    {
        if (isChangingState)
        {
            Debug.LogWarning($"Already changing state.");
            return;
        }

        IMonoState newState;
        if (stateReg.ContainsKey(newStateType))
            newState = stateReg[newStateType];
        else
        {
            Debug.LogError($"Unknown {newStateType} state type");
            return;
        }

        isChangingState = true;
        if (currentState != null)
            currentState.OnDisable(OnInitialCompleted);
        else
            OnInitialCompleted();


        void OnInitialCompleted()
        {
            currentState = newState;

            if (currentState.IsAlreadyTriggered)
                currentState.OnEnable(OnEnableCompleted);
            else
                currentState.OnEnable(OnEnableThenStart);
        }

        void OnEnableThenStart()
        {
            currentState.Start(OnStartCompleted);
        }

        void OnEnableCompleted()
        {
            isChangingState = false;
        }

        void OnStartCompleted()
        {
            isChangingState = false;
        }
    }
}

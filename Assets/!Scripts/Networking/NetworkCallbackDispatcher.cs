using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class NetworkCallbackDispatcher : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] GameLogic gameLogicPrefab;
    [SerializeField] PlayerLogic playerLogicPrefab;

    private NetworkRunner _runner;
    private Dictionary<int, PlayerDataAbs> joinedPlayers;
    private event Action OnSuccessConnected;
    private event Action<string> OnDisconnected;
    private GameLogic gameLogic;
    private bool isGameStartPollingInitiated = false;
    public void StartGameAsHost()
    {
        StartGame(GameMode.Host);
    }

    public void StartGameAsClient()
    { 
        StartGame(GameMode.Client);
    }

    public void RegisterCallbacks(Action OnSuccess , Action<string> OnDisconnected)
    {   
        this.OnSuccessConnected = OnSuccess;
        this.OnDisconnected = OnDisconnected;
    }

    private async void StartGame(GameMode mode)
    {
        _runner = this.gameObject.AddComponent<NetworkRunner>();
        var sceneRef = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (sceneRef.IsValid)
        {
            sceneInfo.AddSceneRef(sceneRef, LoadSceneMode.Additive);
        }

        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "GameRoom",
            Scene = sceneRef,
            PlayerCount = 2,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        OnSuccessConnected?.Invoke();
        Debug.Log("Connected with Server");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {   
        Debug.Log(reason);
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log(reason);
        
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {

        if (runner.IsServer && player == runner.LocalPlayer)
        {
            OnSuccessConnected?.Invoke();
        }

        Debug.Log("New Player Joined : " + player.PlayerId);
        if (runner.IsServer)
        {
            joinedPlayers ??= new();


            if (gameLogic == null)
            { 
                gameLogic = runner.Spawn(gameLogicPrefab, null, null, null, (netRunner,netObj) => 
                {
                    netObj.GetComponent<GameLogic>().InitializeCardLogic();
                });
            }


            var playerLogicInstance = runner.Spawn(playerLogicPrefab, null, null, player, (netRunner, netObj) =>
            {
                netObj.GetComponent<PlayerLogic>().Initialize(gameLogic , joinedPlayers.Count);
            });
            

            joinedPlayers.TryAdd(player.PlayerId, new PlayerDataAbs(player , playerLogicInstance));
            if (!isGameStartPollingInitiated)
            {
                isGameStartPollingInitiated = true;
                StartCoroutine(GameStartRoutine());
            }
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Player Left : " + player.PlayerId);
        if (runner.IsServer)
        {
            if (joinedPlayers.ContainsKey(player.PlayerId))
            {
                runner.Despawn(joinedPlayers[player.PlayerId].playerLogic.GetComponent<NetworkObject>());
                joinedPlayers.Remove(player.PlayerId);
            }
        }
    }


    IEnumerator GameStartRoutine()
    {
        while (joinedPlayers.Count < 2)
        { 
            yield return null;
        }
        EventManager.InitializeHands?.Invoke(joinedPlayers.Count);
        gameLogic.RPC_InitiateGameStart();
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        OnDisconnected?.Invoke(GetShutdownMessage(shutdownReason));
    } 

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        
    }

    private string GetShutdownMessage(ShutdownReason reason)
    {
        switch (reason)
        {
            case ShutdownReason.Ok:
                return "Fusion Shutdown by request";
            case ShutdownReason.GameClosed:
                return "Game you are trying to Join is Closed";
            case ShutdownReason.GameNotFound:
                return "Game you are trying to Join does not exist";
            case ShutdownReason.GameIsFull:
                return "Game you are trying to Join is Full.";
            default:
            case ShutdownReason.Error:
                return "Fusion Shutdown by an Internal Error";
            
        }
    }
}

public class PlayerDataAbs
{   public PlayerDataAbs(PlayerRef playerRef, PlayerLogic playerLogic)
    { 
        this.playerRef = playerRef;
        this.playerLogic = playerLogic;
    }
    public PlayerRef playerRef;
    public PlayerLogic playerLogic;
}
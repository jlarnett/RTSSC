using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RTSNetworkManager : NetworkManager
{

    [SerializeField] private GameObject unitBasePrefab = null;
    [SerializeField] private GameOverHandler gameOverHandlerPrefab = null;

    public static event Action ClientOnConnect;
    public static event Action ClientOnDisconnect;


    private bool isGameInProgress = false;
    public List<RTSPlayer> Players { get; } = new List<RTSPlayer>();


    #region Server

    public override void OnServerConnect(NetworkConnection conn)
    {
        //If game is in progress disconnect connection.
        if (!isGameInProgress) return;
        conn.Disconnect();
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        //Gets player from connection. Removes player from Players List
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();
        Players.Remove(player);

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        //Clears players list & sets game in progress to false
        Players.Clear();
        isGameInProgress = false;
    }

    public void StartGame()
    {
        //If we have 2 player, set game in progress to true & change tell server to change scenes
        if (Players.Count < 2) return;
        isGameInProgress = true;

        ServerChangeScene("Scene_Map_01");
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        //Calls normal spawn player logic. Then spawns the unit spawner in at spawnPoint for Server & over Network for clients.
        base.OnServerAddPlayer(conn);

        //Get player and create new random color
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();
        Players.Add(player);

        //Sets the teams color when a player is added.
        player.SetTeamColor(new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f)));
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        //When server chagnes scene. If this is a valid map spawn GameOverHandlerInstance to Server & Network
        //If the active sceen has the Scene_Map prefix spawn the gameOverHandler
        if (SceneManager.GetActiveScene().name.StartsWith("Scene_Map"))
        {
            //Spawns this in on server
            GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);
            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);

            foreach (RTSPlayer player in Players)
            {
                //For each player in lobby spawn in base instance.
                GameObject baseInstance =  Instantiate(unitBasePrefab, GetStartPosition().position, Quaternion.identity);
                NetworkServer.Spawn(baseInstance, player.connectionToClient);
            }
        }
    }
   
    #endregion


    #region Client

    public override void OnClientConnect(NetworkConnection conn)
    {
        //When client connects invoke event after doing base functions  
        base.OnClientConnect(conn);

        ClientOnConnect?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        //When client disconnects invoke event after doing base functions  
        base.OnClientConnect(conn);

        ClientOnDisconnect?.Invoke();
    }

    public override void OnStopClient()
    {
        //Clears player list
        Players.Clear();
    }

    #endregion



}

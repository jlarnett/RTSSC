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


    //Determines whether game is in progress or not.
    private bool isGameInProgress = false;

    //List of players
    public List<RTSPlayer> Players { get; } = new List<RTSPlayer>();


    #region Server

    public override void OnServerConnect(NetworkConnection conn)
    {
        //When player connects to server & game is inprogress we disconnect
        if (!isGameInProgress) return;
        conn.Disconnect();
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        //Gets player player that disconnects from server & removes from list.
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();
        Players.Remove(player);

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        //When Server stops we just reset player list & inprogress bool
        Players.Clear();
        isGameInProgress = false;
    }

    public void StartGame()
    {
        //Check player count, Set gameinprogress bool & change to online scene.
        if (Players.Count < 2) return;
        isGameInProgress = true;

        //changes scene to maps
        ServerChangeScene("Scene_Map_01");
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        //When Server adds player to scene e.g. Lobby now. Add player to player lobby list. Set team color & set party owner to the first person in lobby.
        //Calls normal spawn player logic. Then spawns the unit spawner in at spawnPoint for Server & over Network for clients.
        base.OnServerAddPlayer(conn);

        //Get player and create new random color
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();
        Players.Add(player);

        //Sets the teams color when a player is added.
        player.SetTeamColor(new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f)));

        //Sets the party owner as whoever is in lobby first.
        player.SetPartyOwner(Players.Count == 1);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        //Whenever the server changes the scene. We check if its an online game map. If so we spawn in our GameOverHandler
        //Then for each player in our Players lobby List<RTSPlayer> we spawn in their base instance @ network start positions.
        //We make sure to give authority to the players connection.
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

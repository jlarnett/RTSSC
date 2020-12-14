using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] private GameObject lobbyUI = null;
    [SerializeField] private Button startGameButton = null;

    private void Start()
    {
        //SUB FROM RTSNETWORKMANAGER client connect event & party owner updated event.
        RTSNetworkManager.ClientOnConnect += HandleClientConnected;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated += AuthorityHandlePartyOwnerStateUpdated;
    }


    private void OnDestroy()
    {
        //UNSUB FROM RTSNETWORKMANAGER client connect event & party owner updated event.
        RTSNetworkManager.ClientOnConnect -= HandleClientConnected;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated -= AuthorityHandlePartyOwnerStateUpdated;
    }

    private void AuthorityHandlePartyOwnerStateUpdated(bool state)
    {
        //Sets the start game button active based on the state passed in. Only Host or first person in lobby has button.
        startGameButton.gameObject.SetActive(state);
    }

    private void HandleClientConnected()
    {
        //Sets lobby UI ACTIVE
        lobbyUI.SetActive(true);
    }

    public void StartGame()
    {
        //Called from Host clients start game butotn. We get RTSPlayer and call start game command. 
        NetworkClient.connection.identity.GetComponentInChildren<RTSPlayer>().CmdStartGame();
    }

    public void LeaveLobby()
    {
        //Handles when player hit leave lobby button. Server & Client
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            //stop client.
            NetworkManager.singleton.StopClient();
            SceneManager.LoadScene(0);
        }
    }
}


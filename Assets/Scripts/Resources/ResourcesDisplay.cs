using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class ResourcesDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text resourcesText = null;

    private RTSPlayer player;

    private void Update()
    {
        if (player == null)
        {
            //This set player on Update. Only sets it until not null
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();               //Get the identity component of the client connection calling this script. This allows us to get RTSPlayer component

            //Once player is found Update UI & subscribe to future Server ResourceUpdates
            if (player != null)
            {
                ClientHandleResourcesUpdated(player.GetResources());
                player.ClientOnResourcesUpdated += ClientHandleResourcesUpdated;
            }
        }
    }


    private void OnDestroy()
    {
        //Unsub
        player.ClientOnResourcesUpdated -= ClientHandleResourcesUpdated;
    }

    private void ClientHandleResourcesUpdated(int resources)
    {
        //Changes Resources UI text
        resourcesText.text = $"Resources: {resources}";
    }
}

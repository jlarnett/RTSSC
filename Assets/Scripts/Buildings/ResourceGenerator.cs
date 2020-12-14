using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Telepathy;
using UnityEngine;

public class ResourceGenerator : NetworkBehaviour
{
    [SerializeField] private Health health = null;
    [SerializeField] private int resourcePerInterval = 25;
    [SerializeField] private float interval = 2f;

    private float resourceTimer;
    private RTSPlayer player;

    public override void OnStartServer()
    {
        resourceTimer = interval;
        player = connectionToClient.identity.GetComponent<RTSPlayer>();     //Can get this component like this because it starts existing when world spawns.

        health.ServerOnDie += ServerHandleDie;
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    [ServerCallback]
    private void Update()
    {
        //Subtract amount of seconds passed from interval amount.
        resourceTimer -= Time.deltaTime;

        //If below Generate resources & reset timer.
        if (resourceTimer <= 0)
        {
            //Generate resources for player
            player.SetResources(player.GetResources() + resourcePerInterval);

            //Reset to,er
            resourceTimer += interval;
        }
    }



    private void ServerHandleGameOver()
    {
        enabled = false;
    }

    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }
}

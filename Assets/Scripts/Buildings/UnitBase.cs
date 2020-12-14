using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitBase : NetworkBehaviour
{
    [SerializeField] private Health health = null;

    public static event Action<int> ServerOnPlayerDie;


    //Events for base spawn & despawn
    public static event Action<UnitBase> ServerOnBaseSpawned;
    public static event Action<UnitBase> ServerOnBaseDespawn;

    #region Server

    public override void OnStartServer()
    {
        //On unitbase spawn invoke subscribe to health & invoke baseSpawned event.
        health.ServerOnDie += ServerHandleDie;
        ServerOnBaseSpawned?.Invoke(this);
    }

    public override void OnStopServer()
    {
        //On unitbase spawn invoke unsubscribe to health & invoke basedespawned event.
        ServerOnBaseDespawn?.Invoke(this);
        health.ServerOnDie -= ServerHandleDie;
    }

    [Server]
    private void ServerHandleDie()
    {
        //Called when health die event is triggered. Destroys base gameobject.
        ServerOnPlayerDie?.Invoke(connectionToClient.connectionId);
        NetworkServer.Destroy(gameObject);
    }


    #endregion



    #region Client




    #endregion


}
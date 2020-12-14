using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class Unit : NetworkBehaviour
{
    [SerializeField] private int resourceCost = 15;
    [SerializeField] private Health health = null;
    [SerializeField] private UnitMovement unitMovement = null;
    [SerializeField] private Targeter targeter = null;

    [SerializeField] private UnityEvent onSelected = null;
    [SerializeField] private UnityEvent onDeselected = null;

    //These events are only being called on the server.
    public static event Action<Unit> ServerOnUnitSpawned;
    public static event Action<Unit> ServerOnUnitDespawn;

    public static event Action<Unit> AuthorityOnUnitSpawned;
    public static event Action<Unit> AuthorityOnUnitDespawn;

    public int GetResourceCost()
    {
        return resourceCost;
    }

    public UnitMovement GetUnitMovement()
    {
        return unitMovement;
    }

    public Targeter GetUnitTargeter()
    {
        return targeter;
    }

    #region Server

    public override void OnStartServer()
    {
        //Trigger onSpawn event. E.G when this script starts no server. We pass the Unit Gameobject attached to this script to RTSPlayer where it adds to correct client.
        ServerOnUnitSpawned?.Invoke(this);

        //Subscribe to Health Death Event
        health.ServerOnDie += ServerHandleDie;
    }

    public override void OnStopServer()
    {
        //UnSub to Health Die Event
        health.ServerOnDie -= ServerHandleDie;


        //Trigger onDespawn event. When this object is despawned from server. We pass the gameobject attacked to the RTSPLAYER script to remove from correct client id.
        ServerOnUnitDespawn?.Invoke(this);
    }

    [Server]
    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    #endregion


    #region Client


    [Client]
    public override void OnStartAuthority()
    {
        if (!hasAuthority) return;
        AuthorityOnUnitSpawned?.Invoke(this);
    }

    [Client]
    public override void OnStopClient()
    {
        if (!hasAuthority) return;
        AuthorityOnUnitDespawn?.Invoke(this);
    }


    [Client]
    public void Select()
    {
        //On selected fire off all Unity events for onSelected

        if (!hasAuthority) return;

        onSelected?.Invoke();
    }

    [Client]
    public void Deselect()
    {
        //On Deselect fire off al unity events for onDeselect 

        if (!hasAuthority) return;          //Verifies that the client has authority over attached object first
        onDeselected?.Invoke();
    }

    #endregion


}

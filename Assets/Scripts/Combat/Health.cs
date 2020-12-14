using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;

    [SyncVar(hook = nameof(HandleHealthUpdated))]
    private int currentHealth;

    public event Action ServerOnDie;

    public event Action<int, int> ClientOnHealthUpdated;

    #region MyRegion

    public override void OnStartServer()
    {
        currentHealth = maxHealth;
        UnitBase.ServerOnPlayerDie += ServerHandlePlayerDie;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnPlayerDie -= ServerHandlePlayerDie;
    }

    [Server]
    private void ServerHandlePlayerDie(int connectionId)
    {
        //If the player who died is not us return
        if (connectionToClient.connectionId != connectionId) return;

        //This kills whatever the health script is attached too. when the player with correct id dies.
        DealDamage(currentHealth);
    }


    [Server]
    public void DealDamage(int damageAmount)
    {
        if (currentHealth == 0) return;
        currentHealth = Mathf.Max(0, currentHealth - damageAmount);

        //Below here is death only.
        if (currentHealth != 0) return;

        //invoke the action above to allow all our different units and stuff to do their death handling.
        ServerOnDie?.Invoke();

        Debug.Log("We died!");
    }



    #endregion


    #region Client

    private void HandleHealthUpdated(int oldHealth, int newHealth)
    {
        ClientOnHealthUpdated?.Invoke(newHealth, maxHealth);
    }


    #endregion
}

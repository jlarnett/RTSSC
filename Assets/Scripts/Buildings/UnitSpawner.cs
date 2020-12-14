using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private Health health = null;
    [SerializeField] private Unit unitPrefab = null;
    [SerializeField] private Transform unitSpawnPoint = null;

    [SerializeField] private TMP_Text remainingUnitText = null;
    [SerializeField] private Image unitProgressImage = null;

    [SerializeField] private int maxUnitQueue = 10;
    [SerializeField] private float spawnMoveRange = 5f;
    [SerializeField] private float unitSpawnDuration = 5f;

    [SyncVar(hook = nameof(ClientHandleQueuedUnitsUpdated))]
    private int queuedUnits;

    [SyncVar]
    private float unitTimer;

    private float progressImageVelocity;


    private void Update()
    {
        if (isServer)
        {
            ProduceUnits();
        }

        if (isClient)
        {
            UpdateTimerDisplay();
        }
    }


    #region Server

    public override void OnStartServer()
    {
        //When this gameeobect is spawned. subscripe to Health ServerOnDie event.
        health.ServerOnDie += ServerHandleDie;
    }

    public override void OnStopServer()
    {
        //Unsubs to Health die event.
        health.ServerOnDie -= ServerHandleDie;
    }


    [Server]
    private void ProduceUnits()
    {
        //If no units queued return
        if (queuedUnits == 0) return;

        //Increase time
        unitTimer += Time.deltaTime;
        if (unitTimer < unitSpawnDuration) return;

        //Spawn unit on client & then server
        GameObject unitInstance = 
            Instantiate(unitPrefab.gameObject, unitSpawnPoint.position, unitSpawnPoint.rotation);
        NetworkServer.Spawn(unitInstance, connectionToClient);

        //Spawn on offset of spawnpoint, but keey same y point
        Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
        spawnOffset.y = unitSpawnPoint.position.y;

        //Server move unit to new vector3 position with offset
        UnitMovement unitMovement = unitInstance.GetComponent<UnitMovement>();
        unitMovement.ServerMove(unitSpawnPoint.position + spawnOffset);

        queuedUnits--;
        unitTimer = 0;
    }


    [Server]
    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    [Command]
    private void CmdSpawnUnit()
    {
        if (queuedUnits == maxUnitQueue) return;

        RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();

        if (player.GetResources() < unitPrefab.GetResourceCost()) return;

        queuedUnits++;

        player.SetResources(player.GetResources() - unitPrefab.GetResourceCost());

    }

    #endregion

    #region Client

    private void UpdateTimerDisplay()
    {
        //Calculates fill amount
        float newProgress = unitTimer / unitSpawnDuration;

        if (newProgress < unitProgressImage.fillAmount)
        {
            //If the current progress is less than image fill amount. Assign fill amount to new progress
            unitProgressImage.fillAmount = newProgress;
        }
        else
        {
            //Modifies the images fill amount normally. Smooths it so no jitterness
            unitProgressImage.fillAmount = Mathf.SmoothDamp(unitProgressImage.fillAmount, newProgress,
                ref progressImageVelocity, 0.1f);
        }
    }

    private void ClientHandleQueuedUnitsUpdated(int oldValue, int newQueuedUnits)
    {
        remainingUnitText.text = newQueuedUnits.ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //Unity calls this function automatically when clicked. Basically any object this script is attached too will run this when object is clicked.
        if (eventData.button != PointerEventData.InputButton.Left) return;      //Check which button clicked
        if (!hasAuthority) return;                                              //Check if client has authority of object.

        CmdSpawnUnit();                                                         //Call server command to spawn unity.
    }
    
    #endregion


}

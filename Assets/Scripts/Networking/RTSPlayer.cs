using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private Transform cameraTransform = null;

    //Array of all possible building types in game
    [SerializeField] private LayerMask buildingBlockLayer = new LayerMask();
    [SerializeField] private Building[] buildings = new Building[0];
    [SerializeField] private float buildingRangeLimit = 5f;

    //Syncvar for players general resources
    [SyncVar(hook = nameof(ClientHandleResourceUpdated))]
    private int resources = 1000;

    //Set to the player who joins the lobby first.
    [SyncVar(hook = nameof(AuthorityHandlePartyOwnerStateUpdated))]
    private bool isPartyOwner = false;

    //List of players units & Buildings
    private Color teamColor = new Color();
    private List<Unit> myUnits = new List<Unit>();
    private List<Building> myBuildings = new List<Building>();

    public event Action<int> ClientOnResourcesUpdated;

    public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;

    public bool GetIsPartyOwner()
    {
        return isPartyOwner;
    }


    public Transform GetCameraTransform()
    {
        return cameraTransform;
    }

    //Class Getters
    public Color GetTeamColor()
    {
        return teamColor;
    }

    public int GetResources()
    {
        return resources;
    }

    public List<Unit> GetMyUnits()
    {
        return myUnits;
    }

    public List<Building> GetMyBuildings()
    {
        return myBuildings;
    }
    //Class Getters

    public bool CanPlaceBuilding(BoxCollider buildingCollider, Vector3 point)
    {
        //Returns whether the specificed boxcollider can be placed at passed position
        if (Physics.CheckBox(point + buildingCollider.center, buildingCollider.size / 2, Quaternion.identity,
            buildingBlockLayer))
        {
            //If overlapping return false
            return false;
        }

        foreach (Building building in myBuildings)
        {
            //If we are in range. We use this for performance.
            if ((point - building.transform.position).sqrMagnitude <= buildingRangeLimit * buildingRangeLimit)
            {
                //If we are in range return true
                return true;
            }
        }

        //Return false if we cant find a building close enough
        return false;
    }

    #region Server

    public override void OnStartServer()
    {
        //Subscribing to Events Unit to handle unit spawn
        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawn += ServerHandleUnitDespawn;

        Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawn += ServerHandleBuildingDespawned;

        DontDestroyOnLoad(gameObject);
    }

    public override void OnStopServer()
    {
        //Unsubscribes all unit events.
        Unit.ServerOnUnitDespawn -= ServerHandleUnitDespawn;
        Unit.ServerOnUnitDespawn -= ServerHandleUnitDespawn;

        Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawn -= ServerHandleBuildingDespawned;
    }

    [Server]
    public void SetPartyOwner(bool state)
    {
        isPartyOwner = state;
    }

    [Server]
    public void SetResources(int resourcesAmount)
    {
        resources = resourcesAmount;
    }

    [Server]
    public void SetTeamColor(Color newTeamColor)
    {
        teamColor = newTeamColor;
    }

    [Command]
    public void CmdStartGame()
    {
        //Server command for start gaming. Basically just checks that the person starting game isPartyOwner
        if (!isPartyOwner) return;
        ((RTSNetworkManager)NetworkManager.singleton).StartGame();
    }


    [Command]
    public void CmdTryPlaceBuilding(Vector3 point, int buildingId)
    {
        //Parameters - Attempted spawn point, building int ID
        //Step 1 = Make sure building exist
        //Step 2 = check if we have enough money & arent overlapping
        //Step 3 = Make sure we are in range.
        //Step 4 = Spawn in

        Building buildingToPlace = null;

        //Check to see if the building passed in to be spawned is null & exist in valid building array.
        foreach (var building in buildings)
        {
            if (building.GetId() == buildingId)
            {
                buildingToPlace = building;
                break;
            }
        }

        //If we are still null, building was not valid
        if (buildingToPlace == null) return;
        if (resources < buildingToPlace.GetPrice()) return;

        BoxCollider buildingCollider = buildingToPlace.GetComponent<BoxCollider>();

        //Check if building is overlapping, within range of other building, and we have resources.
        if (!CanPlaceBuilding(buildingCollider, point)) return;

        //Instantiate the building on local & network
        GameObject buildingInstance = Instantiate(buildingToPlace.gameObject, point, buildingToPlace.transform.rotation);
        NetworkServer.Spawn(buildingInstance, connectionToClient);

        //spend resources
        SetResources(resources - buildingToPlace.GetPrice());
    }


    private void ServerHandleUnitSpawned(Unit unit)
    {
        //Parameter is passed from Unit script event invoke.
        //is the units client has same id as this players client id. If SO add unit to this players unitList.
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) return;
        myUnits.Add(unit);

    }

    private void ServerHandleUnitDespawn(Unit unit)
    {
        //Parameter is passed from Unit script event invoke.
        //If the units associated client id equals client id remove unit from this players unitlist.
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) return;
        myUnits.Remove(unit);
    }

    private void ServerHandleBuildingSpawned(Building building)
    {
        //This is what happens if a Server despawns a player building. Called from Building Script
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) return;
        myBuildings.Add(building);
    }

    private void ServerHandleBuildingDespawned(Building building)
    {
        //THis handles what happens if the Server spawns a building.
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) return;
        myBuildings.Remove(building);
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        //If this machine is running as a server return
        if (NetworkServer.active) return;

        Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawn += AuthorityHandleUnitDespawn;

        Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawn += AuthorityHandleBuildingDespawned;
    }

    public override void OnStartClient()
    {
        if (NetworkServer.active) return;

        DontDestroyOnLoad(gameObject);
        ((RTSNetworkManager)NetworkManager.singleton).Players.Add(this);
    }


    public override void OnStopClient()
    {
        //If we are server we return. if client than remove current clients RTSplayewr from RTSnetworkmanger player list.
        if (!isClientOnly) return;

        ((RTSNetworkManager)NetworkManager.singleton).Players.Remove(this);

        //do an authority check.
        if (!hasAuthority) return;

        //unsub to unit & building spawn events
        Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawn -= AuthorityHandleUnitDespawn;
        
        Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawn -= AuthorityHandleBuildingDespawned;
    }

    
    private void ClientHandleResourceUpdated(int oldResources, int newResources)
    {
        ClientOnResourcesUpdated?.Invoke(newResources);
    }


    private void AuthorityHandlePartyOwnerStateUpdated(bool oldstate, bool newState)
    {
        //Whenever partyOwner changes we invoke the authorityPartyOwner event. Only for the client trhat has authority over it.
        if (!hasAuthority) return;
        AuthorityOnPartyOwnerStateUpdated?.Invoke(newState);
    }



    private void AuthorityHandleUnitSpawned(Unit unit)
    {
        //Parameter is passed from Unit script event invoke.
        myUnits.Add(unit);
    }

    private void AuthorityHandleUnitDespawn(Unit unit)
    {
        //Parameter is passed from Unit script event invoke.
        myUnits.Remove(unit);
    }

    private void AuthorityHandleBuildingDespawned(Building building)
    {
        //This removes a building from player client building list.
        myBuildings.Remove(building);
    }

    private void AuthorityHandleBuildingSpawned(Building building)
    {
        //Adds building to player client building list. No
        myBuildings.Add(building);
    }

    #endregion

}

using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionHandler : MonoBehaviour
{
    [SerializeField] private RectTransform unitSelectionArea = null;
    [SerializeField] private LayerMask layerMask = new LayerMask();

    //Used for storing the start position of UI selection drag
    private Vector2 startPosition;

    //Camera & RTS player Cache
    private Camera mainCamera;
    private RTSPlayer player;

    //List of all currently selected units.
    public List<Unit> SelectedUnits { get; } = new List<Unit>();


    private void Start()
    {
        mainCamera = Camera.main;

        Unit.AuthorityOnUnitDespawn += AuthorityHandleUnitDespawned;
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    private void OnDestroy()
    {
        Unit.AuthorityOnUnitDespawn -= AuthorityHandleUnitDespawned;
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }




    private void Update()
    {
        if (player == null)
        {
            //This set player on Update. Only sets it until not null
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();               //Get the identity component of the client connection calling this script. This allows us to get RTSPlayer component
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            StartSelectionArea();
        }
        else if(Mouse.current.rightButton.wasReleasedThisFrame)
        {
            ClearSelectionArea();
        }
        else if(Mouse.current.rightButton.isPressed)
        {
            UpdateSelectionArea();
        }
    }

    private void StartSelectionArea()
    {
        //Deselects units when we start a new select. Then clears the selected list.
        //start select area

        if (!Keyboard.current.leftShiftKey.isPressed)
        {
            foreach (Unit selectedUnit in SelectedUnits)
            {
                selectedUnit.Deselect();
            }

            SelectedUnits.Clear();
        }
        
        unitSelectionArea.gameObject.SetActive(true);           //Set the selection Area UI enabled.
        startPosition = Mouse.current.position.ReadValue();     //Store the starting area mouse position.

        UpdateSelectionArea();                                  //ForceUpdate Selection area since it wont be called from Update.
    }

    private void UpdateSelectionArea()
    {
        //Calculates mouse position to determine selection area size
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        float areaWidth = mousePosition.x - startPosition.x;
        float areaHeight = mousePosition.y - startPosition.y;

        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));             //Gets the absolute area size of the Updated current selection area
        unitSelectionArea.anchoredPosition = startPosition + new Vector2(areaWidth / 2, areaHeight / 2);    //Sets the anchored position to the center of startpoint & endpoint.
    }

    private void ClearSelectionArea()
    {
        unitSelectionArea.gameObject.SetActive(false);

        if (unitSelectionArea.sizeDelta.magnitude == 0)
        {
            //Single Click selection. Placed here so that by default if the player doesn't drag this is ran.
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) return;
            if (!hit.collider.TryGetComponent(out Unit unit)) return;
            if (!unit.hasAuthority) return;

            SelectedUnits.Add(unit);

            foreach (Unit selectedUnit in SelectedUnits)        //select each of the units list
            {
                selectedUnit.Select();
            }

            return;
        }

        //Logic for multiple unit selection.
        Vector2 min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta / 2);
        Vector2 max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);

        foreach (Unit unit in player.GetMyUnits())
        {
            //Validates we are only adding a unit 1 time to selected Units list.
            if (SelectedUnits.Contains(unit)) continue;

            Vector3 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);

            //Verifies that the units screenposition is within our selection area
            if (screenPosition.x > min.x && screenPosition.x < max.x && screenPosition.y > min.y &&
                screenPosition.y < max.y)
            {
                SelectedUnits.Add(unit);
                unit.Select();
            }
        }
    }

    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        SelectedUnits.Remove(unit);
    }

    private void ClientHandleGameOver(string winnerName)
    {
        //If the clientOnGameOver is called. We disabled this selection component.
        enabled = false;
    }
}

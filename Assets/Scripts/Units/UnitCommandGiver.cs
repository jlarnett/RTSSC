using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitCommandGiver : MonoBehaviour
{
    [SerializeField] private UnitSelectionHandler unitSelectionHandler = null;
    [SerializeField] private LayerMask layerMask = new LayerMask();

    private Camera mainCamera;


    private void Start()
    {
        mainCamera = Camera.main;

        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }


    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    private void Update()
    {
        //This method handles move and targeting logic based upon raycast.
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) return;


        //If we hit targetable gameobject. do below
        if (hit.collider.TryGetComponent<Targetable>(out Targetable target))
        {

            //If unit is our own. We just try to move to that spot.
            if (target.hasAuthority)
            {
                TryMove(hit.point);
                return;
            }

            //Otherwise if we make it here we try to Target the target
            TryTarget(target);
            return;
        }

        //If we dont click on targetable we try to move
        TryMove(hit.point);
    }


    private void TryMove(Vector3 hitPoint)
    {
        //This is called whenever mouse is pressed & we hit the layermask specificied.
        foreach (Unit unit in unitSelectionHandler.SelectedUnits)
        {
            unit.GetUnitMovement().CmdMove(hitPoint);
        }
    }

    private void TryTarget(Targetable target)
    {
        //This is called whenever mouse is pressed & we hit the layermask specificied.
        foreach (Unit unit in unitSelectionHandler.SelectedUnits)
        {
            unit.GetUnitTargeter().CmdSetTarget(target.gameObject);
        }
    }

    private void ClientHandleGameOver(string winnerName)
    {
        enabled = false;
    }

}

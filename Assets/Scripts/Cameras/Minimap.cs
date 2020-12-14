﻿using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Minimap : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private RectTransform minimapRect = null;
    [SerializeField] private float mapScale = 50f;
    [SerializeField] private float offset = -5f;

    private Transform playerCameraTransform;

    private void Update()
    {
        if (playerCameraTransform != null) return;
        if (NetworkClient.connection.identity == null) return;

        playerCameraTransform = NetworkClient.connection.identity.GetComponent<RTSPlayer>().GetCameraTransform();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        MoveCamera();
    }

    public void OnDrag(PointerEventData eventData)
    {
        MoveCamera();
    }

    private void MoveCamera()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        //Get local position of mouse relative to minimap rect
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapRect, mousePosition, null,
            out Vector2 localPoint)) return;

        //Convert the position of mouse in retrospect of minimap to percentage based upon size. Done to gurantee sizing doesn't cause readout issues
        Vector2 lerp = new Vector2((localPoint.x - minimapRect.rect.x) / minimapRect.rect.width,
                                   (localPoint.y - minimapRect.rect.y) / minimapRect.rect.height);

        //Get new position
        Vector3 newCameraPosition = new Vector3(Mathf.Lerp(-mapScale, mapScale, lerp.x),
                                                            playerCameraTransform.position.y,
                                                            Mathf.Lerp(-mapScale, mapScale, lerp.y));

        playerCameraTransform.position = newCameraPosition + new Vector3(0f, 0f, offset);


    }


}

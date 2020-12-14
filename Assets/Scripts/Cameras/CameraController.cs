using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : NetworkBehaviour
{
    [SerializeField] private Transform playerCameraTransform = null;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float screenBorderWidth = 15;


    [SerializeField] private Vector2 screenXLimits = Vector2.zero;
    [SerializeField] private Vector2 screenZLimits = Vector2.zero;

    private Vector2 previousInput;

    //new inputs
    private Controls controls;

    //Entire script is currently Clientside
    public override void OnStartAuthority()
    {
        //Set camera active & instantiate controls
        playerCameraTransform.gameObject.SetActive(true);
        controls = new Controls();

        //subscribtes to controls events?
        controls.Player.MoveCamera.performed += SetPreviousInput;
        controls.Player.MoveCamera.canceled += SetPreviousInput;

        //Enables controlss
        controls.Enable();
    }

    [ClientCallback]
    private void Update()
    {
        //if we dont have authority over camera return
        if (!hasAuthority || !Application.isFocused) return;

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        //Get current cam position
        Vector3 cameraPos = playerCameraTransform.position;

        //nothing on keyboard
        if (previousInput == Vector2.zero)
        {
            //If no keyboard input get mouse position & try to move camera if past move barrier.
            //Move at rate of speed independent of frames

            //Get current Cursor position
            Vector3 cursorMovement = Vector3.zero;
            Vector2 cursorPosition = Mouse.current.position.ReadValue();

            if (cursorPosition.y >= Screen.height - screenBorderWidth)
            {
                //If cursor is past top of screen border movement area
                cursorMovement.z += 1;
            }
            else if (cursorPosition.y <= screenBorderWidth)
            {
                //If below Y barrier
                cursorMovement.z -= 1;
            }
            if (cursorPosition.x >= Screen.width - screenBorderWidth)
            {
                //If cursor is right of barrier
                cursorMovement.x += 1;
            }
            else if (cursorPosition.x <= screenBorderWidth)
            {
                //If cursor is left of barrier
                cursorMovement.x -= 1;
            }

            //Add the cursorMovement normalized so no increase in corners at rate of speed * deltatime to make it framerate independent.
            cameraPos += cursorMovement.normalized * speed * Time.deltaTime;
        }
        else
        {
            //If keyboard input. add new vector3 with previous input positions multiplied by speed & Time.
            cameraPos += new Vector3(previousInput.x, 0f, previousInput.y) * speed * Time.deltaTime;
        }

        //Clamp e.g contain the positions within constraints
        cameraPos.x = Mathf.Clamp(cameraPos.x, screenXLimits.x, screenXLimits.y);
        cameraPos.z = Mathf.Clamp(cameraPos.z, screenZLimits.x, screenZLimits.y);

        //Finally assign camera position.
        playerCameraTransform.position = cameraPos;
    }

    private void SetPreviousInput(InputAction.CallbackContext ctx)
    {
        //Sets the previous input
        previousInput = ctx.ReadValue<Vector2>();
    }



}

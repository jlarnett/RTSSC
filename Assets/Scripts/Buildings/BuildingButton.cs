using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Building building = null;
    [SerializeField] private Image iconImage = null;
    [SerializeField] private TMP_Text priceText = null;
    [SerializeField] private LayerMask floorMask = new LayerMask();
    [SerializeField] private BoxCollider buildingCollider = null;

    private Camera mainCamera;
    private RTSPlayer player;

    private GameObject buildingPreviewInstance;
    private Renderer buildingRendererInstance;

    private void Start()
    {
        mainCamera = Camera.main;
        iconImage.sprite = building.GetIcon();
        priceText.text = building.GetPrice().ToString();
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();               //Get the identity component of the client connection calling this script. This allows us to get RTSPlayer component
        buildingCollider = building.GetComponent<BoxCollider>();
    }

    private void Update()
    {
        //Drag building. If preview is not null
        if (buildingPreviewInstance == null) return;
        UpdateBuildingPreview();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (player.GetResources() < building.GetPrice()) return;

        //Instantiate preview building & get renderer
        buildingPreviewInstance = Instantiate(building.GetBuildingPreview());
        buildingRendererInstance = buildingPreviewInstance.GetComponentInChildren<Renderer>();

        //Set the preview instance active to false. Keeps building from displaying until mouse position is valid
        buildingPreviewInstance.SetActive(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //If the building preview instance is not null. Raycast to mouse position, Place building, Destroy preview instance. 
        if (buildingPreviewInstance == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
        {
            //Place building
            player.CmdTryPlaceBuilding(hit.point, building.GetId());
        }

        //Released destroy preview INSTANCE
        Destroy(buildingPreviewInstance);
    }

    private void UpdateBuildingPreview()
    {
        //If we hit the floor layermask. Set the preview instance = hit position.
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) return;
        //Beyond here mouse is hitting valid placement layer

        //Set preview position to hit position.
        buildingPreviewInstance.transform.position = hit.point;

        //Activate instance if it is not active.
        if (!buildingPreviewInstance.activeSelf)
        {
            buildingPreviewInstance.SetActive(true);
        }

        //Set color based upon RTSPLAYER CanPlaceBuilding Check. We then assign the buildingRendererMaterial Color.
        Color color = player.CanPlaceBuilding(buildingCollider, hit.point) ? Color.blue : Color.red;       //Ternery operator. If true do first. If false do second
        buildingRendererInstance.material.SetColor("_BaseColor", color);
    }







}

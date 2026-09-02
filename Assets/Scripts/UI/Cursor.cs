using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace HackedDesign
{
    public class Cursor : MonoBehaviour
    {
        //[SerializeField] private HoverPresenter hoverPresenter;
        [SerializeField] private CanvasScaler canvas;
        [SerializeField] private Camera uiCamera;
        [SerializeField] private PlayerInput playerInput = null;
        [SerializeField] private RectTransform uiCrosshair = null;
        [SerializeField] private int screenWidth = 320;
        [SerializeField] private int screenHeight = 180;

        private InputAction mousePosAction;

        void Awake()
        {
            canvas = GetComponentInParent<CanvasScaler>();
            mousePosAction = playerInput.actions["Mouse Position"];
            UnityEngine.Cursor.visible = false;
            screenWidth = (int)canvas.referenceResolution.x;
            screenHeight = (int)canvas.referenceResolution.y;
        }

        void OnApplicationQuit()
        {
            UnityEngine.Cursor.visible = true;
        }

        void Update()
        {
            var mousePos = mousePosAction.ReadValue<Vector2>();
            PositionCrosshair(mousePos);
        }

        private void PositionCrosshair(Vector2 mousePos)
        {
            uiCrosshair.anchoredPosition = new Vector2(Mathf.FloorToInt(screenWidth * (mousePos.x / Screen.width)), Mathf.FloorToInt(screenHeight * (mousePos.y / Screen.height)));
        }        
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

public class CardStackClickHandler : MonoBehaviour
{
    public Camera arCamera; // Reference to the AR camera
    private GameManager gameManager;

    void Start()
    {
        // Get the reference to the GameManager (assuming it's in the same scene)
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        // Check for touch input
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            ProcessTouchOrClick(touchPosition);
        }

        // Check for mouse input (useful for testing in the editor)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 clickPosition = Mouse.current.position.ReadValue();
            ProcessTouchOrClick(clickPosition);
        }
    }

    private void ProcessTouchOrClick(Vector2 screenPosition)
    {
        Ray ray = arCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform == transform)
            {
                Debug.Log("CLICKED!!!!!!!!!!!!!");
                // Call the method to add a card in the GameManager
                if (gameManager != null)
                {
                    gameManager.AddRandomCard();
                }
            }
        }
    }
}

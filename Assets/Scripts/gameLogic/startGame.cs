using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class GameManager : MonoBehaviour
{
    public GameObject menuPanel;       // Reference to the menu panel
    public GameObject cardStackPrefab; // Prefab for the card stack
    public GameObject cardPrefab;      // Prefab for individual cards
    public Button startButton;         // Reference to the start button
    public PlaneSpawner planeSpawner;  // Reference to the PlaneSpawner script

    private bool gameStarted = false;  // Flag to check if the game has started
    private GameObject cardStack;      // Reference to the spawned card stack
    private GameObject placedCards;    // Reference to the placed cards object

    void Start()
    {
        startButton.onClick.AddListener(StartGame);
        planeSpawner.OnPlanesDetected += SpawnOrUpdateGameObjects;
        Debug.Log("GameManager started, waiting for button click.");
    }

    void StartGame()
    {
        // Hide the menu
        menuPanel.SetActive(false);
        gameStarted = true;
        Debug.Log("Start button clicked, menu hidden.");
    }

    void SpawnOrUpdateGameObjects()
    {
        if (!gameStarted)
        {
            Debug.Log("Game has not started yet.");
            return;
        }

        // Check if there are any detected planes
        if (planeSpawner.detectedPlanes.Count > 0)
        {
            // Get the first detected plane
            ARPlane firstPlane = planeSpawner.detectedPlanes[0];
            Debug.Log("Detected plane found: " + firstPlane.trackableId);

            // Get the spawn position at the center of the first detected plane
            Vector3 stackPosition = firstPlane.center + new Vector3(0, 0.05f, 0); // Slightly above the plane
            Vector3 placedCardsPosition = stackPosition + new Vector3(0.3f, 0, 0); // Position next to the stack

            if (cardStack == null)
            {
                // Instantiate the card stack if it hasn't been spawned yet
                cardStack = Instantiate(cardStackPrefab, stackPosition, Quaternion.identity);
                if (cardStack != null)
                {
                    Debug.Log("Card stack instantiated successfully.");
                }
                else
                {
                    Debug.LogError("Failed to instantiate card stack.");
                }
            }
            else
            {
                // Reposition the existing card stack
                cardStack.transform.position = stackPosition;
                Debug.Log("Card stack repositioned to: " + stackPosition);
            }

            if (placedCards == null)
            {
                // Create a new parent object for placed cards
                placedCards = new GameObject("PlacedCards");
                placedCards.transform.position = placedCardsPosition;

                // Instantiate two cards as an example
                GameObject card1 = Instantiate(cardPrefab, placedCardsPosition, Quaternion.identity, placedCards.transform);
                GameObject card2 = Instantiate(cardPrefab, placedCardsPosition + new Vector3(0, 0.01f, 0.1f), Quaternion.identity, placedCards.transform); // Slight offset
                if (card1 != null && card2 != null)
                {
                    Debug.Log("Placed cards instantiated successfully.");
                }
                else
                {
                    Debug.LogError("Failed to instantiate placed cards.");
                }
            }
            else
            {
                // Reposition the existing placed cards parent object
                placedCards.transform.position = placedCardsPosition;
                Debug.Log("Placed cards repositioned to: " + placedCardsPosition);
            }
        }
        else
        {
            Debug.Log("No planes detected.");
        }
    }
}

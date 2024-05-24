using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class GameManager : MonoBehaviour
{
    public GameObject menuPanel;       // Reference to the menu panel
    public GameObject cardPanel;       // Reference to the card panel
    public GameObject cardStackPrefab; // Prefab for the card stack
    public GameObject cardPrefab;      // Prefab for individual cards
    public GameObject cardButtonPrefab; // Prefab for card UI button
    public Button startButton;         // Reference to the start button
    public PlaneSpawner planeSpawner;  // Reference to the PlaneSpawner script

    private bool gameStarted = false;  // Flag to check if the game has started
    private GameObject cardStack;      // Reference to the spawned card stack
    private GameObject placedCards;    // Reference to the placed cards object
    private List<GameObject> cardButtons = new List<GameObject>(); // List of card buttons

    public List<Sprite> cardSprites;   // List of card sprites
    public List<Card> cards = new List<Card>(); // List of cards

    private Dictionary<string, Sprite> spriteLookup; // Dictionary for sprite lookup

    void Start()
    {
        // Ensure the card panel is initially inactive
        cardPanel.SetActive(false);

        startButton.onClick.AddListener(StartGame);
        planeSpawner.OnPlanesDetected += SpawnOrUpdateGameObjects;
        Debug.Log("GameManager started, waiting for button click.");

        InitializeSpriteLookup();
        InitializeCards();
    }

    void InitializeSpriteLookup()
    {
        spriteLookup = new Dictionary<string, Sprite>();
        foreach (var sprite in cardSprites)
        {
            string key = sprite.name; // Assumes sprite names are in the format "colorNumber" or "colorAction"
            spriteLookup[key] = sprite;
        }
    }

    void InitializeCards()
    {
        // Define the color and action arrays
        string[] colors = { "red", "green", "blue", "yellow" };
        string[] actions = { "Skip", "Reverse", "DrawTwo" };
        string[] blackActions = { "drawFour", "wild" };

        // Initialize colored cards
        foreach (var color in colors)
        {
            for (int i = 1; i <= 9; i++)
            {
                string key = color + i;
                if (spriteLookup.ContainsKey(key))
                {
                    cards.Add(new Card(spriteLookup[key], color, i.ToString()));
                }
            }
            foreach (var action in actions)
            {
                string key = color + action;
                if (spriteLookup.ContainsKey(key))
                {
                    cards.Add(new Card(spriteLookup[key], color, action));
                }
            }
        }

        // Initialize black cards
        foreach (var action in blackActions)
        {
            if (spriteLookup.ContainsKey(action))
            {
                cards.Add(new Card(spriteLookup[action], "black", action));
            }
        }
    }

    void StartGame()
    {
        // Hide the menu
        menuPanel.SetActive(false);

        // Activate the card panel
        cardPanel.SetActive(true);

        gameStarted = true;
        Debug.Log("Start button clicked, menu hidden and card panel activated.");

        DisplayCards(GetRandomCards(6)); // Display 6 random cards
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

            // Get the spawn position at the center of the first detected plane
            Vector3 stackPosition = firstPlane.center + new Vector3(0, 0.05f, 0); // Slightly above the plane
            Vector3 placedCardsPosition = stackPosition + new Vector3(0.3f, 0, 0); // Position next to the stack

            if (cardStack == null)
            {
                // Instantiate the card stack if it hasn't been spawned yet
                cardStack = Instantiate(cardStackPrefab, stackPosition, cardStackPrefab.transform.rotation);
                if (cardStack != null)
                {
                    // Ensure the child object has the click handler script
                    Transform childTransform = cardStack.transform.Find("CardStackChild");
                    if (childTransform != null)
                    {
                        var clickHandler = childTransform.GetComponent<CardStackClickHandler>();
                        if (clickHandler == null)
                        {
                            clickHandler = childTransform.gameObject.AddComponent<CardStackClickHandler>();
                        }
                        clickHandler.arCamera = Camera.main; // Assign the AR camera
                    }
                    else
                    {
                        Debug.LogError("CardStackChild not found in CardStackParent.");
                    }
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
            }

            if (placedCards == null)
            {
                // Create a new parent object for placed cards
                placedCards = new GameObject("PlacedCards");
                placedCards.transform.position = placedCardsPosition;

                // Instantiate two cards as an example
                GameObject card1 = Instantiate(cardPrefab, placedCardsPosition, cardPrefab.transform.rotation, placedCards.transform);
                GameObject card2 = Instantiate(cardPrefab, placedCardsPosition + new Vector3(0, 0.01f, 0.1f), cardPrefab.transform.rotation, placedCards.transform); // Slight offset
                if (card1 != null && card2 != null)
                {
                    // Debug.Log("Placed cards instantiated successfully.");
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
            }
        }
        else
        {
            Debug.Log("No planes detected.");
        }
    }


    void DisplayCards(List<Card> cardsToDisplay)
    {
        // Clear existing card buttons
        foreach (var button in cardButtons)
        {
            Destroy(button);
        }
        cardButtons.Clear();

        // Debug log to show the number of cards to display
        Debug.Log($"Displaying {cardsToDisplay.Count} cards");

        // Create new card buttons
        foreach (var card in cardsToDisplay)
        {
            GameObject cardButton = Instantiate(cardButtonPrefab, cardPanel.transform);

            if (cardButton != null)
            {
                // Log child components for debugging
                LogChildComponents(cardButton);
                Image image = cardButton.GetComponent<Image>();
                if (image != null)
                {
                    Debug.Log($"Current sprite before setting: {(image.sprite != null ? image.sprite.name : "None")}");
                    // Set the card sprite
                    image.sprite = card.sprite;
                    cardButton.name = $"{card.sprite.name} at index {cardButtons.Count}";
                    Debug.Log($"Setting sprite: {card.sprite.name} for card: {card.color} {card.action}");
                    Debug.Log($"Current sprite after setting: {(image.sprite != null ? image.sprite.name : "None")}");
                }
                else
                {
                    Debug.LogError("CardButton prefab is missing an Image component.");
                }

                // Find the Button component within the card button prefab
                Button button = cardButton.GetComponent<Button>();
                // Find the Image component within the card button prefab
                if (button != null)
                {
                    // Ensure the current card is captured correctly in the closure
                    Card cardCopy = card;
                    button.onClick.AddListener(() => OnCardButtonClick());
                }
                else
                {
                    Debug.LogError("CardButton prefab is missing a Button component.");
                }

                cardButtons.Add(cardButton);

                // Debugging logs for each card button created
                Debug.Log($"Created card button for: {card.color} {card.action} with sprite {card.sprite?.name}");
            }
            else
            {
                Debug.LogError("Failed to instantiate card button prefab.");
            }
        }


        // Force a layout rebuild
        Canvas.ForceUpdateCanvases();

        // Toggle the active state of the card panel to refresh the UI
        cardPanel.SetActive(false);
        cardPanel.SetActive(true);

        // Debug log after all cards are displayed
        Debug.Log($"Displayed {cardsToDisplay.Count} cards");
    }

    void LogChildComponents(GameObject cardButton)
    {
        foreach (Transform child in cardButton.transform)
        {
            Debug.Log("Child: " + child.name);
            foreach (Component component in child.GetComponents<Component>())
            {
                Debug.Log(" - Component: " + component.GetType().Name);
            }
        }
    }

    public void AddRandomCard()
    {
        if (cards.Count > 0)
        {
            List<Card> randomCardList = GetRandomCards(1);
            if (randomCardList.Count > 0)
            {
                Card randomCard = randomCardList[0];
                GameObject cardButton = Instantiate(cardButtonPrefab, cardPanel.transform);

                if (cardButton != null)
                {
                    Image image = cardButton.GetComponent<Image>();
                    if (image != null)
                    {
                        image.sprite = randomCard.sprite;
                        cardButton.name = $"{randomCard.sprite.name} at index {cardButtons.Count}";
                        Debug.Log($"Added new card: {randomCard.color} {randomCard.action} with sprite {randomCard.sprite?.name}");
                    }
                    else
                    {
                        Debug.LogError("CardButton prefab is missing an Image component.");
                    }

                    Button button = cardButton.GetComponent<Button>();
                    if (button != null)
                    {
                        button.onClick.AddListener(() => OnCardButtonClick());
                    }
                    else
                    {
                        Debug.LogError("CardButton prefab is missing a Button component.");
                    }

                    cardButtons.Add(cardButton);
                    Canvas.ForceUpdateCanvases();
                }
                else
                {
                    Debug.LogError("Failed to instantiate card button prefab.");
                }
            }
        }
        else
        {
            Debug.LogError("No cards available to add.");
        }
    }

    void OnCardButtonClick()
    {
        Debug.Log($"Card clicked!");
        // Add your card click handling logic here
    }

    List<Card> GetRandomCards(int count)
    {
        List<Card> randomCards = new List<Card>();
        List<int> usedIndices = new List<int>();

        while (randomCards.Count < count)
        {
            int index = Random.Range(0, cards.Count);
            if (!usedIndices.Contains(index))
            {
                randomCards.Add(cards[index]);
                usedIndices.Add(index);
            }
        }

        return randomCards;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class GameManager : MonoBehaviour
{
    public GameObject menuPanel;       // Reference to the menu panel
    public GameObject cardPanel;       // Reference to the card panel
    public GameObject opponentPanel;       // Reference to the menu panel
    public GameObject opponentCardsPanel;       // Reference to the card panel
    public GameObject winnerPanel;       // Reference to the card panel

    public TMPro.TMP_Text winnerText;

    public GameObject cardStackPrefab; // Prefab for the card stack
    public GameObject cardPrefab;      // Prefab for individual cards
    public GameObject cardButtonPrefab; // Prefab for card UI button
    public GameObject opponentCardPrefab; // Prefab for card UI button


    public Button startButton;         // Reference to the start button
    public PlaneSpawner planeSpawner;  // Reference to the PlaneSpawner script

    private bool gameStarted = false;  // Flag to check if the game has started
    private GameObject cardStack;      // Reference to the spawned card stack
    private GameObject placedCards;    // Reference to the placed cards object
    private List<GameObject> cardButtons = new List<GameObject>(); // List of card buttons

    private List<GameObject> opponentCards = new List<GameObject>(); // List of the opponents cards
    private List<Card> opponentCardInfo = new List<Card>(); // List of the opponents cards

    private GameObject[] lastThreeCards = new GameObject[3]; // Array to manage the last three placed cards
    private Card[] lastThreeSprites = new Card[3]; // Array to manage the last three placed cards
    private Card currentCard;

    public float botPlayDelay = 0.5f;

    public List<Sprite> cardSprites;   // List of card sprites
    public List<Card> cards = new List<Card>(); // List of cards

    private Dictionary<string, Sprite> spriteLookup; // Dictionary for sprite lookup

    void Start()
    {
        // Ensure the card panel is initially inactive
        cardPanel.SetActive(false);
        opponentCardsPanel.SetActive(false);
        opponentPanel.SetActive(false);
        winnerPanel.SetActive(false);

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
        opponentPanel.SetActive(true);
        opponentCardsPanel.SetActive(true);

        gameStarted = true;
        Debug.Log("Start button clicked, menu hidden and card panel activated.");

        DisplayOpponentCards(GetRandomCards(6));

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
                float flt = 0;
                // Instantiate two cards as an example
                for (int i = 0; i < lastThreeCards.Length; i++)
                {

                    GameObject card = Instantiate(cardPrefab, placedCardsPosition + new Vector3(0, i * 0.01f, 0.1f), Quaternion.Euler(0, 10 * flt, 0), placedCards.transform); // Slight offset and rotation
                    lastThreeCards[i] = card;
                    lastThreeCards[i].SetActive(false);
                    flt += 5;
                }
                int l = 0;
                lastThreeSprites[l] = GetRandomCards(1)[l];
                currentCard = lastThreeSprites[l];
                UpdateCardSprite(lastThreeCards[l], lastThreeSprites[l].sprite.texture);
                lastThreeCards[l].SetActive(true);

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

    void SetCardTexture(GameObject card, Texture newTexture)
    {
        Transform frontQuad = card.transform.Find("CardBody/FrontQuad");
        if (frontQuad != null)
        {
            MeshRenderer renderer = frontQuad.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.mainTexture = newTexture;
                Debug.Log($"Updated texture of {card.name} to {newTexture.name}");
            }
            else
            {
                Debug.LogError($"MeshRenderer not found on {frontQuad.name}");
            }
        }
        else
        {
            Debug.LogError($"Front Quad not found on {card.name}");
        }
    }

    void DisplayOpponentCards(List<Card> cardsToDisplay)
    {
        foreach (var img in opponentCards)
        {
            Destroy(img);
        }
        opponentCards.Clear();

        foreach (var card in cardsToDisplay)
        {
            GameObject card_ = Instantiate(opponentCardPrefab, opponentCardsPanel.transform);

            if (card_ != null)
            {
                // Log child components for debugging
                Image image = card_.GetComponent<Image>();
                if (image != null)
                {
                    Debug.Log($"Current sprite before setting: {(image.sprite != null ? image.sprite.name : "None")}");
                    
                    card_.name = $"{card.sprite.name} at index {cardButtons.Count}";
                    Debug.Log($"Setting sprite: {card.sprite.name} for card: {card.color} {card.action}");
                    Debug.Log($"Current sprite after setting: {(image.sprite != null ? image.sprite.name : "None")}");
                }
                else
                {
                    Debug.LogError("CardButton prefab is missing an Image component.");
                }

                // Find the Button component within the card button prefab

                opponentCardInfo.Add(card);
                opponentCards.Add(card_);

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
        opponentCardsPanel.SetActive(false);
        opponentCardsPanel.SetActive(true);


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
                    button.onClick.AddListener(() => OnCardButtonClick(card));
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
                        button.onClick.AddListener(() => OnCardButtonClick(randomCard));
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

    void OnCardButtonClick(Card card)
    {
        Debug.Log($"Card clicked: {card.color} {card.action}");
        

        if (validMove(card))
        {
            currentCard = card;
            PlaceCard(card);
            RemoveCardButton(card);
            if (cardButtons.Count == 0)
            {
                if (winnerText != null)
                {
                    winnerText.text = "You win! :)";
                }
                else
                {
                    Debug.LogError("WinnerText component not found in WinnerPanel.");
                }
                cardPanel.SetActive(false);
                opponentCardsPanel.SetActive(false);
                opponentPanel.SetActive(false);
                winnerPanel.SetActive(true);
                return;
            }
            otherGameLogic(card, true);
            
        }
       

    }

    private bool validMove(Card card)
    {
        // A card can be placed if:
        // 1. The card has the same color as the current card
        // 2. The card has the same value as the current card
        // 3. The card is a Wild card (color "black")
        // 4. The card is a Wild Draw Four card (color "black")

        // Check for Wild cards
        if (card.color == "black")
        {
            return true;
        }

        if (currentCard.color == "black")
        {
            return true;
        }

        // Check for same color or same value
        if (card.color == currentCard.color || card.action == currentCard.action)
        {
            return true;
        }

        // If none of the above conditions are met, the move is not valid
        return false;
    }

    private void otherGameLogic(Card card, Boolean fromUser)
    {
        // Handle the card action for the opponent
        switch (card.action)
        {
            case "DrawTwo":
                if (fromUser)
                {
                    AddOpponentCards(2);
                }
                else
                {
                    AddRandomCard();
                    AddRandomCard();
                    StartCoroutine(HandleBotDecisionWithDelay(card));
                }
                break;
            case "drawFour":
                if (fromUser)
                {
                    AddOpponentCards(4);
                }
                else
                {
                    AddRandomCard();
                    AddRandomCard();
                    AddRandomCard();
                    AddRandomCard();
                    StartCoroutine(HandleBotDecisionWithDelay(card));
                }
                break;
            case "wild":
                if (fromUser)
                {
                    StartCoroutine(HandleBotDecisionWithDelay(card));
                }
                break;
            case "Skip":
                if (!fromUser)
                {
                    StartCoroutine(HandleBotDecisionWithDelay(card));
                }
                break;
            case "Reverse":
                // Handle other actions like changing turn, skipping turn, etc.
                // Example: skip turn logic for "Skip" card
                // currentPlayer = (currentPlayer + 1) % numberOfPlayers;
                if (!fromUser)
                {
                    StartCoroutine(HandleBotDecisionWithDelay(card));
                }
                break;
            default:
                if (fromUser)
                {
                    StartCoroutine(HandleBotDecisionWithDelay(card));
                }
                break;

        }

    }

    private void AddOpponentCards(int numberOfCards)
    {
        for (int i = 0; i < numberOfCards; i++)
        {
            // Instantiate a new card object
            GameObject card = Instantiate(opponentCardPrefab, opponentCardsPanel.transform); // Assuming you have a parent object for opponent's cards
            opponentCards.Add(card);
            opponentCardInfo.Add(GetRandomCards(1)[0]);

        }
    }

    private IEnumerator HandleBotDecisionWithDelay(Card card)
    {
        yield return new WaitForSeconds(0.5f);
        HandleBotDecision(card);
    }

    private void HandleBotDecision(Card card)
    {
        StartCoroutine(BotPlayCards());
    }

    private IEnumerator BotPlayCards()
    {
        while (true)
        {
            bool hasValidCard = false;

            // Check for a valid card in the bot's hand
            for (int i = 0; i < opponentCardInfo.Count; i++)
            {
                Debug.Log("User didn't draw a card");
                if (validMove(opponentCardInfo[i]))
                {
                    // Play the valid card
                    Card chosenCard = opponentCardInfo[i];
                    PlaceCard(chosenCard);

                    currentCard = chosenCard;

                    // Remove the card from the opponent's hand
                    opponentCardInfo.RemoveAt(i);
                    Destroy(opponentCards[i]);
                    opponentCards.RemoveAt(i);

                    otherGameLogic(chosenCard, false);
                    if (opponentCardInfo.Count == 0)
                    {
                        cardPanel.SetActive(false);
                        if (winnerText != null)
                        {
                            winnerText.text = "You lose :(!";
                        }
                        else
                        {
                            Debug.LogError("WinnerText component not found in WinnerPanel.");
                        }
                        opponentCardsPanel.SetActive(false);
                        opponentPanel.SetActive(false);
                        winnerPanel.SetActive(true);
                        yield break;
                    }
                    Debug.Log($"Bot played card: {chosenCard.color} {chosenCard.action}");
                    hasValidCard = true;
                    yield break;
                }
            }

            if (!hasValidCard)
            {
                Debug.Log("User had to draw a card");
                // Draw a card if no valid card is found
                AddOpponentCards(1);
                yield return new WaitForSeconds(0.3f); // Delay between draws
            }
        }
    }

    

    void RemoveCardButton(Card card)
    {
        GameObject buttonToRemove = null;

        // Find the button corresponding to the card
        foreach (GameObject button in cardButtons)
        {
            Image image = button.GetComponent<Image>();
            if (image != null && image.sprite == card.sprite)
            {
                buttonToRemove = button;
                break;
            }
        }

        // Remove the button from the list and destroy the button GameObject
        if (buttonToRemove != null)
        {
            cardButtons.Remove(buttonToRemove);
            Destroy(buttonToRemove);
            Debug.Log($"Removed button for card: {card.color} {card.action}");
        }
        else
        {
            Debug.LogError($"Button for card: {card.color} {card.action} not found");
        }
    }


    void PlaceCard(Card card)
    {
        // Shift existing cards forward
        for (int i = 0; i < lastThreeSprites.Length - 1; i++)
        {
            lastThreeSprites[i] = lastThreeSprites[i + 1];
            if (lastThreeSprites[i] != null)
            {
                UpdateCardSprite(lastThreeCards[i], lastThreeSprites[i].sprite.texture);
                lastThreeCards[i].SetActive(true);
            }
            else
            {
                lastThreeCards[i].SetActive(false);
            }
        }

        // Set the new card at the end
        lastThreeSprites[lastThreeSprites.Length - 1] = card;
        UpdateCardSprite(lastThreeCards[lastThreeCards.Length - 1], card.sprite.texture);
        lastThreeCards[lastThreeCards.Length - 1].SetActive(true);
    }

    void UpdateCardSprite(GameObject card, Texture newTexture)
    {
        Transform actualBody = card.transform.Find("CardBody");
        Transform frontQuad = actualBody.transform.Find("FrontQuad");
        if (frontQuad != null)
        {
            MeshRenderer renderer = frontQuad.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.mainTexture = newTexture;
                Debug.Log($"Updated texture of {card.name} to {newTexture.name}");
            }
            else
            {
                Debug.LogError($"MeshRenderer not found on {frontQuad.name}");
            }
        }
        else
        {
            Debug.LogError($"Front Quad not found on {card.name}");
        }
    }


    public void ResetGame()
    {
        // Hide the winner panel
        winnerPanel.SetActive(false);

        // Clear player and opponent cards
        foreach (GameObject card in opponentCards)
        {
            Destroy(card);
        }
        opponentCards.Clear();
        opponentCardInfo.Clear();
        for (int i = 0; i < lastThreeCards.Length; i++)
        {
            lastThreeSprites[i] = null;
            lastThreeCards[i].SetActive(false);
        }
        int l = 0;
        lastThreeSprites[l] = GetRandomCards(1)[l];
        currentCard = lastThreeSprites[l];
        UpdateCardSprite(lastThreeCards[l], lastThreeSprites[l].sprite.texture);
        lastThreeCards[l].SetActive(true);
        // Add logic to reset player's cards similarly if needed

        // Restart the game logic as needed
        StartGame();
    }

    public void ResetGameAndShowMenu()
    {
        // Call ResetGame to reset the game state
        ResetGame();

        // Show the menu panel
        menuPanel.SetActive(true);
        cardPanel.SetActive(false);
        opponentCardsPanel.SetActive(false);
        opponentPanel.SetActive(false);
        winnerPanel.SetActive(false);
    }


    List<Card> GetRandomCards(int count)
    {
        List<Card> randomCards = new List<Card>();
        List<int> usedIndices = new List<int>();

        while (randomCards.Count < count)
        {
            int index = UnityEngine.Random.Range(0, cards.Count);
            if (!usedIndices.Contains(index))
            {
                randomCards.Add(cards[index]);
                usedIndices.Add(index);
            }
        }

        return randomCards;
    }
}

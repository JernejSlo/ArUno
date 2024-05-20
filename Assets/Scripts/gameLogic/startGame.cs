using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject menuPanel;  // Reference to the menu panel

    // Method to start the game and hide the menu
    public void StartGame()
    {
        menuPanel.SetActive(false);  // Hide the menu panel
    }
}

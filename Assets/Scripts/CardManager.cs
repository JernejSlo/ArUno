using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public GameObject cardPrefab; // Reference to the card prefab
    public List<Texture> frontTextures; // List of front textures
    public List<Texture> backTextures; // List of back textures

    private Material cardFrontMaterial;
    private Material cardBackMaterial;

    void Start()
    {
        // Ensure the prefab has a Material assigned to its front and back quads
        cardFrontMaterial = cardPrefab.transform.Find("FrontQuad").GetComponent<Renderer>().material;
        cardBackMaterial = cardPrefab.transform.Find("BackQuad").GetComponent<Renderer>().material;
    }

    public void CreateCard(int frontTextureIndex, int backTextureIndex, Vector3 position)
    {
        // Instantiate a new card
        GameObject newCard = Instantiate(cardPrefab, position, Quaternion.identity);

        // Get the materials of the new card instance
        Material newCardFrontMaterial = newCard.transform.Find("FrontQuad").GetComponent<Renderer>().material;
        Material newCardBackMaterial = newCard.transform.Find("BackQuad").GetComponent<Renderer>().material;

        // Assign the selected textures to the new card's materials
        newCardFrontMaterial.mainTexture = frontTextures[frontTextureIndex];
        newCardBackMaterial.mainTexture = backTextures[backTextureIndex];
    }
}

using UnityEngine;

public class Card
{
    public Sprite sprite;
    public string color;
    public string action;

    public Card(Sprite sprite, string color, string action)
    {
        this.sprite = sprite;
        this.color = color;
        this.action = action;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public string Name { get; private set; }
    public List<Card> Hand { get; private set; }

    public Player(string name)
    {
        Name = name;
        Hand = new List<Card>();
    }

    public void DrawCard(Card card)
    {
        Hand.Add(card);
    }

    public void PlayCard(Card card)
    {
        Hand.Remove(card);
    }
}

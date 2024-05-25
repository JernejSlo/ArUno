using UnityEngine;





public enum Color 
{
    Red,
    Green,
    Blue,
    Yellow,
    Wild
}


public enum CardType
{
    Number,
    Skip,
    Reverse,
    DrawTwo,
    Wild,
    WildDrawFour
}



public class Card
{
    public Color Color { get; private set; }
    public CardType CardType { get; private set; }
    public int Number { get; private set; }

    public Card(Color color, CardType cardType, int number = -1)
    {
        Color = color;
        CardType = cardType;
        Number = number;
    }

    public override string ToString()
    {
        if (CardType == CardType.Number)
        {
            return $"{Color} {Number}";
        }
        else
        {
            return $"{Color} {CardType}";
        }
    }
}
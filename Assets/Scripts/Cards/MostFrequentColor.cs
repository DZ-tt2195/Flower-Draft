using System.Collections.Generic;
using UnityEngine;

public class MostFrequentColor : Card
{
    protected override void SpecificSetup()
    {
        textBox = "+1 VP per card you have with your most frequent color.";
        value = 2;
        myColor = Color.yellow;
        myType = CardType.OnlyScoring;
    }

    protected override int Ability(Player player)
    {
        Dictionary<Color, int> colorDictionary = new();
        int highestColor = 0;
        foreach (Card card in player.cardsPlayed)
        {
            colorDictionary[card.myColor]++;
            if (colorDictionary[card.myColor] > highestColor)
                highestColor = colorDictionary[card.myColor];
        }
        return highestColor;
    }
}

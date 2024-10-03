using System.Collections.Generic;
using UnityEngine;

public class ColorPair : Card
{
    protected override void SpecificSetup()
    {
        textBox = "For each color, if you have exactly 2 cards with that color, +2 VP.";
        value = 2;
        myColor = Color.yellow;
        myType = CardType.OnlyScoring;
    }

    protected override int Ability(Player player)
    {
        Dictionary<Color, int> colorCount = new()
        {
            { Color.gray, 0 },
            { Color.blue, 0 },
            { Color.yellow, 0 },
            { Color.white, 0 },
            { Color.green, 0 },
        };

        int exactlyTwo = 0;
        foreach (Card card in player.cardsPlayed)
            colorCount[card.myColor]++;

        foreach (KeyValuePair<Color, int> entry in colorCount)
        {
            if (colorCount[entry.Key] == 2)
                exactlyTwo++;
        }
        return exactlyTwo * 2;
    }
}

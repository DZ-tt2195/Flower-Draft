using System.Collections.Generic;
using UnityEngine;

public class ThreeDifferentColors : Card
{
    protected override void SpecificSetup()
    {
        textBox = "If there are 3 different colors among this and adjacent cards, +3 VP.";
        value = 3;
        myColor = Color.yellow;
        myType = CardType.OnlyScoring;
    }

    protected override int Ability(Player player)
    {
        List<Color> allColors = new() { this.myColor };
        List<Card> adjacents = player.AdjacentCards(this);
        foreach (Card card in adjacents)
        {
            if (allColors.Contains(card.myColor))
                break;
            else
                allColors.Add(card.myColor);
        }

        return allColors.Count == 3 ? 3 : 0;
    }
}

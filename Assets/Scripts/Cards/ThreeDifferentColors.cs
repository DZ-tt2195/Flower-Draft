using System.Collections.Generic;
using UnityEngine;

public class ThreeDifferentColors : Card
{
    protected override void SpecificSetup()
    {
        textBox = "If there are 3 different colors among this and adjacent cards, +3 VP.";
        value = 3;
        myColor = Color.red;
        myType = CardType.OnlyScoring;
    }

    public override int Scoring(Player player)
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

        return base.Scoring(player) + allColors.Count == 3 ? 3 : 0;
    }
}

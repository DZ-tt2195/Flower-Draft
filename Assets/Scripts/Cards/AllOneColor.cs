using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AllOneColor : Card
{
    protected override void SpecificSetup()
    {
        textBox = "+1 VP per card with your most frequent color.";
        value = 2;
        myColor = Color.yellow;
        myType = CardType.OnlyScoring;
    }

    protected override int Ability(Player player)
    {
        Dictionary<Color, int> colorCount = new()
        {
            { Color.black, 0 },
            { Color.blue, 0 },
            { Color.yellow, 0 },
            { Color.white, 0 },
            { Color.red, 0 },
        };

        foreach (Card card in player.cardsPlayed)
        {
            colorCount[card.myColor]++;
        }

        return colorCount.Values.Max();
    }
}

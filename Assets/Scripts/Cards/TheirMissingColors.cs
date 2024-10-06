using MyBox;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class TheirMissingColors : Card
{
    protected override void SpecificSetup()
    {
        textBox = "+2 VP per color that you have among your cards, that the next player doesn't have.";
        value = 2;
        myColor = Color.blue;
        myType = CardType.OnlyScoring;
    }

    protected override int Ability(Player player)
    {
        Dictionary<Color, int> thisColor = new()
        {
            { Color.black, 0 },
            { Color.blue, 0 },
            { Color.yellow, 0 },
            { Color.white, 0 },
            { Color.red, 0 },
        };
        foreach (Card card in player.cardsPlayed)
        {
            thisColor[card.myColor]++;
        }

        Dictionary<Color, int> theirColors = new()
        {
            { Color.black, 0 },
            { Color.blue, 0 },
            { Color.yellow, 0 },
            { Color.white, 0 },
            { Color.red, 0 },
        };
        foreach (Card card in Manager.instance.NextPlayer(player).cardsPlayed)
        {
            theirColors[card.myColor]++;
        }

        int total = 0;
        foreach (KeyValuePair<Color, int> entry in thisColor)
        {
            if (thisColor[entry.Key] >= 1 && theirColors[entry.Key] == 0)
                total++;
        }
        return total * 2;
    }
}

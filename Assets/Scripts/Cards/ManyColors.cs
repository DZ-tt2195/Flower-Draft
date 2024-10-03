using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ManyColors : Card
{
    protected override void SpecificSetup()
    {
        textBox = "-1 VP per color missing among your cards.";
        value = 6;
        myColor = Color.yellow;
        myType = CardType.OnlyScoring;
    }

    protected override int Ability(Player player)
    {
        List<Color> colorCount = new();
        foreach (Card card in player.cardsPlayed)
            colorCount.Add(card.myColor);
        return 5 - colorCount.Distinct().Count();
    }
}

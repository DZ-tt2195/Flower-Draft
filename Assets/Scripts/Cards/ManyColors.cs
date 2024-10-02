using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ManyColors : Card
{
    protected override void SpecificSetup()
    {
        textBox = "+1 VP per different color among all your cards.";
        value = 2;
        myColor = Color.yellow;
        myType = CardType.OnlyScoring;
    }

    protected override int Ability(Player player)
    {
        List<Color> colorCount = new();
        foreach (Card card in player.cardsPlayed)
            colorCount.Add(card.myColor);
        return colorCount.Distinct().Count();
    }
}

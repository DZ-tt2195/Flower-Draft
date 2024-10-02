using MyBox;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AdjacentsLargeValue : Card
{
    protected override void SpecificSetup()
    {
        textBox = "+1 VP per adjacent card with 4 or more value.";
        value = 4;
        myColor = Color.green;
        myType = CardType.OnlyScoring;
    }

    protected override int Ability(Player player)
    {
        List<Card> adjacentsFourPlus = player.AdjacentCards(this).Where(card => card.value >= 4).ToList();
        return adjacentsFourPlus.Count;
    }
}

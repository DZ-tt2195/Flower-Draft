using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AdjacentsSmallValue : Card
{
    protected override void SpecificSetup()
    {
        textBox = "+2 VP per adjacent card with 2 or less value.";
        value = 2;
        myColor = Color.red;
        myType = CardType.OnlyScoring;
    }

    public override int Scoring(Player player)
    {
        List<Card> adjacentsTwoLess = player.AdjacentCards(this).Where(card => card.value <= 2).ToList();
        return base.Scoring(player) + (adjacentsTwoLess.Count * 2);
    }
}

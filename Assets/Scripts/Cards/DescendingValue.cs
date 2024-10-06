using System.Collections.Generic;
using UnityEngine;

public class DescendingValue : Card
{
    protected override void SpecificSetup()
    {
        textBox = "If the previous card has a higher value than this, and the next card has a lower value than this, +3 VP.";
        value = 3;
        myColor = Color.red;
        myType = CardType.OnlyScoring;
    }

    protected override int Ability(Player player)
    {
        List<Card> adjacents = player.AdjacentCards(this);
        if (adjacents.Count == 2 && adjacents[0].value > this.value && adjacents[1].value < this.value)
            return 3;
        else
            return 0;
    }
}

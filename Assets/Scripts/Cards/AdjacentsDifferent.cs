using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AdjacentsDifferent : Card
{
    protected override void SpecificSetup()
    {
        textBox = "If one adjacent card is Revealed and the other is Concealed, +3 VP.";
        value = 3;
        myColor = Color.red;
        myType = CardType.OnlyScoring;
    }

    public override int Scoring(Player player)
    {
        List<Card> adjacents = player.AdjacentCards(this);
        if (adjacents.Count == 2 && adjacents[0].status != adjacents[1].status)
            return base.Scoring(player) + 3;
        else
            return base.Scoring(player);
    }
}

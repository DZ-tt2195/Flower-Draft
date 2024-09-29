using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AdjacentsDifferent : Card
{
    protected override void SpecificSetup()
    {
        textBox = "If one adjacent card is Revealed and the other is Concealed, +3 VP.";
        value = 3;
        myColor = Color.gray;
        myType = CardType.OnlyScoring;
    }

    protected override int Ability(Player player)
    {
        List<Card> adjacents = player.AdjacentCards(this);
        if (adjacents.Count == 2 && adjacents[0].status != adjacents[1].status)
            return 3;
        else
            return 0;
    }
}

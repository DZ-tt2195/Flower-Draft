using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AdjacentsConcealed : Card
{
    protected override void SpecificSetup()
    {
        textBox = "+1 VP per adjacent card that's Concealed.";
        value = 4;
        myColor = Color.gray;
        myType = CardType.OnlyScoring;
    }

    public override int Scoring(Player player)
    {
        List<Card> concealedAdjacents = player.AdjacentCards(this).Where(card => card.status == Status.Concealed).ToList();
        return base.Scoring(player) + (concealedAdjacents.Count);
    }
}

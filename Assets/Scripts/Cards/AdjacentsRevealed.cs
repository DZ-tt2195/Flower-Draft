using MyBox;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AdjacentsRevealed : Card
{
    protected override void SpecificSetup()
    {
        textBox = "+1 VP per adjacent card that's Revealed.";
        value = 4;
        myColor = Color.gray;
        myType = CardType.OnlyScoring;
    }

    public override int Scoring(Player player)
    {
        List<Card> revealedAdjacents = player.AdjacentCards(this).Where(card => card.status == Status.Revealed).ToList();
        return base.Scoring(player) + (revealedAdjacents.Count);
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AdjacentsConcealed : Card
{
    protected override void SpecificSetup()
    {
        textBox = "+1 VP per adjacent card that's Concealed.";
        value = 4;
        myColor = Color.black;
        myType = CardType.OnlyScoring;
    }

    protected override int Ability(Player player)
    {
        List<Card> concealedAdjacents = player.AdjacentCards(this).Where(card => card.status == Status.Concealed).ToList();
        return concealedAdjacents.Count;
    }
}

using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AllRevealed : Card
{
    protected override void SpecificSetup()
    {
        textBox = "+2 VP per card you have that's Revealed.";
        value = 0;
        myColor = Color.red;
        myType = CardType.OnlyScoring;
    }

    public override int Scoring(Player player)
    {
        List<Card> revealed = player.cardsPlayed.Where(card => card.status == Status.Revealed).ToList();
        return base.Scoring(player) + revealed.Count * 2;
    }
}

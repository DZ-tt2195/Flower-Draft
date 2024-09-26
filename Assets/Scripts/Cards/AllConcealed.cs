using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AllConcealed : Card
{
    protected override void SpecificSetup()
    {
        textBox = "+2 VP per card you have that's Concealed.";
        value = 0;
        myColor = Color.gray;
        myType = CardType.OnlyScoring;
    }

    public override int Scoring(Player player)
    {
        List<Card> concealed = player.cardsPlayed.Where(card => card.status == Status.Concealed).ToList();
        return base.Scoring(player) + concealed.Count * 2;
    }
}

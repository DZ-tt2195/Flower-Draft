using System.Collections.Generic;
using UnityEngine;

public class ManyColors : Card
{
    protected override void SpecificSetup()
    {
        textBox = "+1 VP per different color among all your cards.";
        value = 2;
        myColor = Color.red;
        myType = CardType.OnlyScoring;
    }

    public override int Scoring(Player player)
    {
        List<Color> colorCount = new();
        foreach (Card card in player.cardsPlayed)
        {
            if (!colorCount.Contains(card.myColor))
                colorCount.Add(card.myColor);
        }
        return base.Scoring(player) + colorCount.Count;
    }
}

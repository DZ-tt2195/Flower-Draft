using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class DifferentValue : Card
{
    protected override void SpecificSetup()
    {
        textBox = "+1 VP per different value among all your cards.";
        value = 2;
        myColor = Color.red;
        myType = CardType.OnlyScoring;
    }

    protected override int Ability(Player player)
    {
        List<int> differentValues = new();
        foreach (Card card in player.cardsPlayed)
            differentValues.Add(card.value);

        return differentValues.Distinct().Count();
    }
}

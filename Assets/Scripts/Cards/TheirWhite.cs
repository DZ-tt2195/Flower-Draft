using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TheirWhite : Card
{
    protected override void SpecificSetup()
    {
        textBox = "+1 VP per card the next player has that's white.";
        value = 3;
        myColor = Color.blue;
        myType = CardType.OnlyScoring;
    }

    protected override int Ability(Player player)
    {
        List<Card> whiteCards = Manager.instance.NextPlayer(player).cardsPlayed.Where(card => card.myColor == Color.white).ToList();
        return whiteCards.Count;
    }
}

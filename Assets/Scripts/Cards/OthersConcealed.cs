using MyBox;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OthersConcealed : Card
{
    protected override void SpecificSetup()
    {
        textBox = "+2 VP per card the next player has that's Concealed.";
        value = 0;
        myColor = Color.blue;
        myType = CardType.OnlyScoring;
    }

    protected override int Ability(Player player)
    {
        List<Card> concealed = Manager.instance.NextPlayer(player).cardsPlayed.Where(card => card.status == Status.Concealed).ToList();
        return concealed.Count * 2;
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OthersWorthThree : Card
{
    protected override void SpecificSetup()
    {
        textBox = "+1 VP per card the next player has with a value of 3.";
        value = 3;
        myColor = Color.blue;
        myType = CardType.OnlyScoring;
    }

    protected override int Ability(Player player)
    {
        List<Card> worthThree = Manager.instance.NextPlayer(player).cardsPlayed.Where(card => card.value == 3).ToList();
        return worthThree.Count;
    }
}

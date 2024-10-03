using System.Linq;
using UnityEngine;

public class TheirHand : Card
{
    protected override void SpecificSetup()
    {
        textBox = "-1 VP per card still left in the next player's hand.";
        value = 0;
        myColor = Color.blue;
        myType = CardType.OnlyScoring;
    }

    protected override int Ability(Player player)
    {
        return -1 * Manager.instance.NextPlayer(player).cardsInHand.Count;
    }
}

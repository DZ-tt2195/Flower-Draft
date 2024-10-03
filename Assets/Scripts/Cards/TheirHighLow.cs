using System.Linq;
using UnityEngine;

public class TheirHighLow : Card
{
    protected override void SpecificSetup()
    {
        textBox = "+VP equal to the next player's highest value card, then -VP equal to their lowest value card.";
        value = 0;
        myColor = Color.blue;
        myType = CardType.OnlyScoring;
    }

    protected override int Ability(Player player)
    {
        int highest = Manager.instance.NextPlayer(player).cardsPlayed.Max(card => card.value);
        int lowest = Manager.instance.NextPlayer(player).cardsPlayed.Min(card => card.value);
        return highest - lowest;
    }
}

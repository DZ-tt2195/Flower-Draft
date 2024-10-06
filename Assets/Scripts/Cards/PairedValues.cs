using MyBox;
using UnityEngine;

public class PairedValues : Card
{
    protected override void SpecificSetup()
    {
        textBox = "+1 VP per pair of adjacent cards with the same value.";
        value = 3;
        myColor = Color.red;
        myType = CardType.OnlyScoring;
    }

    protected override int Ability(Player player)
    {
        int pairs = 0;
        for (int i = 0; i < player.cardsPlayed.Count; i++)
        {
            try
            {
                if (player.cardsPlayed[i].value == player.cardsPlayed[i + 1].value)
                    pairs++;
            }
            catch
            {
                break;
            }
        }

        return pairs;
    }
}

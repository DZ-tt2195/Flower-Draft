using MyBox;
using System.Collections.Generic;
using UnityEngine;

public class ColorBeforeThis : Card
{
    protected override void SpecificSetup()
    {
        textBox = "If there are 2 or more gray cards before this, +4 VP.";
        value = 2;
        myColor = Color.yellow;
        myType = CardType.OnlyScoring;
    }

    protected override int Ability(Player player)
    {
        int thisPosition = player.cardsPlayed.IndexOf(this);
        int counter = 0;
        for (int i = 0; i<thisPosition; i++)
        {
            if (player.cardsPlayed[i].myColor == Color.gray)
                counter++;
        }

        return (counter >= 2 ? 4 : 0);
    }
}

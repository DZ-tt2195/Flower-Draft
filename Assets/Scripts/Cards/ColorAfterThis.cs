using UnityEngine;

public class ColorAfterThis : Card
{
    protected override void SpecificSetup()
    {
        textBox = "If there are 2 or more red cards after this, +6 VP.";
        value = 0;
        myColor = Color.yellow;
        myType = CardType.OnlyScoring;
    }

    protected override int Ability(Player player)
    {
        int thisPosition = player.cardsPlayed.IndexOf(this);
        int counter = 0;
        for (int i = thisPosition; i < player.cardsPlayed.Count; i++)
        {
            if (player.cardsPlayed[i].myColor == Color.red)
                counter++;
        }

        return counter >= 2 ? 6 : 0;
    }
}

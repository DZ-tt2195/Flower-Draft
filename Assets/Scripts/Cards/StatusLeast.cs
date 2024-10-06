using System.Collections.Generic;
using UnityEngine;

public class StatusLeast : Card
{
    protected override void SpecificSetup()
    {
        textBox = "+1 VP per Revealed or Concealed card you have, whichever is less.";
        value = 3;
        myColor = Color.black;
        myType = CardType.OnlyScoring;
    }

    protected override int Ability(Player player)
    {
        int revealed = 0;
        int concealed = 0;

        foreach (Card card in player.cardsPlayed)
        {
            if (card.status == Status.Concealed)
                concealed++;
            else if (card.status == Status.Revealed)
                revealed++;
        }
        return Mathf.Min(revealed, concealed);
    }
}

using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

public class Reveal : Card
{
    protected override void SpecificSetup()
    {
        textBox = "Before scoring: You may Reveal a Concealed card.";
        value = 4;
        myColor = Color.red;
        myType = CardType.BeforeScoring;
    }

    public override void BeforeScoring(Player player, int logged)
    {
        List<Card> revealedCards = player.cardsPlayed.Where(card => card.status == Status.Concealed).ToList();
        player.ChooseCardOnScreen(revealedCards, "Decline", "Reveal a concealed card?", Resolution);

        void Resolution()
        {
            if (player.chosenCard != null)
            {
                Card toConceal = player.chosenCard;
                Log.instance.AddStep(1, player, toConceal, nameof(ChangeStatus),
                    new object[2] { player.playerPosition, (int)Status.Revealed }, logged);
                Log.instance.Continue();
            }
            player.DecisionMade(-1);
        }
    }
}

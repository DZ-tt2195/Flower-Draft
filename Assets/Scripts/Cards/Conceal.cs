using MyBox;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Conceal : Card
{
    protected override void SpecificSetup()
    {
        textBox = "Before scoring: You may Conceal a Revealed card.";
        value = 3;
        myColor = Color.red;
        myType = CardType.BeforeScoring;
    }

    public override void BeforeScoring(Player player, int logged)
    {
        List<Card> revealedCards = player.cardsPlayed.Where(card => card.status == Status.Revealed).ToList();
        player.ChooseCardOnScreen(revealedCards, "Decline", "Conceal a revealed card?", Resolution);

        void Resolution()
        {
            if (player.chosenCard != null)
            {
                Card toConceal = player.chosenCard;
                Log.instance.AddStep(1, player, toConceal, nameof(ChangeStatus),
                    new object[2] { player.playerPosition, (int)Status.Concealed }, logged);
                Log.instance.Continue();
            }
            player.DecisionMade(-1);
        }
    }
}

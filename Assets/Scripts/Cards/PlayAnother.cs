using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayAnother : Card
{
    protected override void SpecificSetup()
    {
        textBox = "Before scoring: Play a non-white card from your hand; it's Revealed and placed at the front.";
        value = 0;
        myColor = Color.white;
        myType = CardType.BeforeScoring;
    }

    protected override void AddToMethodDictionary(string methodName)
    {
        FindMethod(this.GetType(), methodName);
    }

    public override void BeforeScoring(Player player, int logged)
    {
        player.ChooseCardOnScreen(player.cardsInHand.Where(card => card.myColor != Color.white).ToList(),
            "", "Play a card from your hand.", Resolution);

        void Resolution()
        {
            try
            {
                Card toPlay = player.chosenCard;
                Log.instance.AddStep(1, player, this, nameof(CardInFront),
                    new object[1] { toPlay.cardID });
                Log.instance.Continue();
                player.DecisionMade(-1);
            }
            catch
            {
                player.DecisionMade(-1);
            }
        }
    }

    [PunRPC]
    void CardInFront(int cardID)
    {
        NextStep step = Log.instance.GetCurrentStep();
        step.player.PlayCard(cardID, 0, 1);
    }
}

using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreAdjacent : Card
{
    protected override void SpecificSetup()
    {
        textBox = "Before scoring: Choose an adjacent card; ignore its abilities when scoring.";
        value = 6;
        myColor = Color.white;
        myType = CardType.BeforeScoring;
    }

    protected override void AddToMethodDictionary(string methodName)
    {
        FindMethod(this.GetType(), methodName);
    }

    public override void BeforeScoring(Player player, int logged)
    {
        player.ChooseCardOnScreen(player.AdjacentCards(this), "", "Choose a card to ignore instructions.", Resolution);

        void Resolution()
        {
            try
            {
                Card toIgnore = player.chosenCard;
                Log.instance.AddStep(1, player, this, nameof(IgnoreThatCard), new object[1] { toIgnore.cardID });
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
    void IgnoreThatCard(int cardID)
    {
        Manager.instance.listOfCards[cardID].applyAbility = 0;
    }
}

using MyBox;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ChangeThisColor : Card
{
    protected override void SpecificSetup()
    {
        textBox = "Before scoring: You may change this card's colors.";
        value = 4;
        myColor = Color.white;
        myType = CardType.BeforeScoring;
    }

    protected override void AddToMethodDictionary(string methodName)
    {
        FindMethod(this.GetType(), methodName);
    }

    public override void BeforeScoring(Player player, int logged)
    {
        List<string> options = new() { "Black", "Yellow", "Blue", "Red", "White"};

        player.ChooseButton(options, $"Change this card's color", Resolution);

        void Resolution()
        {
            Log.instance.AddStep(1, player, this, nameof(ChangeColor), new object[1] { options[player.choice] });
            Log.instance.Continue();
            player.DecisionMade(-1);
        }
    }

    [PunRPC]
    void ChangeColor(string color)
    {
        NextStep step = Log.instance.GetCurrentStep();

        switch (color)
        {
            case "Black":
                this.myColor = Color.black;
                break;
            case "Yellow":
                this.myColor = Color.yellow;
                break;
            case "Blue":
                this.myColor = Color.blue;
                break;
            case "Red":
                this.myColor = Color.red;
                break;
            case "White":
                this.myColor = Color.white;
                break;
        }
        layout.FillInCards(this);
    }
}

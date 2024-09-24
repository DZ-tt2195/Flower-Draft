using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveMe : Card
{
    protected override void SpecificSetup()
    {
        textBox = "Before scoring: You may move this one spot to the left or right.";
        value = 4;
        myColor = Color.red;
        myType = CardType.BeforeScoring;
    }

    public override void BeforeScoring(Player player, int logged)
    {
        int position = player.cardsPlayed.IndexOf(this);
        List<string> options = new();
        if (position > 0)
            options.Add("Left");
        if (position < 6)
            options.Add("Right");
        options.Add("Don't Move");

        player.ChooseButton(options.ToArray(), $"Move {this.name} left or right?", Resolution);

        void Resolution()
        {
            switch (options[player.choice])
            {
                case "Left":
                    Log.instance.AddStep(1, player, player, nameof(Player.MoveCard),
                        new object[3] { this.cardID, position - 1, (int)this.status }, logged);
                    Log.instance.Continue();
                    player.PreserveLogTextRPC($"{player.name} moves {this.name} to the left.", logged);
                    break;
                case "Right":
                    Log.instance.AddStep(1, player, player, nameof(Player.MoveCard),
                        new object[3] { this.cardID, position + 1, (int)this.status }, logged);
                    Log.instance.Continue();
                    player.PreserveLogTextRPC($"{player.name} moves {this.name} to the right.", logged);
                    break;
                case "Don't Move":
                    break;
            }
            player.DecisionMade(-1);
        }
    }
}

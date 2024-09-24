using MyBox;
using UnityEngine;

public class ThisRevealed : Card
{
    protected override void SpecificSetup()
    {
        textBox = "If this is Revealed, -2 VP.";
        value = 6;
        myColor = Color.red;
        myType = CardType.OnlyScoring;
    }

    public override int Scoring(Player player)
    {
        return base.Scoring(player) + (this.status == Status.Revealed ? -2 : 0);
    }
}

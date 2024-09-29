using MyBox;
using UnityEngine;

public class ThisRevealed : Card
{
    protected override void SpecificSetup()
    {
        textBox = "If this is Revealed, -2 VP.";
        value = 6;
        myColor = Color.gray;
        myType = CardType.OnlyScoring;
    }

    protected override int Ability(Player player)
    {
        return this.status == Status.Revealed ? -2 : 0;
    }
}

using UnityEngine;

public class ThisConcealed : Card
{
    protected override void SpecificSetup()
    {
        textBox = "If this is Concealed, +2 VP.";
        value = 3;
        myColor = Color.red;
        myType = CardType.OnlyScoring;
    }

    public override int Scoring(Player player)
    {
        return base.Scoring(player) + (this.status == Status.Concealed ? 2 : 0);
    }
}

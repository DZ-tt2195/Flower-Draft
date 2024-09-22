using UnityEngine;

public class Demo : Card
{
    protected override void SpecificSetup()
    {
        textBox = "If this is public, +2 VP.";
        value = 3;
        myColor = Color.red;
    }

    public override int Scoring(InPlay playArea)
    {
        return base.Scoring(playArea) + (playArea.status == Status.Revealed ? 2 : 0);
    }
}

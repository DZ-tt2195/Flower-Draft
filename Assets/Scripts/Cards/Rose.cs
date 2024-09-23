using UnityEngine;

public class Rose : Card
{
    protected override void SpecificSetup()
    {
        textBox = "";
        value = 4;
        myColor = Color.red;
        myType = CardType.OnlyScoring;
    }
}

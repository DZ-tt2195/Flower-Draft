using UnityEngine;

public class Rose : Card
{
    protected override void SpecificSetup()
    {
        textBox = "";
        value = 5;
        myColor = Color.red;
        myType = CardType.OnlyScoring;
    }
}

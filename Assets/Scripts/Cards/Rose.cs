using UnityEngine;

public class Rose : Card
{
    protected override void SpecificSetup()
    {
        textBox = "";
        value = 5;
        myColor = Color.green;
        myType = CardType.OnlyScoring;
    }
}

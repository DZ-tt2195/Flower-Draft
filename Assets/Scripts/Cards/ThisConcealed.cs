using UnityEngine;

public class ThisConcealed : Card
{
    protected override void SpecificSetup()
    {
        textBox = "If this is Concealed, -2 VP.";
        value = 6;
        myColor = Color.gray;
        myType = CardType.OnlyScoring;
    }

    protected override int Ability(Player player)
    {
        return this.status == Status.Concealed ? -2 : 0;
    }
}

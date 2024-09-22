using UnityEngine;
using UnityEngine.UI;
using MyBox;
using Photon.Pun;
using TMPro;

public class Card : UndoSource
{
    public string textBox { get; protected set; }
    public int value { get; protected set; }
    public CardLayout layout { get; private set; }
    public Color myColor { get; protected set; }
    public int cardID { get; private set; }

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        layout = GetComponent<CardLayout>();
        SpecificSetup();
        layout.FillInCards(this, 1);
    }

    protected virtual void SpecificSetup()
    {
    }

    public void GetCardID(int ID)
    {
        cardID = ID;
    }

    public virtual int Scoring(InPlay playArea)
    {
        return value;
    }
}

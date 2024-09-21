using UnityEngine;
using UnityEngine.UI;
using MyBox;
using Photon.Pun;

public class Card : UndoSource
{
    public string textBox;
    [ReadOnly] public CardLayout layout;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        layout = GetComponent<CardLayout>();
    }
}

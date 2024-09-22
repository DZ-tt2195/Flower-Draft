using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using MyBox;
using System.Reflection;
using System;

public enum Status { Revealed, Secret };
public class InPlay
{
    public Card card;
    public Status status;

    public InPlay(Card card, Status status)
    {
        this.card = card;
        this.status = status;
    }
}

public class Player : UndoSource
{

#region Variables

    [Foldout("Player info", true)]
        [ReadOnly] public int playerPosition;
        public Photon.Realtime.Player realTimePlayer { get; private set; }

    [Foldout("UI", true)]
        TMP_Text buttonText;
        Button resignButton;
        Transform playArea;
        public List<InPlay> cardsPlayed = new();

    [Foldout("Choices", true)]
        public int choice { get; private set; }
        public Card chosenCard { get; private set; }
        public Stack<List<Action>> decisionReact = new();

    #endregion

#region Setup

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        resignButton = GameObject.Find("Resign Button").GetComponent<Button>();
        playArea = transform.Find("Play Area");

        if (PhotonNetwork.IsConnected && pv.AmOwner)
            pv.Owner.NickName = PlayerPrefs.GetString("Online Username");
    }

    protected override void AddToMethodDictionary(string methodName)
    {
        FindMethod(this.GetType(), methodName);
    }

    private void Start()
    {
        this.transform.SetParent(Manager.instance.storePlayers);
        Manager.instance.storePlayers.transform.localScale = Manager.instance.canvas.transform.localScale;

        if (PhotonNetwork.IsConnected)
        {
            this.name = pv.Owner.NickName;
        }
    }

    internal void AssignInfo(int position)
    {
        this.playerPosition = position;
        this.transform.localPosition = new Vector3(2500 * this.playerPosition, 0, 0);

        if (PhotonNetwork.IsConnected)
        {
            Button newButton = Instantiate(CarryVariables.instance.playerButtonPrefab);
            newButton.transform.SetParent(this.transform.parent.parent);
            newButton.transform.localScale = Manager.instance.transform.localScale;

            newButton.transform.localPosition = new(-1150, -400 - (80 * playerPosition));
            buttonText = newButton.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText.text = this.name;
            newButton.onClick.AddListener(MoveScreen);
        }

        if (PhotonNetwork.IsConnected)
            realTimePlayer = pv.Owner;

        if (InControl())
        {
            resignButton.onClick.AddListener(() =>
            Manager.instance.MultiFunction(nameof(Manager.instance.DisplayEnding),
                RpcTarget.All, new object[1] { this.playerPosition }));
            MoveScreen();
        }
    }

    #endregion

#region Turn

    [PunRPC]
    public void RequestDraw(int cardsToDraw)
    {
        int[] listOfCardIDs = new int[cardsToDraw];

        for (int i = 0; i < cardsToDraw; i++)
        {
            listOfCardIDs[i] = Manager.instance.deck.GetChild(i).GetComponent<Card>().cardID;
        }

        this.MultiFunction(nameof(ChooseFaceDown), realTimePlayer, new object[1] { listOfCardIDs });
    }

    [PunRPC]
    void ChooseFaceDown(int[] listOfCardIDs)
    {
        Player nextPlayer = Manager.instance.playersInOrder[(playerPosition + 1) % Manager.instance.playersInOrder.Count];
        List<Card> cardList = new();
        int[] alphaList = new int[listOfCardIDs.Length];

        for (int i = 0; i < listOfCardIDs.Length; i++)
        {
            Card newCard = Manager.instance.listOfCards[listOfCardIDs[i]];
            cardList.Add(newCard);
            alphaList[i] = 1;
        }
        ChooseCardFromPopup(cardList, alphaList.ToList(), $"Choose one to put face down for {nextPlayer.name}.", Next);

        void Next()
        {
            alphaList[choice] = 0;
            nextPlayer.MultiFunction(nameof(TakeCard), nextPlayer.realTimePlayer, new object[2] { listOfCardIDs, alphaList });
        }
    }

    [PunRPC]
    void TakeCard(int[] listOfCardIDs, int[] alphas)
    {
        Player previousPlayer = Manager.instance.playersInOrder[(playerPosition - 1 + Manager.instance.playersInOrder.Count) % Manager.instance.playersInOrder.Count];
        List<Card> cardList = new();

        for (int i = 0; i < listOfCardIDs.Length; i++)
            cardList.Add(Manager.instance.listOfCards[listOfCardIDs[i]]);

        ChooseCardFromPopup(cardList, alphas.ToList(), $"Take a card.", Next);

        void Next()
        {
            int storeChoice = this.choice;
            int otherChoice = (storeChoice + 1) % 2;
            this.MultiFunction(nameof(PlayCard), RpcTarget.All, new object[2] { listOfCardIDs[storeChoice], alphas[storeChoice] });
            previousPlayer.MultiFunction(nameof(PlayCard), RpcTarget.All, new object[2] { listOfCardIDs[otherChoice], alphas[otherChoice] });
        }
    }

    [PunRPC]
    void PlayCard(int cardID, int alpha)
    {
        Card card = Manager.instance.listOfCards[cardID];
        cardsPlayed.Add(new(card, alpha == 0 ? Status.Secret : Status.Revealed));
        card.layout.ChangeAlpha(InControl() ? 1 : alpha);
        card.transform.SetParent(playArea);
        card.transform.localPosition = new(-1000 + 250 * cardsPlayed.Count, alpha == 0 ? -250 : 250);
    }

    #endregion

#region Decisions

    public void ChooseButton(string[] possibleChoices, string changeInstructions, Action action)
    {
        Popup popup = Instantiate(CarryVariables.instance.textPopup);
        popup.StatsSetup(this, "Choices", new(0, -500));

        for (int i = 0; i < possibleChoices.Length; i++)
            popup.AddTextButton(possibleChoices[i]);

        DecisionMade(CarryVariables.instance.undecided);
        AddDecisionReact(() => Destroy(popup.gameObject));
        AddDecisionReact(action);
        Manager.instance.Instructions(changeInstructions);

        popup.WaitForChoice();
    }

    public void ChooseCardFromPopup(List<Card> listOfCards, List<int> listOfAlphas, string changeInstructions, Action action)
    {
        Manager.instance.Instructions(changeInstructions);
        Popup popup = Instantiate(CarryVariables.instance.cardPopup);
        popup.transform.SetParent(this.transform);
        popup.StatsSetup(this, changeInstructions, Vector3.zero);

        DecisionMade(CarryVariables.instance.undecided);
        AddDecisionReact(Disable);
        AddDecisionReact(action);

        for (int i = 1; i < listOfCards.Count; i++)
            popup.AddCardButton(listOfCards[i], listOfAlphas[i]);

        void Disable()
        {
            if (popup != null)
                Destroy(popup.gameObject);
        }
        popup.WaitForChoice();
    }

    public void DecisionMade(int value, Card card = null)
    {
        //Debug.Log($"decision: {value}");
        choice = value;
        this.chosenCard = card;
        if (value == CarryVariables.instance.undecided)
        {
            decisionReact.Push(new());
        }
        else
        {
            try
            {
                List<Action> next = decisionReact.Pop();
                foreach (Action action in next)
                    action();
            }
            catch{}
        }
    }

    public void AddDecisionReact(Action action)
    {
        decisionReact.Peek().Add(action);
    }

    #endregion

#region Helpers

    public bool InControl()
    {
        if (PhotonNetwork.IsConnected)
            return this.pv.AmOwner;
        else
            return true;
    }

    void MoveScreen()
    {
        Manager.instance.storePlayers.localPosition = new Vector3(-2500 * this.playerPosition, 0, 0);
    }

    #endregion

}

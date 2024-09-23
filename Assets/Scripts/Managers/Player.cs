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

public class Player : UndoSource
{

#region Variables

    [Foldout("Player info", true)]
        [ReadOnly] public int playerPosition;
        public Photon.Realtime.Player realTimePlayer { get; private set; }

    [Foldout("UI", true)]
        TMP_Text buttonText;
        Button resignButton;
        Transform hand;
        Transform playArea;

    [Foldout("Cards", true)]
    public List<Card> cardsPlayed = new();
    List<Card> cardsInHand = new();

    [Foldout("Choices", true)]
        public int choice { get; private set; }
        public Card chosenCard { get; private set; }
        public Stack<List<Action>> decisionReact = new();
        List<NextStep> listOfSteps = new();

    #endregion

#region Setup

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        resignButton = GameObject.Find("Resign Button").GetComponent<Button>();
        playArea = transform.Find("Play Area");
        hand = transform.Find("Hand");

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

#region Hand

    [PunRPC]
    public void RequestDraw(int cardsToDraw)
    {
        int[] listOfCardIDs = new int[cardsToDraw];

        for (int i = 0; i < cardsToDraw; i++)
        {
            listOfCardIDs[i] = Manager.instance.deck.GetChild(i).GetComponent<Card>().cardID;
        }

        this.MultiFunction(nameof(PutCardsInHand), RpcTarget.All, new object[1] { listOfCardIDs });
    }

    [PunRPC]
    void PutCardsInHand(int[] cardsToDraw)
    {
        for (int i = 0; i < cardsToDraw.Length; i++)
        {
            Card card = Manager.instance.listOfCards[cardsToDraw[i]];
            card.transform.SetParent(this.hand);
            card.transform.localPosition = new Vector2(0, -1100);
            card.layout.FillInCards(card);
            card.layout.cg.alpha = 0;
            cardsInHand.Add(card);
        }
        SortHand();
    }

    public void SortHand()
    {
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            Card nextCard = cardsInHand[i];
            nextCard.transform.SetSiblingIndex(i);
            Vector2 newPosition = new(-750 + 225 * i, -535);
            StartCoroutine(nextCard.MoveCard(newPosition, 0.25f));
            if (InControl())
                StartCoroutine(nextCard.RevealCard(0.25f));
        }
    }

    #endregion

#region Turn

    [PunRPC]
    public void OfferCards()
    {
        Player nextPlayer = Manager.instance.NextPlayer(this);
        List<int> cardList = new();
        Loop();

        void Loop()
        {
            if (cardList.Count == 0)
                ChooseCardOnScreen(cardsInHand, "", "Choose a card to offer (face up).", Resolution);
            else if (cardList.Count == 1)
                ChooseCardOnScreen(cardsInHand, "", "Choose a card to offer (face down).", Resolution);
            else
                nextPlayer.MultiFunction(nameof(TakeCard), nextPlayer.realTimePlayer, new object[1] { cardList.ToArray() });
        }

        void Resolution()
        {
            Card card = chosenCard;
            cardsInHand.Remove(card);
            StartCoroutine(card.MoveCard(new(0, -1000), 0.25f));
            SortHand();
            cardList.Add(card.cardID);
            Loop();
        }
    }

    [PunRPC]
    void TakeCard(int[] listOfCardIDs)
    {
        Player previousPlayer = Manager.instance.PrevPlayer(this);
        List<Card> cardList = new();
        List<int> alphas = new() { 1, 0 };

        for (int i = 0; i < listOfCardIDs.Length; i++)
            cardList.Add(Manager.instance.listOfCards[listOfCardIDs[i]]);

        ChooseCardFromPopup(cardList, alphas, $"Which card to play?", Next);

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
        cardsPlayed.Add(card);
        card.transform.SetParent(playArea);
        StartCoroutine(card.MoveCard(new(-800 + 225 * cardsPlayed.Count, 0), 0.25f));
        card.ChangeStatus(this.playerPosition, alpha == 0 ? (int)Status.Concealed : (int)Status.Revealed);

        if (alpha == 1)
            Log.instance.MultiFunction(nameof(Log.AddText), RpcTarget.All, new object[2] { $"{this.name} takes {card.name}, revealed.", 0 });
        else if (InControl())
            Log.instance.MultiFunction(nameof(Log.AddText), RpcTarget.All, new object[2] { $"{this.name} takes {card.name}, concealed.", 0 });
        else
            Log.instance.MultiFunction(nameof(Log.AddText), RpcTarget.All, new object[2] { $"{this.name} takes a card, concealed.", 0 });
    }

    #endregion

#region Scoring

    [PunRPC]
    public void BeforeScoringDecisions()
    {
        List<Card> needDecisions = this.cardsPlayed.Where(card => card.myType == CardType.BeforeScoring).ToList();
        Loop();

        void Loop()
        {
            if (needDecisions.Count > 0)
            {
                ChooseCardOnScreen(needDecisions, "", "Choose the next card to resolve.", ResolveCard);

                void ResolveCard()
                {
                    Card next = chosenCard;
                    needDecisions.Remove(next);
                    DecisionMade(CarryVariables.instance.undecided);
                    AddDecisionReact(Loop);

                    PreserveLogTextRPC($"{this.name} resolves {next.name}.", 0);
                    next.BeforeScoring(this, 1);
                }
            }
            else
            {
                Manager.instance.Instructions($"Wait for others to finish...");
                this.listOfSteps = Log.instance.RememberSteps();
                Manager.instance.MultiFunction(nameof(Manager.PlayerDoneDeciding), RpcTarget.MasterClient);
            }
        }
    }

    public int CalculateScore()
    {
        int total = 0;
        foreach (Card card in cardsPlayed)
            total += card.Scoring(this);
        return total;
    }

    public List<Card> AdjacentCards(Card card)
    {
        int position = cardsPlayed.IndexOf(card);
        List<Card> adjacents = new();
        try { adjacents.Add(cardsPlayed[position - 1]); } catch { }
        try { adjacents.Add(cardsPlayed[position + 1]); } catch { }
        return adjacents;
    }

    [PunRPC]
    public void ShareSteps()
    {
        if (InControl() && PhotonNetwork.IsConnected)
        {
            Log.instance.pv.RPC(nameof(Log.instance.DeleteHistory), RpcTarget.All);
            foreach (NextStep step in listOfSteps)
                Log.instance.AddStepForOthers(1, this, step.source, step.instruction, step.infoToRemember, step.logged);
            listOfSteps.Clear();
        }
        Manager.instance.MultiFunction(nameof(Manager.PlayerDoneSharing), RpcTarget.MasterClient);
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

    public void ChooseCardOnScreen(List<Card> listOfCards, string sideParameter, string changeInstructions, Action action)
    {
        Manager.instance.Instructions(changeInstructions);
        Popup popup = null;
        IEnumerator haveCardsEnabled = KeepCardsOn();

        DecisionMade(CarryVariables.instance.undecided);
        AddDecisionReact(Disable);
        AddDecisionReact(action);

        if (sideParameter != "")
        {
            popup = Instantiate(CarryVariables.instance.textPopup);
            popup.StatsSetup(this, changeInstructions, new(0, -500));
            popup.AddTextButton(sideParameter);
            popup.WaitForChoice();
        }

        if (listOfCards.Count == 0 && sideParameter == "")
        {
            DecisionMade(-1);
        }
        else if (listOfCards.Count == 1 && sideParameter == "")
        {
            DecisionMade(1, listOfCards[0]);
        }
        else
        {
            StartCoroutine(haveCardsEnabled);
        }

        IEnumerator KeepCardsOn()
        {
            float elapsedTime = 0f;
            while (elapsedTime < 0.3f)
            {
                for (int j = 0; j < listOfCards.Count; j++)
                {
                    Card nextCard = listOfCards[j];
                    int buttonNumber = j;

                    nextCard.button.onClick.RemoveAllListeners();
                    nextCard.button.interactable = true;
                    nextCard.button.onClick.AddListener(() => DecisionMade(buttonNumber, nextCard));
                    nextCard.border.gameObject.SetActive(true);
                }
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        void Disable()
        {
            StopCoroutine(haveCardsEnabled);
            if (popup != null)
                Destroy(popup.gameObject);

            foreach (Card nextCard in listOfCards)
            {
                nextCard.button.onClick.RemoveAllListeners();
                nextCard.button.interactable = false;
                nextCard.border.gameObject.SetActive(false);
            }
        }
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

    public void PreserveLogTextRPC(string text, int logged = 0)
    {
        Log.instance.AddStep(1, this, this, nameof(PreserveText), new object[1] { text }, logged);
        Log.instance.Continue();
    }

    [PunRPC]
    void PreserveText(int logged)
    {
        NextStep step = Log.instance.GetCurrentStep();
        string text = (string)step.infoToRemember[0];
        Log.instance.AddText(text, logged);
    }

    #endregion

}

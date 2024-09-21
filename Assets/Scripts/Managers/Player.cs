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

    [Foldout("Choices", true)]
        public int choice { get; private set; }
        public Card chosenCard { get; private set; }
        List<NextStep> listOfSteps = new();
        public Stack<List<Action>> decisionReact = new();

    #endregion

#region Setup

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        if (PhotonNetwork.IsConnected && pv.AmOwner)
            pv.Owner.NickName = PlayerPrefs.GetString("Online Username");

        resignButton = GameObject.Find("Resign Button").GetComponent<Button>();
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

            if (!PhotonNetwork.IsConnected || PhotonNetwork.IsMasterClient)
                StartCoroutine(Simultaneous.instance.AssignStartingSteps());
        }
    }

    #endregion

#region Main Turn


#endregion

#region End of turn

    public void PreserveLogTextRPC(string text, int logged = 0)
    {
        Log.instance.AddStep(1, this, this, nameof(PreserveText), new object[1] { text }, logged);
        Log.instance.Continue();
    }

    [PunRPC]
    void PreserveText(int logged)
    {
        NextStep step = Log.instance.GetCurrentStep();
        try
        {
            string text = (string)step.infoToRemember[0];
            Log.instance.AddText(text, logged);
        }
        catch { }
    }

    public void WrapUp()
    {
        Manager.instance.Instructions($"Wait for others to finish...");
        this.listOfSteps = Log.instance.RememberSteps();
        Simultaneous.instance.MultiFunction(nameof(Simultaneous.instance.CompletedTurn), RpcTarget.MasterClient);
    }

    [PunRPC]
    public void ShareSteps()
    {
        if (InControl() && PhotonNetwork.IsConnected)
        {
            Log.instance.pv.RPC(nameof(Log.instance.DeleteHistory), RpcTarget.All);
            foreach (NextStep step in listOfSteps)
            {
                Log.instance.AddStepForOthers(1, this, step.source, step.instruction, step.infoToRemember, step.logged);
            }
            listOfSteps.Clear();
        }
        Simultaneous.instance.MultiFunction(nameof(Simultaneous.instance.FinishedSharing), RpcTarget.MasterClient);
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

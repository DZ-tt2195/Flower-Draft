using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

public enum GamePhase { Waiting, Offering, BeforeScoring, Scoring }
public class Manager : UndoSource
{

#region Variables

    public static Manager instance;

    [Foldout("Text", true)]
    [SerializeField] TMP_Text instructions;

    [Foldout("Cards", true)]
    public Transform deck;
    [ReadOnly] public List<Card> listOfCards = new();

    [Foldout("Animation", true)]
    public float opacity { get; private set;}
    bool decrease = true;
    public Canvas canvas { get; private set; }

    [Foldout("Players", true)]
    [ReadOnly] public List<Player> playersInOrder = new();
    public Transform storePlayers { get; private set; }
    Player currentPlayer;

    [Foldout("Scoring", true)]
    int waitingOnPlayers;
    bool waitingOnShare;

    [Foldout("Ending", true)]
    [SerializeField] Transform endScreen;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] Button quitGame;

    #endregion

#region Setup

    private void Awake()
    {
        instance = this;
        pv = GetComponent<PhotonView>();
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        storePlayers = GameObject.Find("Store Players").transform;
    }

    private void FixedUpdate()
    {
        if (decrease)
            opacity -= 0.05f;
        else
            opacity += 0.05f;
        if (opacity < 0 || opacity > 1)
            decrease = !decrease;
    }

    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Instantiate(CarryVariables.instance.playerPrefab.name, new(-10000, -10000, 0), new());
            StartCoroutine(Setup());

            if (PhotonNetwork.IsMasterClient)
            {
                for (int i = 0; i < CarryVariables.instance.cardNames.Count; i++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        GameObject next = PhotonNetwork.Instantiate(CarryVariables.instance.cardPrefab.name, new(-10000, -10000, 0), new());
                        next.transform.SetParent(deck);
                        next.name = CarryVariables.instance.cardNames[i];
                    }
                }
            }
        }
    }

    protected override void AddToMethodDictionary(string methodName)
    {
        FindMethod(this.GetType(), methodName);
    }

    IEnumerator Setup()
    {
        CoroutineGroup group = new(this);
        group.StartCoroutine(WaitForPlayers());

        while (group.AnyProcessing)
            yield return null;

        if (!PhotonNetwork.IsConnected || PhotonNetwork.IsMasterClient)
        {
            Invoke(nameof(GetPlayers), 0.5f);
        }
    }

    IEnumerator WaitForPlayers()
    {
        if (PhotonNetwork.IsConnected)
        {
            Instructions($"Waiting for more players ({storePlayers.childCount}/{PhotonNetwork.CurrentRoom.MaxPlayers})");
            while (storePlayers.childCount < PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                Instructions($"Waiting for more players ({storePlayers.childCount} / {PhotonNetwork.CurrentRoom.MaxPlayers})");
                yield return null;
            }
            Instructions($"All players are in.");
        }
    }

    void GetPlayers()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < deck.childCount; i++)
            {
                PhotonView next = deck.GetChild(i).GetComponent<PhotonView>();
                MultiFunction(nameof(AddCard), RpcTarget.All, new object[2] { next.ViewID, next.name });
            }
            deck.Shuffle();
        }

        storePlayers.Shuffle();
        for (int i = 0; i<storePlayers.childCount; i++)
        {
            Player player = storePlayers.transform.GetChild(i).GetComponent<Player>();
            currentPlayer = player;
            MultiFunction(nameof(AddPlayer), RpcTarget.All, new object[2] { player.name, i });
            player.MultiFunction(nameof(Player.RequestDraw), RpcTarget.MasterClient, new object[1] { 8 });
        }
        MultiFunction(nameof(NextTurn), RpcTarget.MasterClient);
    }

    [PunRPC]
    void AddCard(int ID, string name)
    {
        GameObject next = PhotonView.Find(ID).gameObject;
        next.name = name;
        next.transform.SetParent(deck);
        next.transform.localPosition = new(250 * listOfCards.Count, 0);

        next.AddComponent(Type.GetType(name));
        Card card = next.GetComponent<Card>();
        card.GetCardID(listOfCards.Count);
        listOfCards.Add(card);
    }

    [PunRPC]
    void AddPlayer(string name, int position)
    {
        Player nextPlayer = GameObject.Find(name).GetComponent<Player>();
        playersInOrder.Insert(position, nextPlayer);
        nextPlayer.AssignInfo(position);
    }

    #endregion

#region Gameplay

    public void WaitOnOthers()
    {
        MultiFunction(nameof(Instructions), RpcTarget.Others, new object[1] { "Waiting on other players..." });
    }

    [PunRPC]
    public void Instructions(string text)
    {
        instructions.text = (text);
    }

    public Player NextPlayer(Player thisPlayer)
    {
        return playersInOrder[(thisPlayer.playerPosition + 1) % playersInOrder.Count];
    }

    public Player PrevPlayer(Player thisPlayer)
    {
        return playersInOrder[(thisPlayer.playerPosition - 1 + playersInOrder.Count) % playersInOrder.Count];
    }

    [PunRPC]
    public void NextTurn()
    {
        bool keepPlaying = false;
        foreach (Player player in playersInOrder)
        {
            if (player.cardsPlayed.Count < 6)
            {
                keepPlaying = true;
                break;
            }
        }

        if (keepPlaying)
        {
            currentPlayer = playersInOrder[(currentPlayer.playerPosition + 1) % playersInOrder.Count];
            currentPlayer.MultiFunction(nameof(Player.OfferCards), currentPlayer.realTimePlayer);
        }
        else
        {
            StartCoroutine(PlayerScoringDecisions());
        }
    }

    #endregion

#region Scoring

    IEnumerator PlayerScoringDecisions()
    {
        foreach (Player player in playersInOrder)
        {
            player.MultiFunction(nameof(Player.BeforeScoringDecisions), player.realTimePlayer);
            waitingOnPlayers++;
        }
        while (waitingOnPlayers > 0)
            yield return null;

        foreach (Player player in playersInOrder)
        {
            waitingOnShare = true;
            player.pv.RPC(nameof(Player.ShareSteps), player.realTimePlayer);

            while (waitingOnShare)
                yield return null;
        }
        MultiFunction(nameof(DisplayEnding), RpcTarget.All, new object[1] {-1});
    }

    [PunRPC]
    public void PlayerDoneDeciding()
    {
        waitingOnPlayers--;
    }

    [PunRPC]
    public void PlayerDoneSharing()
    {
        waitingOnShare = false;
    }

    #endregion

#region Game End

    [PunRPC]
    public void DisplayEnding(int resignPosition)
    {
        Popup[] allPopups = FindObjectsByType<Popup>(FindObjectsSortMode.None);
        foreach (Popup popup in allPopups)
            Destroy(popup.gameObject);

        List<Player> playerScoresInOrder = playersInOrder.OrderByDescending(player => player.CalculateScore()).ToList();
        int nextPlacement = 1;
        scoreText.text = "";

        Player resignPlayer = null;
        if (resignPosition >= 0)
        {
            resignPlayer = playersInOrder[resignPosition];
        }

        for (int i = 0; i < playerScoresInOrder.Count; i++)
        {
            Player player = playerScoresInOrder[i];
            if (player != resignPlayer)
            {
                scoreText.text += $"{nextPlacement}: {player.name}: {player.CalculateScore()} VP\n";
                if (i == 0 || playerScoresInOrder[i - 1].CalculateScore() != player.CalculateScore())
                    nextPlacement++;
            }
        }

        if (resignPlayer != null)
        {
            scoreText.text += $"\nResigned: {resignPlayer.name}: {resignPlayer.CalculateScore()} VP";
        }

        endScreen.gameObject.SetActive(true);
        quitGame.onClick.AddListener(Leave);
    }

    void Leave()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene("1. Lobby");
        }
        else
        {
            SceneManager.LoadScene("0. Loading");
        }
    }

#endregion

}

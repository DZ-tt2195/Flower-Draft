using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System;

public class Manager : UndoSource
{

#region Variables

    public static Manager instance;

    [Foldout("Text", true)]
    [SerializeField] TMP_Text instructions;

    [Foldout("Animation", true)]
    public float opacity { get; private set;}
    bool decrease = true;
    public Canvas canvas { get; private set; }

    [Foldout("Players", true)]
    [ReadOnly] public List<Player> playersInOrder = new();
    public Transform storePlayers { get; private set; }

    [Foldout("Turn", true)]
    [ReadOnly] public int turnNumber { get; private set; }

    [Foldout("Ending", true)]
    [SerializeField] Transform endScreen;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] Button quitGame;
    [SerializeField] Button copyGame;
    Stopwatch stopwatch;
    string endText;

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
            PhotonNetwork.Instantiate(CarryVariables.instance.playerPrefab.name, new Vector3(-10000, -10000, 0), new Quaternion());
            StartCoroutine(Setup());
        }
        else
        {
            Player solitairePlayer = Instantiate(CarryVariables.instance.playerPrefab, new Vector3(-10000, -10000, 0), new Quaternion());
            solitairePlayer.name = "Solitaire";
            StartCoroutine(Setup());
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

        stopwatch = new Stopwatch();
        stopwatch.Start();
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
        if (CarryVariables.instance.debug)
            Log.instance.AddText("Debug mode.");

        for (int i = 0; i<storePlayers.childCount; i++)
        {
            Player player = storePlayers.transform.GetChild(i).GetComponent<Player>();
            MultiFunction(nameof(AddPlayer), RpcTarget.All, new object[2] { player.name, i });
        }
    }

    [PunRPC]
    void AddPlayer(string name, int position)
    {
        Player nextPlayer = GameObject.Find(name).GetComponent<Player>();
        playersInOrder.Insert(position, nextPlayer);
        nextPlayer.AssignInfo(position);
    }

    int OtherRandom(int max, int notThis)
    {
        if (max <= 1)
            return 0;

        int answer = notThis;
        while (answer == notThis)
            answer = UnityEngine.Random.Range(0, max);
        return answer;
    }

    (int, int) TwoDifferentRandom(int max)
    {
        if (max <= 1)
            return (0, 0);

        if (max == 2)
            return (0, 1);

        int one = 0;
        int two = 0;

        while (one == two)
        {
            one = UnityEngine.Random.Range(0, max);
            two = UnityEngine.Random.Range(0, max);
        }
        return (one, two);
    }

    #endregion

#region Gameplay

    [PunRPC]
    public void UpdateTurnNumber(int newNumber)
    {
        turnNumber = newNumber;
    }

    public void Instructions(string text)
    {
        instructions.text = KeywordTooltip.instance.EditText(text);
    }

    #endregion

#region Seasons

    [ReadOnly] List<int> springTurns = new() { 1, 5, 9 };
    [ReadOnly] List<int> summerTurns = new() { 2, 6, 10 };
    [ReadOnly] List<int> autumnTurns = new() { 3, 7, 11 };
    [ReadOnly] List<int> winterTurns = new() { 4, 8, 12 };

    public int CurrentYearNumber()
    {
        int currentNumber = CurrentSeasonNumber("Spring");
        if (currentNumber >= 0)
            return currentNumber;

        currentNumber = CurrentSeasonNumber("Summer");
        if (currentNumber >= 0)
            return currentNumber;

        currentNumber = CurrentSeasonNumber("Autumn");
        if (currentNumber >= 0)
            return currentNumber;

        currentNumber = CurrentSeasonNumber("Winter");
        if (currentNumber >= 0)
            return currentNumber;

        return -1;
    }

    public int CurrentSeasonNumber(string season)
    {
        return season switch
        {
            "Spring" => springTurns.IndexOf(turnNumber)+1,
            "Summer" => summerTurns.IndexOf(turnNumber)+1,
            "Autumn" => autumnTurns.IndexOf(turnNumber)+1,
            "Winter" => winterTurns.IndexOf(turnNumber)+1,
            _ => -1,
        };
    }

    public string CurrentSeasonText()
    {
        int currentNumber = CurrentSeasonNumber("Spring");
        if (currentNumber >= 1)
            return $"Spring {currentNumber}";

        currentNumber = CurrentSeasonNumber("Summer");
        if (currentNumber >= 1)
            return $"Summer {currentNumber}";

        currentNumber = CurrentSeasonNumber("Autumn");
        if (currentNumber >= 1)
            return $"Autumn {currentNumber}";

        currentNumber = CurrentSeasonNumber("Winter");
        if (currentNumber >= 1)
            return $"Winter {currentNumber}";

        return "failed";
    }

#endregion

#region Game End

    [PunRPC]
    public void DisplayEnding(int resignPosition)
    {
        Popup[] allPopups = FindObjectsByType<Popup>(FindObjectsSortMode.None);
        foreach (Popup popup in allPopups)
            Destroy(popup.gameObject);
        /*
        List<Player> playerScoresInOrder = playersInOrder.OrderByDescending(player => player.CalculateScore()).ToList();
        int nextPlacement = 1;
        scoreText.text = "";

        Log.instance.AddText("");
        Log.instance.AddText("The game has ended.");
        Player resignPlayer = null;
        if (resignPosition >= 0)
        {
            resignPlayer = playersInOrder[resignPosition];
            Log.instance.AddText($"{resignPlayer.name} has resigned.");
        }

        stopwatch.Stop();
        endText = (CarryVariables.instance.debug) ? "Debug Game\n" : "";
        endText += $"Game length: {CalculateTime()}\n";

        string CalculateTime()
        {
            TimeSpan time = stopwatch.Elapsed;
            string part = time.Seconds < 10 ? $"0{time.Seconds}" : $"{time.Seconds}";
            return $"{time.Minutes}:{part}";
        }

        for (int i = 0; i<buyablesInOrder.Count; i++)
        {
            endText += buyablesInOrder[i].name;
            if (i < buyablesInOrder.Count - 1)
                endText += ", ";
        }
        endText += "\n";
        for (int i = 0; i < twistsInOrder.Count; i++)
        {
            endText += twistsInOrder[i].name;
            if (i < twistsInOrder.Count - 1)
                endText += ", ";
        }

        for (int i = 0; i < playerScoresInOrder.Count; i++)
        {
            Player player = playerScoresInOrder[i];
            if (player != resignPlayer)
            {
                EndstatePlayer(player, false);
                scoreText.text += $"{nextPlacement}: {player.name}: {player.CalculateScore()} VP\n";
                if (i == 0 || playerScoresInOrder[i - 1].CalculateScore() != player.CalculateScore())
                    nextPlacement++;
            }
        }

        if (resignPlayer != null)
        {
            EndstatePlayer(resignPlayer, true);
            scoreText.text += $"\nResigned: {resignPlayer.name}: {resignPlayer.CalculateScore()} VP";
        }
        scoreText.text = KeywordTooltip.instance.EditText(scoreText.text);

        endScreen.gameObject.SetActive(true);
        quitGame.onClick.AddListener(Leave);
        copyGame.onClick.AddListener(() => GUIUtility.systemCopyBuffer = endText);
        */
    }

    void EndstatePlayer(Player player, bool resigned)
    {
        /*
        endText += $"\n\n{player.name} - {player.CalculateScore()} VP {(resigned ? $"[Resigned on turn {turnNumber}]" : "")}\n";
        Dictionary<Area, int> dictionary = player.BuildingCount(player.listOfDiceStorages);
        foreach (KeyValuePair<Area, int> pair in dictionary)
            endText += $"{pair.Value} {pair.Key}, ";
        endText += $"{player.peasantsAround.Count} Peasant, {player.resourceDictionary[Resource.Dice]} Dice";
        */
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

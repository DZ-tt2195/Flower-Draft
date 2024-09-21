using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MyBox;
using System.Text.RegularExpressions;
using Photon.Pun;
using System.Reflection;
using System;
using Photon.Realtime;

[Serializable]
public class NextStep
{
    public UndoSource source;
    public Player player;
    public string instruction;
    public object[] infoToRemember;
    public Player canUndoThis;
    public int logged;

    internal NextStep(Player player, UndoSource source, string instruction, object[] infoToRemember, int logged)
    {
        this.player = player;
        this.source = source;
        this.instruction = instruction;
        this.infoToRemember = infoToRemember;
        this.logged = logged;
    }
}

public class Log : UndoSource
{

#region Variables

    public static Log instance;

    [Foldout("Log", true)]
    Scrollbar scroll;
    [SerializeField] RectTransform RT;
    GridLayoutGroup gridGroup;
    [SerializeField] LogText textBoxClone;
    Vector2 startingSize;
    Vector2 startingPosition;

    [Foldout("Undo", true)]
    [ReadOnly][SerializeField] int currentStep = -1;
    [ReadOnly] public List<NextStep> historyStack = new();
    public Dictionary<string, MethodInfo> dictionary = new();

    #endregion

#region Setup

    private void Awake()
    {
        instance = this;
        pv = GetComponent<PhotonView>();
        gridGroup = RT.GetComponent<GridLayoutGroup>();
        scroll = this.transform.GetChild(1).GetComponent<Scrollbar>();

        startingSize = RT.sizeDelta;
        startingPosition = RT.transform.localPosition;
    }

    protected override void AddToMethodDictionary(string methodName)
    {
        FindMethod(this.GetType(), methodName);
    }

    #endregion

#region Add To Log

    public static string Article(string followingWord)
    {
        if (followingWord.StartsWith('A')
            || followingWord.StartsWith('E')
            || followingWord.StartsWith('I')
            || followingWord.StartsWith('O')
            || followingWord.StartsWith('U'))
        {
            return $"an {followingWord}";
        }
        else
        {
            return $"a {followingWord}";
        }
    }

    [PunRPC]
    public void AddText(string logText, int indent = 0)
    {
        if (indent < 0)
            return;

        //Debug.Log($"add to log: {logText}");

        LogText newText = Instantiate(textBoxClone, RT.transform);
        newText.name = $"Log {RT.transform.childCount}";
        newText.textBox.text = "";
        for (int i = 0; i < indent; i++)
            newText.textBox.text += "     ";
        newText.textBox.text += string.IsNullOrEmpty(logText) ? "" : char.ToUpper(logText[0]) + logText[1..];

        newText.textBox.text = KeywordTooltip.instance.EditText(newText.textBox.text);
        ChangeScrolling();
    }

    void ChangeScrolling()
    {
        int goPast = Mathf.FloorToInt((startingSize.y / gridGroup.cellSize.y) - 1);
        //Debug.Log($"{RT.transform.childCount} vs {goPast}");
        if (RT.transform.childCount > goPast)
        {
            RT.sizeDelta = new Vector2(startingSize.x, startingSize.y + ((RT.transform.childCount - goPast) * gridGroup.cellSize.y));
            if (scroll.value <= 0.2f)
            {
                RT.transform.localPosition = new Vector3(RT.transform.localPosition.x, RT.transform.localPosition.y + gridGroup.cellSize.y / 2, 0);
                scroll.value = 0;
            }
        }
        else
        {
            RT.sizeDelta = startingSize;
            RT.transform.localPosition = startingPosition;
            scroll.value = 0;
        }
    }

    private void Update()
    {
        if (Application.isEditor && Input.GetKeyDown(KeyCode.Space))
            AddText($"test {RT.transform.childCount}");
    }

    void OnEnable()
    {
        Application.logMessageReceived += DebugMessages;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= DebugMessages;
    }

    void DebugMessages(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error)
        {
            AddText($"");
            AddText($"the game crashed :(");
        }
    }

    #endregion

#region Steps

    public NextStep GetCurrentStep()
    {
        return historyStack[currentStep];
    }

    public void Continue()
    {
        if (currentStep < historyStack.Count - 1)
        {
            currentStep++;
            NextStep nextUp = GetCurrentStep();
            nextUp.source.MultiFunction(nextUp.instruction, null, new object[1] { nextUp.logged });
        }
    }

    public void AddStepForOthers(int insertion, Player player, UndoSource source, string instruction, object[] infoToRemember, int logged)
    {
        pv.RPC(nameof(AddStep), RpcTarget.Others, insertion, player == null ? -1 : player.playerPosition, source == null ? -1 : source.pv.ViewID, instruction, infoToRemember, logged);
    }

    [PunRPC]
    void AddStep(int insertion, int playerPosition, int source, string instruction, object[] infoToRemember, int logged)
    {
        AddStep(insertion, playerPosition < 0 ? null : Manager.instance.playersInOrder[playerPosition],
            source < 0 ? null : PhotonView.Find(source).GetComponent<UndoSource>(), instruction, infoToRemember, logged);
        Continue();
    }

    public void AddStep(int insertion, Player player, UndoSource source, string instruction, object[] infoToRemember, int logged)
    {
        NextStep newStep = new(player, source, instruction, infoToRemember, logged);
        //Debug.Log($"add: {player.name}: {instruction}");
        try
        {
            historyStack.Insert(currentStep + insertion, newStep);
        }
        catch
        {
            historyStack.Add(newStep);
        }
    }

    public List<NextStep> RememberSteps()
    {
        List<NextStep> retainedHistory = new();
        foreach (NextStep step in historyStack)
            retainedHistory.Add(step);
        DeleteHistory();
        return retainedHistory;
    }

    [PunRPC]
    public void DeleteHistory()
    {
        currentStep = -1;
        historyStack.Clear();
    }

    #endregion

}

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

    internal NextStep(Player player, UndoSource source, string instruction, object[] infoToRemember)
    {
        this.player = player;
        this.source = source;
        this.instruction = instruction;
        this.infoToRemember = infoToRemember;
    }
}

public class Log : UndoSource
{

#region Variables

    public static Log instance;

    [SerializeField] int currentStep = -1;
    [ReadOnly] public List<NextStep> historyStack = new();
    public Dictionary<string, MethodInfo> dictionary = new();

    #endregion

#region Setup

    private void Awake()
    {
        instance = this;
        pv = GetComponent<PhotonView>();
    }

    protected override void AddToMethodDictionary(string methodName)
    {
        FindMethod(this.GetType(), methodName);
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
            nextUp.source.MultiFunction(nextUp.instruction, null, new object[0]);
        }
    }

    public void AddStepForOthers(int insertion, Player player, UndoSource source, string instruction, object[] infoToRemember)
    {
        pv.RPC(nameof(AddStep), RpcTarget.Others, insertion, player == null ? -1 : player.playerPosition, source == null ? -1 : source.pv.ViewID, instruction, infoToRemember);
    }

    [PunRPC]
    void AddStep(int insertion, int playerPosition, int source, string instruction, object[] infoToRemember)
    {
        AddStep(insertion, playerPosition < 0 ? null : Manager.instance.playersInOrder[playerPosition],
            source < 0 ? null : PhotonView.Find(source).GetComponent<UndoSource>(), instruction, infoToRemember);
        Continue();
    }

    public void AddStep(int insertion, Player player, UndoSource source, string instruction, object[] infoToRemember)
    {
        NextStep newStep = new(player, source, instruction, infoToRemember);

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

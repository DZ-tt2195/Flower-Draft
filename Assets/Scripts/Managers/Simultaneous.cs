using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using MyBox;
using System;
using System.Reflection;

public class Simultaneous : UndoSource
{

#region Setup

    public static Simultaneous instance;
    int waitingOnPlayers;
    bool waitingOnShare;
    int[] attackArray;

    [ReadOnly][SerializeField] int currentStep = -1;
    [ReadOnly] public List<Action> actionStack = new();

    protected override void AddToMethodDictionary(string methodName)
    {
        FindMethod(this.GetType(), methodName);
    }

    private void Awake()
    {
        instance = this;
        pv = GetComponent<PhotonView>();
    }

    public IEnumerator AssignStartingSteps()
    {
        attackArray = new int[Manager.instance.playersInOrder.Count];
        for (int i = 0; i < 12; i++)
        {
            AddStep(SimultaneousTurns);
        }
        AddStep(this.TurnsFinished);

        if (PhotonNetwork.IsConnected)
        {
            while (Manager.instance.playersInOrder.Count < PhotonNetwork.CurrentRoom.MaxPlayers)
                yield return null;
        }
        Continue();
    }

    public void AddStep(Action action, int position = -1)
    {
        if (!PhotonNetwork.IsConnected || PhotonNetwork.IsMasterClient)
        {
            if (position < 0 || currentStep < 0)
                actionStack.Add(action);
            else
                actionStack.Insert(currentStep + position, action);
        }
    }

    public void Continue()
    {
        if (currentStep < actionStack.Count - 1)
        {
            currentStep++;
            Log.instance.MultiFunction(nameof(Log.instance.DeleteHistory), RpcTarget.All);
            Action nextUp = actionStack[currentStep];
            waitingOnPlayers = Manager.instance.playersInOrder.Count;
            //Debug.Log($"{nextUp.GetMethodInfo()}, {currentStep}");
            nextUp();
        }
    }

    [PunRPC]
    public IEnumerator CompletedTurn()
    {
        int currentWait = waitingOnPlayers;

        if (!PhotonNetwork.IsConnected)
        {
            Continue();
        }
        else
        {
            waitingOnPlayers--;
            if (waitingOnPlayers == 0)
            {
                foreach (Player player in Manager.instance.playersInOrder)
                {
                    waitingOnShare = true;
                    player.pv.RPC(nameof(player.ShareSteps), player.realTimePlayer);

                    while (waitingOnShare)
                        yield return null;
                }

                Continue();
            }
        }
    }

    [PunRPC]
    public void FinishedSharing()
    {
        waitingOnShare = false;
    }

    #endregion

#region Steps

    public void SimultaneousTurns()
    {
        Manager.instance.MultiFunction(nameof(Manager.instance.UpdateTurnNumber), RpcTarget.All, new object[1] { Manager.instance.turnNumber + 1 });
        Log.instance.MultiFunction(nameof(Log.AddText), RpcTarget.All, new object[2] { $"", 0 });
        Log.instance.MultiFunction(nameof(Log.AddText), RpcTarget.All, new object[2] { $"Turn {Manager.instance.turnNumber} / 12 - {Manager.instance.CurrentSeasonText()}", 0 });
        /*foreach (Player player in Manager.instance.playersInOrder)
            player.MultiFunction(nameof(player.StartTurn), player.realTimePlayer, new object[0]);*/
    }

    [PunRPC]
    public void AddAttack(int amount, int playerPosition)
    {
        Debug.Log($"player {playerPosition} attacks {amount} times");
        for (int i = 0; i<attackArray.Length; i++)
        {
            if (i != playerPosition)
                attackArray[i] += amount;
        }
    }

    void TurnsFinished()
    {
        Manager.instance.MultiFunction(nameof(Manager.instance.DisplayEnding), RpcTarget.All, new object [1] { -1 });
    }

#endregion

}

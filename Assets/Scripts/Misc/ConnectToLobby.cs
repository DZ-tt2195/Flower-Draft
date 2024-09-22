using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using System;

public class ConnectToLobby : MonoBehaviourPunCallbacks
{

#region Setup

    [SerializeField] GameObject connectScreen;

    public void Start()
    {
        Application.targetFrameRate = 60;
    }

    void LocalPlay(bool debug)
    {
        SceneManager.LoadScene("2. Game");
    }

    #endregion

#region Connection

    public void Join(string region)
    {
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = region;
        PhotonNetwork.ConnectUsingSettings();
        PlayerPrefs.SetString("Online Username", "");
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        SceneManager.LoadScene("1. Lobby");
    }

    #endregion

}

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

        LogText newText = Instantiate(textBoxClone, RT.transform);
        newText.name = $"Log {RT.transform.childCount}";
        newText.textBox.text = "";
        for (int i = 0; i < indent; i++)
            newText.textBox.text += "     ";
        newText.textBox.text += string.IsNullOrEmpty(logText) ? "" : char.ToUpper(logText[0]) + logText[1..];
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

}

using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System.Reflection;
using Photon.Pun;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class CarryVariables : MonoBehaviour
{
    public static CarryVariables instance;
    [Foldout("Prefabs", true)]
    public Player playerPrefab;
    public Popup textPopup;
    public Popup cardPopup;
    public Button playerButtonPrefab;

    [Foldout("Right click", true)]
    [SerializeField] Transform rightClickBackground;
    [SerializeField] CardLayout rightClickCard;

    [Foldout("Misc", true)]
    [SerializeField] Transform permanentCanvas;
    [ReadOnly] public Dictionary<string, MethodInfo> dictionary = new();
    public Sprite faceDownSprite;
    public int undecided { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Application.targetFrameRate = 60;
            undecided = -100000;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            rightClickBackground.gameObject.SetActive(false);
    }

    public void RightClickDisplay(Card card, float alpha)
    {
        rightClickBackground.gameObject.SetActive(true);
        rightClickCard.FillInCards(card, alpha);
    }
}

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
    public Button playerButtonPrefab;

    [Foldout("Right click", true)]
    [SerializeField] Transform rightClickBackground;
    [SerializeField] Image cardImage;

    [Foldout("Misc", true)]
    [SerializeField] Transform permanentCanvas;
    [ReadOnly] public Dictionary<string, MethodInfo> dictionary = new();
    public Sprite faceDownSprite;
    [SerializeField] Image transitionImage;
    public bool debug { get; private set; }
    public int undecided { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Application.targetFrameRate = 60;
            undecided = -100000;
            debug = false;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public IEnumerator TransitionImage(float time)
    {
        float elapsedTime = 0f;
        transitionImage.SetAlpha(1);
        transitionImage.gameObject.SetActive(true);

        while (elapsedTime < time)
        {
            transitionImage.SetAlpha(1-(elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transitionImage.SetAlpha(0);
        transitionImage.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            rightClickBackground.gameObject.SetActive(false);
    }

    public void RightClickDisplay(Sprite bigArt)
    {
        rightClickBackground.gameObject.SetActive(true);
        cardImage.transform.gameObject.SetActive(true);
        cardImage.sprite = bigArt;
    }

    public void DebugMode(bool confirm)
    {
        debug = confirm;
    }
}

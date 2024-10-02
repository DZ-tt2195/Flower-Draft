using UnityEngine;
using UnityEngine.UI;
using MyBox;
using Photon.Pun;
using TMPro;
using System.Collections;

public enum CardType { OnlyScoring, BeforeScoring }
public enum Status { Concealed, Revealed };
public class Card : UndoSource
{

#region Variables

    public Button button { get; private set; }
    public Image border { get; private set; }
    public CardLayout layout { get; private set; }

    public string textBox { get; protected set; }
    public int value { get; protected set; }
    public Color myColor { get; protected set; }
    public CardType myType { get; protected set; }
    public int cardID { get; private set; }
    public Status status { get; private set; }
    public int applyAbility;

    #endregion

#region Setup

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        border = this.transform.Find("Border").GetComponent<Image>();
        button = GetComponent<Button>();
        layout = GetComponent<CardLayout>();
        this.transform.localScale = Vector3.Lerp(Vector3.one, Manager.instance.canvas.transform.localScale, 0.5f);

        SpecificSetup();
        layout.FillInCards(this);
        applyAbility = 1;
    }

    protected override void AddToMethodDictionary(string methodName)
    {
        FindMethod(this.GetType(), methodName);
    }

    protected virtual void SpecificSetup()
    {
    }

    public void GetCardID(int ID)
    {
        cardID = ID;
    }

    #endregion

#region Gameplay

    [PunRPC]
    public void ChangeStatus(int playerID, int newStatus)
    {
        Player player = Manager.instance.playersInOrder[playerID];
        this.status = (Status)newStatus;
        this.layout.FillInCards(this);
        player.SortPlayArea();

        if (this.status == Status.Revealed)
            this.layout.cg.alpha = 1;
        else
            this.layout.cg.alpha = player.InControl() ? 1 : 0;
    }

    public virtual void BeforeScoring(Player player, int logged)
    {
    }

    public int Scoring(Player player)
    {
        int addOn = 0;
        for (int i = 0; i < applyAbility; i++)
            addOn += Ability(player);
        return value + addOn;
    }

    protected virtual int Ability(Player player)
    {
        return 0;
    }

    #endregion

#region Animations

    public IEnumerator MoveCard(Vector2 newPos, float waitTime)
    {
        float elapsedTime = 0;
        Vector2 originalPos = this.transform.localPosition;

        while (elapsedTime < waitTime)
        {
            this.transform.localPosition = Vector2.Lerp(originalPos, newPos, elapsedTime / waitTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        this.transform.localPosition = newPos;
    }

    public IEnumerator RevealCard(float totalTime)
    {
        if (this.layout.cg.alpha == 1)
            yield break;

        transform.localEulerAngles = new Vector3(0, 0, 0);
        float elapsedTime = 0f;

        Vector3 originalRot = this.transform.localEulerAngles;
        Vector3 newRot = new(0, 90, 0);

        while (elapsedTime < totalTime)
        {
            this.transform.localEulerAngles = Vector3.Lerp(originalRot, newRot, elapsedTime / totalTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        layout.cg.alpha = 1;
        elapsedTime = 0f;

        while (elapsedTime < totalTime)
        {
            this.transform.localEulerAngles = Vector3.Lerp(newRot, originalRot, elapsedTime / totalTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        this.transform.localEulerAngles = originalRot;
    }

    private void FixedUpdate()
    {
        try
        {
            this.border.SetAlpha(Manager.instance.opacity);
        }
        catch
        {
        }
    }

    #endregion

}

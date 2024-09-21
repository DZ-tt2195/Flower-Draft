using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MyBox;
using TMPro;

public class CardLayout : MonoBehaviour, IPointerClickHandler
{
    CanvasGroup cg;
    public Image background { get; private set; }
    TMP_Text titleText;
    TMP_Text description;
    Card myCard;

    private void Awake()
    {
        cg = transform.Find("Canvas Group").GetComponent<CanvasGroup>();
        background = cg.transform.Find("Background").GetComponent<Image>();
        titleText = cg.transform.Find("Title").GetComponent<TMP_Text>();
        description = cg.transform.Find("Description").GetComponent<TMP_Text>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            CarryVariables.instance.RightClickDisplay(this.myCard, this.background.color, cg.alpha);
        }
    }

    public void FillInCards(Card card, Color color, float alpha)
    {
        myCard = card;
        background.color = color;
        titleText.text = card.name;
        description.text = card.textBox;
        cg.alpha = alpha;
    }
}

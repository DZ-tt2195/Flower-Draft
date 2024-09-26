using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MyBox;
using TMPro;

public class CardLayout : MonoBehaviour, IPointerClickHandler
{
    public CanvasGroup cg { get; private set; }
    public Image background { get; private set; }
    TMP_Text valueText;
    TMP_Text description;
    Card myCard;

    private void Awake()
    {
        cg = transform.Find("Canvas Group").GetComponent<CanvasGroup>();
        background = cg.transform.Find("Background").GetComponent<Image>();
        valueText = cg.transform.Find("Value").GetComponent<TMP_Text>();
        description = cg.transform.Find("Description").GetComponent<TMP_Text>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            CarryVariables.instance.RightClickDisplay(this.myCard, cg.alpha);
        }
    }

    public void FillInCards(Card card)
    {
        myCard = card;
        background.color = card.myColor;
        valueText.text = $"{card.value} VP";
        description.text = card.textBox;
    }
}

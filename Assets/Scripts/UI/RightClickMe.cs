using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;

public class RightClickMe : MonoBehaviour, IPointerClickHandler
{
    Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            CarryVariables.instance.RightClickDisplay(image.sprite);
        }
    }
}

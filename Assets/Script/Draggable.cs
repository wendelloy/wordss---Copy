using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    public Vector3 originalPosition;
    private HorizontalLayoutGroup layoutGroup; // Represents the LetterContainer's layout group

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        originalPosition = rectTransform.anchoredPosition;

        // Find the HorizontalLayoutGroup in the LetterContainer (parent)
        layoutGroup = GetComponentInParent<HorizontalLayoutGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (layoutGroup != null)
        {
            layoutGroup.enabled = false; // Disable layout group to allow free dragging
        }

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1.0f;
        canvasGroup.blocksRaycasts = true;

        GameObject slot = eventData.pointerEnter;
        if (slot != null && slot.CompareTag("Slot"))
        {
            transform.SetParent(slot.transform, false);
            rectTransform.localPosition = Vector3.zero;
            Debug.Log($"'{name}' placed in slot '{slot.name}'");
            FindObjectOfType<LevelManager>().CheckIfAllSlotsFilled(); // Trigger slot check
        }
        else
        {
            transform.SetParent(layoutGroup.transform, true);
            rectTransform.anchoredPosition = originalPosition;
            Debug.Log($"'{name}' returned to original position in LetterContainer.");
        }

        if (layoutGroup != null && transform.parent == layoutGroup.transform)
        {
            layoutGroup.enabled = true; // Re-enable layout group
        }
    }
}
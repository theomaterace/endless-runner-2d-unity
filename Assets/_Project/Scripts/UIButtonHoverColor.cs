using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class UIButtonHoverColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI targetText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;

    [Header("Scale")]
    [SerializeField] private Transform targetTransform;
    [SerializeField, Range(1f, 1.3f)] private float hoverScale = 1.05f;

    private Vector3 initialScale;

    private void Reset()
    {
        targetText = GetComponentInChildren<TextMeshProUGUI>();
        targetTransform = transform;
    }

    private void Awake()
    {
        if (targetTransform == null) targetTransform = transform;
        initialScale = targetTransform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetText != null)
            targetText.color = hoverColor;

        if (targetTransform != null)
            targetTransform.localScale = initialScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetText != null)
            targetText.color = normalColor;

        if (targetTransform != null)
            targetTransform.localScale = initialScale;
    }
}
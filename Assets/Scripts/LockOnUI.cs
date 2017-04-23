using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockOnUI : MonoBehaviour
{
    public Image UI;
    public Transform Target;

    private RectTransform CanvasRect;

    public void SetTarget(Transform target)
    {
        CanvasRect = UI.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        Target = target;
    }

    void Update()
    {
        if (!CanvasRect || !Target)
        {
            UI.enabled = false;
            return;
        }

        UI.enabled = true;

        Vector2 viewportPosition = Camera.main.WorldToViewportPoint(Target.position);
        Vector2 sreenPos = new Vector2 (
            ((viewportPosition.x * CanvasRect.sizeDelta.x) - (CanvasRect.sizeDelta.x * 0.5f)),
            ((viewportPosition.y * CanvasRect.sizeDelta.y) - (CanvasRect.sizeDelta.y * 0.5f))
        );
        
        UI.rectTransform.anchoredPosition = sreenPos;
    }
}

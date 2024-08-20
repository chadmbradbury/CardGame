using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class OrientationHelper : MonoBehaviour
{
    public Vector2 m_portraitPosition;
    public Vector2 m_landscapePosition;

    private void Awake()
    {
        ResolutionChangeDetector.OnScreenSizeChanged += ChangePosition;
    }

    private void OnDestroy()
    {
        ResolutionChangeDetector.OnScreenSizeChanged -= ChangePosition;
    }

    private void ChangePosition()
    {
        RectTransform rt = GetComponent<RectTransform>();
    
        if (Screen.width > Screen.height)
            rt.anchoredPosition = m_portraitPosition;
        else
            rt.anchoredPosition = m_landscapePosition;
    }
}

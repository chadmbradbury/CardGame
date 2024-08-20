using System;
using UnityEngine;

public class ResolutionChangeDetector : MonoBehaviour
{
    public static event Action OnScreenSizeChanged;

    private void OnRectTransformDimensionsChange()
    {
        OnScreenSizeChanged?.Invoke();
    }
}

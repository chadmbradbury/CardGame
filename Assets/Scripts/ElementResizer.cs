using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.UI;
using UnityEngine.TextCore.LowLevel;
using System.ComponentModel;
//using Unity.Collections;

[ExecuteAlways]
public class ElementResizer : MonoBehaviour
{
    [Unity.Collections.ReadOnly]
    public Vector2 m_defaultSize;

    public GameObject m_elementPrefab;
    public GridLayoutGroup m_gridGroup;
    public RectTransform m_rectTransform;

    protected bool m_performLateUpdate = false;

    public void Awake()
    {
        ResolutionChangeDetector.OnScreenSizeChanged += ScreenSizedChanged;

        if (m_gridGroup == null)
            m_gridGroup = GetComponent<GridLayoutGroup>();

        if (m_rectTransform == null)
            m_rectTransform = GetComponent<RectTransform>();

        m_defaultSize = m_elementPrefab.GetComponent<RectTransform>().rect.size;
        m_gridGroup.constraintCount = 3;
        m_gridGroup.cellSize = m_defaultSize;

        ScreenSizedChanged();
        m_performLateUpdate = false;
        OnTransformChildrenChanged();
    }

    protected void LateUpdate()
    {
        if (m_performLateUpdate)
        {
            m_gridGroup.constraintCount = 3;
            m_gridGroup.cellSize = m_defaultSize;
            UpdateScale();

            m_performLateUpdate = false;
        }
    }

    public void OnTransformChildrenChanged()
    {
        m_performLateUpdate = true;
    }
    
    protected void UpdateScale()
    {
        m_gridGroup.cellSize = m_defaultSize;

        float widthRatio = 1f;
        float heightRatio = 1f;

        int childMod = transform.childCount % m_gridGroup.constraintCount;
        int numRows = (transform.childCount / m_gridGroup.constraintCount) + ((childMod > 0) ? 1 : 0);

        float neededHeight = (m_gridGroup.cellSize.y + m_gridGroup.spacing.y) * numRows;
        float neededWidth = (m_gridGroup.cellSize.x + m_gridGroup.spacing.x) * m_gridGroup.constraintCount;

        // Calculate the width ratio
        if (m_rectTransform.rect.width < neededWidth)
        {
            float newCellSizeX = (m_rectTransform.rect.width - (m_gridGroup.spacing.x * m_gridGroup.constraintCount)) / m_gridGroup.constraintCount;
            widthRatio = newCellSizeX / m_gridGroup.cellSize.x;
        }

        // Calculate the height ratio
        if (m_rectTransform.rect.height < neededHeight)
        {
            float newCellSizeY = (m_rectTransform.rect.height - (m_gridGroup.spacing.y * numRows)) / numRows;
            heightRatio = newCellSizeY / m_gridGroup.cellSize.y;
        }

        m_gridGroup.cellSize *= Mathf.Min(widthRatio, heightRatio);

        // If we have gotten too thin from extra rows, adjust the constraint count,
        // unless it would cause there to not be even rows for no reason
        if (numRows > 1)
        {
            neededWidth = (m_gridGroup.cellSize.x + m_gridGroup.spacing.x) * m_gridGroup.constraintCount;
            if ((neededWidth / m_rectTransform.rect.width) < 0.75f/* && childMod != 0*/)
            {
                if (childMod != 0 || (transform.childCount / m_gridGroup.constraintCount > 2))
                {
                    m_gridGroup.constraintCount++;
                    UpdateScale();
                }
                //else
                //{
                //    if (transform.childCount / m_gridGroup.constraintCount > 2)
                //}
            }
        }
    }

    private void OnDestroy()
    {
        m_gridGroup.constraintCount = 3;
        m_gridGroup.cellSize = m_defaultSize;
        m_rectTransform.sizeDelta = new Vector2(-600f, 0f);
        ResolutionChangeDetector.OnScreenSizeChanged -= ScreenSizedChanged;
    }

    private void ScreenSizedChanged()
    {
        if (Screen.width < Screen.height)
        {
            m_rectTransform.sizeDelta = new Vector2(0f, -1000f);
        }
        else
        {
            m_rectTransform.sizeDelta = new Vector2(-600f, 0f);
        }

        m_performLateUpdate = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TooltipMenu : MonoBehaviour
{
    [SerializeField] protected RectTransform m_tFrame = null;

    private GameObject m_goCached = null;
    protected RectTransform m_rtCached = null;
    private GameObject m_goBlocker = null;

    private int m_nLayerID = 0;
    private int m_nOrder = 100;

    private void Awake()
    {
        m_goCached = gameObject;
        m_rtCached = GetComponent<RectTransform>();
        m_nLayerID = MenuManager.Singleton.GetUICanvas().sortingLayerID;
    }

    public virtual void Open()
    {
        m_goCached.SetActive(true);

        CreateBlocker();
    }

    public virtual void Close()
    {
        m_goCached.SetActive(false);
        Destroy(m_goBlocker);
        m_goBlocker = null;

        Destroy(gameObject);
    }

    void CreateBlocker()
    {
        m_goBlocker = new GameObject("Blocker");

        RectTransform blockerRect = m_goBlocker.AddComponent<RectTransform>();
        blockerRect.SetParent(MenuManager.Singleton.GetUIRoot(), false);
        blockerRect.anchorMin = Vector3.zero;
        blockerRect.anchorMax = Vector3.one;
        blockerRect.sizeDelta = Vector2.zero;

        Canvas blockerCanvas = m_goBlocker.AddComponent<Canvas>();
        blockerCanvas.overrideSorting = true;
        blockerCanvas.sortingLayerID = m_nLayerID;
        blockerCanvas.sortingOrder = m_nOrder;

        m_goBlocker.AddComponent<GraphicRaycaster>();
        Image imgBlock = m_goBlocker.AddComponent<Image>();
        imgBlock.color = Color.clear;

        Button btnBlock = m_goBlocker.AddComponent<Button>();
        btnBlock.onClick.AddListener(Close);
    }

    public virtual void SetPos(Vector2 vPos)
    {
        if (vPos == Vector2.zero)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_rtCached, vPos, null, out Vector2 vLocalPos);

        float fHalfScreenSizeX = MenuManager.Singleton.GetUIRootSize().x * 0.5f;
        float fHalfBoardSizeX = m_tFrame.sizeDelta.x * 0.5f;
        if (-fHalfScreenSizeX > (vLocalPos.x - fHalfBoardSizeX))
        {
            vLocalPos.x = -fHalfScreenSizeX + fHalfBoardSizeX;
        }
        else if (fHalfScreenSizeX < (vLocalPos.x + fHalfBoardSizeX))
        {
            vLocalPos.x = fHalfScreenSizeX - fHalfBoardSizeX;
        }

        m_tFrame.anchoredPosition = vLocalPos;
    }

    public virtual void Resize(float fSizeX, float fSizeY)
    {
        m_tFrame.sizeDelta = new Vector2(fSizeX, fSizeY);
    }
}

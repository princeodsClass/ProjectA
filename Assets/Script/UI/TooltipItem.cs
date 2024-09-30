using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TooltipItem : TooltipMenu
{
    [SerializeField] Image m_imgItemFrame = null;
    [SerializeField] Image m_imgItemIcon = null;
    [SerializeField] Text m_txtVolume = null;
    [SerializeField] Text m_txtName = null;
    [SerializeField] Text m_txtDesc = null;

    public void Initialize(ItemBase cItem, float fSizeX = 500f, float fSizeY = 150f)
    {
        m_imgItemIcon.sprite = cItem.GetIcon();
        m_txtVolume.text = 1 < cItem.nVolume ? $"{cItem.nVolume}" : string.Empty;
        m_txtName.text = cItem.strName;
        m_txtDesc.text = cItem.strDesc;

        Open();
        Resize(fSizeX, fSizeY);
    }

    public void Initialize(TooltipItemData cData)
    {
        m_imgItemIcon.sprite = cData.sprIcon;
        m_txtVolume.text = string.Empty;
        m_txtName.text = cData.strName;
        m_txtDesc.text = cData.strDesc;

        Open();

        if (Vector2.zero != cData.vSize) Resize(cData.vSize.x, cData.vSize.y);
    }
}

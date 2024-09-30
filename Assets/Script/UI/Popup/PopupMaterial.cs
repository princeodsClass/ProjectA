using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupMaterial : UIDialog
{
    [SerializeField]
    Image _imgFrame, _imgGlow, _imgIcon;

    [SerializeField]
    Color[] _colorFrame, _colorGlow;

    [SerializeField]
    TextMeshProUGUI _txtTitle, _txtGet, _txtUse, _txtGetTitle, _txtUseTitle, _txtButtonCaption;

    [SerializeField]
    GameObject _goScrapIcon;

    public void InitializeInfo(ItemMaterial item)
    {
        string temp = string.Empty;

        _txtGetTitle.text = UIStringTable.GetValue("ui_popup_material_gettitle");
        _txtUseTitle.text = UIStringTable.GetValue("ui_popup_material_usetitle");
        _txtButtonCaption.text = UIStringTable.GetValue("ui_common_close");

        _txtTitle.text = item.strName;

        SetGetCase(item);
        SetUseCase(item);

        _imgIcon.sprite = item.GetIcon();

        _imgFrame.color = _colorFrame[item.nGrade];
        _imgGlow.color = _colorGlow[item.nGrade];

        _goScrapIcon.SetActive(item.eType == EItemType.Material || item.eType == EItemType.MaterialG);
    }

    void SetGetCase(ItemMaterial item)
    {
        string temp = string.Empty;

        for (int i = 0; i < item.nContents4GetKey.Length; i++)
        {
            if (item.nContents4GetKey[i] != 0)
            {
                if (temp != string.Empty) temp = temp + "\n";
                temp = temp + $"- {ContentsStringTable.GetValue(item.nContents4GetKey[i])}";
            }
        }

        _txtGet.text = temp;
    }

    void SetUseCase(ItemMaterial item)
    {
        string temp = string.Empty;

        for (int i = 0; i < item.nContents4UseKey.Length; i++)
        {
            if (item.nContents4UseKey[i] != 0)
            {
                if (temp != string.Empty) temp = temp + "\n";
                temp = temp + $"- {ContentsStringTable.GetValue(item.nContents4UseKey[i])}";
            }
        }

        _txtUse.text = temp;
    }

    private void Awake()
    {
        Initialize();
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Open()
    {
        base.Open();

        m_MenuMgr.ShowPopupDimmed(true);
    }

    public override void Close()
    {
        base.Close();

        m_MenuMgr.ShowPopupDimmed(false);
    }

    public override void Escape()
    {
        base.Escape();
    }
}

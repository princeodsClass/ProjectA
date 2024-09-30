using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotAbyssRewards : MonoBehaviour
{
    [SerializeField]
    Image _imgIcon;

    [SerializeField]
    TextMeshProUGUI _txtVolume;

    [SerializeField]
    GameObject _goScrapIcon, _goRandomIcon;

    [SerializeField]
    Color[] _colorFrame, _colorGlow;

    [SerializeField]
    Image _imgFrame, _imgGlow;

    uint _key;
    bool _isRandom;

    public void InitializeInfo(uint key, Sprite icon, int count, bool isRandom = false)
    {
        _key = key;
        _isRandom = isRandom;

        _imgIcon.sprite = icon;
        _txtVolume.text = $"{count}";

        _goRandomIcon.SetActive(_isRandom);

        SetColor();
        SetScrapIcon();
    }

    void SetColor()
    {
        int grade = ComUtil.GetItemGrade(_key);

        _imgFrame.color = _colorFrame[grade];
        _imgGlow.color = _colorGlow[grade];
    }

    void SetScrapIcon()
    {
        if ("22" == _key.ToString("X").Substring(0, 2))
        {
            string sd = _key.ToString("X").Substring(3, 1);

            EItemType type = (EItemType)Convert.ToInt32(sd, 16);
            _goScrapIcon.SetActive(type == EItemType.Material || type == EItemType.MaterialG);
        }
        else
            _goScrapIcon.SetActive(false);
    }

    public void OnClick()
    {
        if (_isRandom)
        {
            PopupSysMessage ranpopup = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
            ranpopup.InitializeInfo("ui_hint_title", "ui_hint_random_desc", "ui_common_close", null, "TutorialRandom");
        }
        else
        {
            switch (_key.ToString("X").Substring(0, 2))
            {
                case "22":
                    PopupMaterial ma = MenuManager.Singleton.OpenPopup<PopupMaterial>(EUIPopup.PopupMaterial, true);
                    ma.InitializeInfo(new ItemMaterial(0, _key, 0));
                    break;
                case "24":
                    PopupBoxNormal bo = MenuManager.Singleton.OpenPopup<PopupBoxNormal>(EUIPopup.PopupBoxNormal, true);
                    bo.InitializeInfo(new ItemBox(0, _key, 0), false, false);
                    break;
            }
        }
    }
}

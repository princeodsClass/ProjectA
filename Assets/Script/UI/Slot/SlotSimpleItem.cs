using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotSimpleItem : MonoBehaviour
{
    [SerializeField]
    Image _imgIcon, _imgFrame, _imgGlow;

    [SerializeField]
    TextMeshProUGUI _txtVolume;

    [SerializeField]
    GameObject _goScrapIcon, _goRandomIcon;

    [SerializeField]
    Color[] _colorFrame, _colorGlow;

    uint _nKey;
    bool _isRandom;

    public void Initialize(uint key, Sprite icon, int min, int max, bool isRange = true, bool isRandom = false)
    {
        _nKey = key;
        _isRandom = isRandom;

        string cPre, cSuf;

        cPre = !isRange && min < max ? "<color=red>" : "";
        cSuf = !isRange && min < max ? "</color>" : "";

        _imgIcon.sprite = icon;

        if ( isRange )
            _txtVolume.text = min == max ? min.ToString() : $"{Mathf.Min(min, max)} ~ {Mathf.Max(min, max)}";
        else
            _txtVolume.text = $"{min} / {max}";

        _txtVolume.text = $"{cPre}{_txtVolume.text}{cSuf}";
        _goRandomIcon.SetActive(_isRandom);

        SetScrapIcon(key);
        SetBGColor(key);
    }

    void SetScrapIcon(uint key)
    {
        if ( ComUtil.GetItemType(key) == EItemType.Material )
        {
            string sd = key.ToString("X").Substring(3, 1);

            EItemType type = (EItemType)Convert.ToInt32(sd, 16);
            _goScrapIcon.SetActive(type == EItemType.Material || type == EItemType.MaterialG);
        }
        else
            _goScrapIcon.SetActive(false);
    }

    void SetBGColor(uint key)
    {
        int grade = ComUtil.GetItemGrade(key);

        _imgFrame.color = _colorFrame[grade];
        _imgGlow.color = _colorGlow[grade];
    }

    public void SetVolume(string value)
    {
        _txtVolume.text = value;
    }

    public void OnClick()
    {
        if ( _isRandom )
        {
            PopupSysMessage ranpopup = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
            ranpopup.InitializeInfo("ui_hint_title", "ui_hint_random_desc", "ui_common_close", null, "TutorialRandom");
        }
        else
        {
            switch (ComUtil.GetItemType(_nKey))
            {
                case EItemType.Weapon:
                    PopupWeapon weaponpopup = MenuManager.Singleton.OpenPopup<PopupWeapon>(EUIPopup.PopupWeapon, true);
                    weaponpopup.InitializeInfo(new ItemWeapon(0, _nKey, 1, 0, 0, 0, false), false, true);
                    break;
                case EItemType.Gear:
                    PopupGear gearpopup = MenuManager.Singleton.OpenPopup<PopupGear>(EUIPopup.PopupGear, true);
                    gearpopup.InitializeInfo(new ItemGear(0, _nKey, 1, 0, 0, 0, false), false, true);
                    break;
                case EItemType.Material:
                    PopupMaterial matpopup = MenuManager.Singleton.OpenPopup<PopupMaterial>(EUIPopup.PopupMaterial, true);
                    matpopup.InitializeInfo(new ItemMaterial(0, _nKey, 1));
                    break;
                case EItemType.Box:
                    PopupBoxNormal boxpopup = MenuManager.Singleton.OpenPopup<PopupBoxNormal>(EUIPopup.PopupBoxNormal, true);
                    boxpopup.InitializeInfo(new ItemBox(0, _nKey, 0), false, false);
                    break;
                default:
                    break;
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotWorkshopSymbol : MonoBehaviour
{
    [SerializeField]
    Image _imgIcon, _imgFrame, _imgGlow;

    [SerializeField]
    TextMeshProUGUI _txtVolume;

    [SerializeField]
    GameObject _goScrapIcon, _goPlus, _goMinus, _goDice, _goVolume;

    [SerializeField]
    Color[] _colorFrame, _colorGlow;

    public enum SlotState
    {
        none,
        workshop,
        workshoppop,
        workshopregist,
    }

    PopupWorkshop _pop = null;
    PopupWorkshopItem _popupWorkshopItem = null;
    SlotState _state = SlotState.none;
    RecipeTable _recipe;
    public uint _key;
    public int _volume;
    public long _id;

    public void InitializeInfo(PopupWorkshop pop, RecipeTable recipe, long id, uint key, int volume,
                               PopupWorkshopItem popItem = null,
                               bool isVolume = false, bool isRandom = true, SlotState state = SlotState.none)
    {
        _popupWorkshopItem = popItem;
        _recipe = recipe;
        _state = state;
        _id = id;
        _key = key;
        _volume = volume;

        SetIcon();
        SetScrapIcon();
        SetBGColor();
        SetVolume();
        SetPlus();

        _goDice.SetActive(isRandom);
        _goVolume.SetActive(isVolume);
    }

    void SetVolume()
    {
        _txtVolume.text = _volume.ToString();
    }

    void SetIcon()
    {
        string icon = string.Empty;

        switch (_key.ToString("X").Substring(0, 2))
        {
            case "20":
                icon = WeaponTable.GetData(_key).Icon;
                break;
            case "23":
                icon = GearTable.GetData(_key).Icon;
                break;
            case "22":
                icon = MaterialTable.GetData(_key).Icon;
                break;
        }

        _imgIcon.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons, icon);
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

    void SetBGColor()
    {
        int grade = default;

        switch ( _key.ToString("X").Substring(0, 2) )
        {
            case "20":
            case "23":
                grade = Convert.ToInt32(_key.ToString("X").Substring(2, 1), 16);
                break;
            case "22":
                grade = Convert.ToInt32(_key.ToString("X").Substring(2, 1), 16);
                break;
        }

        _imgFrame.color = _colorFrame[grade];
        _imgGlow.color = _colorGlow[grade];
    }

    void SetPlus()
    {
        _goPlus.SetActive(_state == SlotState.workshop || _state == SlotState.workshoppop);
        _goMinus.SetActive(_state == SlotState.workshopregist);
    }

    public void OnClick()
    {
        if (_state == SlotState.workshop)
        {
            _popupWorkshopItem.Regist(_id, _key);
        }
        else if (_state == SlotState.workshopregist)
        {
            _popupWorkshopItem.UnRegist(_id, _key);
        }
        else if (_state == SlotState.workshoppop)
        {
            PopupWorkshopRegist pop = MenuManager.Singleton.OpenPopup<PopupWorkshopRegist>(EUIPopup.PopupWorkshopRegist, true);
            pop.InitializeInfo(_id, _key, _volume, _recipe);
        }
    }

    public void OnClickTutorial(bool isTutorial)
    {
        OnClick();

        GameObject pop = GameObject.Find("PopupWorkshopRegist");
        GameObject button = pop.transform.Find("Base").Find("Button").Find("Regist").gameObject;
        RectTransform rt = button.GetComponent<RectTransform>();
        GameManager.Singleton.tutorial.SetFinger(button,
                                                 pop.GetComponent<PopupWorkshopRegist>().OnClickRegistTutorial,
                                                 rt.rect.width, rt.rect.height, 750);
    }
}

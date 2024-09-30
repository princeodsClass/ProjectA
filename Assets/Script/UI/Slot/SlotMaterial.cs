using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotMaterial : MonoBehaviour
{
    [SerializeField]
    Image[] _imgFrame;

    [SerializeField]
    Image _imgGlow, _Icon;

    [SerializeField]
    Color[] _colorFrame, _colorGlow;

    [SerializeField]
    TextMeshProUGUI _textName, _textVolume;

    [SerializeField]
    GameObject _goName, _goBottom, _goPlusIcon, _goDiceIcon, _goScrapIcon;

    [SerializeField]
    Transform _tAlarmRoot;

    RecipeTable _recipe;

    public ItemMaterial _Item;
    public int _volume;

    bool _isOverlap = false;
    bool _isRandom = false;

    uint _goodsKey = 0;
    int _slotNumber = -1;

    public enum EVolumeType
    {
        value,
        inven,
    }

    EVolumeType _volumeType;

    public enum SlotState
    {
        none,
        normal,
        goods,
    }

    SlotState _state = SlotState.normal;

    public void SetSlotNumber(int slotNumber)
    {
        _slotNumber = slotNumber;
    }

    public void SetRecipe(RecipeTable recipe)
    {
        _recipe = recipe;
    }

    public void Initialize(ItemMaterial item, bool isOverlap = false, bool isName = true, bool isBottom = true, EVolumeType type = EVolumeType.value, SlotState state = SlotState.normal, uint goodsKey = 0)
    {
        _Item = item;
        _volume = item.nVolume;
        _isOverlap = isOverlap;
        _goName.SetActive(isName);
        _goBottom.SetActive(isBottom);
        _volumeType = type;
        _state = state;
        _goodsKey = goodsKey;

        InitializeInfo(_Item.nKey, item.nVolume);
    }

    public void Initialize(uint key, int count, bool isOverlap = false, bool isName = true, bool isBottom= true, EVolumeType type = EVolumeType.value, SlotState state = SlotState.normal, uint goodsKey = 0)
    {
        ItemMaterial item = new ItemMaterial(0, key, count);

        _Item = item;
        _volume = count;
        _isOverlap = isOverlap;
        _goName.SetActive(isName);
        _goBottom.SetActive(isBottom);
        _volumeType = type;
        _state = state;
        _goodsKey = goodsKey;

        InitializeInfo(key, count);
    }

    public void InitializeInfo(uint key, int count)
    {
        MaterialTable item = MaterialTable.GetData(key);

        int upgradeMax = GlobalTable.GetData<int>("ratioCritical");

        _goPlusIcon.SetActive(false);
        _goScrapIcon.SetActive(_Item.eType == EItemType.Material || _Item.eType == EItemType.MaterialG);
        
        SetRandom(false);

        _imgFrame[0].gameObject.SetActive(item.Type == (int)EItemType.Part ||
                                          item.Type == (int)EItemType.GearPart);
        _imgFrame[1].gameObject.SetActive(item.Type == (int)EItemType.Token);
        _imgFrame[2].gameObject.SetActive(item.Type == (int)EItemType.Currency);
        _imgFrame[3].gameObject.SetActive(item.Type == (int)EItemType.Material ||
                                          item.Type == (int)EItemType.MaterialG ||
                                          item.Type == (int)EItemType.ETC ||
                                          item.Type == (int)EItemType.Common);

        for (int i = 0; i < _imgFrame.Length; i++)
        {
            if ( _imgFrame[i].gameObject.activeSelf )
                _imgFrame[i].color = _colorFrame[item.Grade];
        }

        _imgGlow.color = _colorGlow[item.Grade];
        _Icon.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons, item.Icon);

        _textName.text = NameTable.GetValue(item.NameKey);

        switch ( _volumeType )
        {
            case EVolumeType.value:
                _textVolume.text = count.ToString();
                break;
            case EVolumeType.inven:
                int inven = GameManager.Singleton.invenMaterial.GetItemCount(key);

                string cPre, cSuf;

                cPre = inven < count ? "<color=red>" : "";
                cSuf = inven < count ? "</color>" : "";

                _textVolume.text = $"{cPre}{ComUtil.ChangeNumberFormat(inven)} / {count}{cSuf}";
                break;
        }
    }

    public void SetState(SlotState state)
    {
        _state = state;
    }

    public void SetRandom(bool state)
    {
        _isRandom = state;
        _goDiceIcon.SetActive(state);
    }

    public void SetAlarm()
    {
        ComUtil.DestroyChildren(_tAlarmRoot);

        int u = GlobalTable.GetData<int>("countStandardUpgrade");
        int maxLevel;

        foreach (ItemWeapon we in GameManager.Singleton.invenWeapon)
        {
            maxLevel = u + we.nCurReinforce * u;

            if (we.nCurUpgrade == maxLevel &&
                 we.nCurReinforce == we.nCurLimitbreak &&
                 we.nCurLimitbreak < we.nGrade)
            {
                foreach (KeyValuePair<uint, int> mk in we.CalcLimitbreakMaterial())
                {
                    if (mk.Key == _Item.nKey && mk.Value > GameManager.Singleton.invenMaterial.GetItemCount(mk.Key))
                    {
                        SlotAlarm alarm = MenuManager.Singleton.LoadComponent<SlotAlarm>(_tAlarmRoot, EUIComponent.SlotAlarm);
                        alarm.InitializeInfo(we.id);
                        continue;
                    }
                }
            }
        }
    }

    public void DetailPopup()
    {
        if ( _state == SlotState.normal )
        {
            PopupMaterial detail = MenuManager.Singleton.OpenPopup<PopupMaterial>(EUIPopup.PopupMaterial, _isOverlap);
            detail.InitializeInfo(_Item);
        }
        else if ( _state == SlotState.goods )
        {
            PopupItemBuy buy = MenuManager.Singleton.OpenPopup<PopupItemBuy>(EUIPopup.PopupItemBuy, true);
            buy.InitializeInfo(_goodsKey, _slotNumber);
        }
    }
}

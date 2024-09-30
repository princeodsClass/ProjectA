using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupItemBuy : UIDialog
{
    [SerializeField]
    GameObject _goSlotRoot, _goButtonBuy, _goMissionKey, _goGameMoney, _goCrystal;

    [SerializeField]
    TextMeshProUGUI _txtTitle, _txtMissionKey, _txtGameMoney, _txtCrystal;

    [SerializeField]
    Image _imgMissionKey, _imgGameMoney, _imgCrystal;

    [SerializeField]
    Image _imgCostIcon;

    uint _DealKey, _itemKey, _costKey;
    int _itemCount, _costCount, _slotNumber;
    Sprite _icon;

    enum EBType
    {
        DailyDeal,
        GameMoneyDeal,
        CrystalDeal,
    }

    EBType _bType;
    bool _isBonus;

    public void InitializeInfo(uint primaryKey, int slotNumber)
    {
        ComUtil.DestroyChildren(_goSlotRoot.transform);

        _DealKey = primaryKey;
        _bType = EBType.DailyDeal;

        SetItem(primaryKey);
        SetTop();
        SetButton();

        _slotNumber = slotNumber;

        string type = _itemKey.ToString("X").Substring(0, 2);

        switch (type)
        {
            case "20":  SetWeapon();    break;
            case "23":      break;
            case "11":      break;
            case "22":  SetMaterial();  break;
        }
    }

    public void InitializeInfo(MoneyDealTable deal, bool isBonus = false)
    {
        ComUtil.DestroyChildren(_goSlotRoot.transform);

        _bType = EBType.GameMoneyDeal;

        _DealKey = deal.PrimaryKey;
        _isBonus = isBonus;

        _itemKey = deal.RewardItemKey;
        _itemCount = (int)(deal.RewardItemCount * ( _isBonus ? GlobalTable.GetData<float>("ratioGameMoneyDealBonus") : 1 ));

        _costKey = deal.CostItemKey;
        _costCount = deal.CostItemCount;

        SetMaterial();
        SetTop();
        SetButton();
    }

    public void InitializeInfo(CrystalDealTable deal, bool isBonus = false)
    {
        ComUtil.DestroyChildren(_goSlotRoot.transform);

        _bType = EBType.CrystalDeal;

        _DealKey = deal.PrimaryKey;
        _isBonus = isBonus;

        _itemKey = deal.RewardItemKey;
        _itemCount = (int)(deal.RewardItemCount * ( _isBonus ? GlobalTable.GetData<float>("ratioCrystalDealBonus") : 1 ));

        _costKey = deal.CostItemKey;
        _costCount = deal.CostItemCount;

        SetMaterial();
        SetButton();
    }

    public void SetTop()
    {
        _txtMissionKey.text = ComUtil.ChangeNumberFormat(m_GameMgr.invenMaterial.CalcMissionTicket());
        _txtGameMoney.text = ComUtil.ChangeNumberFormat(m_GameMgr.invenMaterial.CalcTotalMoney());
        _txtCrystal.text = ComUtil.ChangeNumberFormat(m_GameMgr.invenMaterial.CalcTotalCrystal());
    }

    void SetItem(uint primaryKey)
    {
        DailyDealTable temp = DailyDealTable.GetData(primaryKey);

        _itemKey = temp.ItemKey;
        _itemCount = temp.Count;
        _costKey = temp.CostItemKey;
        _costCount = temp.CostItemCount;
    }

    void SetButton()
    {
        _imgCostIcon.sprite = m_ResourceMgr.LoadSprite(EAtlasType.Icons, MaterialTable.GetData(_costKey).Icon);
        _goButtonBuy.GetComponentInChildren<TextMeshProUGUI>().text = _costCount.ToString();

        LayoutRebuilder.ForceRebuildLayoutImmediate(_goButtonBuy.GetComponentInChildren<HorizontalLayoutGroup>().GetComponent<RectTransform>());
    }

    void SetWeapon()
    {
        ItemWeapon weapon = new ItemWeapon(0, _itemKey, _itemCount, 0, 0, 0, false);
        _txtTitle.text = weapon.strName;

        SlotWeapon item = m_MenuMgr.LoadComponent<SlotWeapon>(_goSlotRoot.transform, EUIComponent.SlotWeapon);
        item.Initialize(weapon);
        item.SetState(SlotWeapon.SlotState.reward);
    }

    void SetMaterial()
    {
        ItemMaterial material = new ItemMaterial(0, _itemKey, _itemCount);
        _txtTitle.text = material.strName;

        SlotMaterial item = m_MenuMgr.LoadComponent<SlotMaterial>(_goSlotRoot.transform, EUIComponent.SlotMaterial);
        item.Initialize(material, true);
    }

    public void OnClickBuy()
    {
        _goButtonBuy.GetComponent<SoundButton>().interactable = false;
        StartCoroutine(Buy());
    }

    IEnumerator Buy()
    {
        if ( !m_InvenMaterial.CheckCost(_costKey, _costCount) ) // !CheckCost() )
        {
            if ( _costKey == ComType.KEY_ITEM_CRYSTAL_FREE || _costKey == ComType.KEY_ITEM_CRYSTAL_PAY )
                MenuManager.Singleton.OpenPopup<PopupShopCrystal>(EUIPopup.PopupShopCrystal, true);
            else if ( _costKey == ComType.KEY_ITEM_GOLD )
                MenuManager.Singleton.OpenPopup<PopupShopGameMoney>(EUIPopup.PopupShopGameMoney, true);

            _goButtonBuy.GetComponent<SoundButton>().interactable = true;
            yield break;
        }

        PopupWait4Response wait = m_MenuMgr.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

        yield return StartCoroutine(ConsumeCost());
        yield return StartCoroutine(m_GameMgr.AddItemCS(_itemKey, _itemCount));
        yield return StartCoroutine(SetInfo());

        _goButtonBuy.GetComponent<SoundButton>().interactable = true;
        GameObject.Find("InventoryPage")?.GetComponent<PageLobbyInventory>().InitializeInventory();
        GameObject.Find("PopupWeapon")?.GetComponent<PopupWeapon>().RefreshInfo();
        GameObject.Find("PopupGear")?.GetComponent<PopupGear>().RefreshInfo();

        wait.Close();

        PopupSysMessage s = m_MenuMgr.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
        s.InitializeInfo("ui_popup_option_alarm_caption", "ui_shop_deal_complete", "ui_popup_button_confirm");

        Close();
    }

    IEnumerator ConsumeCost()
    {
        if (_costKey == ComType.KEY_ITEM_CRYSTAL_FREE || _costKey == ComType.KEY_ITEM_CRYSTAL_PAY)
        {
            int t = m_InvenMaterial.GetItemCount(ComType.KEY_ITEM_CRYSTAL_FREE) - _costCount;

            if (t < 0)
            {
                yield return StartCoroutine(m_GameMgr.AddItemCS(ComType.KEY_ITEM_CRYSTAL_PAY, t));

                if (_costCount != -t)
                    yield return StartCoroutine(m_GameMgr.AddItemCS(ComType.KEY_ITEM_CRYSTAL_FREE, -(_costCount + t)));
            }
            else
            {
                yield return StartCoroutine(m_GameMgr.AddItemCS(ComType.KEY_ITEM_CRYSTAL_FREE, -_costCount));
            }
        }
        else
            yield return StartCoroutine(m_GameMgr.AddItemCS(_costKey, -_costCount));
    }

    IEnumerator SetInfo()
    {
        switch ( _bType )
        {
            case EBType.DailyDeal:
                yield return StartCoroutine(m_DataMgr.SetDailyDealInfo(_slotNumber, _DealKey));
                FindObjectOfType<ComShopDailyDeal>().SetCover(_slotNumber, true);
                m_Account.RefreshQuestCount(EQuestActionType.CompleteDailyDeal, 1);
                break;
            case EBType.GameMoneyDeal:
                yield return StartCoroutine(m_DataMgr.SetGameMoneyDealInfo(_DealKey));
                break;
            case EBType.CrystalDeal:
                yield return StartCoroutine(m_DataMgr.SetCrystalDealInfo(_DealKey));
                break;
        }
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

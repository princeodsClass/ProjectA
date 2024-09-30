using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupWeaponDownGrade : UIDialog
{
    [SerializeField]
    GameObject _objRootWeaponBefore, _objRootWeaponAfter, _objRootMaterial;

    [SerializeField]
    TextMeshProUGUI _txtTitle, _txtDesc, _txtResetButton;

    [SerializeField]
    GameObject _goResetButton;

    [SerializeField]
    GameObject[] _goResizeTarget;

    int u;

    ItemWeapon _item;
    SlotWeapon _slot;
    PopupWeapon _popup;

    Dictionary<uint, int> _Material;

    private void Awake()
    {
        Initialize();
        u = GlobalTable.GetData<int>("countStandardUpgrade");
    }

    public void InitializeInfo(ItemWeapon item, SlotWeapon slot, PopupWeapon popup)
    {
        _item = item;
        _slot = slot;
        _popup = popup;

        RefreshInfo();
        ReSize();
    }

    void RefreshInfo()
    {
        ComUtil.DestroyChildren(_objRootWeaponBefore.transform);
        ComUtil.DestroyChildren(_objRootWeaponAfter.transform);
        ComUtil.DestroyChildren(_objRootMaterial.transform);

        InitializeWeapon();
        InitializeText();
        SetMaterialArea();

        _slot.RefreshInfo();
        _popup.RefreshInfo();
    }

    void SetMaterialArea()
    {
        _Material = new Dictionary<uint, int>();

        for ( int i = _item.nCurLimitbreak; i >= 0; i-- )
        {
            int maxLevel = i == _item.nCurLimitbreak ? _item.nCurUpgrade : u + i * u;

            for (int j = 0; j <  maxLevel; j++)
                ReCalcMaterial(_item.CalcUpgradeMaterial(j, i));

            int reinforce = i == _item.nCurLimitbreak ? _item.nCurReinforce : i;

            for (int j = 0; j < reinforce; j++)
                ReCalcMaterial(_item.CalcReinforceMaterial(i));

            if ( i > 0 ) ReCalcMaterial(_item.CalcLimitbreakMaterial(i - 1));
        }

        SetSlot();
    }

    void ReCalcMaterial(Dictionary<uint, int> t)
    {
        _Material = ComUtil.MergeDictionaries(_Material, t);
    }

    void SetSlot()
    {
        foreach( KeyValuePair<uint, int> m in _Material )
        {
            SlotMaterial material = m_MenuMgr.LoadComponent<SlotMaterial>(_objRootMaterial.transform, EUIComponent.SlotMaterial);
            material.Initialize(m.Key, m.Value, true, false, true, SlotMaterial.EVolumeType.value);
        }
    }

    public void OnClickReset()
    {
        StartCoroutine(Reset());
    }

    IEnumerator Reset()
    {
        _goResetButton.GetComponent<SoundButton>().interactable = false;

        foreach ( KeyValuePair<uint, int> m in _Material )
            yield return StartCoroutine(m_GameMgr.AddItemCS(m.Key, m.Value));

        yield return StartCoroutine(m_DataMgr.ItemReset(_item));

        m_InvenWeapon.ModifyItem(_item.id, InventoryData<ItemWeapon>.EItemModifyType.Upgrade, 0);
        m_InvenWeapon.ModifyItem(_item.id, InventoryData<ItemWeapon>.EItemModifyType.Reinforce, 0);
        m_InvenWeapon.ModifyItem(_item.id, InventoryData<ItemWeapon>.EItemModifyType.Limitbreak, 0);

        RefreshInfo();

        GameAudioManager.PlaySFX("SFX/UI/sfx_ui_initialize_00", 0f, false, ComType.UI_MIX);

        _goResetButton.GetComponent<SoundButton>().interactable = true;
        GameObject.Find("LobbyTop")?.GetComponent<PageLobbyTop>().InitializeCurrency();
		GameObject.Find("BattleInventory")?.GetComponent<BattleInventory>().Reset();

        _popup.RefreshInfo();

        Close();
    }

    void ReSize()
    {
        for (int i = 0; i < _goResizeTarget.Length; i++)
            LayoutRebuilder.ForceRebuildLayoutImmediate(_goResizeTarget[i].GetComponent<RectTransform>());
    }

    void InitializeText()
    {
        _txtTitle.text = _item.strName;
        _txtDesc.text = UIStringTable.GetValue("ui_popup_weaponreset_desc");
        _txtResetButton.text = UIStringTable.GetValue("ui_popup_weapon_button_downgrade_caption");
    }

    void InitializeWeapon()
    {
        SlotWeapon before = m_MenuMgr.LoadComponent<SlotWeapon>(_objRootWeaponBefore.transform, EUIComponent.SlotWeapon);
        before.Initialize(_item, false);

        SlotWeapon after = m_MenuMgr.LoadComponent<SlotWeapon>(_objRootWeaponAfter.transform, EUIComponent.SlotWeapon);
        after.Initialize(_item, false, SlotWeapon.EPresentType.origin);
    }

    private void OnEnable()
    {
        _goResetButton.GetComponent<SoundButton>().interactable = true;
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Open()
    {
        base.Open();
    }

    public override void Close()
    {
        base.Close();
    }

    public override void Escape()
    {
        base.Escape();
    }
}

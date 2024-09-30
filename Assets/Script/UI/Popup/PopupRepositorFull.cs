using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupRepositorFull : UIDialog
{
    [SerializeField]
    TextMeshProUGUI _txtTitle, _txtCounter, _txtDesc, _txtButtonRecycle, _txtButtonExtent;

    PageLobbyInventory _page;

    private void Awake()
    {
        Initialize();
    }

    public override void Initialize()
    {
        base.Initialize();

        _page = GameObject.Find("InventoryPage").GetComponent<PageLobbyInventory>();

        _txtTitle.text = UIStringTable.GetValue("ui_error_invenweapon_full");
        _txtDesc.text = UIStringTable.GetValue("ui_error_cannotget");
        _txtButtonRecycle.text = UIStringTable.GetValue("ui_page_lobby_inventory_button_recycle");
        _txtButtonExtent.text = UIStringTable.GetValue("ui_popup_repository_extent_button_caption");
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

    public void OnEnable()
    {
        _txtCounter.text = $"{m_InvenWeapon.GetItemCount()} / {m_Account.m_nMaxWeaponRepository}";
    }

    public void OnClickRecycle()
    {
        _page.OnClickRecycle(0);
    }

    public void OnClickExtent()
    {
        PopupRepositoryExtent extent = m_MenuMgr.OpenPopup<PopupRepositoryExtent>(EUIPopup.PopupRepositoryExtent);
        extent.InitializeInfo(0, 1);
    }

    public void OnClickConfirm()
    {
        Close();
    }
}

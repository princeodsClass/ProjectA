using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Purchasing;

public class PopupShopVIP : UIDialog
{
    [SerializeField]
    TextMeshProUGUI _txtTitle;

    [SerializeField]
    Transform _tRootButtonArea;

    [SerializeField]
    TextMeshProUGUI[] _txtHint;

    List<VIPDealTable> _dealList = new List<VIPDealTable>();
    List<SlotBuyVIP> _slot = new List<SlotBuyVIP>();

    VIPDealTable _curDeal;

    private void Refresh()
    {
        ComShopVIP popup = GetComponentInChildren<ComShopVIP>();
        ComShopVIP compo = GameObject.Find("ShopPage")?.GetComponentInChildren<ComShopVIP>();

        if (null != compo) compo.Initialize();
        if (true == popup.gameObject.activeInHierarchy) popup.Initialize();
    }

    private void Awake()
    {
        InitializeText();
    }

    void SetButton()
    {
        _slot.Clear();
        ComUtil.DestroyChildren(_tRootButtonArea);

        _dealList = VIPDealTable.GetList();
        _dealList.RemoveAt(GameManager.Singleton.user.m_dtEndVip == default ? 1 : 0);

        for (int i = 0; i < _dealList.Count; i++)
        {
            SlotBuyVIP t = MenuManager.Singleton.LoadComponent<SlotBuyVIP>(_tRootButtonArea, EUIComponent.SlotBuyVIP);
            t.InitializeInfo(_dealList[i], this);

            _slot.Add(t);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_tRootButtonArea.GetComponent<RectTransform>());
    }

    private void OnEnable()
    {        
        Initialize();
    }

    public override void Initialize()
    {
        base.Initialize();

        SetButton();
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

    void InitializeText()
    {
        _txtTitle.text = UIStringTable.GetValue("ui_component_vip_title");

        for (int i = 0; i < _txtHint.Length; i++)
            _txtHint[i].text = UIStringTable.GetValue($"ui_hint_shop_vip_{i}");
    }

    public void BuyVIP(VIPDealTable curDeal)
    {
        _curDeal = curDeal;

        if (AbleToBuyVIP())
        {
            string productID = Application.platform == RuntimePlatform.IPhonePlayer ?
                               curDeal.IAPiOS : curDeal.IAPAOS;
            C3rdPartySDKManager.Singleton.PurchaseProduct(productID, CheckPurchase);
        }
    }

    bool AbleToBuyVIP()
    {
        if (GameManager.Singleton.user.IsVIP())
        {
            PopupSysMessage popup = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
            popup.InitializeInfo("ui_error_title", "ui_error_alreadyvip", "ui_popup_button_confirm");

            return false;
        }

        if (_curDeal.BuyType == 0 && GameManager.Singleton.user.m_dtEndVip != default)
        {
            PopupSysMessage popup = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
            popup.InitializeInfo("ui_error_title", "ui_error_onlyone_purchase", "ui_popup_button_confirm");

            return false;
        }

        return true;
    }

    void CheckPurchase(CPurchaseManager purchaseManager, string pID, bool result, string receipt)
    {
        if (result)
        {
            GameManager.Singleton.StartCoroutine(VerifiedReceipt(pID, receipt));
            C3rdPartySDKManager.Singleton.SendPurchaseLog(C3rdPartySDKManager.Singleton.GetProduct(pID));
        }
        else
        {
            PopupSysMessage fail = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
            fail.InitializeInfo("ui_error_title", "ui_error_purchase", "ui_popup_button_confirm");
        }
    }

    IEnumerator VerifiedReceipt(string pID, string receipt)
    {
        yield return GameManager.Singleton.StartCoroutine(GameDataManager.Singleton.VerifyReceipt(receipt, pID, () =>
        {
            GameManager.Singleton.StartCoroutine(CoBuyVIP(pID));
        }));
    }

    IEnumerator CoBuyVIP(string pID)
    {
        PopupWait4Response wait = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response);

        C3rdPartySDKManager.Singleton.SendPurchaseLog(C3rdPartySDKManager.Singleton.GetProduct(pID));

        yield return GameManager.Singleton.StartCoroutine(GameDataManager.Singleton.SetViplDealInfo(_curDeal, wait));

        PopupSysMessage s = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
        s.InitializeInfo("ui_popup_option_alarm_caption", "ui_shop_deal_complete", "ui_popup_button_confirm");
    }
}

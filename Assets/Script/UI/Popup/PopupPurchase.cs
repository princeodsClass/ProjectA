using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using TMPro;

public class PopupPurchase : UIDialog
{
    [SerializeField] GameObject _goWaitCom;
    [SerializeField] TextMeshProUGUI _txtTitle = null;
    [SerializeField] TextMeshProUGUI _txtMessage = null;
    [SerializeField] TextMeshProUGUI _txtConfirm = null;
    [SerializeField] Image _imgImage = null;

    [SerializeField] RectTransform[] _rtResize;

    string _tTitle, _tDesc;
    string _tTarCode;
    string _IAPAOS;
    string _IAPiOS;

    Sprite _spImage;

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
    }

    public override void Close()
    {
        base.Close();
    }

    public override void Escape()
    {
        base.Escape();
    }

    public void InitializeInfo(uint key)
    {
        CheckInfo(key);
        StartCoroutine(SetCost());

        _txtTitle.text = _tTitle;
        _txtMessage.text = _tDesc;
        _imgImage.sprite = _spImage;

        StartCoroutine(Resize());
    }

    void CheckInfo(uint key)
    {
        switch (key.ToString("X").Substring(0, 2))
        {
            case "64":
                PassDealTable p = PassDealTable.GetData(key);
                _tTitle = NameTable.GetValue(p.NameKey);
                _tDesc = DescTable.GetValue(p.DescKey);
                _tTarCode = p.TargetCode;
                _IAPAOS = p.IAPAOS;
                _IAPiOS = p.IAPiOS;
                _spImage = m_ResourceMgr.LoadSprite(EAtlasType.Outgame, p.Image);
                break;
        }
    }

    IEnumerator SetCost()
    {
        while (true)
        {
            Product product = C3rdPartySDKManager.Singleton.GetProduct(GetProdectID());

            _goWaitCom.SetActive(product == null);
            _txtConfirm.gameObject.SetActive(product != null);
            _txtConfirm.text = product.metadata.localizedPriceString;

            if (null != product) yield break;

            yield return new WaitForSeconds(1f);
        }
    }

    string GetProdectID()
    {
        return Application.platform == RuntimePlatform.IPhonePlayer ? _IAPiOS : _IAPAOS;
    }

    IEnumerator Resize()
    {
        yield return null;
        Array.ForEach(_rtResize, rt => LayoutRebuilder.ForceRebuildLayoutImmediate(rt));
    }

    public void OnClickConfirm()
    {
        string productID = GetProdectID();

        C3rdPartySDKManager.Singleton.PurchaseProduct(productID, CheckPurchase);
    }

    void CheckPurchase(CPurchaseManager purchaseManager, string pID, bool result, string a_oReceipt)
    {
        if (result)
        {
            StartCoroutine(VerifiedReceipt(pID, a_oReceipt)); 
            C3rdPartySDKManager.Singleton.SendPurchaseLog(C3rdPartySDKManager.Singleton.GetProduct(pID));
        }
        else
        {
            PopupSysMessage fail = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
            fail.InitializeInfo("ui_error_title", "ui_error_purchase", "ui_popup_button_confirm");
        }
    }

    IEnumerator VerifiedReceipt(string pID, string a_oReceipt)
    {
        yield return StartCoroutine(GameDataManager.Singleton.VerifyReceipt(a_oReceipt, pID, () =>
        {
            StartCoroutine(Buy(pID));
        }));
    }

    IEnumerator Buy(string pID)
    {
        PopupWait4Response wait = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

        C3rdPartySDKManager.Singleton.SendPurchaseLog(C3rdPartySDKManager.Singleton.GetProduct(pID));

        yield return StartCoroutine(GameDataManager.Singleton.ActiveBattlePass(_tTarCode));

        PopupSysMessage s = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
        s.InitializeInfo("ui_popup_option_alarm_caption", "ui_shop_deal_complete", "ui_popup_button_confirm");

        this.Close();
    }
}

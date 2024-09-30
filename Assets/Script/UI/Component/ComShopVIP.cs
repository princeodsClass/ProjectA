using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json.Linq;

public class ComShopVIP : MonoBehaviour
{
    [SerializeField]
    GameObject _goDimmed, _goTimer, _goJorgi;

    [SerializeField]
    Transform _SlotRoot;

    [SerializeField]
    TextMeshProUGUI _txtTitle, _txtDesc, _txtTimerTitle, _txtTimerText, _txtJorgi, _txtPerksDesc;

    List<VIPDealTable> _dealList = new List<VIPDealTable>();
    List<SlotShopVIP> _slot = new List<SlotShopVIP>();

    VIPDealTable _ctbl;

    int _index;
    string D, h, m, s = string.Empty;

    private void Awake()
    {
        Initialize();
        InitializeText();
    }

    public void Initialize()
    {
        if ( GameManager.Singleton.user.IsVIP() )
        {
            _goDimmed.SetActive(true);
            _goTimer.SetActive(true);
            _goJorgi.SetActive(false);

            StartCoroutine(SetTimer());
        }
        else
        {
            _goDimmed.SetActive(false);
            _goTimer.SetActive(false);
            _goJorgi.SetActive(true);
        }

        SetSlot();
        SetMaker();
    }

    void SetSlot()
    {
        ComUtil.DestroyChildren(_SlotRoot);
        _slot = new List<SlotShopVIP>();

        _dealList = VIPDealTable.GetList();
        _dealList.RemoveAt(GameManager.Singleton.user.m_dtEndVip == default ? 1 : 0);

        for ( int i = 0; i < _dealList.Count; i++ )
        {
            _slot.Add(MenuManager.Singleton.LoadComponent<SlotShopVIP>(_SlotRoot, EUIComponent.SlotShopVIP));
            _slot[i].InitializeInfo(_dealList[i], this);
        }
    }

    IEnumerator SetTimer()
    {
        TimeSpan time = GameManager.Singleton.user.m_dtEndVip - DateTime.UtcNow;

        while (time.TotalMilliseconds > 0f)
        {
            _txtTimerText.text = string.Empty;

            _txtTimerText.text = time.Days > 0 ? $"{time.Days}{D} " : "";
            _txtTimerText.text = _txtTimerText.text + (time.Hours > 0 ? $"{time.Hours}{h} " : " ");
            _txtTimerText.text = _txtTimerText.text + (time.Minutes > 0 ? $"{time.Minutes}{m} " : " ");
            _txtTimerText.text = _txtTimerText.text + (time.Seconds > 0 ? $"{time.Seconds}{s} " : " ");

            time = time.Subtract(TimeSpan.FromSeconds(1));

            yield return new WaitForSecondsRealtime(1f);
        }

        SetMaker();
        Initialize();
    }

    public void OnClickDesc()
    {
        if (GameManager.Singleton.user.m_nLevel >= GlobalTable.GetData<int>("valueShopOpenLevel"))
        {
            PopupShopVIP ct = MenuManager.Singleton.OpenPopup<PopupShopVIP>(EUIPopup.PopupShopVIP, true);
        }
        else
        {
            PopupSysMessage pop = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
            pop.InitializeInfo("ui_error_title", "ui_error_notenoughlevel_shop", "ui_common_close", null, "TutorialShop");
        }
    }

    public void SetMaker()
    {
        bool state = GameManager.Singleton.user.IsVIP();

        GameObject.Find("ComShopADS")?.GetComponent<ComShopADS>().SetSlotMaker();
        GameObject.Find("ButtonVIP")?.SetActive(!state);
    }

    void InitializeText()
    {
        _txtTitle.text = UIStringTable.GetValue("ui_component_vip_title");
        _txtDesc.text = UIStringTable.GetValue("ui_component_vip_desc");
        _txtTimerTitle.text = UIStringTable.GetValue("ui_component_vip_timer_title");
        _txtJorgi.text = UIStringTable.GetValue("ui_component_vip_jorgi");
        _txtPerksDesc.text = UIStringTable.GetValue("ui_component_vip_already");

        D = $"<size=70%>{UIStringTable.GetValue("ui_day")}</size>";
        h = $"<size=70%>{UIStringTable.GetValue("ui_hour")}</size>";
        m = $"<size=70%>{UIStringTable.GetValue("ui_minute")}</size>";
        s = $"<size=70%>{UIStringTable.GetValue("ui_second")}</size>";
    }

    public IEnumerator Startle()
    {
        _slot.ForEach(slot => StartCoroutine(slot.SetSlotAniFocus()));
        yield break;
    }

    public void BuyVIP(VIPDealTable ctbl)
    {
        _ctbl = ctbl;

        if (AbleToBuyVIP())
        {
            string productID = Application.platform == RuntimePlatform.IPhonePlayer ? _ctbl.IAPiOS : _ctbl.IAPAOS;
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

        if (_ctbl.BuyType == 0 && GameManager.Singleton.user.m_dtEndVip != default)
        {
            PopupSysMessage popup = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
            popup.InitializeInfo("ui_error_title", "ui_error_onlyone_purchase", "ui_popup_button_confirm");

            return false;
        }

        return true;
    }

    void CheckPurchase(CPurchaseManager purchaseManager, string pID, bool result, string a_oReceipt)
    {
        if (result)
        {
            GameManager.Singleton.StartCoroutine(VerifiedReceipt(pID, a_oReceipt));
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
        yield return GameManager.Singleton.StartCoroutine(GameDataManager.Singleton.VerifyReceipt(a_oReceipt, pID, () =>
        {
            GameManager.Singleton.StartCoroutine(CoBuyVIP(pID));
        }));
    }

    IEnumerator CoBuyVIP(string pID)
    {
        PopupWait4Response wait = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response);

        C3rdPartySDKManager.Singleton.SendPurchaseLog(C3rdPartySDKManager.Singleton.GetProduct(pID));

        yield return GameManager.Singleton.StartCoroutine(GameDataManager.Singleton.SetViplDealInfo(_ctbl, wait));

        PopupSysMessage s = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
        s.InitializeInfo("ui_popup_option_alarm_caption", "ui_shop_deal_complete", "ui_popup_button_confirm");

        Initialize();
    }
}

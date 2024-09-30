using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupBoxMaterialPremium : UIDialog
{
    [SerializeField]
    GameObject _goTip, _goTipBonus,
               _goMaterialRoot,
               _goButtonTip, _goButtonBonusTip,
               _goButtonBuyToken, _goButtonBuyTokenTen, _goButtonBuyOne, _goButtonBuyTen;

    [SerializeField]
    TextMeshProUGUI _txtTitle, _txtDesc, _txtItemTitle, _txtTimerTitle, _txtTimer;

    [SerializeField]
    Image _imgCostOneIcon, _imgCostTenIcon, _imgCostTokenIcon, _imgCostTokenTenIcon;

    [SerializeField]
    TextMeshProUGUI _txtCostOne, _txtCostTen, _txtCostToken, _txtCostTokenTen,
                    _txtDescCostOne, _txtDescCostTen, _txtDescCostToken, _txtDescCostTokenTen,
                    _txtBonusDesc,
                    _txtBonusTip,
                    _txtCrystal;

    [SerializeField]
    Slider _slBonus;

    List<RewardTable> g;

    BoxDealTable _dealKey;
    ItemWeapon _symbolWeapon = new ItemWeapon();

    uint _boxKey, _costItemKey, _costTokenKey;
    int _rewardGroup, _costItemCount, _costTokenCount;

    DateTime _resetLastDatetime;
    string D, h, m, s = string.Empty;

    public void InitializeInfo()
    {
        _boxKey = BoxDealTable.GetSymbolDataPerGroup(1)[2].BoxKey;
        _rewardGroup = BoxTable.GetData(_boxKey).RewardGroup;

        StartCoroutine(InitInfo());
    }

    IEnumerator InitInfo()
    {
        _dealKey = BoxDealTable.GetGroup(2)[0];

        yield return StartCoroutine(SelectSymbolIndex());

        SetKeys();
        SetTitle();
        SetButton();
        SetBonus();

        CheckToken();
        SetTop();
        StartCoroutine(SetTimerText());

        Resize();
        ResetButton();
    }

    void ResetButton()
    {
        _goButtonBuyToken.GetComponent<SoundButton>().interactable = true;
        _goButtonBuyTokenTen.GetComponent<SoundButton>().interactable = true;
        _goButtonBuyOne.GetComponent<SoundButton>().interactable = true;
        _goButtonBuyTen.GetComponent<SoundButton>().interactable = true;
    }

    public void SetTop()
    {
        _txtCrystal.text = ComUtil.ChangeNumberFormat(m_GameMgr.invenMaterial.CalcTotalCrystal());
    }

    void SetKeys()
    {
        _costItemKey = _dealKey.CostItemKey;
        _costItemCount = _dealKey.CostItemCount;

        _costTokenKey = _dealKey.CostTokenKey;
        _costTokenCount = _dealKey.CostTokenCount;
    }

    IEnumerator SelectSymbolIndex()
    {
        ComUtil.DestroyChildren(_goMaterialRoot.transform);

        DateTime preTime = m_GameMgr.eventController._eMaterialPremiumBoxDeal.CheckPreTime();

        if (preTime >= m_Account.m_dtPMaterialBoxReset)
        {
            List<RewardTable> rindex = RewardTable.GetGroup(_rewardGroup);
            List<RewardListTable> rlindex = new List<RewardListTable>();
            Dictionary<string, string> fields = new Dictionary<string, string>();

            for (int i = 1; i < 3; i++)
            {
                rlindex.Clear();
                rlindex = RewardListTable.RandomResultByFactorInGroup(rindex[i].RewardListGroup, 3);

                for (int j = 0; j < rlindex.Count; j++)
                {
                    fields.Add($"currentPBoxG{i + 1}index{j}", rlindex[j].RewardKey.ToString());
                    AddSlot(rlindex[j].RewardKey);
                }
            }

            fields.Add("auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString());
            fields.Add("accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN));
            fields.Add("pMaterialBoxResetDatetime", ComUtil.EnUTC());

            yield return GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.ModifyAccount(fields));
        }
        else
        {
            for (int i = 0; i < m_Account.m_nCurrentPBoxG2Index.Length; i++)
                AddSlot(m_Account.m_nCurrentPBoxG2Index[i]);

            for (int i = 0; i < m_Account.m_nCurrentPBoxG3Index.Length; i++)
                AddSlot(m_Account.m_nCurrentPBoxG3Index[i]);
        }
    }

    [SerializeField]
    GameObject[] _goPer;

    [SerializeField]
    TextMeshProUGUI[] _per;

    void SetDesc()
    {
        float[] t = BoxDealTable.GetPercentage(_dealKey.PrimaryKey);

        for (int i = 0; i < _goPer.Length; i++)
        {
            _goPer[i].SetActive(0 < t[i]);
            _per[i].text = $"{t[i] * 100} %";
        }
    }

    void AddSlot(uint pk)
    {
        SlotMaterial m = MenuManager.Singleton.LoadComponent<SlotMaterial>(_goMaterialRoot.transform, EUIComponent.SlotMaterial);
        m.Initialize(pk, 1, true, true, true, SlotMaterial.EVolumeType.value, SlotMaterial.SlotState.normal);
        m.SetAlarm();
    }

    void SetTitle()
    {
        _txtTitle.text = NameTable.GetValue(BoxTable.GetData(_boxKey).NameKey);
    }

    IEnumerator SetTimerText()
    {
        TimeSpan time = m_GameMgr.eventController._eMaterialPremiumBoxDeal.CheckRemainTime();

        while (time.TotalMilliseconds > 0f)
        {
            _txtTimer.text = string.Empty;

            _txtTimer.text = time.Days > 0 ? $"{time.Days}{D} " : "";
            _txtTimer.text = _txtTimer.text + (time.Hours > 0 ? $"{time.Hours}{h} " : " ");
            _txtTimer.text = _txtTimer.text + (time.Minutes > 0 ? $"{time.Minutes}{m} " : " ");
            _txtTimer.text = _txtTimer.text + (time.Seconds > 0 ? $"{time.Seconds}{s} " : " ");

            time = time.Subtract(TimeSpan.FromSeconds(1));

            yield return new WaitForSecondsRealtime(1f);
        }

        InitializeInfo();

        yield break;
    }

    void SetBonus()
    {
        int countBonus = GlobalTable.GetData<int>("countPremiumBoxBonus");
        int remainCount = m_Account.m_nCountBuyPMaterialBox % countBonus;

        _slBonus.value = Mathf.Min(1f, remainCount / (float)countBonus);

        _txtBonusDesc.text =
        $"{UIStringTable.GetValue("ui_popup_box_open_bonus_desc")} ( <color=yellow>{remainCount} / {countBonus}</color> )";
    }

    void InitializeText()
    {
        _txtDesc.text = UIStringTable.GetValue("ui_popup_boxweapon_desc");
        _txtItemTitle.text = UIStringTable.GetValue("ui_popup_boxweapon_item_title");
        _txtTimerTitle.text = UIStringTable.GetValue("ui_shop_resettime");

        _txtBonusTip.text = UIStringTable.GetValue("ui_popup_box_open_bonus_tip_desc");

        D = $"<size=70%>{UIStringTable.GetValue("ui_day")}</size>";
        h = $"<size=70%>{UIStringTable.GetValue("ui_hour")}</size>";
        m = $"<size=70%>{UIStringTable.GetValue("ui_minute")}</size>";
        s = $"<size=70%>{UIStringTable.GetValue("ui_second")}</size>";
    }

    void SetButton()
    {
        _imgCostOneIcon.sprite =
        _imgCostTenIcon.sprite =
        m_ResourceMgr.LoadSprite(EAtlasType.Icons, MaterialTable.GetData(_costItemKey).Icon);

        _imgCostTokenIcon.sprite =
        _imgCostTokenTenIcon.sprite =
        m_ResourceMgr.LoadSprite(EAtlasType.Icons, MaterialTable.GetData(_costTokenKey).Icon);

        _txtCostOne.text = _dealKey.CostItemCount.ToString();
        _txtCostTen.text = ((_dealKey.CostItemCount
                                  * 10f
                                  * GlobalTable.GetData<float>("ratioShopBuyTenDiscount")
                                 )).ToString();
        _txtCostToken.text = _dealKey.CostTokenCount.ToString();
        _txtCostTokenTen.text = (_dealKey.CostTokenCount * 10).ToString();

        _txtDescCostOne.text =
        _txtDescCostToken.text =
        $"<color=yellow>1</color> {UIStringTable.GetValue("ui_popup_box_open_button_desc")}";
        _txtDescCostTen.text = _txtDescCostTokenTen.text =
        $"<color=yellow>10</color> {UIStringTable.GetValue("ui_popup_box_open_button_desc")}";
    }

    void Resize()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(_txtDesc.rectTransform.parent.GetComponent<RectTransform>());

        LayoutRebuilder.ForceRebuildLayoutImmediate(_txtBonusTip.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(_txtBonusTip.rectTransform.parent.GetComponent<RectTransform>());

        LayoutRebuilder.ForceRebuildLayoutImmediate(_goButtonBuyOne.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(_goButtonBuyTen.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(_goButtonBuyToken.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(_goButtonBuyTokenTen.GetComponent<RectTransform>());
    }

    void CheckToken()
    {
        _goButtonBuyToken.SetActive(_costTokenCount <= m_InvenMaterial.GetItemCount(_costTokenKey));
        _goButtonBuyTokenTen.SetActive((_costTokenCount * 10) <= m_InvenMaterial.GetItemCount(_costTokenKey));

        _goButtonBuyOne.SetActive(!_goButtonBuyToken.activeSelf);
        _goButtonBuyTen.SetActive(!_goButtonBuyTokenTen.activeSelf);
    }

    public void OnClickBuyOne()
    {
        if (!m_DataMgr.AbleToConnect()) return;

        if (m_InvenMaterial.CalcTotalCrystal() < _costItemCount)
        {
            SetButtonState(true);

            PopupShopCrystal ct = MenuManager.Singleton.OpenPopup<PopupShopCrystal>(EUIPopup.PopupShopCrystal, true);

            return;
        }

        SetButtonState(false);
        StartCoroutine(BuyOne());
    }

    IEnumerator BuyOne()
    {
        yield return StartCoroutine(m_InvenMaterial.ConsumeCrystal(_costItemCount));
        yield return StartCoroutine(m_DataMgr.AddBuyCount(EDealType.PremiumMaterial));

        SetButtonState(true);

        PopupBoxReward popupReward = m_MenuMgr.OpenPopup<PopupBoxReward>(EUIPopup.PopupBoxReward);
        ItemBox box = new ItemBox(0, _boxKey, 0);
        popupReward.InitializeInfo(box, false, false, 1, PopupBoxReward.EBoxType.shopbuy, _dealKey);
    }

    public void OnClickBuyToken()
    {
        if (!m_DataMgr.AbleToConnect()) return;

        SetButtonState(false);
        StartCoroutine(BuyToken());
    }

    IEnumerator BuyToken()
    {
        yield return StartCoroutine(m_GameMgr.AddItemCS(_costTokenKey, -_costTokenCount));
        yield return StartCoroutine(m_DataMgr.AddBuyCount(EDealType.PremiumMaterial));

        SetButtonState(true);

        PopupBoxReward popupReward = m_MenuMgr.OpenPopup<PopupBoxReward>(EUIPopup.PopupBoxReward);
        ItemBox box = new ItemBox(0, _boxKey, 0);
        popupReward.InitializeInfo(box, false, false, 1, PopupBoxReward.EBoxType.shopbuy, _dealKey);
    }

    public void OnClickBuyTen()
    {
        if (!m_DataMgr.AbleToConnect()) return;

        int cost = (int)(_costItemCount * 10 * GlobalTable.GetData<float>("ratioShopBuyTenDiscount"));

        if (m_InvenMaterial.CalcTotalCrystal() < cost)
        {
            SetButtonState(true);

            PopupShopCrystal ct = MenuManager.Singleton.OpenPopup<PopupShopCrystal>(EUIPopup.PopupShopCrystal, true);

            return;
        }

        SetButtonState(false);
        StartCoroutine(BuyTen(cost));
    }

    IEnumerator BuyTen(int cost)
    {
        yield return StartCoroutine(m_InvenMaterial.ConsumeCrystal(cost));
        yield return StartCoroutine(m_DataMgr.AddBuyCount(EDealType.PremiumMaterial, 10));

        SetButtonState(true);

        PopupBoxReward popupReward = m_MenuMgr.OpenPopup<PopupBoxReward>(EUIPopup.PopupBoxReward);
        ItemBox box = new ItemBox(0, _boxKey, 0);
        popupReward.InitializeInfo(box, false, false, 10, PopupBoxReward.EBoxType.shopbuy, _dealKey);
    }

    public void OnClickBuyToken10()
    {
        if (!m_DataMgr.AbleToConnect()) return;

        SetButtonState(false);
        StartCoroutine(BuyToken10());
    }

    IEnumerator BuyToken10()
    {
        yield return StartCoroutine(m_GameMgr.AddItemCS(_costTokenKey, -_costTokenCount * 10));
        yield return StartCoroutine(m_DataMgr.AddBuyCount(EDealType.Weapon, 10));

        SetButtonState(true);

        PopupBoxReward popupReward = m_MenuMgr.OpenPopup<PopupBoxReward>(EUIPopup.PopupBoxReward);
        ItemBox box = new ItemBox(0, _boxKey, 0);
        popupReward.InitializeInfo(box, false, false, 10, PopupBoxReward.EBoxType.shopbuy, _dealKey);
    }

    void SetButtonState(bool state)
    {
        _goButtonBuyOne.GetComponent<SoundButton>().interactable = state;
        _goButtonBuyTen.GetComponent<SoundButton>().interactable = state;
        _goButtonBuyToken.GetComponent<SoundButton>().interactable = state;
        _goButtonBuyTokenTen.GetComponent<SoundButton>().interactable = state;
    }

    public void OnClickDesc()
    {
        _goTip.SetActive(true);

        SetDesc();
    }

    public void OnClickBonus()
    {
        _goTipBonus.SetActive(true);
    }

    public void OnClickTip()
    {
        _goTip.SetActive(false);
    }

    public void OnClickBonusTip()
    {
        _goTipBonus.SetActive(false);
    }    

    private void Awake()
    {
        Initialize();
        InitializeText();
    }

    private void OnEnable()
    {
        OnClickTip();
        OnClickBonusTip();
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

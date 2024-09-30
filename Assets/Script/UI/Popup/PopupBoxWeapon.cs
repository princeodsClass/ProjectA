using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupBoxWeapon : UIDialog
{
    [SerializeField]
    GameObject _goTip, _goTipBonus,
               _goWeaponRoot,
               _goButtonRenewalPay, _goButtonRenewalFree,
               _goButtonBuyToken, _goButtonBuyTokenTen, _goButtonBuyOne, _goButtonBuyTen;

    [SerializeField]
    TextMeshProUGUI _txtTitle, _txtDesc, _txtItemTitle, _txtTimerTitle, _txtTimer;

    [SerializeField]
    Image _imgFrame, _imgGlow, _imgTypeIcon;

    [SerializeField]
    Color[] _colorFrame, _colorGlow;

    [Header("그레이드 별")]
    [SerializeField]
    GameObject[] _objGradeIndicator;

    [SerializeField]
    Image _imgCostResetIcon, _imgCostOneIcon, _imgCostTenIcon, _imgCostTokenIcon, _imgCostTokenTenIcon;

    [SerializeField]
    TextMeshProUGUI _txtCostReset, _txtCostOne, _txtCostTen, _txtCostToken, _txtCostTokenTen,
                    _txtDescCostOne, _txtDescCostTen, _txtDescCostToken, _txtDescCostTokenTen,
                    _txtBonusLightDesc, _txtBonusCoreDesc,
                    _txtBonusTip, _txtCrystal;

    [SerializeField]
    Slider _slLightBonus, _slCoreBonus;

    [SerializeField]
    RectTransform _rtTip;

    [SerializeField]
    RectTransform[] _rt4Rebuild;

    BoxDealTable _dealKey;
    ItemWeapon _symbolWeapon = new ItemWeapon();
    Coroutine _coRotateWeapon;
    GameObject _goWeapon;

    uint _dealIndex = 0;
    uint _boxKey, _costItemKey, _costTokenKey, _resetItemKey;
    int _costItemCount, _costTokenCount, _resetItemStandardCost, _resetItemAdditionalCost;
    DateTime _resetLastDatetime;
    string D, h, m, s = string.Empty;

    public void InitializeInfo(int group)
    {
        StartCoroutine(InitInfo(group));
    }

    IEnumerator InitInfo(int group)
    {
        yield return StartCoroutine(SelectDealIndex(group));

        InitializeText();

        SetBonus();
        
        OnClickTip();
        OnClickBonusTip();

        CheckToken();
        SetTop();
        ResetButton();
    }

    public void SetTop()
    {
        _txtCrystal.text = ComUtil.ChangeNumberFormat(m_GameMgr.invenMaterial.CalcTotalCrystal());
    }

    void ResetButton()
    {
        _goButtonRenewalPay.GetComponent<SoundButton>().interactable = true;
        _goButtonRenewalFree.GetComponent<SoundButton>().interactable = true;
        _goButtonBuyToken.GetComponent<SoundButton>().interactable = true; 
        _goButtonBuyTokenTen.GetComponent<SoundButton>().interactable = true; 
        _goButtonBuyOne.GetComponent<SoundButton>().interactable = true; 
        _goButtonBuyTen.GetComponent<SoundButton>().interactable = true;
    }

    void SetKeys()
    {
        _boxKey = _dealKey.BoxKey;

        _costItemKey = _dealKey.CostItemKey;
        _costItemCount = _dealKey.CostItemCount;

        _costTokenKey = _dealKey.CostTokenKey;
        _costTokenCount = _dealKey.CostTokenCount;

        _resetItemKey = _dealKey.CostResetKey;
        _resetItemStandardCost = _dealKey.CostResetStandardCount;
        _resetItemAdditionalCost = _dealKey.CostResetAdditionalCount;
    }

    IEnumerator SelectDealIndex(int group, bool isForce = false)
    {
        DateTime preTime = group == 0 ?
                                    m_GameMgr.eventController._eWeaponBoxDeal.CheckPreTime() :
                                    m_GameMgr.eventController._eWeaponPremiumBoxDeal.CheckPreTime();
        DateTime lastTime = group == 0 ?
                                     m_Account.m_dtWeaponBoxReset :
                                     m_Account.m_dtWeaponPremiumBoxReset;

        if (preTime >= lastTime || isForce)
        {
            if ( group == 0 )
            {
                m_Account.m_nCurrentWeaponBoxRewardIndex =
                _dealIndex = BoxDealTable.RandomIndexByFactorInGroup(group, _dealIndex);
            }
            else if ( group == 3 )
            {
                m_Account.m_nCurrentWeaponPremiumBoxRewardIndex =
                _dealIndex = BoxDealTable.RandomIndexByFactorInGroup(group, _dealIndex);
            }

            RewardTable wk = RewardTable.GetGroup(BoxTable.GetData(BoxDealTable.GetData(_dealIndex).BoxKey).RewardGroup)[0];
            _symbolWeapon.Initialize(0, wk.ItemKey, 1, 0, 0, 0, false);

            Dictionary<string, string> fields = new Dictionary<string, string>();

            fields.Add("auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString());
            fields.Add("accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN));


            if ( 0 == group )
            {
                m_Account.m_nCountResetWeaponBox = isForce ? m_Account.m_nCountResetWeaponBox + 1 : 0;

                fields.Add("currentWeaponBoxRewardIndex", _dealIndex.ToString());
                fields.Add("weaponBoxResetDatetime", ComUtil.EnUTC());
                fields.Add("countResetWeaponBox", m_Account.m_nCountResetWeaponBox.ToString());
            }
            else
            {
                m_Account.m_nCountResetWeaponPremiumBox = isForce ? m_Account.m_nCountResetWeaponPremiumBox + 1 : 0;

                fields.Add("currentWeaponPremiumBoxRewardIndex", _dealIndex.ToString());
                fields.Add("weaponPremiumBoxResetDatetime", ComUtil.EnUTC());
                fields.Add("countResetWeaponPremiumBox", m_Account.m_nCountResetWeaponPremiumBox.ToString());
            }

            yield return GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.ModifyAccount(fields));
        }
        else
        {
            _dealIndex = group == 0 ?
                         m_Account.m_nCurrentWeaponBoxRewardIndex :
                         m_Account.m_nCurrentWeaponPremiumBoxRewardIndex;

            RewardTable wk = RewardTable.GetGroup(BoxTable.GetData(BoxDealTable.GetData(_dealIndex).BoxKey).RewardGroup)[0];
            _symbolWeapon.Initialize(0, wk.ItemKey, 1, 0, 0, 0, false);
        }

        _dealKey = BoxDealTable.GetData(_dealIndex);
        TimeSpan time = group == 0 ? 
                        m_GameMgr.eventController._eWeaponBoxDeal.CheckRemainTime() :
                        m_GameMgr.eventController._eWeaponPremiumBoxDeal.CheckRemainTime();

        StartCoroutine(SetTimerText(time));
        SetKeys();
        SetBody();
        SetButton();
        StartCoroutine(Resize());
    }

    void SetBody()
    {
        InitializeWeapon();

        SetGrade();

        _imgTypeIcon.sprite = ComUtil.GetWeaponSubtypeIcon(_symbolWeapon.eSubType);
    }

    void SetGrade()
    {
        for (int i = 0; i < _objGradeIndicator.Length; i++)
            _objGradeIndicator[i].SetActive(i < _symbolWeapon.nGrade);

        _imgFrame.color = _colorFrame[_symbolWeapon.nGrade];
        _imgGlow.color = _colorGlow[_symbolWeapon.nGrade];
    }

    void InitializeWeapon()
    {
        if (null != _goWeapon) Destroy(_goWeapon);

        _goWeapon = m_ResourceMgr.CreateObject(EResourceType.Weapon, _symbolWeapon.strPrefab, _goWeaponRoot.transform);

        GameObject obj = _goWeapon.GetComponent<WeaponModelInfo>().GetUIDummy();

        _goWeapon.transform.localPosition =
            new Vector3(0,
                       -obj.transform.localPosition.z * obj.transform.localScale.z,
                        obj.transform.localPosition.y * obj.transform.localScale.y);
        _goWeapon.transform.localRotation = obj.transform.localRotation;
        _goWeapon.transform.localScale = obj.transform.localScale;

        StopRotateWeapon();
        StartRotateWeapon();
    }

    IEnumerator RotateWeapon(Transform t)
    {
        float rotationAngle = 0f;

        while (gameObject.activeSelf)
        {
            rotationAngle = 30f * Time.deltaTime;

            t.transform.Rotate(Vector3.up, rotationAngle);

            yield return null;
        }

        yield break;
    }

    public void StartRotateWeapon()
    {
        _coRotateWeapon = StartCoroutine(RotateWeapon(_goWeapon.transform.parent.transform));
    }

    public void StopRotateWeapon()
    {
        if (null != _coRotateWeapon)
        {
            StopCoroutine(_coRotateWeapon);
            _coRotateWeapon = null;
        }
    }

    IEnumerator FRotateWeapon(Transform t)
    {
        float curTime = 0f;
        float elapsedTime = 1f;
        float rotationAmount;
        float rotationSpeed = 0f;

        while (curTime < elapsedTime)
        {
            if ( curTime < elapsedTime / 2f )
                rotationSpeed = Mathf.Lerp(0f, 1080f, curTime / ( elapsedTime / 2f ));
            else
                rotationSpeed = Mathf.Lerp(1080f, 0f, (curTime - (1f - (elapsedTime / 2f))) / (elapsedTime / 2f));

            rotationAmount = rotationSpeed * Time.deltaTime;
            t.Rotate(Vector3.up, rotationAmount);

            curTime += Time.deltaTime;

            yield return null;
        }
    }

    IEnumerator SetTimerText(TimeSpan time)
    {
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

        StartCoroutine(InitInfo(_dealKey.Group));

        yield break;
    }

    void SetBonus()
    {
        int countLightBonus = GlobalTable.GetData<int>("countLightBonus");
        int remainCount = m_Account.m_nCountBuyWeaponBox % countLightBonus;

        _slLightBonus.value = Mathf.Min(1f, remainCount / (float)countLightBonus);

        _txtBonusLightDesc.text =
        $"{UIStringTable.GetValue("ui_popup_box_open_bonus_desc")} ( <color=yellow>{remainCount} / {countLightBonus}</color> )";
    }

    void InitializeText()
    {
        _txtItemTitle.text = UIStringTable.GetValue("ui_popup_boxweapon_item_title");
        _txtTimerTitle.text = UIStringTable.GetValue("ui_shop_resettime");

        _txtDesc.text = UIStringTable.GetValue("ui_popup_boxweapon_desc");
        _txtTitle.text = NameTable.GetValue(BoxTable.GetData(_boxKey).NameKey);

        _goButtonRenewalFree.GetComponentInChildren<TextMeshProUGUI>().text =
            UIStringTable.GetValue("ui_paytype_free");

        _txtBonusTip.text = UIStringTable.GetValue("ui_popup_box_open_bonus_tip_desc");

        D = $"<size=70%>{UIStringTable.GetValue("ui_day")}</size>";
        h = $"<size=70%>{UIStringTable.GetValue("ui_hour")}</size>";
        m = $"<size=70%>{UIStringTable.GetValue("ui_minute")}</size>";
        s = $"<size=70%>{UIStringTable.GetValue("ui_second")}</size>";
    }

    void SetButton()
    {
        SetRenewalButton();

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
        _txtCostTokenTen.text = (_dealKey.CostTokenCount * 10 ).ToString();

        _txtDescCostOne.text =
        _txtDescCostToken.text =
        $"<color=yellow>1</color> {UIStringTable.GetValue("ui_popup_box_open_button_desc")}";
        _txtDescCostTen.text =
        _txtDescCostTokenTen.text =
        $"<color=yellow>10</color> {UIStringTable.GetValue("ui_popup_box_open_button_desc")}";
    }

    void SetRenewalButton()
    {
        int cost = CalcRenewalCost();

        if (cost == 0)
        {
            _goButtonRenewalFree.SetActive(true);
            _goButtonRenewalPay.SetActive(false);
        }
        else
        {
            _goButtonRenewalFree.SetActive(false);
            _goButtonRenewalPay.SetActive(true);

            _txtCostReset.text = CalcRenewalCost().ToString();

            _imgCostResetIcon.sprite =
            m_ResourceMgr.LoadSprite(EAtlasType.Icons, MaterialTable.GetData(_resetItemKey).Icon);
        }
    }

    IEnumerator Resize()
    {
        foreach ( RectTransform rt in _rt4Rebuild )
        {
            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }
    }

    void CheckToken()
    {
        _goButtonBuyToken.SetActive(_costTokenCount <= m_InvenMaterial.GetItemCount(_costTokenKey));
        _goButtonBuyTokenTen.SetActive( (_costTokenCount * 10) <= m_InvenMaterial.GetItemCount(_costTokenKey) );

        _goButtonBuyOne.SetActive(!_goButtonBuyToken.activeSelf);
        _goButtonBuyTen.SetActive(!_goButtonBuyTokenTen.activeSelf);
    }

    public void OnClickRenewal()
    {
        if ( !m_DataMgr.AbleToConnect() ) return;

        _goButtonRenewalPay.GetComponent<SoundButton>().interactable = false;
        _goButtonRenewalFree.GetComponent<SoundButton>().interactable = false;

        StartCoroutine(Renewal());
    }

    IEnumerator Renewal()
    {
        int cost = CalcRenewalCost();

        if (m_GameMgr.invenMaterial.CalcTotalCrystal() >= cost)
        {
            yield return StartCoroutine(m_InvenMaterial.ConsumeCrystal(cost));

            yield return StartCoroutine(SelectDealIndex(_dealKey.Group, true));

            StartCoroutine(FRotateWeapon(_goWeapon.transform.parent.transform));
            SetTop();
        }
        else
        {
            PopupShopCrystal ct = MenuManager.Singleton.OpenPopup<PopupShopCrystal>(EUIPopup.PopupShopCrystal, true);
        }

        _goButtonRenewalPay.GetComponent<SoundButton>().interactable = true;
        _goButtonRenewalFree.GetComponent<SoundButton>().interactable = true;
    }

    int CalcRenewalCost()
    {
        int resetCount = _dealKey.Group == 0 ? m_Account.m_nCountResetWeaponBox : m_Account.m_nCountResetWeaponPremiumBox;

        return _resetItemStandardCost + ( (resetCount - 1 ) * _resetItemAdditionalCost);
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
        yield return StartCoroutine(m_DataMgr.AddBuyCount(EDealType.Weapon));

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

    public void OnClickBuyToken(bool isTutorial)
    {
        OnClickBuyToken();
    }

    IEnumerator BuyToken()
    {
        yield return StartCoroutine(m_GameMgr.AddItemCS(_costTokenKey, -_costTokenCount));
        yield return StartCoroutine(m_DataMgr.AddBuyCount(EDealType.Weapon));

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
        yield return StartCoroutine(m_DataMgr.AddBuyCount(EDealType.Weapon, 10));

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

    public void OnClickDetail()
    {
        PopupWeapon detail = MenuManager.Singleton.OpenPopup<PopupWeapon>(EUIPopup.PopupWeapon, true);
        detail.InitializeInfo(_symbolWeapon, false, true);
    }

    public void OnClickDesc()
    {
        _goTip.SetActive(true);

        SetDesc();
    }

    [SerializeField]
    GameObject[] _goPer;

    [SerializeField]
    Transform _tTipSymbol;

    [SerializeField]
    TextMeshProUGUI[] _per;

    [SerializeField]
    TextMeshProUGUI _perSymbol, _textTipTitle;

    void SetDesc()
    {
        _textTipTitle.text = UIStringTable.GetValue("ui_popup_boxweapon_item_title");
        _perSymbol.text = $"{_dealKey.PercentageSymbol * 100} %";

        ComUtil.DestroyChildren(_tTipSymbol);
        SlotSimpleItemReward symbol = m_MenuMgr.LoadComponent<SlotSimpleItemReward>(_tTipSymbol, EUIComponent.SlotSimpleItemReward);
        symbol.Initialize(_symbolWeapon.GetIcon(), "1", "", _symbolWeapon.nGrade, true, false);
        symbol.SetAppear();

        float[] t = BoxDealTable.GetPercentage(_dealKey.PrimaryKey);

        for ( int i = 0; i < _goPer.Length; i++ )
        {
            _goPer[i].SetActive(0 < t[i]);
            _per[i].text = $"{t[i] * 100} %";
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_rtTip);
    }

    public void OnClickBonus()
    {
        _goTipBonus.SetActive(true);

        LayoutRebuilder.ForceRebuildLayoutImmediate(_txtBonusTip.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(_txtBonusTip.rectTransform.parent.GetComponent<RectTransform>());
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
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        ComShopPremiumWeaponBox premiumWeaponBox = FindObjectOfType<ComShopPremiumWeaponBox>();

        if (premiumWeaponBox != null)
            m_GameMgr.StartCoroutine(premiumWeaponBox.InitInfo());
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

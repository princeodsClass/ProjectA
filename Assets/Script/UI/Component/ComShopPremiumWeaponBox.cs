using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComShopPremiumWeaponBox : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _txtTitle, _txtTimerTitle, _txtTimerValue, _txtTicketCover, _txtToken, _txtCostCount;

    [SerializeField]
    Slider _tokenSlider;

    [SerializeField]
    Image _imgBoxIcon, _imgTokenIcon, _imgCostIcon;

    [SerializeField]
    GameObject[] _goTokentCover;

    [SerializeField]
    Transform _tWeaponRoot;

    [SerializeField]
    PageLobbyShop _pageLobbyShop;

    List<BoxDealTable>_boxDeal = new List<BoxDealTable>();
    GameObject _goWeapon;
    ItemWeapon _symbolWeapon = new ItemWeapon();
    BoxDealTable _dealKey;
    Coroutine _coRotateWeapon;

    string D, h, m, s = string.Empty;
    uint _dealIndex, _boxKey, _costItemKey, _costTokenKey;
    int _costItemCount, _costTokenCount;

    GameManager m_GameMgr;
    GameDataManager m_DataMgr;
    UserAccount m_Account;

    private void Awake()
    {
        if ( null == m_GameMgr ) m_GameMgr = GameManager.Singleton;
        if ( null == m_DataMgr ) m_DataMgr = GameDataManager.Singleton;
        if ( null == m_Account ) m_Account = GameManager.Singleton.user;

        InitializeText();

        StartCoroutine(InitInfo());

        _boxDeal = BoxDealTable.GetSymbolDataPerGroup(3);
        StartCoroutine(StartTimer());
    }

    public IEnumerator InitInfo()
    {
        yield return StartCoroutine(SelectDealIndex());

        InitializeText();
        InitializeSlotToken();
    }

    public void InitializeSlotToken()
    {
        MaterialTable token = MaterialTable.GetData(_boxDeal[0].CostTokenKey);

        int hc = GameManager.Singleton.invenMaterial.GetItemCount(token.PrimaryKey);
        int rc = _boxDeal[0].CostTokenCount;

        _txtToken.text = $"{hc} / {rc}";
        _tokenSlider.value = hc / (float)rc;

        _txtCostCount.text = _boxDeal[0].CostItemCount.ToString();

        _imgBoxIcon.sprite = ComUtil.GetIcon(BoxTable.GetData(_boxDeal[0].BoxKey).PrimaryKey);
        _imgTokenIcon.sprite = ComUtil.GetIcon(_boxDeal[0].CostTokenKey);
        _imgCostIcon.sprite = ComUtil.GetIcon(_boxDeal[0].CostItemKey);

        AddCounterNew(3, hc / rc);

        Array.ForEach(_goTokentCover, go => go.SetActive(hc >= rc));
    }

    IEnumerator StartTimer()
    {
        TimeSpan time = GameManager.Singleton.eventController._eWeaponPremiumBoxDeal.CheckRemainTime();

        while (time.TotalMilliseconds > 0f)
        {
            _txtTimerValue.text = string.Empty;

            _txtTimerValue.text = time.Days > 0 ? $"{time.Days}{D} " : "";
            _txtTimerValue.text = _txtTimerValue.text + (time.Hours > 0 ? $"{time.Hours}{h} " : " ");
            _txtTimerValue.text = _txtTimerValue.text + (time.Minutes > 0 ? $"{time.Minutes}{m} " : " ");
            _txtTimerValue.text = _txtTimerValue.text + (time.Seconds > 0 ? $"{time.Seconds}{s} " : " ");

            time = time.Subtract(TimeSpan.FromSeconds(1));

            yield return new WaitForSecondsRealtime(1f);
        }

        StartCoroutine(InitInfo());

        yield break;
    }

    void InitializeText()
    {
        _txtTitle.text = UIStringTable.GetValue("ui_shop_premium_weapon_boxdeal_title");
        _txtTimerTitle.text = UIStringTable.GetValue("ui_shop_resettime");
        _txtTicketCover.text = UIStringTable.GetValue("ui_slot_box_complete_caption");

        D = $"<size=70%>{UIStringTable.GetValue("ui_day")}</size>";
        h = $"<size=70%>{UIStringTable.GetValue("ui_hour")}</size>";
        m = $"<size=70%>{UIStringTable.GetValue("ui_minute")}</size>";
        s = $"<size=70%>{UIStringTable.GetValue("ui_second")}</size>";
    }

    IEnumerator SelectDealIndex(bool isForce = false)
    {
        DateTime preTime = m_GameMgr.eventController._eWeaponPremiumBoxDeal.CheckPreTime();
        DateTime lastTime = m_Account.m_dtWeaponPremiumBoxReset;

        if (preTime >= lastTime || isForce)
        {
            m_Account.m_nCurrentWeaponPremiumBoxRewardIndex =
            _dealIndex = BoxDealTable.RandomIndexByFactorInGroup(3, _dealIndex);

            RewardTable wk = RewardTable.GetGroup(BoxTable.GetData(BoxDealTable.GetData(_dealIndex).BoxKey).RewardGroup)[0];
            _symbolWeapon.Initialize(0, wk.ItemKey, 1, 0, 0, 0, false);

            Dictionary<string, string> fields = new Dictionary<string, string>();

            fields.Add("auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString());
            fields.Add("accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN));

            m_Account.m_nCountResetWeaponPremiumBox = isForce ?
                                                      m_Account.m_nCountResetWeaponPremiumBox++ :
                                                      m_Account.m_nCountResetWeaponPremiumBox = 0;

            fields.Add("currentWeaponPremiumBoxRewardIndex", _dealIndex.ToString());
            fields.Add("weaponPremiumBoxResetDatetime", ComUtil.EnUTC());
            fields.Add("countResetWeaponPremiumBox", m_Account.m_nCountResetWeaponPremiumBox.ToString());

            yield return m_DataMgr.StartCoroutine(m_DataMgr.ModifyAccount(fields));
        }
        else
        {
            _dealIndex = m_Account.m_nCurrentWeaponPremiumBoxRewardIndex;

            RewardTable wk = RewardTable.GetGroup(BoxTable.GetData(BoxDealTable.GetData(_dealIndex).BoxKey).RewardGroup)[0];
            _symbolWeapon.Initialize(0, wk.ItemKey, 1, 0, 0, 0, false);
        }

        _dealKey = BoxDealTable.GetData(_dealIndex);
        TimeSpan time = m_GameMgr.eventController._eWeaponPremiumBoxDeal.CheckRemainTime();

        StartCoroutine(StartTimer());
        SetKeys();
        SetBody();
    }

    void SetKeys()
    {
        _boxKey = _dealKey.BoxKey;

        _costItemKey = _dealKey.CostItemKey;
        _costItemCount = _dealKey.CostItemCount;

        _costTokenKey = _dealKey.CostTokenKey;
        _costTokenCount = _dealKey.CostTokenCount;
    }

    void SetBody()
    {
        InitializeWeapon();
    }

    void InitializeWeapon()
    {
        if (null != _goWeapon) Destroy(_goWeapon);

        RewardTable wk = RewardTable.GetGroup(BoxTable.GetData(BoxDealTable.GetData(m_Account.m_nCurrentWeaponPremiumBoxRewardIndex).BoxKey).RewardGroup)[0];
        _symbolWeapon.Initialize(0, wk.ItemKey, 1, 0, 0, 0, false);

        _goWeapon = GameResourceManager.Singleton.CreateObject(EResourceType.Weapon, _symbolWeapon.strPrefab, _tWeaponRoot);
        GameObject go = _goWeapon.GetComponent<WeaponModelInfo>().GetUIDummy();

        _goWeapon.transform.localPosition =
            new Vector3(0,
                       -go.transform.localPosition.z * go.transform.localScale.z,
                        go.transform.localPosition.y * go.transform.localScale.y);
        _goWeapon.transform.localRotation = go.transform.localRotation;
        _goWeapon.transform.localScale = go.transform.localScale;

        StopRotateWeapon();
        StartRotateWeapon();
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

    public void AddCounterNew(int slotIndex, int count)
    {
        _pageLobbyShop.AddBoxTokenCounter(slotIndex, count);
    }

    public void OnClick()
    {
        PopupBoxWeapon pbw = MenuManager.Singleton.OpenPopup<PopupBoxWeapon>(EUIPopup.PopupBoxWeapon);
        pbw.InitializeInfo(3);
    }
}

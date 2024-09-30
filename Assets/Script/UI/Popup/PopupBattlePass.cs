using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class PopupBattlePass : UIDialog
{
    [SerializeField]
    Transform _tListRoot;

    [SerializeField]
    ScrollRect _viewArea;

    [SerializeField]
    RectTransform[] _rtResize;

    [SerializeField]
    TextMeshProUGUI _txtGaugeCaption,
                    _txtButtonCaptionPlus, _txtButtonCaptionElite, _txtButtonCaptionFree,
                    _txtButtonCaptionPlusDone, _txtButtonCaptionEliteDone, _txtButtonBack,
                    _txtGameMoney, _txtCrystal,
                    _txtLevel, _txtExp,
                    _txtRemainTime;

    [SerializeField]
    Slider _slExpGauge, _slPassGauge;

    [SerializeField]
    GameObject _goButtonCaptionPlusDone, _goButtonCaptionEliteDone;

    List<BattlePassTable> _liBattlePass = new List<BattlePassTable>();
    SlotBattlePassSet[] _passSet;

    string D, h, m, s = string.Empty;

    private void OnEnable()
    {
        _liBattlePass = BattlePassTable.GetList();
        InitializeInfo();
        Invoke("SetFocus", 0.5f);
    }

    public void SetFocus()
    {
        Tween ani = null;

        RectTransform[] allHorizons = _tListRoot.GetComponentsInChildren<RectTransform>();
        RectTransform rtTarget = null;

        foreach (RectTransform rt in allHorizons)
        {
            if (rt.name == "Horizon" && rt.gameObject.activeSelf)
            {
                rtTarget = rt;
                break;
            }
        }

        if ( null != _viewArea && null != rtTarget )
        {
            RectTransform tar = rtTarget.GetComponent<RectTransform>();
            
            Vector3[] corners = new Vector3[4];
            tar.GetWorldCorners(corners);

            bool isVisible = AreaContain(_viewArea.viewport, corners[0]) && AreaContain(_viewArea.viewport, corners[1]) &&
                             AreaContain(_viewArea.viewport, corners[2]) && AreaContain(_viewArea.viewport, corners[3]);

            if ( !isVisible )
            {
                Vector3 targetLocalPos = rtTarget.parent.localPosition;
                Vector3 contentLocalPos = _viewArea.content.localPosition;

                contentLocalPos.y = - targetLocalPos.y
                                    - _viewArea.viewport.rect.height / 2
                                    - rtTarget.parent.GetComponent<RectTransform>().rect.height;

                ani = DOTween.To(GetContentLocalPosition, SetContentLocalPosition, contentLocalPos, 0.5f);
            }
        }
    }

    bool AreaContain(RectTransform area, Vector3 tar)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(area, tar);
    }

    Vector3 GetContentLocalPosition()
    {
        return _viewArea.content.localPosition;
    }

    void SetContentLocalPosition(Vector3 pos)
    {
        _viewArea.content.localPosition = pos;
    }

    public void InitializeInfo()
    {
        ComUtil.DestroyChildren(_tListRoot);

        _passSet = new SlotBattlePassSet[_liBattlePass.Count];

        _goButtonCaptionPlusDone.SetActive(m_Account.m_bIsPlus);
        _goButtonCaptionEliteDone.SetActive(m_Account.m_bIsElite);

        InitializeText();
        InitializeAccountInfo();
        InitializeCurrency();

        for (int i = 0; i < _liBattlePass.Count; i++)
        {
            _passSet[i] = m_MenuMgr.LoadComponent<SlotBattlePassSet>(_tListRoot, EUIComponent.SlotBattlePassSet);
        }

        InitializePassInfo();

        StartCoroutine(SetTimerText());
    }

    void InitializeSlot()
    {
        for (int i = 0; i < _liBattlePass.Count; i++)
        {
            _passSet[i].InitializeInfo(this, _liBattlePass[i]);
        }
    }

    IEnumerator SetTimerText()
    {
        TimeSpan time = m_Account.m_dtPass.AddMilliseconds(GlobalTable.GetData<int>("timeBattlePass")) - DateTime.UtcNow;

        while (time.TotalMilliseconds > 0f)
        {
            _txtRemainTime.text = string.Empty;

            _txtRemainTime.text = time.Days > 0 ? $"{time.Days}{D} " : "";
            _txtRemainTime.text = _txtRemainTime.text + (time.Hours > 0 ? $"{time.Hours}{h} " : " ");
            _txtRemainTime.text = _txtRemainTime.text + (time.Minutes > 0 ? $"{time.Minutes}{m} " : " ");
            _txtRemainTime.text = _txtRemainTime.text + (time.Seconds > 0 ? $"{time.Seconds}{s} " : " ");

            time = time.Subtract(TimeSpan.FromSeconds(1));

            yield return new WaitForSecondsRealtime(1f);
        }

        FindObjectOfType<ButtonBattlePass>().InitializeStartTime(this);

        yield break;
    }

    void InitializeText()
    {
        _txtButtonCaptionPlus.text = UIStringTable.GetValue("ui_popup_battlepass_purchase_plus_caption");
        _txtButtonCaptionElite.text = UIStringTable.GetValue("ui_popup_battlepass_purchase_elite_caption");
        _txtButtonCaptionFree.text = UIStringTable.GetValue("ui_popup_battlepass_purchase_free_caption");

        _txtButtonCaptionPlusDone.text = UIStringTable.GetValue("ui_popup_battlepass_purchase_plus_caption_done");
        _txtButtonCaptionEliteDone.text = UIStringTable.GetValue("ui_popup_battlepass_purchase_elite_caption_done");

        _txtButtonBack.text = UIStringTable.GetValue("ui_common_close");

        D = $"<size=70%>{UIStringTable.GetValue("ui_day")}</size>";
        h = $"<size=70%>{UIStringTable.GetValue("ui_hour")}</size>";
        m = $"<size=70%>{UIStringTable.GetValue("ui_minute")}</size>";
        s = $"<size=70%>{UIStringTable.GetValue("ui_second")}</size>";
    }

    public void InitializePassInfo()
    {
        int cExp = m_Account.m_nPassExp;
        int tExp = _liBattlePass[m_Account.m_nPassLevel - 1].Exp;

        _txtGaugeCaption.text = $"{cExp} / {tExp}";
        _slPassGauge.value = (float)cExp / tExp;

        InitializeSlot();
    }

    public void InitializeCurrency()
    {
        _txtGameMoney.text = GameManager.Singleton.invenMaterial.CalcTotalMoney().ToString();
        _txtCrystal.text = GameManager.Singleton.invenMaterial.CalcTotalCrystal().ToString();
    }

    void InitializeAccountInfo()
    {
        _txtLevel.text = GameManager.Singleton.user.m_nLevel.ToString();

        int clevel = GameManager.Singleton.user.m_nLevel;
        int cExp = GameManager.Singleton.user.m_nExp;
        int tExp = AccountLevelTable.GetData((uint)(0x01000000 + clevel)).Exp;

        _txtExp.text = $"{cExp} / {tExp}";
        _slExpGauge.value = (float)cExp / tExp;
    }

    public void OnClickBuyElite()
    {
        if ( m_Account.m_bIsElite ) return;

        PopupPurchase pop = m_MenuMgr.OpenPopup<PopupPurchase>(EUIPopup.PopupPurchase, true);
        pop.InitializeInfo(GlobalTable.GetData<uint>("valueElitePassKey"));
    }

    public void OnClickBuyPlus()
    {
        if (m_Account.m_bIsPlus) return;

        PopupPurchase pop = m_MenuMgr.OpenPopup<PopupPurchase>(EUIPopup.PopupPurchase, true);
        pop.InitializeInfo(GlobalTable.GetData<uint>("valuePlusPassKey"));
    }

    public void OnClickAddGameMoney()
    {
        PopupShopGameMoney ga = MenuManager.Singleton.OpenPopup<PopupShopGameMoney>(EUIPopup.PopupShopGameMoney, true);
    }

    public void OnClickAddCrystal()
    {
        PopupShopCrystal ct = MenuManager.Singleton.OpenPopup<PopupShopCrystal>(EUIPopup.PopupShopCrystal, true);
    }

    void Resize()
    {
        // LayoutRebuilder.ForceRebuildLayoutImmediate(_tGroupRoot.GetComponent<RectTransform>());
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
    }

    public override void Close()
    {
        GameObject.Find("ButtonBattlePass").GetComponent<ButtonBattlePass>().InitializePassInfo();
        m_GameMgr.RefreshInventory(GameManager.EInvenType.All);

        base.Close();
    }

    public override void Escape()
    {
        base.Escape();
    }
}

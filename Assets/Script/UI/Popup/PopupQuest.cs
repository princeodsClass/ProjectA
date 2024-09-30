using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using TMPro;

public class PopupQuest : UIDialog
{
    [SerializeField]
    Transform _tRootMiddleRewardSlot, _tRootFullRewardSlot, _tRootQuestSlot;

    [SerializeField]
    TextMeshProUGUI _txtTitle,
                    _txtTabCaptionDaily, _txtTabCaptionWeekly,
                    _txtRemainTimeDaily, _txtRemainTimeWeekly,
                    _txtExitButtonCaption, _txtGauage;

    [SerializeField]
    GameObject _goVIPBanner;

    [SerializeField]
    Slider _slGauge;

    [SerializeField]
    RectTransform[] _rt4Resize;

    Coroutine _Timer;
    string D, h, m, s = string.Empty;

    private void OnEnable()
    {
        _Timer = m_GameMgr.StartCoroutine(SetRemainTime());
        InitializeInfo();
    }

    private void OnDisable()
    {
        FindObjectOfType<PageLobbyInventory>()?.InitializeMaterial();
    }

    public void InitializeInfo()
    {
        m_GameMgr.StartCoroutine(InitializeQuest());
        FindObjectOfType<QuestButton>().SetNewQuestMark(false);
    }

    IEnumerator InitializeQuest()
    { 
        yield return m_GameMgr.StartCoroutine(SetQuest());
        _goVIPBanner.SetActive(!m_Account.IsVIP());
        Array.ForEach(_rt4Resize, rt => LayoutRebuilder.ForceRebuildLayoutImmediate(rt));
    }

    public void SetAccountRewards()
    {
        int total = 0;
        int current = 0;

        QuestTable quest;

        for( int i = 0; i < m_Account.m_nQuestKey.Length; i++ )
        {
            quest = QuestTable.GetData(m_Account.m_nQuestKey[i]);
            total += quest.Point;

            if ( m_Account.m_bQuestIsComplete[i] )
                current += (int)((float)quest.Point * ((i == 4 || i == 5) ? GlobalTable.GetData<float>("ratioVIPQuestPoint") : 1f));
        }

        _txtGauage.text = $"{current} / {total}";
        _slGauge.value = (float)current / (float)total;

        m_GameMgr.StartCoroutine(SetRewards());
    }

    IEnumerator SetRewards()
    {
        AccountLevelTable lt = AccountLevelTable.GetData((uint)(0x01000000 + m_Account.m_nLevel));

        ComUtil.DestroyChildren(_tRootMiddleRewardSlot);
        ComUtil.DestroyChildren(_tRootFullRewardSlot);

        yield return m_GameMgr.StartCoroutine(m_DataMgr.GetQuestAccountRewardsInfo());

        SlotQuestReward normalSlot = m_MenuMgr.LoadComponent<SlotQuestReward>(_tRootMiddleRewardSlot, EUIComponent.SlotQuestReward);
        normalSlot.InitializeInfo(this,
                                  lt.QuestRewardsKey00,
                                  SlotQuestReward.ERewardType.Half,
                                  lt.QuestRewardsCount00,
                                  m_Account.m_bIsRecieveQuestAccouuntRewardsMiddle,
                                  _slGauge.value < 0.5f);

        SlotQuestReward vipSlot = m_MenuMgr.LoadComponent<SlotQuestReward>(_tRootFullRewardSlot, EUIComponent.SlotQuestReward);
        vipSlot.InitializeInfo(this,
                               lt.QuestRewardsKey01,
                               SlotQuestReward.ERewardType.Full,
                               lt.QuestRewardsCount01,
                               m_Account.m_bIsRecieveQuestAccouuntRewardsFull,
                               _slGauge.value < 1.0f);
    }

    IEnumerator SetRemainTime()
    {
        DateTime now = DateTime.UtcNow;
        DateTime nextTime = now.AddDays(1);
        nextTime = new DateTime(nextTime.Year, nextTime.Month, nextTime.Day, 0, 0, 0);
        TimeSpan time = nextTime - now;

        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(1f);

        while (time.TotalMilliseconds > 0f)
        {
            _txtRemainTimeDaily.text = string.Empty;

            _txtRemainTimeDaily.text = time.Days > 0 ? $"{time.Days}{D} " : "";
            _txtRemainTimeDaily.text = _txtRemainTimeDaily.text + (time.Hours > 0 ? $"{time.Hours}{h} " : " ");
            _txtRemainTimeDaily.text = _txtRemainTimeDaily.text + (time.Minutes > 0 ? $"{time.Minutes}{m} " : " ");
            _txtRemainTimeDaily.text = _txtRemainTimeDaily.text + (time.Seconds > 0 ? $"{time.Seconds}{s} " : " ");

            time = time.Subtract(TimeSpan.FromSeconds(1));

            yield return delay;
        }

        InitializeInfo();
    }

    public IEnumerator SetQuest()
    {
        ComUtil.DestroyChildren(_tRootMiddleRewardSlot);
        ComUtil.DestroyChildren(_tRootFullRewardSlot);

        PopupWait4Response wait = m_MenuMgr.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

        yield return m_GameMgr.StartCoroutine(m_DataMgr.GetQuestInfo());

        ComUtil.DestroyChildren(_tRootQuestSlot);

        if ( Array.Exists(m_Account.m_nQuestKey, x => x == 0) )
            yield return m_GameMgr.StartCoroutine(CreateQuestList(wait));
        else
            SetQuestList(wait);

        SetAccountRewards();
    }

    IEnumerator CreateQuestList(PopupWait4Response wait)
    {
        List<QuestTable> list = QuestTable.GetList(m_Account.m_nLevel, 6);

        for ( int i = 0; i < m_Account.m_nQuestKey.Length; i++ )
            yield return m_GameMgr.StartCoroutine(m_DataMgr.CreateQuest(i, list[i].PrimaryKey));

        yield return m_GameMgr.StartCoroutine(m_DataMgr.GetQuestInfo());

        m_Account.m_bIsRecieveQuestAccouuntRewardsMiddle = false;
        m_Account.m_bIsRecieveQuestAccouuntRewardsFull = false;
        
        Array.ForEach(m_Account.m_bUseQuestCard, isUsed => isUsed = false);

        SetQuestList(wait);
    }

    void SetQuestList(PopupWait4Response wait)
    {
        for ( int i = 0; i < m_Account.m_nQuestKey.Length; i++ )
        {
            SlotQuest slot = m_MenuMgr.LoadComponent<SlotQuest>(_tRootQuestSlot, EUIComponent.SlotQuest);
            
            QuestTable quest = QuestTable.GetData(m_Account.m_nQuestKey[i]);
            int count = m_Account.m_nQuestCount[i];
            bool isComplete = m_Account.m_bQuestIsComplete[i];

            slot.InitializeInfo(this, quest, i, count, isComplete);
            m_Account.m_bUseQuestCard[i] = quest.RequireCount == count;
        }

        wait.Close();
    }

    void InitializeText()
    {
        _txtTitle.text = UIStringTable.GetValue("ui_popup_quest_title");

        _txtTabCaptionDaily.text = UIStringTable.GetValue("ui_popup_quest_tab_daily");
        _txtTabCaptionWeekly.text = UIStringTable.GetValue("ui_popup_quest_tab_weekly");

        _txtExitButtonCaption.text = UIStringTable.GetValue("ui_common_close");

        D = $"<size=70%>{UIStringTable.GetValue("ui_day")}</size>";
        h = $"<size=70%>{UIStringTable.GetValue("ui_hour")}</size>";
        m = $"<size=70%>{UIStringTable.GetValue("ui_minute")}</size>";
        s = $"<size=70%>{UIStringTable.GetValue("ui_second")}</size>";
    }

    public void OnClickVIPBanner()
    {
        if ( false == m_Account.IsVIP() )
        {
            PopupShopVIP vippop = m_MenuMgr.OpenPopup<PopupShopVIP>(EUIPopup.PopupShopVIP, true);
        }
    }

    private void Awake()
    {
        Initialize();
        InitializeText();
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
        StopCoroutine(_Timer);
        base.Close();
    }

    public override void Escape()
    {
        Close();
    }
}

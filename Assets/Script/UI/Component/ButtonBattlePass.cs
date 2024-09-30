using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonBattlePass : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _txtButtonCaption, _txtGaugeCaption, _txtTagCaption, _txtComplete;

    [SerializeField]
    Slider _slPassGauge;

    [SerializeField]
    GameObject _goNewTag, _goComplete;

    UserAccount m_Account = null;
    MenuManager m_MenuMgr = null;

    int _time = default;

    private void Awake()
    {
        _time = GlobalTable.GetData<int>("timeBattlePass");
    }

    private void OnEnable()
    {
        if ( m_MenuMgr == null ) m_MenuMgr = MenuManager.Singleton;
        if ( m_Account == null ) m_Account = GameManager.Singleton.user;

        InitializeStartTime();
        InitializePassInfo();
        InitializeText();
    }

    public void InitializePassInfo()
    {
        int cExp = m_Account.m_nPassExp;
        int tExp = BattlePassTable.GetData((uint)(0x03000000 + m_Account.m_nPassLevel)).Exp;

        _txtGaugeCaption.text = $"{cExp} / {tExp}";
        _slPassGauge.value = (float)cExp / tExp;

        _goNewTag.SetActive(ObtainableNew());
        _goComplete.SetActive(m_Account.m_nPassLevel >= BattlePassTable.GetList().Count);
        _txtComplete.text = UIStringTable.GetValue("ui_popup_battlereward_titleresult_success");

        GameObject.Find("BattlePage").GetComponent<PageLobbyBattle>().CheckBoxState();
    }

    public void InitializeStartTime(PopupBattlePass popup = null)
    {
        if ( m_Account.m_dtPass == default )
        {
            m_Account.m_dtPass = DateTime.UtcNow;
            StartCoroutine(GameDataManager.Singleton.StartBattlePass());
        }
        else
        {
            if (m_Account.m_dtPass.AddMilliseconds(_time) < DateTime.UtcNow)
                StartCoroutine(RenewalStartTime(popup));
        }
    }

    IEnumerator RenewalStartTime(PopupBattlePass popup)
    {
        PopupWait4Response wait = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

        while (m_Account.m_dtPass.AddMilliseconds(_time) < DateTime.UtcNow)
        {
            m_Account.m_dtPass = m_Account.m_dtPass.AddMilliseconds(_time);
        }

        yield return StartCoroutine(GameDataManager.Singleton.StartBattlePass(m_Account.m_dtPass));
        yield return StartCoroutine(GameDataManager.Singleton.GetBattlePassInfo(popup));

        wait.Close();
    }

    bool ObtainableNew()
    {
        for (int i = 0; i < m_Account.m_nPassLevel; i++)
        {
            if ( m_Account.m_bIsPlus && m_Account.m_bpPass[i][0] == false ) return true;
            if ( m_Account.m_bIsElite && m_Account.m_bpPass[i][1] == false ) return true;
            if ( m_Account.m_bpPass[i][2] == false ) return true;
        }

        return false;
    }

    void InitializeText()
    {
        _txtButtonCaption.text = UIStringTable.GetValue("ui_page_lobby_battle_pass_title");
        _txtTagCaption.text = UIStringTable.GetValue("ui_com_rewards");
    }

    public void OnClickBattlePass()
    {
        PopupBattlePass pass = m_MenuMgr.OpenPopup<PopupBattlePass>(EUIPopup.PopupBattlePass);
    }

    public bool IsNew()
    {
        return _goNewTag.activeSelf;
    }
}

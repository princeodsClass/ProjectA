using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComCommunityAbyss : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _txtTitle, _txtTimerTitle, _txtTimer,
                    _txtAdjustCoverTitle, _txtCompleteCoverTitle, _txtLimitLevel;

    [SerializeField]
    GameObject _goCover, _goNewTag;

    [SerializeField]
    GameObject _goAdjustCover, _goCompleteCover;

    string D, h, m, s = string.Empty;

    PopupAbyss _popup = null;

    private void Awake()
    {
        InitializeInfo();
    }

    public void InitializeInfo()
    {
        SetCover();
        InitializeText();
        StartCoroutine(SetTimerText());
    }

    void InitializeText()
    {
        _txtTitle.text = $"{UIStringTable.GetValue("ui_popup_abyss_tab_abyss")} ( S.{GameManager.Singleton.user.m_nAbyssSeason.ToString("D2")} )";
        _txtAdjustCoverTitle.text = UIStringTable.GetValue("ui_comp_abyss_adjust_title");
        _txtCompleteCoverTitle.text = UIStringTable.GetValue("ui_comp_abyss_adjust_completetitle");
        _txtLimitLevel.text = $"Lv.{GlobalTable.GetData<int>("valueAbyssOpenLevel")}";

        D = $"<size=70%>{UIStringTable.GetValue("ui_day")}</size>";
        h = $"<size=70%>{UIStringTable.GetValue("ui_hour")}</size>";
        m = $"<size=70%>{UIStringTable.GetValue("ui_minute")}</size>";
        s = $"<size=70%>{UIStringTable.GetValue("ui_second")}</size>";
    }

    IEnumerator SetTimerText()
    {
        SetAdjustCover(false);

        TimeSpan time = default;
        _txtTimerTitle.text = UIStringTable.GetValue("ui_popup_adventure_endin");

        while (GameManager.Singleton.user.m_dtAbyssEnd > DateTime.UtcNow)
        {
            time = GameManager.Singleton.user.m_dtAbyssEnd - DateTime.UtcNow;
            _txtTimer.text = string.Empty;

            _txtTimer.text = time.Days > 0 ? $"{time.Days}{D} " : "";
            _txtTimer.text = _txtTimer.text + (time.Hours > 0 ? $"{time.Hours}{h} " : " ");
            _txtTimer.text = _txtTimer.text + (time.Minutes > 0 ? $"{time.Minutes}{m} " : " ");
            _txtTimer.text = _txtTimer.text + (time.Seconds > 0 ? $"{time.Seconds}{s} " : " ");

            if (null != _popup && _popup.gameObject.activeSelf)
                _popup.SetTimerValue(_txtTimer.text);

            yield return YieldInstructionCache.WaitForSecondsRealtime(1f);
        }

        if (null != _popup && _popup.gameObject.activeSelf)
            _popup.Close();

        StartCoroutine(SetTimerTextAdjust());
    }

    IEnumerator SetTimerTextAdjust()
    {
        SetAdjustCover(true);

        yield return StartCoroutine(GameDataManager.Singleton.GetAbyssInfo());

        TimeSpan time = default;
        _txtTimerTitle.text = UIStringTable.GetValue("ui_comp_abyss_adjust_remaintime");

        while (GameManager.Singleton.user.m_dtAbyssAdjustEnd > DateTime.UtcNow)
        {
            time = GameManager.Singleton.user.m_dtAbyssAdjustEnd - DateTime.UtcNow;
            _txtTimer.text = string.Empty;

            _txtTimer.text = time.Days > 0 ? $"{time.Days}{D} " : "";
            _txtTimer.text = _txtTimer.text + (time.Hours > 0 ? $"{time.Hours}{h} " : " ");
            _txtTimer.text = _txtTimer.text + (time.Minutes > 0 ? $"{time.Minutes}{m} " : " ");
            _txtTimer.text = _txtTimer.text + (time.Seconds > 0 ? $"{time.Seconds}{s} " : " ");

            if (null != _popup && _popup.gameObject.activeSelf)
                _popup.SetTimerValue(_txtTimer.text);

            yield return YieldInstructionCache.WaitForSecondsRealtime(1f);
        }

        yield return StartCoroutine(GameDataManager.Singleton.GetAbyssInfo());
        InitializeInfo();
    }

    public void OnClick()
    {
        if (!IsAvailable())
        {
            PopupDefault pop = MenuManager.Singleton.OpenPopup<PopupDefault>(EUIPopup.PopupDefault);
            pop.SetTitle("ui_error_title");
            pop.SetMessage("ui_hint_abyss_desc_2");
            pop.AddMessage("ui_error_notenoughlevel_abyss", 2);
            pop.SetButtonText("ui_comp_abyss_button_view_rank", "ui_common_close", () =>
            {
                StartCoroutine(AbyssPopup(true));
            }, null, "TutorialAbyss");
        }
        else
        {
            StartCoroutine(AbyssPopup());
        }
    }

    bool IsAvailable()
    {
        return GameManager.Singleton.user.m_nLevel >= GlobalTable.GetData<int>("valueAbyssOpenLevel");
    }

    public void OnClickTutorial(bool isTutorial)
    {
        OnClick();
    }

    IEnumerator AbyssPopup(bool isViewer = false)
    {
        PopupWait4Response wait = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response);
        yield return StartCoroutine(GameDataManager.Singleton.GetAbyssInfo());
        wait.Close();

        if ( _goCompleteCover.activeSelf )
        {
            PopupDefault me = MenuManager.Singleton.OpenPopup<PopupDefault>(EUIPopup.PopupDefault);
            me.SetTitle("ui_hint_title");
            me.SetMessage("ui_comp_abyss_adjust_already_received");
            me.SetButtonText("ui_comp_abyss_button_view_rank", "ui_common_close", () =>
            {
                _popup = MenuManager.Singleton.OpenPopup<PopupAbyss>(EUIPopup.PopupAbyss);
                _popup.InitializeInfo(true);
            });
        }
        else if ( _goAdjustCover.activeSelf )
        {
            yield return StartCoroutine(GameDataManager.Singleton.SetAbyssSeasonRewardsGet(this));            
        }
        else
        {
            _popup = MenuManager.Singleton.OpenPopup<PopupAbyss>(EUIPopup.PopupAbyss);
            _popup.InitializeInfo(isViewer);
        }
    }

    public void SetAdjustCover(bool state)
    {
        if ( state )
        {
            _goCompleteCover.SetActive(GameManager.Singleton.user.m_bIsAbyssRewardsReceived ||
                                       GameManager.Singleton.user.m_nAbyssCurRank == 0);
            _goAdjustCover.SetActive(!_goCompleteCover.activeSelf);
        }
        else
        {
            _goAdjustCover.SetActive(false);
            _goCompleteCover.SetActive(false);
        }
    }

    void SetCover()
    {
        _goCover.SetActive(!IsAvailable());
    }

    private void OnEnable()
    {
    }
}

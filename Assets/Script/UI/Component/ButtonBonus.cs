using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonBonus : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _txtTimer, _txtCaption;

    UserAccount m_Account = null;
    MenuManager m_MenuMgr = null;

    Coroutine _coTimer;

    string D, h, m, s;

    private void Awake()
    {
        D = $"<size=70%>{UIStringTable.GetValue("ui_day")}</size>";
        h = $"<size=70%>{UIStringTable.GetValue("ui_hour")}</size>";
        m = $"<size=70%>{UIStringTable.GetValue("ui_minute")}</size>";
        s = $"<size=70%>{UIStringTable.GetValue("ui_second")}</size>";

        _txtCaption.text = UIStringTable.GetValue("ui_robbery");
    }

    private void OnEnable()
    {
        if ( m_MenuMgr == null ) m_MenuMgr = MenuManager.Singleton;
        if ( m_Account == null ) m_Account = GameManager.Singleton.user;

        _coTimer = StartCoroutine(StartTime());
    }

    IEnumerator StartTime()
    {
        DateTime dtTar = m_Account.m_dtBonusEnd;
        TimeSpan ct; 

        while (dtTar > DateTime.UtcNow)
        {
            ct = dtTar - DateTime.UtcNow;
            _txtTimer.text = "";

            if (ct.TotalDays > 1)
            {
                _txtTimer.text = $"{ct.Days}{D} ";
                _txtTimer.text = _txtTimer.text + (ct.Hours > 0 ? $"{ct.Hours}{h}" : "");

                yield return YieldInstructionCache.WaitForSecondsRealtime(3600.0f);
            }
            else if (ct.TotalHours > 1)
            {
                _txtTimer.text = $"{ct.Hours}{h} ";
                _txtTimer.text = _txtTimer.text + (ct.Minutes > 0 ? $"{ct.Minutes}{m}" : "");

                yield return YieldInstructionCache.WaitForSecondsRealtime(60.0f);
            }
            else if (ct.TotalSeconds > 1)
            {
                _txtTimer.text = _txtTimer.text + (ct.Minutes > 0 ? $"{ct.Minutes}{m} " : "");
                _txtTimer.text = _txtTimer.text + (ct.Seconds > 0 ? $"{ct.Seconds}{s}" : "");

                yield return YieldInstructionCache.WaitForSecondsRealtime(1.0f);
            }

            // yield return YieldInstructionCache.WaitForSecondsRealtime(1.0f);
        }

        EndTimer();
    }

    public void EndTimer()
    {
        m_Account.m_bIsBonusTime = false;

        StopAllCoroutines();
        gameObject.SetActive(false);
    }

    public void OnClick()
    {
        var oHandler = new PopupLoadingHandlerBattleReady();
        oHandler.Init(PopupLoadingHandlerBattleReady.MakeParams(EMapInfoType.BONUS, new STIDInfo(0, 0, 0)));

        var oLoadingPopup = m_MenuMgr.OpenPopup<PopupLoading>(EUIPopup.PopupLoading);
        oLoadingPopup.Init(PopupLoading.MakeParams(PopupLoading.ELoadingType.BATTLE_READY, oHandler, (a_oSender) => this.OnCompleteLoadingBattleReady(a_oSender, 1, 1)));
    }

    private void OnCompleteLoadingBattleReady(PopupLoading a_oSender, int a_nEpisode, int a_nChapter)
    {
        var oMapInfoDict = CMapInfoTable.Singleton.LoadMapInfosDownloaded(EMapInfoType.BONUS, a_nChapter - 1, a_nEpisode - 1);

        GameDataManager.Singleton.SetPlayStageID(0);
        GameDataManager.Singleton.SetContinueTimes(0);
        GameDataManager.Singleton.SetupPlayMapInfo(EMapInfoType.BONUS, EPlayMode.BONUS, oMapInfoDict);

        GameDataManager.Singleton.ExLateCallFunc((a_oFuncSender) =>
        {
            m_MenuMgr.SceneEnd();
            m_MenuMgr.SceneNext(ESceneType.Battle);
            m_Account.ResetBonusPoint();
        }, ComType.G_DURATION_LOADING_PROGRESS_ANI);
    }
}

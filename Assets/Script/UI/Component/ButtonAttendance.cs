using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonAttendance : MonoBehaviour
{
    [SerializeField]
    GameObject _goNewTag;

    [SerializeField]
    TextMeshProUGUI _txtTimer;

    PageLobby _lobby;
    PopupAttendance _popup;
    List<AttendanceTable> _attendance;

    string h, m, s;

    private void Awake()
    {
        InitializeText();

        StartCoroutine(SetTimer());
    }

    void InitializeText()
    {
        h = $"<size=70%>{UIStringTable.GetValue("ui_hour")}</size>";
        m = $"<size=70%>{UIStringTable.GetValue("ui_minute")}</size>";
        s = $"<size=70%>{UIStringTable.GetValue("ui_second")}</size>";
    }

    IEnumerator SetTimer()
    {
        TimeSpan remainTime;
        DateTime now, midnight;

        WaitForSecondsRealtime second = new WaitForSecondsRealtime(1.0f);

        while (true)
        {
            now = DateTime.UtcNow;
            midnight = now.Date.AddDays(1);

            remainTime = midnight - now;

            if ( remainTime.Hours > 0 )
                _txtTimer.text = $"{remainTime.Hours}{h} {remainTime.Minutes}{m} {remainTime.Seconds}{s}";
            else if ( remainTime.Minutes > 0 )
                _txtTimer.text = $"{remainTime.Minutes}{m} {remainTime.Seconds}{s}";
            else
                _txtTimer.text = $"{remainTime.Seconds}{s}";

            yield return second;
        }
    }

    public void Initialize()
    {
        gameObject.SetActive((GameManager.Singleton.user.CorrectEpisode + 1)
                                >= GlobalTable.GetData<int>("valueAttendanceOpenEpisode") &&
                              GameManager.Singleton.user.m_nAttendanceContinuouslyCount
                                < AttendanceTable.GetList(1).Count);
    }

    public void OnClick()
    {
        _attendance = AttendanceTable.GetList(GameManager.Singleton.user.m_nLevel);
        _popup = MenuManager.Singleton.OpenPopup<PopupAttendance>(EUIPopup.PopupAttendance, true);
        _popup.InitializeInfo(_attendance);
    }

    public void SetNewTag(bool state)
    {
        _goNewTag.SetActive(state);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}

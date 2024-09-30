using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventADSDeal : IEvent
{
    DateTime _preTime = default;
    DateTime _nextTime = default;

    public void Initialize()
    {
        GameManager.Log("ADS Start!");
        CheckEventTime();
    }

    public void CheckEventTime()
    {
        DateTime now = DateTime.UtcNow;

        _preTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);

        _nextTime = _preTime.AddDays(1);

        while (_nextTime < now)
            _nextTime = _nextTime.AddDays(1);
    }

    public TimeSpan CheckRemainTime()
    {
        CheckEventTime();

        return _nextTime - DateTime.UtcNow;
    }

    public IEnumerator RecodeEvent()
    {
        Dictionary<string, string> fields = new Dictionary<string, string>();

        fields.Add("auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString());
        fields.Add("accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN));
        fields.Add("adsDealResetDatetime", ComUtil.EnUTC());

        for ( int i = 0; i < 4; i++ )
            fields.Add($"adsDealItem{i}", GameManager.Singleton.user.m_nADSDealKey[i].ToString());

        yield return GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.ModifyAccount(fields));
    }

    public DateTime CheckPreTime()
    {
        return _preTime;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventMaterialPremiumBoxDeal : MonoBehaviour, IEvent
{
    DateTime _preTime = default;
    DateTime _nextTime = default;

    public void Initialize()
    {
        GameManager.Log("Premium Material Box Deal Start!");
        CheckEventTime();
    }

    public void CheckEventTime()
    {
        DateTime now = DateTime.UtcNow;
        DateTime t = GameManager.Singleton.user.m_dtPMaterialBoxReset;

        _nextTime = t == default ? now.AddDays(2) : t.AddDays(2);
        _nextTime = new DateTime(_nextTime.Year, _nextTime.Month, _nextTime.Day, 0, 0, 0);

        while (_nextTime < now)
            _nextTime = _nextTime.AddDays(2);
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
        fields.Add("pMaterialBoxResetDatetime", ComUtil.EnUTC());

        yield return GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.ModifyAccount(fields));
    }

    public DateTime CheckPreTime()
    {
        DateTime now = DateTime.UtcNow;
        DateTime t = GameManager.Singleton.user.m_dtPMaterialBoxReset;
        DateTime rt = t == default ? now : t;

        rt = new DateTime(rt.Year, rt.Month, rt.Day, 0, 0, 0);

        return rt;
    }
}

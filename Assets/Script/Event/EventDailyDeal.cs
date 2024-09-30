using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventDailyDeal : IEvent
{
    DateTime _preTime = default;
    DateTime _nextTime = default;

    public void Initialize()
    {
        GameManager.Log("Daily Start!");
        CheckEventTime();
    }

    public void CheckEventTime()
    {
        DateTime now = DateTime.UtcNow;

        _preTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);

        _nextTime = now.AddDays(1);
        _nextTime = new DateTime(_nextTime.Year, _nextTime.Month, _nextTime.Day, 0, 0, 0);

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
        fields.Add("dailyDealResetDatetime", ComUtil.EnUTC());

        for ( int i = 0; i < 4; i++ )
            fields.Add($"dailyDealItem{i}", PlayerPrefs.GetString($"{ComType.SHOP_DAILY_GOODS_LIST}[{i}]"));

        yield return GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.ModifyAccount(fields));
    }

    public DateTime CheckPreTime()
    {
        return _preTime;
    }
}

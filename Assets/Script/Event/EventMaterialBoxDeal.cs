using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventMaterialBoxDeal : MonoBehaviour, IEvent
{
    DateTime _preTime = default;
    DateTime _nextTime = default;

    public void Initialize()
    {
        GameManager.Log("Material Box Deal Start!");
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
        PlayerPrefs.SetString(ComType.SHOP_WEAPON_BOX_START_TIME, DateTime.UtcNow.ToString());
        yield break;
    }

    public DateTime CheckPreTime()
    {
        return _preTime;
    }
}

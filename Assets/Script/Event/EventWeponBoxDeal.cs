using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventWeaponBoxDeal : IEvent
{
    DateTime _preTime = default;
    DateTime _nextTime = default;

    public void Initialize()
    {
        GameManager.Log("Weapon Box Deal Start!");
        CheckEventTime();
    }

    public void CheckEventTime()
    {
        DateTime now = DateTime.UtcNow;
        CheckPreTime();

        int nextHours = ( now.Hour / 8 + 1 ) * 8;
        
        nextHours = nextHours > 23 ? 0 : nextHours;
        _nextTime = nextHours > 23 ? now.AddDays(1) : now;
        _nextTime = new DateTime(_nextTime.Year, _nextTime.Month, _nextTime.Day, nextHours, 0, 0);

        while (_nextTime < now)
            _nextTime = _nextTime.AddHours(8);
    }

    public TimeSpan CheckRemainTime()
    {
        CheckEventTime();
        return _nextTime - DateTime.UtcNow;
    }

    public IEnumerator RecodeEvent()
    {
        DateTime now = DateTime.UtcNow;
        Dictionary<string, string> fields = new Dictionary<string, string>();

        PlayerPrefs.SetString(ComType.SHOP_WEAPON_BOX_START_TIME, now.ToString());

        fields.Add("auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString());
        fields.Add("accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN));
        fields.Add("weaponBoxResetDatetime", now.ToString("yyyy-MM-dd tt hh:mm:ss", new System.Globalization.CultureInfo("en-US")));

        yield return GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.ModifyAccount(fields));

        PlayerPrefs.SetString(ComType.SHOP_WEAPON_BOX_START_TIME, now.ToString());
        yield break;
    }

    public DateTime CheckPreTime()
    {
        DateTime now = DateTime.UtcNow;

        int preHours = now.Hour / 8 * 8;

        _preTime = new DateTime(now.Year, now.Month, now.Day, preHours, 0, 0);

        return _preTime;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventGameMoneyDeal : IEvent
{
    DateTime _nextTime = default;

    public void Initialize()
    {
        GameManager.Log("GameMoney Start!");
        CheckEventTime();
    }

    public void CheckEventTime()
    {
        DateTime now = DateTime.UtcNow;
        DayOfWeek currentDayOfWeek = now.DayOfWeek;

        int daysUntilNextSunday = ((int)DayOfWeek.Sunday - (int)currentDayOfWeek + 7) % 7;
        DateTime nextSunday = now.AddDays(daysUntilNextSunday);

        DateTime nextMonday = nextSunday.AddDays(1);

        DateTime nextMondayMidnight = new DateTime(nextMonday.Year, nextMonday.Month, nextMonday.Day, 0, 0, 0);

        _nextTime = nextMondayMidnight;
    }

    public TimeSpan CheckRemainTime()
    {
        CheckEventTime();

        return _nextTime - DateTime.UtcNow;
    }

    public IEnumerator RecodeEvent()
    {
        yield return null;
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

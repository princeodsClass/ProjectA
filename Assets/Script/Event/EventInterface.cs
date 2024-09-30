using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEvent
{
    public void Initialize();
    public void CheckEventTime();
    public TimeSpan CheckRemainTime();
    public DateTime CheckPreTime();
    public IEnumerator RecodeEvent();
}

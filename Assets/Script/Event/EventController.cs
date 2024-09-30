using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventController
{
    Dictionary<string, Action> eventActions = new Dictionary<string, Action>();

    public EventDailyDeal _eDailyDeal = new EventDailyDeal();
    public EventWeaponBoxDeal _eWeaponBoxDeal = new EventWeaponBoxDeal();
    public EventWeaponPremiumBoxDeal _eWeaponPremiumBoxDeal = new EventWeaponPremiumBoxDeal();
    public EventMaterialBoxDeal _eMaterialBoxDeal = new EventMaterialBoxDeal();
    public EventMaterialPremiumBoxDeal _eMaterialPremiumBoxDeal = new EventMaterialPremiumBoxDeal();
    public EventGameMoneyDeal _eGameMoneyDeal = new EventGameMoneyDeal();
    public EventADSDeal _eADSDeal = new EventADSDeal();

    public void  Initialize()
    {
        eventActions.Add("EventDaily", EventDaily);
        eventActions.Add("EventWeaponBoxDeal", EventWeaponBoxDeal);
        eventActions.Add("EventMaterialBoxDeal", EventMaterialBoxDeal);
        eventActions.Add("EventMaterialPremiumBoxDeal", EventMaterialPremiumBoxDeal);
        eventActions.Add("EventGameMoneyDeal", EventGameMoneyDeal);
        eventActions.Add("EventADSDeal", EventADSDeal);

        for ( int i = 0; i < (int)EEventType.END; i++)
            ExcuteAction(eventActions[((EEventType)i).ToString()]);
    }

    void ExcuteAction(Action e)
    {
        e.Invoke();
    }

    void EventDaily()
    {
        _eDailyDeal.Initialize();
    }

    void EventWeaponBoxDeal()
    {
        _eWeaponBoxDeal.Initialize();
    }

    void EventMaterialBoxDeal()
    {
        _eMaterialBoxDeal.Initialize();
    }

    void EventMaterialPremiumBoxDeal()
    {
        _eMaterialPremiumBoxDeal.Initialize();
    }

    void EventGameMoneyDeal()
    {
        _eGameMoneyDeal.Initialize();
    }

    void EventADSDeal()
    {
        _eADSDeal.Initialize();
    }
}

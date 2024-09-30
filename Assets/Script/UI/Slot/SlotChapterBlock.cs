using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotChapterBlock : MonoBehaviour
{
    [Header("완료 오브젝트")]
    public GameObject _goComplete;

    [Header("현재 오브젝트")]
    public GameObject _goCurrent;

    [Header("보스 인디케이터")]
    public GameObject _goBoss;

    [Header("중간 보스 인디케이터")]
    public GameObject _goMidBoss;

    public void OnClickMidBossBeacon()
    {
        PopupSysMessage pop = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
        pop.InitializeInfo("ui_error_title", "ui_hint_mission_zombie_desc_4", "ui_common_close", null, "TutorialMidBoss");
        pop.AddMessage("ui_hint_mission_zombie_desc_5", 2);
    }

    public void OnClickBossBeacon()
    {
        PopupSysMessage pop = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
        pop.InitializeInfo("ui_error_title", "ui_hint_boss_desc", "ui_common_close", null, "TutorialBoss");
    }
}

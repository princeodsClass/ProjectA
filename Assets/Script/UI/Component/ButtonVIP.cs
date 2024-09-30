using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonVIP : MonoBehaviour
{
    public void OnClick()
    {
        if (GameManager.Singleton.user.m_nLevel >= GlobalTable.GetData<int>("valueShopOpenLevel"))
        {
            PopupShopVIP ct = MenuManager.Singleton.OpenPopup<PopupShopVIP>(EUIPopup.PopupShopVIP, true);
        }
        else
        {
            PopupSysMessage pop = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
            pop.InitializeInfo("ui_error_title", "ui_error_notenoughlevel_shop", "ui_common_close", null, "TutorialShop");
        }

        return;
    }
}

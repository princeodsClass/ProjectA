using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PageLobbyTop : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _tLevel, _tExp, _tGameMoney, _tCrystal, _tGlobalTicket, _txtNickname;

    [SerializeField]
    Image _imgGold, _imgCrystal, _imgGoldenGauge;

    [SerializeField]
    Slider _sExpGauage;

    Sprite t;
    private void OnEnable()
    {
        InitializeAccountInfo();
        InitializeCurrency();
    }

    public void InitializeCurrency()
    {
		uint nItemKey = GlobalTable.GetData<uint>(ComType.G_VALUE_GLOBAL_TICKET_DEC_KEY);
		int nNumItems = GameManager.Singleton.invenMaterial.GetItemCount(nItemKey);

        _tGameMoney.text = GameManager.Singleton.invenMaterial.CalcTotalMoney().ToString();
        _tCrystal.text = GameManager.Singleton.invenMaterial.CalcTotalCrystal().ToString();
		_tGlobalTicket.text = nNumItems.ToString();

		_tGlobalTicket.color = (nNumItems <= 0) ? Color.red : Color.white;

        LateInitializeCurrency();
    }

    void LateInitializeCurrency()
    {
        if (MenuManager.Singleton.m_liCurPopupType.Contains(EUIPopup.PopupAgent) )
        {
            int t = MenuManager.Singleton.m_liCurPopupType.IndexOf(EUIPopup.PopupAgent);
            (MenuManager.Singleton.m_liCurPopup[t] as PopupAgent).UpdateUIsState();
        }

        if (MenuManager.Singleton.m_liCurPopupType.Contains(EUIPopup.PopupAttendance))
        {
            int t = MenuManager.Singleton.m_liCurPopupType.IndexOf(EUIPopup.PopupAttendance);
            (MenuManager.Singleton.m_liCurPopup[t] as PopupAttendance).SetTop();
        }

        if (MenuManager.Singleton.m_liCurPopupType.Contains(EUIPopup.PopupBattlePass))
        {
            int t = MenuManager.Singleton.m_liCurPopupType.IndexOf(EUIPopup.PopupBattlePass);
            (MenuManager.Singleton.m_liCurPopup[t] as PopupBattlePass).InitializeCurrency();
        }

        if (MenuManager.Singleton.m_liCurPopupType.Contains(EUIPopup.PopupBoxMaterial))
        {
            int t = MenuManager.Singleton.m_liCurPopupType.IndexOf(EUIPopup.PopupBoxMaterial);
            (MenuManager.Singleton.m_liCurPopup[t] as PopupBoxMaterial).SetTop();
        }

        if (MenuManager.Singleton.m_liCurPopupType.Contains(EUIPopup.PopupBoxMaterialPremium))
        {
            int t = MenuManager.Singleton.m_liCurPopupType.IndexOf(EUIPopup.PopupBoxMaterialPremium);
            (MenuManager.Singleton.m_liCurPopup[t] as PopupBoxMaterialPremium).SetTop();
        }

        if (MenuManager.Singleton.m_liCurPopupType.Contains(EUIPopup.PopupBoxWeapon))
        {
            int t = MenuManager.Singleton.m_liCurPopupType.IndexOf(EUIPopup.PopupBoxWeapon);
            (MenuManager.Singleton.m_liCurPopup[t] as PopupBoxWeapon).SetTop();
        }

        if (MenuManager.Singleton.m_liCurPopupType.Contains(EUIPopup.PopupItemBuy))
        {
            int t = MenuManager.Singleton.m_liCurPopupType.IndexOf(EUIPopup.PopupItemBuy);
            (MenuManager.Singleton.m_liCurPopup[t] as PopupItemBuy).SetTop();
        }

        if (MenuManager.Singleton.m_liCurPopupType.Contains(EUIPopup.PopupMissionAdventure))
        {
            int t = MenuManager.Singleton.m_liCurPopupType.IndexOf(EUIPopup.PopupMissionAdventure);
            (MenuManager.Singleton.m_liCurPopup[t] as PopupMissionAdventure).UpdateUIsState();
        }

        if (MenuManager.Singleton.m_liCurPopupType.Contains(EUIPopup.PopupShopCrystal))
        {
            int t = MenuManager.Singleton.m_liCurPopupType.IndexOf(EUIPopup.PopupShopCrystal);
            (MenuManager.Singleton.m_liCurPopup[t] as PopupShopCrystal).SetTop();
        }

        if (MenuManager.Singleton.m_liCurPopupType.Contains(EUIPopup.PopupShopGameMoney))
        {
            int t = MenuManager.Singleton.m_liCurPopupType.IndexOf(EUIPopup.PopupShopGameMoney);
            (MenuManager.Singleton.m_liCurPopup[t] as PopupShopGameMoney).SetTop();
        }
    }

    public void InitializeAccountInfo()
    {
        _tLevel.text = GameManager.Singleton.user.m_nLevel.ToString();
		_txtNickname.text = GameManager.Singleton.user.m_strNickname;

        int clevel = GameManager.Singleton.user.m_nLevel;
        int cExp = GameManager.Singleton.user.m_nExp;
        int tExp = AccountLevelTable.GetData((uint)(0x01000000 + clevel)).Exp;
        int tGolden = AccountLevelTable.GetData((uint)(0x01000000 + clevel)).BonusTargetPoint;

        _tExp.text = clevel == GlobalTable.GetData<int>("levelMaxAccount") ? UIStringTable.GetValue("ui_max") : $"{cExp} / {tExp}";
        _sExpGauage.value = (float)cExp / tExp;

        _imgGoldenGauge.fillAmount = (float)GameManager.Singleton.user.m_nCurrrentBonusPoint / (float)tGolden;
    }

	public void OnClickGlobalTicket()
	{
		PopupBoxMaterial pbm = MenuManager.Singleton.OpenPopup<PopupBoxMaterial>(EUIPopup.PopupBoxMaterial, true);
		pbm.InitializeInfo(true);
	}

    public void OnClickAddGameMoney()
    {
        if ( GameManager.Singleton.user.m_nLevel >= GlobalTable.GetData<int>("valueShopOpenLevel") )
        {
            PopupShopGameMoney ga = MenuManager.Singleton.OpenPopup<PopupShopGameMoney>(EUIPopup.PopupShopGameMoney, true);
        }
        else
        {
            PopupSysMessage pop = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
            pop.InitializeInfo("ui_error_title", "ui_error_notenoughlevel_shop", "ui_common_close", null, "TutorialShop");
        }
    }

    public void OnClickAddCrystal()
    {
        if ( GameManager.Singleton.user.m_nLevel >= GlobalTable.GetData<int>("valueShopOpenLevel") )
        {
            PopupShopCrystal ct = MenuManager.Singleton.OpenPopup<PopupShopCrystal>(EUIPopup.PopupShopCrystal, true);
        }
        else
        {
            PopupSysMessage pop = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
            pop.InitializeInfo("ui_error_title", "ui_error_notenoughlevel_shop", "ui_common_close", null, "TutorialShop");
        }
    }

    public void OnClickLevel()
    {
        PopupSysMessage pop = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
        pop.InitializeInfo("ui_error_title", "ui_hint_robbery_desc", "ui_common_close", null, "TutorialRobbery");
    }

	public void OnClickNickname()
	{
		PopupInputMail m = MenuManager.Singleton.OpenPopup<PopupInputMail>(EUIPopup.PopupInputMail);
        m.InitializeInfo();
	}

	public void ReStart()
	{
		GameManager.Singleton.Restart();
	}
}

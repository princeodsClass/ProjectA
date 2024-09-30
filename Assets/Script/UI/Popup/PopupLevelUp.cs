using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupLevelUp : UIDialog
{
	[SerializeField]
	TextMeshProUGUI _txtLevel, _txtLevelUpDesc, _txtRewardTitle, _txtButtonCloseCaption, txtHPGrowup;

	[SerializeField]
	Transform _tRewardRoot;

	PageLobby _lobby;
	AccountLevelTable _tarLevel, _preLevel;
	Action _callback;

	public void InitializeInfo(PageLobby lobby, Action callback = null)
	{
		_preLevel = AccountLevelTable.GetData((uint)(0x01000000 + m_GameMgr._nPreLevel));
		_tarLevel = AccountLevelTable.GetData((uint)(0x01000000 + m_Account.m_nLevel));

		m_GameMgr._nPreLevel = m_Account.m_nLevel;
		_lobby = lobby;
		_callback = callback;

		InitializeText();
		InitializeReward();
	}

	void InitializeText()
	{
		_txtLevel.text = m_Account.m_nLevel.ToString();

		_txtLevelUpDesc.text = UIStringTable.GetValue("ui_popup_levelup_desc");
		_txtRewardTitle.text = UIStringTable.GetValue("ui_popup_levelup_reward_title");
		_txtButtonCloseCaption.text = UIStringTable.GetValue("ui_common_close");

		txtHPGrowup.text = $"+ {_tarLevel.HP - _preLevel.HP}";
	}

	void InitializeReward()
	{
		ComUtil.DestroyChildren(_tRewardRoot);

		Dictionary<uint, int> reward = new Dictionary<uint, int>();

		if (_tarLevel.RewardCharacterKey > 0)
			reward.Add(_tarLevel.RewardCharacterKey, 1);
		if (_tarLevel.RewardWeaponKey > 0)
			reward.Add(_tarLevel.RewardWeaponKey, 1);
		if (_tarLevel.RewarditemKey00 > 0 && _tarLevel.Rewarditemcount00 > 0)
			reward.Add(_tarLevel.RewarditemKey00, _tarLevel.Rewarditemcount00);
        if (_tarLevel.RewarditemKey01 > 0 && _tarLevel.Rewarditemcount01 > 0)
            reward.Add(_tarLevel.RewarditemKey01, _tarLevel.Rewarditemcount01);
        if (_tarLevel.RewarditemKey02 > 0 && _tarLevel.Rewarditemcount02 > 0)
            reward.Add(_tarLevel.RewarditemKey02, _tarLevel.Rewarditemcount02);
        if (_tarLevel.RewarditemKey03 > 0 && _tarLevel.Rewarditemcount03 > 0)
            reward.Add(_tarLevel.RewarditemKey03, _tarLevel.Rewarditemcount03);
        if (_tarLevel.RewarditemKey04 > 0 && _tarLevel.Rewarditemcount04 > 0)
            reward.Add(_tarLevel.RewarditemKey04, _tarLevel.Rewarditemcount04);
        if (_tarLevel.RewarditemKey05 > 0 && _tarLevel.Rewarditemcount05 > 0)
            reward.Add(_tarLevel.RewarditemKey05, _tarLevel.Rewarditemcount05);
        if (_tarLevel.RewarditemKey06 > 0 && _tarLevel.Rewarditemcount06 > 0)
            reward.Add(_tarLevel.RewarditemKey06, _tarLevel.Rewarditemcount06);
        if (_tarLevel.RewarditemKey07 > 0 && _tarLevel.Rewarditemcount07 > 0)
            reward.Add(_tarLevel.RewarditemKey07, _tarLevel.Rewarditemcount07);

        foreach (KeyValuePair<uint, int> re in reward)
        {
			string type = re.Key.ToString("X").Substring(0, 2);

			switch (type)
			{
				case "20":
					SlotWeapon w = m_MenuMgr.LoadComponent<SlotWeapon>(_tRewardRoot, EUIComponent.SlotWeapon);
					ItemWeapon item = new ItemWeapon(0, re.Key, 1, 0, 0, 0, false);
					w.Initialize(item);
					w.SetState(SlotWeapon.SlotState.reward);
					break;
				case "23":
					break;
				case "11":
					break;
				case "22":
					SlotMaterial m = m_MenuMgr.LoadComponent<SlotMaterial>(_tRewardRoot, EUIComponent.SlotMaterial);
					m.Initialize(re.Key, re.Value, true);
					break;
			}
		}
    }

    private void OnEnable()
    {
	}

    public void PlaySound()
    {
		GameAudioManager.PlaySFX("SFX/UI/sfx_ui_levelup_01", 0f, false, ComType.UI_MIX);
	}

	private void Awake()
	{
		Initialize();
	}

	public override void Initialize()
	{
		base.Initialize();
	}

	public override void Open()
	{
		base.Open();
	}

	public override void Escape()
	{
		return;
	}

	public override void Close()
	{
		/*
		if ( null != _callback )
        {
			_callback.Invoke();
		}
		*/

		base.Close();
	}

	public void CallbackControl()
    {
        PopupSysMessage pop;
		PopupDefault popdefault;

		if (_tarLevel.Level == GlobalTable.GetData<int>("valueRecycleOpenLevel"))
		{
			popdefault = m_MenuMgr.OpenPopup<PopupDefault>(EUIPopup.PopupDefault);
            popdefault.SetMessage("ui_hint_recycle_desc");
            popdefault.SetButtonText("ui_popup_button_experience", "ui_popup_boxreward_button_skip", OpenRecycle, _callback, "TutorialRecycle");
        }/*
		else if (_tarLevel.Level == GlobalTable.GetData<int>("valueShopBoxTutorialLevel"))
		{
			pop = m_MenuMgr.OpenPopup<PopupDefault>(EUIPopup.PopupDefault);
			t = m_ResourceMgr.LoadSprite(EAtlasType.Outgame, "tutorial_shop");
			pop.SetMessage("ui_hint_shop_desc");
			pop.SetButtonText("ui_popup_button_experience", "ui_popup_boxreward_button_skip", MoveToShop, null, t);
		}*/
		else if (_tarLevel.Level == GlobalTable.GetData<int>("valueAdventureOpenLevel"))
		{
            popdefault = m_MenuMgr.OpenPopup<PopupDefault>(EUIPopup.PopupDefault);
            popdefault.SetMessage("ui_hint_mission_desc");
            popdefault.SetButtonText("ui_popup_button_experience", "ui_popup_boxreward_button_skip", OpenMissionAdventure, _callback, "TutorialAdventure");
        }
		else if (_tarLevel.Level == GlobalTable.GetData<int>("valueHuntOpenLevel"))
		{
			pop = m_MenuMgr.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
			pop.InitializeInfo("ui_error_title", "ui_hint_hunt_desc", "ui_popup_button_confirm", null, "TutorialHunt");
		}
		else if (_tarLevel.Level == GlobalTable.GetData<int>("valueGearOpenLevel") )
		{
			pop = m_MenuMgr.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
            pop.InitializeInfo("ui_error_title", "ui_hint_workshop_desc", "ui_popup_button_experience", OpenWorkshopGear, "TutorialGear");
		}
		else if (_tarLevel.Level == GlobalTable.GetData<int>("valueMissionDefenceOpenLevel"))
        {
            popdefault = m_MenuMgr.OpenPopup<PopupDefault>(EUIPopup.PopupDefault);
            popdefault.SetMessage("ui_hint_mission_defence_desc");
            popdefault.SetButtonText("ui_popup_button_experience", "ui_popup_boxreward_button_skip", OpenDefence, _callback, "TutorialDefence");
        }
		else if (_tarLevel.Level == GlobalTable.GetData<int>("valueAbyssOpenLevel"))
		{
            popdefault = m_MenuMgr.OpenPopup<PopupDefault>(EUIPopup.PopupDefault);
            popdefault.SetMessage("ui_hint_abyss_desc");
            popdefault.SetButtonText("ui_popup_button_experience", "ui_popup_boxreward_button_skip", OpenAbyss, _callback, "TutorialAbyss");

        }
		else if (_tarLevel.Level == GlobalTable.GetData<int>("valueCharacterGuideLevel"))
		{
            pop = m_MenuMgr.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
            pop.InitializeInfo("ui_error_title", "ui_popup_upgradeguide_character_title", "ui_move", OpenAgent, "TutorialReinforceCharacter");
        }
        else if (_tarLevel.Level == GlobalTable.GetData<int>("valueMissionZombieOpenLevel"))
        {
            popdefault = m_MenuMgr.OpenPopup<PopupDefault>(EUIPopup.PopupDefault);
            popdefault.SetMessage("ui_hint_mission_zombie_desc");
            popdefault.SetButtonText("ui_popup_button_experience", "ui_popup_boxreward_button_skip", OpenMissionZombie, _callback, "TutorialInfinite");
        }
		else
		{
            if (null != _callback)
            {
                _callback.Invoke();
				_callback = null;
            }
        }
    }

	void MoveToInventory()
	{
		FindObjectOfType<PageLobby>().OnButtonMenuClick(1);
	}

	void MoveToMession()
    {
        FindObjectOfType<PageLobby>().OnButtonMenuClick(3);
    }

	void MoveToCommunity()
    {
        FindObjectOfType<PageLobby>().OnButtonMenuClick(4);
    }

	void OpenAgent()
	{
        GameObject menu = GameObject.Find("InvenDisableBtn");
        Rect rt = menu.GetComponent<RectTransform>().rect;
        m_Tutorial.SetFinger(menu,
                             GameObject.Find("PageLobby").GetComponent<PageLobby>().OnButtonMenuClickCharacterTutorial,
                             rt.width, rt.height);
    }

	void OpenRecycle()
    {
		GameObject menu = GameObject.Find("InvenDisableBtn");
		Rect rt = menu.GetComponent<RectTransform>().rect;
		m_Tutorial.SetFinger(menu,
							 GameObject.Find("PageLobby").GetComponent<PageLobby>().OnButtonMenuClickRecycleTutorial,
							 rt.width, rt.height);
	}

	void MoveToShop()
	{
		GameObject menu = GameObject.Find("ShopDisableBtn");
		Rect rt = menu.GetComponent<RectTransform>().rect;
		m_Tutorial.SetFinger(menu,
							 GameObject.Find("PageLobby").GetComponent<PageLobby>().OnButtonMenuClickShopTutorial,
							 rt.width, rt.height);
		//GameObject.Find("PageLobby").GetComponent<PageLobby>().OnButtonMenuClick(0);
	}

	void OpenHunt()
	{
		PopupHunt hunt = m_MenuMgr.OpenPopup<PopupHunt>(EUIPopup.PopupHunt);
		hunt.InitializeInfo(GameManager.Singleton.user.m_nHuntLevel);
	}

	void OpenWorkshop()
    {
		MoveToInventory();
		GameObject.Find("InventoryPage").GetComponent<PageLobbyInventory>().OnClickWorkshop();
	}

	void OpenWorkshopGear()
	{
		GameObject menu = GameObject.Find("InvenDisableBtn");
		Rect rt = menu.GetComponent<RectTransform>().rect;
		m_Tutorial.SetFinger(menu,
							 GameObject.Find("PageLobby").GetComponent<PageLobby>().OnButtonMenuClickWorkshopGearTutorial,
							 rt.width, rt.height);
	}

	void OpenMissionZombie()
	{
        GameObject menu = GameObject.Find("MissionDisableBtn");
        Rect rt = menu.GetComponent<RectTransform>().rect;
        m_Tutorial.SetFinger(menu,
                             GameObject.Find("PageLobby").GetComponent<PageLobby>().OnButtonMenuClickZombieTutorial,
                             rt.width, rt.height);
    }

	void OpenMissionAdventure()
    {
        GameObject menu = GameObject.Find("AdventureButton");
		Rect rt = menu.GetComponent<RectTransform>().rect;
		m_Tutorial.SetMessage("ui_tip_adventure_00", 0);
        m_Tutorial.SetFinger(menu,
							 GameObject.Find("BattlePage").GetComponent<PageLobbyBattle>().OnClickAdventure,
							 rt.width, rt.height);
	}

	void OpenAbyss()
    {
		GameObject menu = GameObject.Find("CommunityDisableBtn");
		Rect rt = menu.GetComponent<RectTransform>().rect;
		m_Tutorial.SetFinger(menu,
							 GameObject.Find("PageLobby").GetComponent<PageLobby>().OnButtonMenuClickAbyssTutorial,
							 rt.width, rt.height);
	}

	void OpenDefence()
    {
		GameObject menu = GameObject.Find("MissionDisableBtn");
		Rect rt = menu.GetComponent<RectTransform>().rect;
		m_Tutorial.SetFinger(menu,
							 GameObject.Find("PageLobby").GetComponent<PageLobby>().OnButtonMenuClickDefenceTutorial,
							 rt.width, rt.height);
	}

	#region 추가
	/** 보너스 전투로 이동한다 */
	private void MoveToBonusBattle()
	{
		var oHandler = new PopupLoadingHandlerBattleReady();
		oHandler.Init(PopupLoadingHandlerBattleReady.MakeParams(EMapInfoType.BONUS, new STIDInfo(0, 0, 0)));

		var oLoadingPopup = m_MenuMgr.OpenPopup<PopupLoading>(EUIPopup.PopupLoading);
		oLoadingPopup.Init(PopupLoading.MakeParams(PopupLoading.ELoadingType.BATTLE_READY, oHandler, (a_oSender) => this.OnCompleteLoadingBattleReady(a_oSender, 1, 1)));
	}

	/** 전투 준비 로딩이 완료 되었을 경우 */
	private void OnCompleteLoadingBattleReady(PopupLoading a_oSender, int a_nEpisode, int a_nChapter)
	{
		var oMapInfoDict = CMapInfoTable.Singleton.LoadMapInfosDownloaded(EMapInfoType.BONUS, a_nChapter - 1, a_nEpisode - 1);

		m_DataMgr.SetPlayStageID(0);
		m_DataMgr.SetContinueTimes(0);
		m_DataMgr.SetupPlayMapInfo(EMapInfoType.BONUS, EPlayMode.BONUS, oMapInfoDict);

		m_DataMgr.ExLateCallFunc((a_oFuncSender) =>
		{
			m_MenuMgr.SceneEnd();
			m_MenuMgr.SceneNext(ESceneType.Battle);
		}, ComType.G_DURATION_LOADING_PROGRESS_ANI);
	}
	#endregion // 추가
}

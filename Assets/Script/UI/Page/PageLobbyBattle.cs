using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PageLobbyBattle : MonoBehaviour
{
	[SerializeField]
	PageLobby _pageLobby;

	[SerializeField]
	RectTransform[] _BoxPosition;

	[SerializeField]
	TextMeshProUGUI _BattlePassTitle, _ButtonHuntCaption, _ButtonCampaignCaption, _ButtonQuestCaption,
					_txtButtonAdventureCaption, _txtButtonAdventureLimitLevel, _txtButtonAdventureRemainTime,
                    _txtEpisodeName, _txtButtonHuntLimitLevel, _txtButtonQuestLimitLevel, _txtEpisodeInfo,
					_txtStageName;

	[SerializeField]
	GameObject _goStage, _goRootMiniMap, _goRootChapterInfo, _goCampaignEnd,
			   _goButtonCampaign, _goButtonHunt, _goButtonHuntCover, _goButtonAdventure, _goButtonAdventureCover,
               _goButtonQuest, _goButtonQuestCover,
			   _goBattlePassButton, _goAttendanceButton,
			   _goQuestNewTag, _goBonusButton;

	[SerializeField]
	TextMeshProUGUI _txtCampaignEndTitle;

	[SerializeField]
	Image _imgAdventureTopReward;

	GameManager m_GameMgr = null;
	MenuManager m_MenuMgr = null;
	GameResourceManager m_ResourceMgr = null;
	GameDataManager m_DataMgr = null;

	UserAccount m_Account = null;
	InventoryData<ItemBox> m_InvenBox = null;

	GameObject _goMiniMap;
	EpisodeTable _curEpisode;
	ChapterTable _curChapter;

	string D, h, m, s;

    /// <summary>
    /// 다른데서 간단하게 처리할 수 있게 static 로 처리하자.
    /// 어짜피 박스 인벤은 하나다.
    /// </summary>
    public Dictionary<long, GameObject> _dicBox { get; set; } = new Dictionary<long, GameObject>();
	public int itest;

	public bool _isBoxNew = false;
	public bool _isExistCompleteQuest = false;

	private void Awake()
	{
		SetManager();
		InitializeText();
		CheckBoxState();
		CheckQuestState();

		if ( PlayerPrefs.GetString("isNew") == "First" )
        {
			/*
			PopupDefault pop = m_MenuMgr.OpenPopup<PopupDefault>(EUIPopup.PopupDefault);
            pop.SetTitle("ui_error_title");
            pop.SetMessage("ui_hint_new_desc");
            pop.SetButtonText("ui_start", "ui_popup_button_explore", StartCampaign, null, "TutorialNew");
			*/

			PopupSysMessage pop = m_MenuMgr.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
			pop.InitializeInfo("ui_error_title", "ui_hint_new_desc", "ui_start", StartCampaignForNewbie, "TutorialNew");

            PlayerPrefs.SetString("isNew", "Old");
        }

		int bp = GlobalTable.GetData<int>("valueBattlePassOpenEpisode");
		_goBattlePassButton.SetActive((m_Account.CorrectEpisode + 1) >= bp);
		// _goAttendanceButton.SetActive(m_Account.m_nAttendanceContinuouslyCount < AttendanceTable.GetList(1).Count);
		_goBonusButton.SetActive(m_Account.m_bIsBonusTime);

		SetAttendanceButtonTag();
	}

	void StartTutorial()
	{
		GameObject btn = GameObject.Find("ScenarioBtn");
		RectTransform rt = btn.GetComponent<RectTransform>();
		GameManager.Singleton.tutorial.SetFinger(btn, OnClickCampaign, rt.rect.width, rt.rect.height);
	}

	void SetManager()
    {
		m_GameMgr = GameManager.Singleton;
		m_MenuMgr = MenuManager.Singleton;
		m_ResourceMgr = GameResourceManager.Singleton;
		m_DataMgr = GameDataManager.Singleton;

		m_Account = m_GameMgr.user;
		m_InvenBox = m_GameMgr.invenBox;
	}

	public void SetAttendanceButtonTag()
    {
		ButtonAttendance button = _goAttendanceButton.GetComponent<ButtonAttendance>();

        button.Initialize();

		if ( m_Account.m_bIsAbleToAttendanceReward && _goAttendanceButton.activeSelf )
        {
            button.SetNewTag(true);

			if ( !m_GameMgr._isAttendanceShow ) // && GameManager.Singleton.user.CorrectEpisode > 0 )
            {
				button.OnClick();
				m_GameMgr._isAttendanceShow = true;
			}			
		}
		else
        {
            button.SetNewTag(false);
		}
		
		SetNewTag();
	}

	void InitializeText()
	{
		_BattlePassTitle.text = UIStringTable.GetValue("ui_page_lobby_battle_pass_title");
		_ButtonHuntCaption.text = UIStringTable.GetValue("ui_page_lobby_battle_button_hunt");
		_ButtonCampaignCaption.text = UIStringTable.GetValue("ui_page_lobby_battle_button_campaign");
		_ButtonQuestCaption.text = UIStringTable.GetValue("ui_popup_quest_title");
		_txtButtonAdventureCaption.text = UIStringTable.GetValue("ui_mission_adventure");

        _txtEpisodeInfo.text = UIStringTable.GetValue("ui_button_episode_roadmap");

		for (int i = 0; i < _BoxPosition.Length; i++)
			_BoxPosition[i].GetComponentInChildren<TextMeshProUGUI>().text = UIStringTable.GetValue("ui_page_lobby_battle_slot_box");

        D = $"<size=70%>{UIStringTable.GetValue("ui_day")}</size>";
        h = $"<size=70%>{UIStringTable.GetValue("ui_hour")}</size>";
        m = $"<size=70%>{UIStringTable.GetValue("ui_minute")}</size>";
        s = $"<size=70%>{UIStringTable.GetValue("ui_second")}</size>";
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnEnable()
	{
        _goButtonAdventure.SetActive(false);

        Initialize();
	}

	void Initialize()
	{
		InitializeEpisode();
		InitializeBox();
		InitializeHunt();
		//InitializeAdventure();
        InitializeQuest();
	}

	void InitializeHunt()
    {
		// _goButtonHunt.SetActive(m_Account.m_nLevel >= GlobalTable.GetData<int>("valueHuntOpenLevel"));
		_goButtonHuntCover.SetActive(m_Account.m_nLevel < GlobalTable.GetData<int>("valueHuntOpenLevel"));
		_txtButtonHuntLimitLevel.text = GlobalTable.GetData<int>("valueHuntOpenLevel").ToString();
	}

    public void InitializeAdventure()
    {
		StopCoroutine("CoAdventureTimer");
        List<MissionAdventureTable> tList = MissionAdventureTable.GetGroup(GameManager.Singleton.user.m_nAdventureGroup);

        _goButtonAdventure.SetActive(m_Account.m_nLevel >= GlobalTable.GetData<int>("valueAdventureOpenLevel") &&
                                     m_Account.m_nAdventureLevel < tList[tList.Count - 1].Order );
        _goButtonAdventureCover.SetActive(m_Account.m_nLevel < GlobalTable.GetData<int>("valueAdventureOpenLevel"));
        _txtButtonAdventureLimitLevel.text = GlobalTable.GetData<int>("valueAdventureOpenLevel").ToString();

		if ( 0 != m_Account.m_nAdventureGroup )
		{
			_imgAdventureTopReward.gameObject.SetActive(true);
            _imgAdventureTopReward.sprite = ComUtil.GetIcon(tList[tList.Count - 1].RewardMain);

            StartCoroutine(CoAdventureTimer());
        }
		else
		{
            _imgAdventureTopReward.gameObject.SetActive(false);
        }
    }

    void InitializeQuest()
    {
		_goButtonQuest.SetActive(m_Account.m_nLevel >= GlobalTable.GetData<int>("valueQuestOpenLevel"));
		_txtButtonQuestLimitLevel.text = GlobalTable.GetData<int>("valueQuestOpenLevel").ToString();
		
		if ( m_Account.m_nLevel < GlobalTable.GetData<int>("valueQuestOpenLevel") )
			_goButtonQuestCover.SetActive(true);
		else
        {
			_goButtonQuestCover.SetActive(false);
			_goButtonQuest.GetComponent<QuestButton>().InitializeInfo();
		}
	}

	IEnumerator CoAdventureTimer()
	{
        int nResetCycle = GlobalTable.GetData<int>(ComType.G_TIME_ADVENTURE_RESET);

        DateTime dtFinishTime = m_Account.m_dtStartAdventure.AddMilliseconds(nResetCycle);
        TimeSpan dtRemainTime = dtFinishTime - DateTime.UtcNow;

		WaitForSecondsRealtime second = new WaitForSecondsRealtime(1.0f);
        WaitForSecondsRealtime minute = new WaitForSecondsRealtime(60.0f);
        WaitForSecondsRealtime hour = new WaitForSecondsRealtime(3600.0f);

        while ( 0 < dtRemainTime.TotalSeconds )
		{
            dtRemainTime = dtFinishTime - DateTime.UtcNow;

			_txtButtonAdventureRemainTime.text = "";

            if (dtRemainTime.TotalDays > 1)
            {
                _txtButtonAdventureRemainTime.text = $"{dtRemainTime.Days}{D} ";
                _txtButtonAdventureRemainTime.text += (dtRemainTime.Hours > 0 ? $"{dtRemainTime.Hours}{h}" : "");

                yield return hour;
            }
            else if (dtRemainTime.TotalHours > 1)
            {
                _txtButtonAdventureRemainTime.text = $"{dtRemainTime.Hours}{h} ";
                _txtButtonAdventureRemainTime.text += (dtRemainTime.Minutes > 0 ? $"{dtRemainTime.Minutes}{m}" : "");

                yield return minute;
            }
            else if (dtRemainTime.TotalSeconds > 1)
            {
                _txtButtonAdventureRemainTime.text += (dtRemainTime.Minutes > 0 ? $"{dtRemainTime.Minutes}{m} " : "");
                _txtButtonAdventureRemainTime.text += (dtRemainTime.Seconds > 0 ? $"{dtRemainTime.Seconds}{s}" : "");

                yield return second;
            }
        }

		_pageLobby.InitializeAdventure();

        yield break;
	}

	void InitializeEpisode()
	{
		uint epKey = ComUtil.GetEpisodeKey(m_GameMgr.user.CorrectEpisode + 1);

		_curEpisode = EpisodeTable.IsContainsKey(epKey) ? EpisodeTable.GetData(epKey) : null;

		ComUtil.DestroyChildren(_goRootMiniMap.transform, false);

		if (null != _curEpisode)
		{
			_goStage.SetActive(true);
			_goCampaignEnd.SetActive(false);
			_txtEpisodeName.text = $"[ EP. {_curEpisode.Order} ] " + NameTable.GetValue(_curEpisode.NameKey);
			_txtStageName.text = $"Stage : {m_Account.m_nStage} / {StageTable.GetMax(m_Account.m_nEpisode + 1, 1)}";

			string n = _curEpisode.PrefebMini;
			_goMiniMap = m_ResourceMgr.CreateObject(EResourceType.ObjectLevel, n, _goRootMiniMap.transform);

			// InitializeChapter();
		}
		else
        {
			_goStage.SetActive(false);
			_goCampaignEnd.SetActive(true);
			_goButtonCampaign.SetActive(false);

			_txtCampaignEndTitle.text = UIStringTable.GetValue("ui_page_lobby_battle_campaignend_title");
		}
	}

	void InitializeChapter()
	{
		uint chapterKey = ComUtil.GetChapterKey(m_GameMgr.user.CorrectEpisode + 1, m_GameMgr.user.CorrectChapter + 1);

		_curChapter = ChapterTable.GetData(chapterKey);

		ComUtil.DestroyChildren(_goRootChapterInfo.transform, false);
		int nBonusNPSAppearEpisode = GlobalTable.GetData<int>(ComType.G_VALUE_SAMI_BOSS_START_EPISODE) - 1;

#if !DISABLE_THIS
		bool bIsEnableMidBossIcon = m_GameMgr.user.CorrectEpisode >= nBonusNPSAppearEpisode && m_GameMgr.user.CorrectChapter <= 3;
#else
		bool bIsEnableMidBossIcon = m_GameMgr.user.CorrectChapter <= 3;
#endif // #if DISABLE_THIS

		for (int i = 0; i < _curEpisode.MaxChapter; i++)
		{
			SlotChapterBlock slotChapterBlock = m_MenuMgr.LoadComponent<SlotChapterBlock>(_goRootChapterInfo.transform, EUIComponent.SlotChapterBlock); ;
			slotChapterBlock._goComplete.SetActive(i < m_GameMgr.user.CorrectChapter);
			slotChapterBlock._goCurrent.SetActive(i == m_GameMgr.user.CorrectChapter);

#if !DISABLE_THIS
            slotChapterBlock._goBoss.SetActive(i == 7);
            slotChapterBlock._goMidBoss.SetActive(i == 3);
            // slotChapterBlock._goBoss.SetActive(i == _curEpisode.MaxChapter - 1);
            // slotChapterBlock._goMidBoss.SetActive(i == ( _curEpisode.MaxChapter / 2 - 1 ) && bIsEnableMidBossIcon);
#else
			slotChapterBlock._goBoss.SetActive(i == _curEpisode.MaxChapter - 1);
			slotChapterBlock._goMidBoss.SetActive(i == ( _curEpisode.MaxChapter / 2 - 1 ) && bIsEnableMidBossIcon);
#endif // #if DISABLE_THIS
        }
	}

	/// <summary>
	/// 박스 전체 지우고 초기화
	/// </summary>
	/// <param name="isNew"></param>
	public void InitializeBox()
	{
		DeleteBox();

		SlotBox tbox;

		if (0 < m_InvenBox.GetItemCount())
		{
			foreach (ItemBox box in m_InvenBox)
			{
				CheckBoxState();
				tbox = AddBox(box);
				tbox.SetNewTag(_isBoxNew);
				_dicBox.Add(box.id, tbox.gameObject);
			}
		}
	}

	public void CheckBoxState()
	{
		_isBoxNew = m_GameMgr.IsExistOpeningBox() == -1 &&
				    m_GameMgr.IsExistReadyBox() >= 0;
		
		SetNewTag();
	}

	public void CheckQuestState()
    {
		QuestTable tar;
		float curPoint = 0;
		float totalPoint = 0;
		_isExistCompleteQuest = false;
		_goQuestNewTag.SetActive(_isExistCompleteQuest);

		for ( int i = 0; i < m_Account.m_nQuestKey.Length; i++ )
        {
			if ( QuestTable.IsContainsKey(m_Account.m_nQuestKey[i]) )
            {
				tar = QuestTable.GetData(m_Account.m_nQuestKey[i]);

				if (m_Account.m_nQuestCount[i] == tar.RequireCount &&
					m_Account.m_bQuestIsComplete[i] == false &&
					( i < 4 ? true : m_Account.IsVIP() ))
				{
					_isExistCompleteQuest = true;
					_goQuestNewTag.SetActive(_isExistCompleteQuest);

					SetNewTag();
					StartCoroutine(m_GameMgr.questCard.Activate());
					
					return;
				}

				if (m_Account.m_nQuestCount[i] == tar.RequireCount && true == m_Account.m_bQuestIsComplete[i])
					curPoint += ( tar.Point * ( i < 4 ? 1.0f : GlobalTable.GetData<float>("ratioVIPQuestPoint") ) );

				totalPoint += tar.Point;
			}
        }

		if ( m_Account.m_nQuestKey[0] != 0 )
        {
			if ( ((curPoint >= totalPoint * 0.5f - float.Epsilon) && !m_Account.m_bIsRecieveQuestAccouuntRewardsMiddle) ||
				 ((curPoint >= totalPoint - float.Epsilon) && !m_Account.m_bIsRecieveQuestAccouuntRewardsFull))
				_isExistCompleteQuest = true;
			else
				_isExistCompleteQuest = false;

			_goQuestNewTag.SetActive(_isExistCompleteQuest);
			SetNewTag();
		}
	}

	void SetNewTag()
	{
		_pageLobby.SetNewTag(2, _isBoxNew || _isExistCompleteQuest ||
								( m_Account.m_bIsAbleToAttendanceReward && _goAttendanceButton.activeSelf ) ||
							    ( _goBattlePassButton.GetComponent<ButtonBattlePass>().IsNew()  &&
									( m_Account.CorrectEpisode + 1 ) >= GlobalTable.GetData<int>("valueBattlePassOpenEpisode")));
	}

	/// <summary>
	/// 박스 하나만 초기화
	/// </summary>
	/// <param name="box"></param>
	/// <param name="isNew"></param>
	public void InitializeBox(ItemBox box, bool isNew = false)
	{
		DeleteBox(box.id);

		SlotBox tbox;
		tbox = AddBox(box);

		_dicBox.Add(box.id, tbox.gameObject);
	}

	public void DeleteBox(long id = 0)
	{
		if (id == 0)
		{
			foreach (KeyValuePair<long, GameObject> box in _dicBox)
			{
				Destroy(box.Value);
			}

			_dicBox.Clear();
		}
		else
		{
			GameObject temp = null;

			foreach (KeyValuePair<long, GameObject> box in _dicBox)
			{
				if (id == box.Key)
				{
					_dicBox.Remove(id);
					temp = box.Value;
					break;
				}
			}

			if (temp != null) Destroy(temp);
		}
	}

	public SlotBox AddBox(ItemBox box, bool isNew = false)
	{
		SlotBox slotBox = m_MenuMgr.LoadComponent<SlotBox>(_BoxPosition[box.nSlotNumber], EUIComponent.SlotBox);

		//slotBox.Open();
		slotBox.Initialize(box);

		return slotBox;
	}

	public void OnClickCampaign(bool isTutorial = false)
	{
		if ( m_GameMgr.invenWeapon.GetItemCount() < m_GameMgr.user.m_nMaxWeaponRepository)
        {
			if ( !IsEmptyGear() ) StartCampaign();
		}
		else
        {
			PopupRepositorFull fu = m_MenuMgr.OpenPopup<PopupRepositorFull>(EUIPopup.PopupRepositorFull);			
		}
	}

	public void OnClickQuest()
    {
		if (_goButtonQuestCover.activeSelf)
		{
			PopupSysMessage pop = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
			pop.InitializeInfo("ui_error_title", "ui_error_notenoughlevel_quest", "ui_common_close", null);
		}
		else
		{
			PopupQuest quest = m_MenuMgr.OpenPopup<PopupQuest>(EUIPopup.PopupQuest);
		}
	}

	bool IsEmptyGear()
    {
		List<EGearType> emptyGear = new List<EGearType>();

		if ( m_Account.m_nLevel >= GlobalTable.GetData<int>("valueGearOpenLevel") )
        {
			if ( m_Account.m_nLevel >= GlobalTable.GetData<int>("valueHeadOpenLevel") )
				if ( m_Account.m_nGearID[0] == 0 ) emptyGear.Add(EGearType.head);
			if ( m_Account.m_nLevel >= GlobalTable.GetData<int>("valueUpperOpenLevel") )
				if ( m_Account.m_nGearID[1] == 0 ) emptyGear.Add(EGearType.upper);
			if ( m_Account.m_nLevel >= GlobalTable.GetData<int>("valueHandsOpenLevel") )
				if ( m_Account.m_nGearID[2] == 0 ) emptyGear.Add(EGearType.hands);
			if ( m_Account.m_nLevel >= GlobalTable.GetData<int>("valueLowerOpenLevel") )
				if ( m_Account.m_nGearID[3] == 0 ) emptyGear.Add(EGearType.lower);
        }

		if ( emptyGear.Count > 0 && false == m_GameMgr._isShowAlertGear)
        {
			string msg = string.Empty;
			emptyGear.ForEach(part => msg += $"{UIStringTable.GetValue($"ui_gear_{part.ToString()}")} \n");
			msg += UIStringTable.GetValue("ui_error_empty_gear");

			PopupDefault check = m_MenuMgr.OpenPopup<PopupDefault>(EUIPopup.PopupDefault);
			check.SetTitle("ui_error_title");
			check.SetMessage(msg);
			check.SetButtonText("ui_page_lobby_battle_campaign_start", "ui_popup_upgradeguide_button_caption",
				                StartCampaign, MoveToInventory, "TutorialGearSlot");
			m_GameMgr._isShowAlertGear = true;

			return true;
		}
		else
			return false;
    }

    void StartCampaignForNewbie()
    {
        var oHandler = new PopupLoadingHandlerBattleReady();
        oHandler.Init(PopupLoadingHandlerBattleReady.MakeParams(EMapInfoType.TUTORIAL, new STIDInfo(0, 0, 0)));

        var oLoadingPopup = m_MenuMgr.OpenPopup<PopupLoading>(EUIPopup.PopupLoading);
        oLoadingPopup.Init(PopupLoading.MakeParams(PopupLoading.ELoadingType.BATTLE_READY, oHandler, this.OnCompleteLoadingBattleReadyForNewbie));
    }

    void StartCampaign()
    {
		var oHandler = new PopupLoadingHandlerBattleReady();
		oHandler.Init(PopupLoadingHandlerBattleReady.MakeParams(EMapInfoType.CAMPAIGN, new STIDInfo(0, m_Account.CorrectChapter, m_Account.CorrectEpisode)));

		var oLoadingPopup = m_MenuMgr.OpenPopup<PopupLoading>(EUIPopup.PopupLoading);
		oLoadingPopup.Init(PopupLoading.MakeParams(PopupLoading.ELoadingType.BATTLE_READY, oHandler, this.OnCompleteLoadingBattleReady));
	}

	public void MoveToInventory()
    {
		GameObject.Find("PageLobby").GetComponent<PageLobby>().OnButtonMenuClick(1);
	}

	public void OnClickHunt()
	{
		if (_goButtonHuntCover.activeSelf)
        {
			PopupSysMessage pop = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
			pop.InitializeInfo("ui_error_title", "ui_error_notenoughlevel_hunt", "ui_common_close", null, "TutorialHunt");
		}
		else
        {
			PopupHunt hunt = m_MenuMgr.OpenPopup<PopupHunt>(EUIPopup.PopupHunt);
			hunt.InitializeInfo(m_Account.m_nHuntLevel);
		}		
	}

	public void OnClickAdventure(bool isTutorial)
	{
        if (_goButtonAdventureCover.activeSelf)
        {
            PopupSysMessage pop = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
            pop.InitializeInfo("ui_error_title", "ui_hint_mission_desc_2", "ui_common_close", null, "TutorialAdventure");
            pop.AddMessage("ui_error_notenoughlevel_adventure", 2);
        }
        else
        {
            List<MissionAdventureTable> tList = MissionAdventureTable.GetGroup(GameManager.Singleton.user.m_nAdventureGroup);

            if (GameManager.Singleton.user.m_nAdventureLevel >= tList.Count)
            {
                PopupSysMessage.ShowMissionAdventureClearMsg();
            }
            else
            {
				_pageLobby._Page[3].GetComponent<PageLobbyMission>()?.ShowMissionAdventurePopup(false);

#if DISABLE_THIS
				// 기존 구문
                PopupMissionAdventure adPop = MenuManager.Singleton.OpenPopup<PopupMissionAdventure>(EUIPopup.PopupMissionAdventure);
                adPop.Init(PopupMissionAdventure.MakeParams(GameManager.Singleton.user.m_nAdventureGroup - 1, false));
#endif // #if DISABLE_THIS
            }
        }
    }

    private void OnCompleteLoadingBattleReadyForNewbie(PopupLoading a_oSender)
    {
        var oMapInfoDict = CMapInfoTable.Singleton.LoadMapInfosDownloaded(EMapInfoType.TUTORIAL, 0, 0);

        m_DataMgr.SetPlayStageID(0);
        m_DataMgr.SetContinueTimes(0);

#if DISABLE_THIS
		m_DataMgr.SetupPlayMapInfo(EMapInfoType.CAMPAIGN, EPlayMode.CAMPAIGN, oMapInfoDict, m_Account.CorrectChapter == 3);
#else
        m_DataMgr.SetupPlayMapInfo(EMapInfoType.TUTORIAL, EPlayMode.TUTORIAL, oMapInfoDict);
#endif // #if DISABLE_THIS

        this.ExLateCallFunc((a_oFuncSender) =>
        {
            m_MenuMgr.SceneEnd();
            m_MenuMgr.SceneNext(ESceneType.Battle);
        }, ComType.G_DURATION_LOADING_PROGRESS_ANI);
    }

    /** 전투 준비 로딩이 완료 되었을 경우 */
    private void OnCompleteLoadingBattleReady(PopupLoading a_oSender)
	{
		var oMapInfoDict = CMapInfoTable.Singleton.LoadMapInfosDownloaded(EMapInfoType.CAMPAIGN,
			m_Account.CorrectChapter, m_Account.CorrectEpisode);

		m_DataMgr.SetPlayStageID(0);
		m_DataMgr.SetContinueTimes(0);

#if DISABLE_THIS
		m_DataMgr.SetupPlayMapInfo(EMapInfoType.CAMPAIGN, EPlayMode.CAMPAIGN, oMapInfoDict, m_Account.CorrectChapter == 3);
#else
		m_DataMgr.SetupPlayMapInfo(EMapInfoType.CAMPAIGN, EPlayMode.CAMPAIGN, oMapInfoDict);
#endif // #if DISABLE_THIS

		this.ExLateCallFunc((a_oFuncSender) =>
		{
			m_MenuMgr.SceneEnd();
			m_MenuMgr.SceneNext(ESceneType.Battle);
		}, ComType.G_DURATION_LOADING_PROGRESS_ANI);
	}

	public IEnumerator BattleButtonShake()
    {
		if (_goButtonCampaign.activeSelf) _goButtonCampaign.GetComponent<Animator>().SetBool("IsReadyChange", true);
		if (_goButtonHunt.activeSelf && !_goButtonHuntCover.activeSelf)
			_goButtonHunt.GetComponent<Animator>().SetBool("IsReadyChange", true);

		yield return new WaitForSecondsRealtime(2.0f);

		if (m_MenuMgr.CurScene != ESceneType.Lobby) yield break;

		if (_goButtonCampaign.activeSelf )
		{
			_goButtonCampaign.GetComponent<Animator>().SetBool("IsReadyChange", false);
			_goButtonCampaign.GetComponent<Animator>().SetTrigger("Selected");
		}

		if (_goButtonHunt.activeSelf)
        {
			_goButtonHunt.GetComponent<Animator>().SetBool("IsReadyChange", false);
			_goButtonHunt.GetComponent<Animator>().SetTrigger("Selected");
		}
	}

	public void OpenOption()
	{
		PopupOption option = m_MenuMgr.OpenPopup<PopupOption>(EUIPopup.PopupOption);
	}

	public void OpenNotice()
	{
		PopupNotice notice = m_MenuMgr.OpenPopup<PopupNotice>(EUIPopup.PopupNotice);
	}
}

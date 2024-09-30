using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PageLobby : UIDialog
{
	[SerializeField]
	private TMP_Text m_oNumGlobalTicketsText = null;

	[SerializeField]
	private Image m_oAgentNewTagImg = null;

	[SerializeField]
	private PageLobbyBattle m_oLobbyBattle = null;

	[SerializeField]
	private PageLobbyInventory m_oLobbyInventory = null;

	[SerializeField]
	private PageLobbyMission m_oLobbyMission = null;

	[SerializeField]
	public GameObject[] _Page;

	[SerializeField]
	SoundButton[] _ButtonMenuSelect;

	[SerializeField]
	GameObject[] _ButtonActiveCaption;

	[SerializeField]
	GameObject[] _ButtonActiveIcon;

	[SerializeField]
	GameObject[] _ButtonNewTag, _ButtonNewTagBG;

	[SerializeField]
	RectTransform _ButtonActiveBGRectTransform;

	[SerializeField]
	RectTransform _LobbyPageRectTransform;

	[SerializeField]
	GameObject _CharacterRoot;

	[SerializeField]
	GameObject _goShopLock, _goVIPButton, _goShopCover,
			   _goInventoryLock, _goInventoryCover,
			   _goMissionLock, _goMissionCover,
			   _goCommunityLock, _goCommunityCover,
			   _goCharacterCam, _goMinimapCam;

	[SerializeField]
	TextMeshProUGUI _txtShopOpen, _txtShopLevel,
					_txtInventoryOpen, _txtInventoryLevel,
					_txtMissionOpen, _txtMissionLevel,
					_txtCommunityOpen, _txtCommunityLevel;

	Vector3 _TargetPagePosition, _TargetCameraPosition, _TargetButtonActiveBGPosition;
	int _CurrentMenuIndex = 2;

	Sprite t;

	void Awake()
	{
		base.Initialize();
		Physics.gravity = new Vector3(0.0f, -GlobalTable.GetData<float>(ComType.G_VALUE_GRAVITY_FOR_GRENADE), 0.0f);

		CheckLevel();
		CheckBox();

		InitializeText();

		_ButtonActiveCaption[_CurrentMenuIndex].gameObject.SetActive(true);

		EventSystem eventSystem = FindObjectOfType<EventSystem>();
		eventSystem.pixelDragThreshold = GlobalTable.GetData<int>("valueUIMultyScroolSensitivity");
	}

	void InitializeText()
	{
		_ButtonActiveCaption[0].GetComponent<TextMeshProUGUI>().text = UIStringTable.GetValue("ui_page_lobby_maintab_caption_shop");
		_ButtonActiveCaption[1].GetComponent<TextMeshProUGUI>().text = UIStringTable.GetValue("ui_page_lobby_maintab_caption_inventory");
		_ButtonActiveCaption[2].GetComponent<TextMeshProUGUI>().text = UIStringTable.GetValue("ui_page_lobby_maintab_caption_battle");
		_ButtonActiveCaption[3].GetComponent<TextMeshProUGUI>().text = UIStringTable.GetValue("ui_page_lobby_maintab_caption_mission");
		_ButtonActiveCaption[4].GetComponent<TextMeshProUGUI>().text = UIStringTable.GetValue("ui_page_lobby_maintab_caption_cummunity");
	}

	private void Start()
	{
		m_GameMgr._gameState = GameManager.GameState.lobby;

        GameAudioManager.PlayBGM(ESceneType.Lobby);

        this.ExLateCallFunc((a_oSender) =>
		{
			// 탐험 이동 상태 일 경우
			if (MenuManager.Singleton.IsMoveToMissionAdventure)
			{
#if DISABLE_THIS
				this.OnButtonMenuClick(3);
#endif // #if DISABLE_THIS

				_Page[3].GetComponent<PageLobbyMission>().ShowMissionAdventurePopup(MenuManager.Singleton.IsClearMissionAdventure);
			}
			// 심연 이동 상태 일 경우
			else if (MenuManager.Singleton.IsMoveToAbyss)
			{
				this.OnButtonMenuClick(4);
				_Page[4].GetComponentInChildren<ComCommunityAbyss>()?.OnClick();
			}
			// 방어전 이동 상태 일 경우
			else if (MenuManager.Singleton.IsMoveToMissionDefence)
			{
				this.OnButtonMenuClick(3);
				_Page[3].GetComponent<PageLobbyMission>().ComMissionDefence.OnClick();
			}
			// 인벤토리 이동 상태 일 경우
			else if (MenuManager.Singleton.IsMoveToInventory)
			{
				this.OnButtonMenuClick(1);
			}

			MenuManager.Singleton.IsMoveToAbyss = false;
			MenuManager.Singleton.IsMoveToInventory = false;
			MenuManager.Singleton.IsMoveToMissionDefence = false;
			MenuManager.Singleton.IsClearMissionAdventure = false;
			MenuManager.Singleton.IsMoveToMissionAdventure = false;
		});

		#region 추가
		StopCoroutine("CoUpdateAgentUIsState");
		StartCoroutine(this.CoUpdateAgentUIsState());

		StopCoroutine("CoUpdateMissionUIsState");
		StartCoroutine(this.CoUpdateMissionUIsState());

		if (m_Account.m_nLevel < GlobalTable.GetData<int>("valueAdventureOpenLevel"))
			return;

		StopCoroutine("CoUpdateTimeUIsState");
		StartCoroutine(this.CoUpdateTimeUIsState());

        // 탐험 시작 상태 일 경우
        if (GameManager.Singleton.IsEnableStartMissionAdventure())
            StartCoroutine(this.CoStartMissionAdventure());
        else
            m_oLobbyBattle?.InitializeAdventure();

        #endregion // 추가
    }

	public void InitializeAdventure()
	{
		Start();
	}

	void CheckLevel()
	{
		int shop = GlobalTable.GetData<int>("valueShopOpenLevel");

		_goShopLock.SetActive(m_Account.m_nLevel < shop);
		_goShopCover.SetActive(m_Account.m_nLevel < shop);
		_goVIPButton.SetActive(m_Account.m_nEpisode + 1 >= GlobalTable.GetData<int>("valuePopupVIPEpisode") &&
							   false == m_Account.IsVIP());
		_txtShopOpen.text = shop.ToString();
		_txtShopLevel.text = UIStringTable.GetValue("ui_level");

		int inven = GlobalTable.GetData<int>("valueInventoryOpenLevel");

		_goInventoryLock.SetActive(m_Account.m_nLevel < inven);
		_goInventoryCover.SetActive(m_Account.m_nLevel < inven);

		_txtInventoryOpen.text = inven.ToString();
		_txtInventoryLevel.text = UIStringTable.GetValue("ui_level");

		int mission = GlobalTable.GetData<int>("valueMissionOpenLevel");

		_goMissionLock.SetActive(m_Account.m_nLevel < mission);
		_goMissionCover.SetActive(m_Account.m_nLevel < mission);

		_txtMissionOpen.text = mission.ToString();
		_txtMissionLevel.text = UIStringTable.GetValue("ui_level");

		int community = GlobalTable.GetData<int>("valueCommunityOpenLevel");

		_goCommunityLock.SetActive(m_Account.m_nLevel < community);
		_goCommunityCover.SetActive(m_Account.m_nLevel < community);

		_txtCommunityOpen.text = community.ToString();
		_txtCommunityLevel.text = UIStringTable.GetValue("ui_level");

		int nTutorialStep = PlayerPrefs.GetInt(ComType.G_KEY_TUTORIAL_STEP, (int)ETutorialStep.NONE);

		if (m_GameMgr._nPreLevel < m_Account.m_nLevel && m_Account.m_nLevel > 1)
		{
			PopupLevelUp lup = m_MenuMgr.OpenPopup<PopupLevelUp>(EUIPopup.PopupLevelUp);
			lup.InitializeInfo(this, () =>
			{
				if (m_GameMgr._nPreEpisode * 10 + m_GameMgr._nPreChapter != m_Account.m_nEpisode * 10 + m_Account.m_nChapter &&
					 m_Account.m_nChapter == ChapterTable.GetMax(m_Account.m_nEpisode + 1))
				{
					PopupEPRoadMap pop = MenuManager.Singleton.OpenPopup<PopupEPRoadMap>(EUIPopup.PopupEPRoadMap, true);
					pop.SetAddAction(CheckVIPPopup);
				}
				else
                    CheckVIPPopup();
			});
		}
		else if (m_GameMgr._nPreEpisode * 10 + m_GameMgr._nPreChapter != m_Account.m_nEpisode * 10 + m_Account.m_nChapter &&
				  m_Account.m_nChapter == ChapterTable.GetMax(m_Account.m_nEpisode + 1))
		{
			PopupEPRoadMap pop = MenuManager.Singleton.OpenPopup<PopupEPRoadMap>(EUIPopup.PopupEPRoadMap, true);
			pop.SetAddAction(CheckVIPPopup);
		}
		else
		{
            CheckVIPPopup();
		}
	}

	public void ShopLock()
	{
		PopupSysMessage pop = m_MenuMgr.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
		pop.InitializeInfo("ui_error_title", "ui_error_notenoughlevel_shop", "ui_common_close", null, "TutorialShop");
	}

	public void InventoryLock()
	{
		PopupSysMessage pop = m_MenuMgr.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
		pop.InitializeInfo("ui_error_title", "ui_error_notenoughlevel_inventory", "ui_common_close", null, "TutorialInventory");
	}

	public void MissionLock()
	{
		PopupSysMessage pop = m_MenuMgr.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
		pop.InitializeInfo("ui_error_title", "ui_error_notenoughlevel_mission", "ui_common_close", null, "TutorialAdventure");
	}

	public void CommunityLock()
	{
		PopupSysMessage pop = m_MenuMgr.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
		pop.InitializeInfo("ui_error_title", "ui_error_notenoughlevel_community", "ui_common_close", null, "TutorialAbyss");
	}

	void CheckBox()
	{
		foreach (ItemBox box in m_InvenBox)
		{
			box.bNew = !m_GameMgr._liPreBoxID.Contains(box.id);
		}
	}

	void CheckVIPPopup()
	{
        /*
		if (m_Account.m_nEpisode == 0 && m_Account.m_nChapter == 0) return;

		uint chapterKey = ComUtil.GetChapterKey(m_Account.m_nEpisode + 1, m_Account.m_nChapter);
		ChapterTable chapter = ChapterTable.GetData(chapterKey);

		if (false == m_Account.m_bIsObtainChapterWeapon && (chapter.RewardItemKey > 0 && chapter.RewardItemCount > 0))
		{
			switch (ComUtil.GetItemType(chapter.RewardItemKey))
			{
				case EItemType.Weapon:
					PopupWeaponReward popW = m_MenuMgr.OpenPopup<PopupWeaponReward>(EUIPopup.PopupWeaponReward, true);
					popW.InitializeInfo(chapter.RewardItemKey, chapter.RewardItemCount, m_oLobbyInventory, this);
					break;
				case EItemType.Gear:
				case EItemType.Material:
					PopupETCReward popE = m_MenuMgr.OpenPopup<PopupETCReward>(EUIPopup.PopupETCReward, true);
					popE.InitializeInfo(chapter.RewardItemKey, chapter.RewardItemCount, m_oLobbyInventory, this);
					break;
			}
		}
		else */

        if (m_Account.m_nEpisode >= GlobalTable.GetData<int>("valuePopupVIPEpisode") &&
				  false == m_Account.IsVIP())
		{
			float r = UnityEngine.Random.Range(0f, 1f);

			if (r <= GlobalTable.GetData<float>("ratioPopupVIP"))
				GameObject.Find("ButtonVIP").GetComponent<ButtonVIP>().OnClick();
		}
	}

	public void OnButtonMenuClick(int index)
	{
		OnButtonMenuClick(index, 0);
	}

	public void OnButtonMenuClickEquipTutorial()
	{
		OnButtonMenuClick(1, GlobalTable.GetData<int>("valueInventoryOpenLevel"));
	}

	public void OnButtonMenuClickRecycleTutorial(bool isTutorial)
	{
		OnButtonMenuClick(1, GlobalTable.GetData<int>("valueRecycleOpenLevel"));
	}

	public void OnButtonMenuClickShopTutorial(bool isTutorial)
	{
		OnButtonMenuClick(0, GlobalTable.GetData<int>("valueShopBoxTutorialLevel"));
	}

	public void OnButtonMenuClickAdventureTutorial(bool isTutorial)
	{
		OnButtonMenuClick(3, GlobalTable.GetData<int>("valueAdventureOpenLevel"));
	}

	public void OnButtonMenuClickDefenceTutorial(bool isTutorial)
	{
		OnButtonMenuClick(3, GlobalTable.GetData<int>("valueMissionDefenceOpenLevel"));
	}

	public void OnButtonMenuClickWorkshopGearTutorial(bool isTutorial)
	{
		OnButtonMenuClick(1, GlobalTable.GetData<int>("valueGearOpenLevel"));
	}

	public void OnButtonMenuClickAbyssTutorial(bool isTutorial)
	{
		OnButtonMenuClick(4, GlobalTable.GetData<int>("valueAbyssOpenLevel"));
	}

	public void OnButtonMenuClickCharacterTutorial(bool isTutorial)
	{
		OnButtonMenuClick(1, GlobalTable.GetData<int>("valueCharacterGuideLevel"));
	}

	public void OnButtonMenuClickZombieTutorial(bool isTutorial)
	{
        OnButtonMenuClick(3, GlobalTable.GetData<int>("valueMissionZombieOpenLevel"));
    }


    public void OnButtonMenuClick(int index, int tutorialIndex)
	{
		if (index == 0)
			if (m_Account.m_nLevel < GlobalTable.GetData<int>("valueShopOpenLevel"))
				return;

		if (index == 1)
			if (m_Account.m_nLevel < GlobalTable.GetData<int>("valueInventoryOpenLevel"))
				return;

		if (index == 3)
			if (m_Account.m_nLevel < GlobalTable.GetData<int>("valueMissionOpenLevel"))
				return;

		if (index == 4)
			if (m_Account.m_nLevel < GlobalTable.GetData<int>("valueCommunityOpenLevel"))
				return;

		if (_CurrentMenuIndex == index) return;

		ChangeButtonState();

		for (int i = 0; i < _ButtonActiveCaption.Length; i++)
		{
			_ButtonActiveCaption[i].gameObject.SetActive(index == i);
			_ButtonActiveIcon[i].gameObject.SetActive(index == i);
		}

		float tempValue = _CurrentMenuIndex - index;

		_TargetPagePosition = _LobbyPageRectTransform.position +
							  new Vector3(tempValue * Screen.width, 0, 0);
		_TargetButtonActiveBGPosition = _ButtonActiveBGRectTransform.position -
										new Vector3(tempValue * (Screen.width / 5f), 0, 0);

		StopCoroutine("menuMove");
		StartCoroutine(menuMove(0.5f, tempValue, _TargetPagePosition, _TargetCameraPosition, _TargetButtonActiveBGPosition));

		_CurrentMenuIndex = index;

		if (index == 0) _Page[index].GetComponent<PageLobbyShop>().ReSize();
		else if (index == 1) _Page[index].GetComponent<PageLobbyInventory>().ReSize();
		else if (index == 3) _Page[index].GetComponent<PageLobbyMission>().ReSize();

		SetCamera();

		TutorialControl(tutorialIndex);
	}

	void TutorialControl(int index)
	{
		if (index == GlobalTable.GetData<int>("valueInventoryOpenLevel"))
		{
			GameObject weapon = GameObject.Find("Slot_0").gameObject;
			RectTransform rt = weapon.GetComponent<RectTransform>();
			GameManager.Singleton.tutorial.SetFinger(weapon,
													 weapon.GetComponentInChildren<SlotWeapon>().DetailPopupTutorial,
													 rt.rect.width, rt.rect.height, 750);
        }
		else if (index == GlobalTable.GetData<int>("valueRecycleOpenLevel"))
		{
			GameObject recycle = GameObject.Find("Recycle");
			RectTransform rt = recycle.GetComponent<RectTransform>();
            GameManager.Singleton.tutorial.SetMessage("ui_tip_recycle_00", -250);
			GameManager.Singleton.tutorial.SetFinger(recycle,
													 _Page[1].GetComponent<PageLobbyInventory>().OnClickRecycle,
													 rt.rect.width, rt.rect.height, 750);
        }
		else if (index == GlobalTable.GetData<int>("valueShopBoxTutorialLevel"))
		{
			GameObject shopBox = GameObject.Find("SlotShopBox");
			RectTransform rt = shopBox.GetComponent<RectTransform>();
			GameManager.Singleton.tutorial.SetFinger(shopBox,
													 shopBox.GetComponent<SlotShopBox>().OnClickTutorial,
													 rt.rect.width, rt.rect.height, 750);
		}
		/*else if (index == GlobalTable.GetData<int>("valueAdventureOpenLevel"))
		{
			GameObject adventure = GameObject.Find("AdventureBtnUIs");
			RectTransform rt = adventure.GetComponent<RectTransform>();
            GameManager.Singleton.tutorial.SetMessage("ui_tip_adventure_00", -370);
			GameManager.Singleton.tutorial.SetFinger(adventure,
													 GameObject.Find("MissionPage").GetComponent<PageLobbyMission>().OnTouchAdventureBtn,
													 rt.rect.width, rt.rect.height, 750);
        }*/
		else if (index == GlobalTable.GetData<int>("valueMissionDefenceOpenLevel"))
		{
			GameObject adventure = GameObject.Find("ComMissionDefence");
			RectTransform rt = adventure.GetComponent<RectTransform>();
            GameManager.Singleton.tutorial.SetMessage("ui_tip_defence_00", -350);
            GameManager.Singleton.tutorial.SetFinger(adventure,
													 GameObject.Find("ComMissionDefence").GetComponent<ComMissionDefence>().OnClickTutorial,
													 rt.rect.width, rt.rect.height, 750);
		}
		else if (index == GlobalTable.GetData<int>("valueGearOpenLevel"))
		{
			GameObject workshop = GameObject.Find("Workshop");
			RectTransform rt = workshop.GetComponent<RectTransform>();
			GameManager.Singleton.tutorial.SetFinger(workshop,
													 _Page[1].GetComponent<PageLobbyInventory>().OnClickWorkshopGearTutorial,
													 rt.rect.width, rt.rect.height, 750);
		}
		else if (index == GlobalTable.GetData<int>("valueAbyssOpenLevel"))
		{
			GameObject abyss = GameObject.Find("ComCommunityAbyss");
			RectTransform rt = abyss.GetComponent<RectTransform>();
            GameManager.Singleton.tutorial.SetMessage("ui_tip_abyss_00", -250);
            GameManager.Singleton.tutorial.SetFinger(abyss,
													 abyss.GetComponent<ComCommunityAbyss>().OnClickTutorial,
													 rt.rect.width, rt.rect.height, 750);
		}
		else if (index == GlobalTable.GetData<int>("valueCharacterGuideLevel"))
		{
			GameObject agent = GameObject.Find("AgentBtn");
			RectTransform rt = agent.GetComponent<RectTransform>();
            GameManager.Singleton.tutorial.SetMessage("ui_tip_character_upgrade_00", -250);
			GameManager.Singleton.tutorial.SetFinger(agent,
													 FindObjectOfType<PageLobbyInventory>().OnTouchAgentBtn,
													 rt.rect.width, rt.rect.height, 750);
        }
        else if (index == GlobalTable.GetData<int>("valueMissionZombieOpenLevel"))
        {
            GameObject zombie = GameObject.Find("PageLobbyMissionInfiniteUIs");
            RectTransform rt = zombie.GetComponent<RectTransform>();
            GameManager.Singleton.tutorial.SetMessage("ui_tip_zombie_00", -250);
            GameManager.Singleton.tutorial.SetFinger(zombie,
                                                     FindObjectOfType<PageLobbyMission>().OnTouchInfiniteBtn,
                                                     rt.rect.width, rt.rect.height, 750);
        }
    }

	public void SetCamera(bool state = true)
	{
		_goCharacterCam.SetActive(_CurrentMenuIndex == 1 && state);
		_goMinimapCam.SetActive(_CurrentMenuIndex == 2 && state);
	}

	IEnumerator menuMove(float duration, float temp, Vector3 canvasPosition, Vector3 cameraPosition, Vector3 bgposition)
	{
		float runTime = 0f;

		GameAudioManager.PlaySFX("SFX/UI/sfx_ui_page_change_01", 0f, false, ComType.UI_MIX);

		while (runTime < duration)
		{
			runTime += Time.deltaTime;

			_LobbyPageRectTransform.position = Vector3.Lerp(_LobbyPageRectTransform.position,
															 canvasPosition,
															 runTime / duration);
			_ButtonActiveBGRectTransform.position = Vector3.Lerp(_ButtonActiveBGRectTransform.position,
																 bgposition,
																 runTime / duration);
			yield return null;
		}

		ChangeButtonState();

		yield break;
	}

	public void SetNewTag(int index, bool state)
	{
		_ButtonNewTag[index].SetActive(state);
		if (index == 1) SetInventoryNewTagOR();
	}

	public void SetNewTagKeep(int index, bool state)
	{
		_ButtonNewTag[index].SetActive(_ButtonNewTag[index].activeSelf || state);
		if (index == 1) SetInventoryNewTagOR();
	}

	public void SetInventoryNewTagOR()
	{
		SlotWeapon sweapon;
		SlotGear sgear;
		bool abletoupgrade = false;

		if (m_oLobbyInventory._dicWeapon.Count > 0)
		{
			for (long i = 0; i < m_Account.m_nWeaponID.Length; i++)
			{
				if (m_oLobbyInventory._dicWeapon.ContainsKey(m_Account.m_nWeaponID[i]))
				{
					sweapon = m_oLobbyInventory._dicWeapon[m_Account.m_nWeaponID[i]].GetComponent<SlotWeapon>();
					if (sweapon._ableToUpgrade || sweapon._ableToReInforce || sweapon._ableToLimitBreak)
						abletoupgrade = true;
				}
			}
		}

		if (m_oLobbyInventory._dicGear.Count > 0)
		{
			for (long i = 0; i < m_Account.m_nGearID.Length; i++)
			{
				if (m_oLobbyInventory._dicGear.ContainsKey(m_Account.m_nGearID[i]))
				{
					sgear = m_oLobbyInventory._dicGear[m_Account.m_nGearID[i]].GetComponent<SlotGear>();
					if (sgear._ableToUpgrade || sgear._ableToReInforce || sgear._ableToLimitBreak)
						abletoupgrade = true;
				}
			}
		}

		_ButtonNewTag[1].SetActive(_ButtonNewTag[1].activeSelf || abletoupgrade);
	}

	public void SetNewTagBG(int index, bool state)
	{
		_ButtonNewTagBG[index].SetActive(state);
	}

	void ChangeButtonState()
	{
		for (int i = 0; i < _ButtonMenuSelect.Length; i++)
		{
			_ButtonMenuSelect[i].enabled = !_ButtonMenuSelect[i].enabled;

			if (_CurrentMenuIndex == 1)
				GameObject.Find("InventoryPage").GetComponent<PageLobbyInventory>().ReSize();
		}
	}

	float _vStartXPosition, _vEndXPosition;

	public void PageVerticalDrage(UnityEngine.EventSystems.PointerEventData eventData)
	{
		float sizeX = Screen.width;

		float startX = eventData.pressPosition.x;
		float endX = eventData.position.x;
		float accelerationX = eventData.delta.x;

		if ((accelerationX < -7f || (startX - endX) > (sizeX / 2)) &&
			 _CurrentMenuIndex < _ButtonActiveCaption.Length - 1)
		{
			OnButtonMenuClick(_CurrentMenuIndex + 1);
		}
		else if ((accelerationX > 7f || (startX - endX) < -(sizeX / 2)) &&
				  _CurrentMenuIndex > 0)
		{
			OnButtonMenuClick(_CurrentMenuIndex - 1);
		}
	}

	public Action ExitApp()
	{
		return () =>
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
		};
	}


	public override void EscapePage()
	{
		PopupDefault exit = m_MenuMgr.OpenPopup<PopupDefault>(EUIPopup.PopupDefault);
		exit.SetTitle("ui_error_title");
		exit.SetMessage("ui_popup_default_title_exit");
		exit.SetButtonText("ui_popup_default_button_exit", "ui_popup_default_button_cancel", ExitApp());
	}

	/** 제거되었을 경우 */
	public void OnDestroy()
	{
		var oCameras = UnityEngine.Object.FindObjectsOfType<Camera>(true);

		for (int i = 0; i < oCameras.Length; ++i)
		{
			oCameras[i].targetTexture = null;
		}
	}

	#region 코루틴 함수
	/** 에이전트 UI 를 갱신한다 */
	private IEnumerator CoUpdateAgentUIsState()
	{
		var oCharacterTableList = CharacterTable.GetList();
		var oWaitForSecondsRealtime = new WaitForSecondsRealtime(0.25f);

		do
		{
			bool bIsEnableOpen = false;

			for (int i = 0; i < oCharacterTableList.Count; ++i)
			{
				var oCharacterTable = oCharacterTableList[i];

				// 강화 가능 할 경우
				if (ComUtil.IsEnableEnhanceAgent(oCharacterTable) || ComUtil.IsEnableOpenAgent(oCharacterTable))
				{
					bIsEnableOpen = true;
					break;
				}
			}

			this.SetNewTag(1, bIsEnableOpen);
			m_oAgentNewTagImg.gameObject.SetActive(bIsEnableOpen);

			yield return oWaitForSecondsRealtime;
		} while (_LobbyPageRectTransform.gameObject.activeSelf);
	}

	/** 미션 UI 를 갱신한다 */
	private IEnumerator CoUpdateMissionUIsState()
	{
		int nDefenceOpenLV = GlobalTable.GetData<int>("valueMissionDefenceOpenLevel");
		int nInfiniteOpenLV = GlobalTable.GetData<int>("valueMissionZombieOpenLevel");

		var oWaitForSecondsRealtime = new WaitForSecondsRealtime(0.25f);
		uint nItemKey = GlobalTable.GetData<uint>(ComType.G_VALUE_GLOBAL_TICKET_DEC_KEY);

		do
		{
			int nNumItems = GameManager.Singleton.invenMaterial.GetItemCount(nItemKey);

			bool bIsNewTagDefence = GameManager.Singleton.user.m_nLevel >= nDefenceOpenLV && nNumItems >= 1;
			bool bIsNewTagInfinite = GameManager.Singleton.user.m_nLevel >= nInfiniteOpenLV && nNumItems >= 1;

			m_oLobbyMission.MissionInfiniteUIs.TagImg.gameObject.SetActive(bIsNewTagInfinite);

			this.SetNewTag(3, bIsNewTagDefence || bIsNewTagInfinite);
			yield return oWaitForSecondsRealtime;
		} while (_LobbyPageRectTransform.gameObject.activeSelf);
	}

	/** 시간 UI 를 갱신한다 */
	private IEnumerator CoUpdateTimeUIsState()
	{	
		var oAdventureTableList = MissionAdventureTable.GetGroup(GameManager.Singleton.user.m_nAdventureGroup);
		var oWaitForSecondsRealtime = new WaitForSecondsRealtime(0.25f);

		uint nItemKey = GlobalTable.GetData<uint>(ComType.G_VALUE_GLOBAL_TICKET_DEC_KEY);

		do
		{
			yield return m_oLobbyMission.UpdateAdventureUIsState();
			m_oLobbyMission.RebuildLayouts(m_oLobbyMission.MissionAdventureUIs.SizeFitterList);

#if DISABLE_THIS
			int nNumItems = GameManager.Singleton.invenMaterial.GetItemCount(nItemKey);

			// 그룹 설정이 필요 할 경우
			if (!oAdventureTableList.ExIsValid())
			{
				oAdventureTableList = MissionAdventureTable.GetGroup(GameManager.Singleton.user.m_nAdventureGroup);
			}

#if DISABLE_THIS
			bool bIsEnableTagImgA = GameManager.Singleton.user.m_nAdventureLevel < oAdventureTableList.Count;
			bool bIsEnableTagImgB = GameManager.Singleton.user.m_nCurrentAdventureKeyCount >= 1;
			bool bIsReserveDefenceKey = m_InvenMaterial.GetItemCount(GlobalTable.GetData<int>("valueDefenceTicketDecKey")) > 0;
			bool bIsEnableDefence = GameManager.Singleton.user.m_nLevel >= GlobalTable.GetData<int>("valueMissionDefenceOpenLevel");
#else
			bool bIsEnableTagImgA = GameManager.Singleton.user.m_nAdventureLevel < oAdventureTableList.Count;
			bool bIsEnableTagImgB = nNumItems >= 1;
#endif // #if DISABLE_THIS

            this.SetNewTagKeep(3, bIsEnableTagImgA && bIsEnableTagImgB);
            this.SetNewTagKeep(3, bIsEnableTagImgB);
#endif // #if DISABLE_THIS

            yield return oWaitForSecondsRealtime;
		} while (_LobbyPageRectTransform.gameObject.activeSelf);
	}

	/** 탐험을 시작한다 */
	private IEnumerator CoStartMissionAdventure()
	{
		int nGroup = m_oLobbyMission.GetPlayableAdventureGroup();
		var oWaitPopup = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

		yield return GameDataManager.Singleton.StartAdventure(nGroup, System.DateTime.UtcNow);
		var stParams = PageLobbyMissionAdventureUIs.MakeParams(nGroup - 1, GameManager.Singleton.user.m_nAdventureLevel);

		m_oLobbyMission.MissionAdventureUIs.Init(stParams);
        m_oLobbyBattle?.InitializeAdventure();
        oWaitPopup.Close();
	}
	#endregion // 코루틴 함수
}

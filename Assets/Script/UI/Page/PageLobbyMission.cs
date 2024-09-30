using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/** 로비 미션 페이지 */
public class PageLobbyMission : MonoBehaviour
{
	#region 변수
	[SerializeField] private ComMissionDefence _comMissionDefence = null;
	[SerializeField] private PageLobbyMissionInfiniteUIs m_oMissionInfiniteUIs = null;
	[SerializeField] private PageLobbyMissionAdventureUIs m_oMissionAdventureUIs = null;

	private PopupMissionAdventure m_oPopupMissionAdventure = null;
	private WaitForSecondsRealtime m_oWaitRealtime = new WaitForSecondsRealtime(0.25f);
	#endregion // 변수

	#region 프로퍼티
	public ComMissionDefence ComMissionDefence => _comMissionDefence;
	public PageLobbyMissionInfiniteUIs MissionInfiniteUIs => m_oMissionInfiniteUIs;
	public PageLobbyMissionAdventureUIs MissionAdventureUIs => m_oMissionAdventureUIs;
	#endregion // 프로퍼티

	#region 함수
	/** 무한 버튼을 눌렀을 경우 */
	public void OnTouchInfiniteBtn()
	{
		int nOpenLV = GlobalTable.GetData<int>("valueMissionZombieOpenLevel");

		// 잠금 상태 일 경우
		if (GameManager.Singleton.user.m_nLevel < nOpenLV)
		{
			var oPopup = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
			oPopup.InitializeInfo("ui_error_title", "ui_hint_mission_zombie_desc_2", "ui_common_close", null, "TutorialInfinite");
			oPopup.AddMessage("ui_error_notenoughlevel_zombie", 2);
		}
		else
		{
			if ( false == GameManager.Singleton._isShowAlertWeapon )
			{
                PopupDefault check = MenuManager.Singleton.OpenPopup<PopupDefault>(EUIPopup.PopupDefault);
                check.SetTitle("ui_error_title");
                check.SetMessage("ui_page_lobby_mission_zombie_desc");
                check.SetButtonText("ui_page_lobby_mission_zombie_start", "ui_popup_upgradeguide_button_caption",
                                    StartInfinite, () =>
									{
										FindObjectOfType<PageLobby>().OnButtonMenuClick(1);
                                        FindObjectOfType<PageLobbyInventory>().OnPageButtonClick(0);
                                    }, "TutorialCheckWepon");
                GameManager.Singleton._isShowAlertWeapon = true;
            }
			else
			{
				StartInfinite();
            }
		}
	}

    public void OnTouchInfiniteBtn(bool isTutorial)
	{
        StartInfinite();
    }

    // 무한 모드를 시작한다
    void StartInfinite()
	{
		uint nItemKey = GlobalTable.GetData<uint>(ComType.G_VALUE_GLOBAL_TICKET_DEC_KEY);
		int nNumItems = GameManager.Singleton.invenMaterial.GetItemCount(nItemKey);

		var oInfiniteGroupTable = MissionZombieGroupTable.GetDataByEpisode(1);

		// 아이템이 부족 할 경우
		if (nNumItems < oInfiniteGroupTable.ItemCount)
		{
			PopupBoxMaterial pbm = MenuManager.Singleton.OpenPopup<PopupBoxMaterial>(EUIPopup.PopupBoxMaterial, true);
			pbm.InitializeInfo(true);
		}
		else
		{
			StartCoroutine(GameDataManager.Singleton.StartZombie(GameManager.Singleton.invenMaterial.GetItemID(nItemKey),
				oInfiniteGroupTable.ItemCount, () => this.OnHandleStartInfinite(nItemKey, oInfiniteGroupTable.ItemCount)));
		}
	}

	// 무한 모드 시작을 처리한다
	private void OnHandleStartInfinite(uint a_nItemKey, int a_nNumItems)
	{
		GameManager.Singleton.invenMaterial.ModifyItem(GameManager.Singleton.invenMaterial.GetItemID(a_nItemKey),
                                                        InventoryData<ItemMaterial>.EItemModifyType.Volume,
                                                        Mathf.Max(0, GameManager.Singleton.invenMaterial.GetItemCount(a_nItemKey) - a_nNumItems));

		var oHandler = new PopupLoadingHandlerBattleReady();
		oHandler.Init(PopupLoadingHandlerBattleReady.MakeParams(EMapInfoType.INFINITE, new STIDInfo(0, 0, 0)));

		var oLoadingPopup = MenuManager.Singleton.OpenPopup<PopupLoading>(EUIPopup.PopupLoading, true);
		oLoadingPopup.Init(PopupLoading.MakeParams(PopupLoading.ELoadingType.BATTLE_READY, oHandler, this.OnCompleteLoadingBattleReadyInfinite));
	}

	/** 무한 모드 준비 로딩이 완료 되었을 경우 */
	private void OnCompleteLoadingBattleReadyInfinite(PopupLoading a_oSender)
	{
		var oMapInfoDict = CMapInfoTable.Singleton.LoadMapInfosDownloaded(EMapInfoType.INFINITE, 0, 0);

		GameDataManager.Singleton.SetPlayStageID(0);
		GameDataManager.Singleton.SetContinueTimes(0);
		GameDataManager.Singleton.SetupPlayMapInfo(EMapInfoType.INFINITE, EPlayMode.INFINITE, oMapInfoDict, true);

		this.ExLateCallFunc((a_oFuncSender) =>
		{
			MenuManager.Singleton.SceneEnd();
			MenuManager.Singleton.SceneNext(ESceneType.Battle);
		}, ComType.G_DURATION_LOADING_PROGRESS_ANI);
	}

	/** 탐험 버튼을 눌렀을 경우 */
	public void OnTouchAdventureBtn()
	{
		int nOpenLV = GlobalTable.GetData<int>("valueAdventureOpenLevel");

		// 오픈 상태가 아닐 경우
		if (GameManager.Singleton.user.m_nLevel < nOpenLV)
		{
			var oPopup = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
			oPopup.InitializeInfo("ui_error_title", "ui_hint_mission_desc_2", "ui_common_close", null, "TutorialAdventure");
			oPopup.AddMessage("ui_error_notenoughlevel_adventure", 2);
		}
		else
		{
			var oAdventureTableList = MissionAdventureTable.GetGroup(GameManager.Singleton.user.m_nAdventureGroup);

			// 탐험을 완료했을 경우
			if (GameManager.Singleton.user.m_nAdventureLevel >= oAdventureTableList.Count)
			{
				PopupSysMessage.ShowMissionAdventureClearMsg();
			}
			else
			{
				this.ShowMissionAdventurePopup(false);
			}
		}
	}

	/** 튜토리얼 용 탐험 버튼 */
	public void OnTouchAdventureBtn(bool isTutorial)
	{
		OnTouchAdventureBtn();
	}

	/** 탐험 팝업을 출력한다 */
	public void ShowMissionAdventurePopup(bool a_bIsClear)
	{
		m_oPopupMissionAdventure = MenuManager.Singleton.OpenPopup<PopupMissionAdventure>(EUIPopup.PopupMissionAdventure);
		m_oPopupMissionAdventure.Init(PopupMissionAdventure.MakeParams(GameManager.Singleton.user.m_nAdventureGroup - 1, a_bIsClear));
	}

	/** 크기를 다시 계산한다 */
	public void ReSize()
	{
		var stInfiniteParams = PageLobbyMissionInfiniteUIs.MakeParams();
		m_oMissionInfiniteUIs.Init(stInfiniteParams);

		var stAdventureParams = PageLobbyMissionAdventureUIs.MakeParams(GameManager.Singleton.user.m_nAdventureGroup - 1,
			GameManager.Singleton.user.m_nAdventureLevel);

		m_oMissionAdventureUIs.Init(stAdventureParams);

#if DISABLE_THIS
		StopCoroutine("CoUpdateTimeUIsState");
		StartCoroutine(this.CoUpdateTimeUIsState());

		// 탐험 시작 상태 일 경우
		if (GameManager.Singleton.IsEnableStartMissionAdventure())
		{
			StartCoroutine(this.CoStartMissionAdventure());
		}
		else
		{
			
		}
#endif // #if DISABLE_THIS
	}

	/** 레이아웃을 재배치한다 */
	public void RebuildLayouts(List<ContentSizeFitter> a_oSizeFitterList)
	{
		for (int i = 0; i < a_oSizeFitterList.Count; ++i)
		{
			LayoutRebuilder.MarkLayoutForRebuild(a_oSizeFitterList[i].transform as RectTransform);
		}

		for (int i = a_oSizeFitterList.Count - 1; i >= 0; --i)
		{
			LayoutRebuilder.MarkLayoutForRebuild(a_oSizeFitterList[i].transform as RectTransform);
		}
	}

	/** 탐험 UI 상태를 갱신한다 */
	public IEnumerator UpdateAdventureUIsState()
	{
		int nResetCycle = GlobalTable.GetData<int>(ComType.G_TIME_ADVENTURE_RESET);
		int nChargeCycle = GlobalTable.GetData<int>(ComType.G_TIME_ADVENTURE_KEY_OBTAIN);

		var stTime = System.DateTime.UtcNow;
		var stFinishTime = GameManager.Singleton.user.m_dtStartAdventure.AddMilliseconds(nResetCycle);
		var stChargeFinishTime = GameManager.Singleton.user.m_dtAdventureKeyStart.AddMilliseconds(nChargeCycle);

		var stDeltaTime = stFinishTime - stTime;
		var stChargeDeltaTime = stChargeFinishTime - stTime;

		var oKeyMatTable = MaterialTable.GetData(ComType.G_KEY_MAT_ADVENTURE_KEY);

		string oName = NameTable.GetValue(oKeyMatTable.NameKey);
		string oChargeTimeStr = (stChargeDeltaTime.TotalSeconds.ExIsLessEquals(0.0f) || !GameManager.Singleton.IsEnableChargeAdventureKey()) ? default(System.TimeSpan).ExGetTimeStr() : stChargeDeltaTime.ExGetTimeStr();

		m_oMissionAdventureUIs.RemainTimeText.text = stDeltaTime.TotalSeconds.ExIsLessEquals(0.0f) ? default(System.TimeSpan).ExGetTimeStr() : stDeltaTime.ExGetTimeStr();
		m_oMissionAdventureUIs.TicketChargeTimeText.text = oName + " : " + oChargeTimeStr;

		m_oMissionAdventureUIs.TicketChargeTimeText.gameObject.SetActive(GameManager.Singleton.IsEnableChargeAdventureKey());
		m_oMissionAdventureUIs.UpdateUIsState();

		m_oPopupMissionAdventure?.UpdateTimeInfos(m_oMissionAdventureUIs.RemainTimeText.text, oChargeTimeStr, m_oMissionAdventureUIs.TicketChargeTimeText.gameObject.activeSelf);

		// 탐험이 종료 되었을 경우
		if (stDeltaTime.TotalSeconds.ExIsLessEquals(0.0f))
		{
			int nGroup = this.GetPlayableAdventureGroup();
			var oWaitPopup = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

			yield return GameDataManager.Singleton.StartAdventure(nGroup, System.DateTime.UtcNow);

			oWaitPopup.Close();
			this.ResetAdventureGroup(nGroup);
		}
		// 키 충전이 완료 되었을 경우
		else if (stChargeDeltaTime.TotalSeconds.ExIsLessEquals(0.0f) && GameManager.Singleton.IsEnableChargeAdventureKey())
		{
			var oWaitPopup = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);
			GameManager.Singleton.user.m_dtAdventureKeyStart = stChargeFinishTime;

			yield return GameManager.Singleton.user.IncrAdventureKeyCount(1);

			oWaitPopup.Close();
			m_oPopupMissionAdventure?.UpdateUIsState();
		}

		yield return YieldInstructionCache.WaitForEndOfFrame;
	}

	/** 탐험 그룹을 리셋한다 */
	private void ResetAdventureGroup(int a_nGroup)
	{
		m_oPopupMissionAdventure?.ResetAdventureGroup(a_nGroup);
	}

	/// <summary>
	/// 방어 모드 컴포넌트 초기화
	/// </summary>
	public void InitializeCompDefence()
	{
		_comMissionDefence.CheckItemCount();
	}

	public void OnClickDefence()
	{
		_comMissionDefence.OnClick();
	}
	#endregion // 함수

	#region 접근 함수
	/** 플레이 가능한 탐험 그룹을 반환한다 */
	public int GetPlayableAdventureGroup()
	{
		// 첫 탐험 플레이 일 경우
		if (GameManager.Singleton.user.m_nAdventureGroup <= 0)
		{
			return 1;
		}

		int nGroup = GameManager.Singleton.user.m_nAdventureGroup % 2;
		return nGroup + 1;
	}
	#endregion // 접근 함수

	#region 코루틴 함수
#if DISABLE_THIS
	/** 시간 UI 를 갱신한다 */
	private IEnumerator CoUpdateTimeUIsState()
	{
		do
		{
			yield return this.UpdateAdventureUIsState();
			this.RebuildLayouts(m_oMissionAdventureUIs.SizeFitterList);

			yield return m_oWaitRealtime;
		} while (this.gameObject.activeSelf);
	}

	/** 탐험을 시작한다 */
	private IEnumerator CoStartMissionAdventure()
	{
		int nGroup = this.GetPlayableAdventureGroup();
		var oWaitPopup = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

		yield return GameDataManager.Singleton.StartAdventure(nGroup, System.DateTime.UtcNow);

		var stParams = PageLobbyMissionAdventureUIs.MakeParams(GameManager.Singleton.user.m_nAdventureGroup - 1,
			GameManager.Singleton.user.m_nAdventureLevel);

		m_oMissionAdventureUIs.Init(stParams);
		oWaitPopup.Close();
	}
#endif // #if DISABLE_THIS
	#endregion // 코루틴 함수
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/** 탐험 미션 로비 페이지 UI */
public class PageLobbyMissionAdventureUIs : MonoBehaviour
{
	/** 매개 변수 */
	public struct STParams
	{
		public int m_nGroup;
		public int m_nOrder;
	}

	#region 변수
	[SerializeField] private List<ContentSizeFitter> m_oSizeFitterList = new List<ContentSizeFitter>();

	[Header("=====> UIs <=====")]
	[SerializeField] private TMP_Text m_oOpenLVText = null;
	[SerializeField] private TMP_Text m_oProgressText = null;
	[SerializeField] private TMP_Text m_oRemainTimeText = null;
	[SerializeField] private TMP_Text m_oTicketChargeTimeText = null;

	[SerializeField] private Image m_oTagImg = null;
	[SerializeField] private Slider m_oProgressSlider = null;

	[Header("=====> Game Objects <=====")]
	[SerializeField] private GameObject m_oNormUIs = null;
	[SerializeField] private GameObject m_oLockUIs = null;
	[SerializeField] private GameObject m_oClearUIs = null;
	[SerializeField] private GameObject m_oTopRewardUIs = null;

	[SerializeField] private List<GameObject> m_oRewardUIsList = new List<GameObject>();
	[SerializeField] private List<GameObject> m_oPlayTicketUIsList = new List<GameObject>();
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }
	public TMP_Text RemainTimeText => m_oRemainTimeText;
	public TMP_Text TicketChargeTimeText => m_oTicketChargeTimeText;
	public List<ContentSizeFitter> SizeFitterList => m_oSizeFitterList;
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;

		uint nKey = ComUtil.GetAdventureKey(a_stParams.m_nGroup + 1, a_stParams.m_nOrder + 1);
		int nOpenLV = GlobalTable.GetData<int>("valueAdventureOpenLevel");

		// 보상 UI 설정이 가능 할 경우
		if (!this.IsClear() || GameManager.Singleton.user.m_nLevel < nOpenLV)
		{
			uint nLockUIsRewardMain = 0x243E2021;
			int nLockUIsRewardGroup = 0x130011;

			uint nRewardMain = (GameManager.Singleton.user.m_nLevel >= nOpenLV) ? 
				MissionAdventureTable.GetData(nKey).RewardMain : nLockUIsRewardMain;

			int nRewardGroup = (GameManager.Singleton.user.m_nLevel >= nOpenLV) ? 
				MissionAdventureTable.GetData(nKey).RewardGroup : nLockUIsRewardGroup;

			var oRewardGroupList = RewardTable.GetGroup(nRewardGroup);
			var oRewardTableList = RewardListTable.GetGroup(oRewardGroupList[0].RewardListGroup);

			var stTopRewardParams = SlotMissionAdventureReward.MakeParams(nRewardMain, null);
			m_oTopRewardUIs.GetComponent<SlotMissionAdventureReward>().Init(stTopRewardParams);

			for (int i = 0; i < m_oRewardUIsList.Count; ++i)
			{
				m_oRewardUIsList[i].SetActive(oRewardTableList.ExIsValidIdx(i));

				// 보상이 없을 경우
				if (!oRewardTableList.ExIsValidIdx(i))
				{
					continue;
				}

				var stParams = SlotMissionAdventureReward.MakeParams(oRewardTableList[i].RewardKey, oRewardTableList[i]);
				m_oRewardUIsList[i]?.GetComponent<SlotMissionAdventureReward>().Init(stParams);
			}
		}

		this.UpdateUIsState();
	}

	/** UI 상태를 갱신한다 */
	public void UpdateUIsState()
	{
		var oAdventureTableList = MissionAdventureTable.GetGroup(this.Params.m_nGroup + 1);
		int nOpenLV = GlobalTable.GetData<int>("valueAdventureOpenLevel");

		bool bIsOpen = GameManager.Singleton.user.m_nLevel >= nOpenLV;
		uint nItemKey = GlobalTable.GetData<uint>(ComType.G_VALUE_GLOBAL_TICKET_DEC_KEY);

		int nNumItems = GameManager.Singleton.invenMaterial.GetItemCount(nItemKey);

		float fPercent = bIsOpen ? GameManager.Singleton.user.m_nAdventureLevel / (float)oAdventureTableList.Count : 0.0f;
		fPercent = Mathf.Clamp01(fPercent);

		// UI 객체를 설정한다 {
		m_oLockUIs.SetActive(!bIsOpen);
		m_oNormUIs.SetActive(!bIsOpen || GameManager.Singleton.user.m_nAdventureLevel < oAdventureTableList.Count);

		m_oClearUIs.SetActive(bIsOpen && GameManager.Singleton.user.m_nAdventureLevel >= oAdventureTableList.Count);
		// UI 객체를 설정한다 }

		m_oOpenLVText.text = $"Lv.{nOpenLV}";

		m_oProgressText.text = string.Format("{0} {1}%", UIStringTable.GetValue("ui_popup_adventure_progress"), (int)(fPercent * 100.0f));
		m_oProgressSlider.value = fPercent;

		bool bIsEnableTagImgA = GameManager.Singleton.user.m_nAdventureLevel < oAdventureTableList.Count;
		bool bIsEnableTagImgB = nNumItems >= 1;

		m_oTagImg.gameObject.SetActive(bIsOpen && bIsEnableTagImgA && bIsEnableTagImgB);

		for (int i = 0; i < m_oPlayTicketUIsList.Count; ++i)
		{
			var oIconImg = m_oPlayTicketUIsList[i].transform.Find("IconImg");
			oIconImg.gameObject.SetActive(i < GameManager.Singleton.user.m_nCurrentAdventureKeyCount);
		}

		for (int i = 0; i < m_oSizeFitterList.Count; ++i)
		{
			LayoutRebuilder.MarkLayoutForRebuild(m_oSizeFitterList[i].transform as RectTransform);
		}
	}
	#endregion // 함수

	#region 접근 함수
	/** 클리어 여부를 반환한다 */
	public bool IsClear()
	{
		var oAdventureTableList = MissionAdventureTable.GetGroup(this.Params.m_nGroup + 1);
		return GameManager.Singleton.user.m_nAdventureLevel >= oAdventureTableList.Count;
	}
	#endregion // 접근 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(int a_nGroup, int a_nOrder)
	{
		return new STParams()
		{
			m_nGroup = a_nGroup,
			m_nOrder = a_nOrder
		};
	}
	#endregion // 클래스 팩토리 함수
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/** 탐험 미션 팝업 스크롤러 셀 뷰 */
public class PopupMissionAdventureScrollerCellView : MonoBehaviour
{
	/** 매개 변수 */
	public struct STParams
	{
		public int m_nGroup;
		public int m_nOrder;

		public MissionAdventureTable m_oTable;
		public PopupMissionAdventure m_oPopup;
		public PopupMissionAdventureFGScrollerCellView m_oFGScrollerCellView;
	}

	#region 변수
	private Tween m_oAcquireAni = null;
	private System.Action<PopupMissionAdventureScrollerCellView, RewardListTable> m_oAcquireCallback = null;

	[Header("=====> UIs <=====")]
	[SerializeField] private TMP_Text m_oTitleText = null;
	[SerializeField] private TMP_Text m_oClearText = null;

	[SerializeField] private List<Image> m_oHighlightImgList = new List<Image>();

	[Header("=====> Game Objects <=====")]
	[SerializeField] private GameObject m_oNormUIs = null;
	[SerializeField] private GameObject m_oClearUIs = null;

	[SerializeField] private GameObject m_oPlayerPosDummy = null;
	[SerializeField] private List<GameObject> m_oRewardUIsList = new List<GameObject>();
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }
	public List<RewardListTable> RewardTableList { get; } = new List<RewardListTable>();
	public PopupMissionAdventure.EScrollerCellViewState State { get; private set; } = PopupMissionAdventure.EScrollerCellViewState.NONE;

	public GameObject PlayerPosDummy => m_oPlayerPosDummy;
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;

		uint nKey = ComUtil.GetAdventureKey(a_stParams.m_nGroup + 1, a_stParams.m_nOrder + 1);
		var oRewardGroupList = RewardTable.GetGroup(MissionAdventureTable.GetData(nKey).RewardGroup);
		var oRewardTableList = RewardListTable.GetGroup(oRewardGroupList[0].RewardListGroup);

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

			this.RewardTableList.ExAddVal(oRewardTableList[i]);
		}
	}

	/** 그룹 상태를 리셋한다 */
	public void ResetAdventureGroup(int a_nGroup)
	{
		for (int i = 0; i < m_oRewardUIsList.Count; ++i)
		{
			m_oRewardUIsList[i].transform.localScale = Vector3.one;
		}
	}

	/** 제거 되었을 경우 */
	public void OnDestroy()
	{
		m_oAcquireAni?.Kill();
	}

	/** 잠금 해제 애니메이션을 시작한다 */
	public void StartUnlockAni()
	{
		this.Params.m_oFGScrollerCellView.StartUnlockAni();
	}

	/** 보상 획득 애니메이션을 시작한다 */
	public void StartAcquireRewardAni(RewardListTable a_oRewardTable,
		System.Action<PopupMissionAdventureScrollerCellView, RewardListTable> a_oCallback)
	{
		int nResult = this.RewardTableList.FindIndex((a_oCompareRewardTable) => a_oCompareRewardTable == a_oRewardTable);
		float fDuration = 0.35f;

		// 보상이 없을 경우
		if (!this.RewardTableList.ExIsValidIdx(nResult))
		{
			return;
		}

		nResult = Mathf.Min(nResult, m_oRewardUIsList.Count - 1);

		var stStartPos = m_oRewardUIsList[nResult].transform.localPosition;
		var stEndPos = m_oPlayerPosDummy.transform.position.ExToLocal(m_oRewardUIsList[nResult].transform.parent.gameObject);

		var oPosList = new List<Vector3>() {
			stStartPos,
			(stStartPos + stEndPos) / 2.0f + (Vector3.up * 75.0f),
			stEndPos
		};

		var oSequence = DOTween.Sequence();
		oSequence.Join(m_oRewardUIsList[nResult].transform.DOScale(Vector3.zero, fDuration));
		oSequence.Join(m_oRewardUIsList[nResult].transform.DOLocalPath(oPosList.ToArray(), fDuration, PathType.CatmullRom, PathMode.Sidescroller2D));
		oSequence.AppendCallback(() => this.OnCompleteAcquireRewardAni(oSequence, a_oRewardTable));

		m_oAcquireCallback = a_oCallback;
		ComUtil.AssignVal(ref m_oAcquireAni, oSequence);
	}

	/** 보상 획득 애니메이션이 완료 되었을 경우 */
	private void OnCompleteAcquireRewardAni(Sequence a_oSender, RewardListTable a_oRewardTable)
	{
		a_oSender?.Kill();
		m_oAcquireCallback?.Invoke(this, a_oRewardTable);
	}

	/** UI 상태를 갱신한다 */
	private void UpdateUIsState()
	{
		for (int i = 0; i < m_oHighlightImgList.Count; ++i)
		{
			var stColor = this.Params.m_oPopup.GetScrollerCellViewStateColor(this.Params.m_nOrder, this.State);
			m_oHighlightImgList[i].color = new Color(stColor.r, stColor.g, stColor.b, m_oHighlightImgList[i].color.a);
		}

		string oLevelStr = UIStringTable.GetValue("ui_level");
		string oCompleteStr = UIStringTable.GetValue("ui_popup_battlereward_titleresult_success");

		m_oTitleText.text = $"{oLevelStr} {this.Params.m_nOrder + 1}";
		m_oClearText.text = $"{oLevelStr} {this.Params.m_nOrder + 1} {oCompleteStr}";

		m_oNormUIs?.SetActive(this.State != PopupMissionAdventure.EScrollerCellViewState.CLEAR);
		m_oClearUIs?.SetActive(this.State == PopupMissionAdventure.EScrollerCellViewState.CLEAR);
	}
	#endregion // 함수

	#region 접근 함수
	/** 상태를 변경한다 */
	public void SetState(PopupMissionAdventure.EScrollerCellViewState a_eState)
	{
		this.State = a_eState;
		this.Params.m_oFGScrollerCellView.SetState(a_eState);

		this.UpdateUIsState();
	}
	#endregion // 접근 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(int a_nGroup,
		int a_nOrder, PopupMissionAdventure a_oPopup, PopupMissionAdventureFGScrollerCellView a_oFGScrollerCellView, MissionAdventureTable a_oTable)
	{
		return new STParams()
		{
			m_nGroup = a_nGroup,
			m_nOrder = a_nOrder,

			m_oTable = a_oTable,
			m_oPopup = a_oPopup,
			m_oFGScrollerCellView = a_oFGScrollerCellView
		};
	}
	#endregion // 클래스 팩토리 함수
}

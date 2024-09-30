using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

/** 에이전트 팝업 - 정보 */
public partial class PopupAgent : UIDialog
{
	/** 정보 키 */
	private enum EInfoKey
	{
		NONE = -1,
		HP,
		DEFENCE,
		MOVE_SPEED,
		[HideInInspector] MAX_VAL
	}

	#region 변수
	[Header("=====> Popup Agent - Etc (Info) <=====")]
	[SerializeField] private ParticleSystem m_oAgentEnhanceParticle = null;

	private int m_nCurIdxAgentInfo = 0;
	private Color m_stOriginColorAgentInfoPagination = Color.white;
	private Tween m_oAgentInfoSwipeAnim = null;

	[Header("=====> Popup Agent - UIs (Info) <=====")]
	[SerializeField] private TMP_Text m_oCPText = null;
	[SerializeField] private TMP_Text m_oNameText = null;
	[SerializeField] private TMP_Text m_oLevelText = null;

	[SerializeField] private UIMultiScrollRect m_oAgentInfoScrollRect = null;

	[SerializeField] private List<Image> m_oAgentInfoPaginationImgList = new List<Image>();
	[SerializeField] private List<PopupAgentScrollerCellViewInfo> m_oAgentInfoScrollerCellViewList = new List<PopupAgentScrollerCellViewInfo>();

	[Header("=====> Popup Agent - Game Objects (Info) <=====")]
	[SerializeField] private GameObject m_oAgentInfoLockUIs = null;
	[SerializeField] private GameObject m_oInfoScrollViewViewport = null;
	[SerializeField] private GameObject m_oInfoScrollViewContents = null;

	[SerializeField] private List<GameObject> m_oAgentInfoUIsList = new List<GameObject>();
	#endregion // 변수

	#region 함수
	/** 정보 UI 를 설정한다 */
	private void SetupUIsInfo()
	{
		m_oAgentInfoScrollRect.Callback = this.OnReceiveCallbackAgentInfoDrag;
		m_stOriginColorAgentInfoPagination = m_oAgentInfoPaginationImgList.FirstOrDefault().color;
	}

	/** 정보 UI 상태를 갱신한다 */
	private void UpdateUIsStateInfo()
	{
		var oItemCharacter = ComUtil.GetItemCharacter(m_oSelCharacterTable);

		int nCurLevel = (oItemCharacter != null) ? oItemCharacter.nCurUpgrade : 0;
		int nMaxLevel = ComUtil.GetAgentMaxLevel(m_oSelCharacterTable);

		var oCurLevelTable = CharacterLevelTable.GetTable(m_oSelCharacterTable.PrimaryKey, nCurLevel);

		var oNextLevelTable = CharacterLevelTable.GetTable(m_oSelCharacterTable.PrimaryKey, 
			(oItemCharacter != null) ? Mathf.Min(nMaxLevel, oItemCharacter.nCurUpgrade + 1) : 0);

		string oNameStr = NameTable.GetValue(m_oSelCharacterTable.NameKey);
		string oLevelStr = UIStringTable.GetValue("ui_level");

		m_oNameText.text = oNameStr;
		m_oLevelText.text = $"<color=#ffffff>{oLevelStr}</color> {nCurLevel + 1}";

		m_oAgentInfoLockUIs.SetActive(!this.IsOpenAgent(m_oSelCharacterTable));

		this.UpdateUIsStateInfoScrollerCellView(EInfoKey.HP, oCurLevelTable, oNextLevelTable);
		this.UpdateUIsStateInfoScrollerCellView(EInfoKey.DEFENCE, oCurLevelTable, oNextLevelTable);
		this.UpdateUIsStateInfoScrollerCellView(EInfoKey.MOVE_SPEED, oCurLevelTable, oNextLevelTable);

		for (int i = 0; i < m_oAgentInfoPaginationImgList.Count; ++i)
		{
			bool bIsCurAgentInfo = m_nCurIdxAgentInfo == i;

			m_oAgentInfoPaginationImgList[i].color = bIsCurAgentInfo ?
				m_stOriginColorAgentInfoPagination : m_stOriginColorAgentInfoPagination * 0.5f;
		}
	}

	/** 에이전트 정보 스크롤러 셀 뷰 상태를 갱신한다 */
	private void UpdateUIsStateInfoScrollerCellView(EInfoKey a_eInfoKey,
		CharacterLevelTable a_oCurLevelTable, CharacterLevelTable a_oNextLevelTable)
	{
		int nDeltaHP = a_oNextLevelTable.HP - a_oCurLevelTable.HP;
		int nDeltaDP = a_oNextLevelTable.DP - a_oCurLevelTable.DP;

		float fDeltaMoveSpeed = a_oNextLevelTable.MoveSpeed - a_oCurLevelTable.MoveSpeed;

		switch (a_eInfoKey)
		{
			case EInfoKey.HP:
				m_oAgentInfoScrollerCellViewList[(int)a_eInfoKey].ValText.text = $"{nDeltaHP}";
				m_oAgentInfoScrollerCellViewList[(int)a_eInfoKey].DescText.text = $"{a_oCurLevelTable.HP}";

				m_oAgentInfoScrollerCellViewList[(int)a_eInfoKey].EnhanceUIs.SetActive(nDeltaHP >= 0);
				break;

			case EInfoKey.DEFENCE:
				m_oAgentInfoScrollerCellViewList[(int)a_eInfoKey].ValText.text = $"{nDeltaDP}";
				m_oAgentInfoScrollerCellViewList[(int)a_eInfoKey].DescText.text = $"{a_oCurLevelTable.DP}";

				m_oAgentInfoScrollerCellViewList[(int)a_eInfoKey].EnhanceUIs.SetActive(nDeltaDP >= 0);
				break;
				
			case EInfoKey.MOVE_SPEED:
				m_oAgentInfoScrollerCellViewList[(int)a_eInfoKey].ValText.text = $"{fDeltaMoveSpeed:0.00}";
				m_oAgentInfoScrollerCellViewList[(int)a_eInfoKey].DescText.text = $"{a_oCurLevelTable.MoveSpeed:0.00}";

				m_oAgentInfoScrollerCellViewList[(int)a_eInfoKey].EnhanceUIs.SetActive(fDeltaMoveSpeed.ExIsGreat(0.0f));
				break;
		}
	}

	/** 드래그 콜백을 수신했을 경우 */
	private void OnReceiveCallbackAgentInfoDrag(UIMultiScrollRect a_oSender, PointerEventData a_oEventData)
	{
		int nIdxSwipe = 0;
		var stDeltaPos = a_oEventData.position - a_oEventData.pressPosition;

		bool bIsSwipe = stDeltaPos.magnitude.ExIsGreat(7.0f);
		bIsSwipe = bIsSwipe && Mathf.Abs(stDeltaPos.x).ExIsGreat(Mathf.Abs(stDeltaPos.y));

		// 스와이프가 발생했을 경우
		if (bIsSwipe)
		{
			nIdxSwipe = stDeltaPos.x.ExIsLess(0.0f) ? 1 : -1;
		}

		this.SetIdxAgentInfo(m_nCurIdxAgentInfo + nIdxSwipe);
		this.UpdateUIsState();
	}
	#endregion // 함수

	#region 접근 함수
	/** 에이전트 정보 인덱스를 변경한다 */
	private void SetIdxAgentInfo(int a_nIdx)
	{
		var stViewportRectTrans = m_oInfoScrollViewViewport.transform as RectTransform;
		var oContentsRectTrans = m_oInfoScrollViewContents.transform as RectTransform;

		var stViewportSize = stViewportRectTrans.rect.size;
		m_nCurIdxAgentInfo = Mathf.Clamp(a_nIdx, 0, m_oAgentInfoUIsList.Count - 1);

		var oSwipeAni = oContentsRectTrans.DOAnchorPosX(m_nCurIdxAgentInfo * -stViewportSize.x, 0.25f);
		ComUtil.AssignVal(ref m_oAgentInfoSwipeAnim, oSwipeAni);

		m_oVideoPlayer.Stop();
		m_oVideoPlayer.Play();

		m_oAgentInfoScrollRect.velocity = Vector3.zero;
	}
	#endregion // 접근 함수
}

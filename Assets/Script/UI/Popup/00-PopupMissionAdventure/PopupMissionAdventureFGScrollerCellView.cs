using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/** 탐험 미션 팝업 상단 스크롤러 셀 뷰 */
public class PopupMissionAdventureFGScrollerCellView : MonoBehaviour
{
	/** 매개 변수 */
	public struct STParams
	{
		public int m_nGroup;
		public int m_nOrder;

		public MissionAdventureTable m_oTable;
		public PopupMissionAdventure m_oPopup;

		public System.Action<PopupMissionAdventureFGScrollerCellView> m_oBuyCallback;
		public System.Action<PopupMissionAdventureFGScrollerCellView> m_oPlayCallback;
		public System.Action<PopupMissionAdventureFGScrollerCellView> m_oPassCallback;
	}

	#region 변수
	private bool m_bIsPassing = false;
	private bool m_bIsEnableOverlayInfoUIs = true;

	private Tween m_oPassAni = null;
	private Animator m_oUnlockAnimator = null;

	[Header("=====> UIs <=====")]
	[SerializeField] private TMP_Text m_oPriceText = null;
	[SerializeField] private List<Image> m_oHighlightImgList = new List<Image>();

	[SerializeField] private Image m_oBossIconImg = null;
	[SerializeField] private Image m_oLockIconImg = null;
	[SerializeField] private Image m_oPassGaugeImg = null;

	[SerializeField] private SoundButton m_oPlayBtn = null;
	[SerializeField] private SoundButton m_oPassBtn = null;

	[Header("=====> Game Objects <=====")]
	[SerializeField] private GameObject m_oBuyUIs = null;
	[SerializeField] private GameObject m_oLockUIs = null;
	[SerializeField] private GameObject m_oPassUIs = null;
	[SerializeField] private GameObject m_oActiveUIs = null;
	[SerializeField] private GameObject m_oOverlayInfoUIs = null;
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }
	public PopupMissionAdventure.EScrollerCellViewState State { get; private set; } = PopupMissionAdventure.EScrollerCellViewState.NONE;

	public TMP_Text PriceText => m_oPriceText;
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public void Awake()
	{
		m_oUnlockAnimator = this.GetComponentInChildren<Animator>();
	}

	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;
	}

	/** 제거 되었을 경우 */
	public void OnDestroy()
	{
		m_oPassAni?.Kill();
	}

	/** 비활성 되었을 경우 */
	public void OnDisable()
	{
		m_oPassAni?.Kill();
		m_bIsPassing = false;
	}

	/** 플레이 버튼을 눌렀을 경우 */
	public void OnTouchPlayBtn()
	{
		this.Params.m_oPlayCallback?.Invoke(this);
	}

	/** 빠른 진행 버튼을 눌렀을 경우 */
	public void OnTouchPassBtn()
	{
		// 빠른 진행이 불가능 할 경우
		if (!this.Params.m_oPopup.IsEnablePass(this.Params.m_oTable))
		{
			PopupBoxMaterial pbm = MenuManager.Singleton.OpenPopup<PopupBoxMaterial>(EUIPopup.PopupBoxMaterial, true);
			pbm.InitializeInfo(true);

			return;
		}

		m_bIsPassing = true;
		m_oPassGaugeImg.rectTransform.anchoredPosition = new Vector2(0.0f, m_oPassGaugeImg.rectTransform.anchoredPosition.y);

		var oSequence = DOTween.Sequence();
		oSequence.Append(m_oPassGaugeImg.rectTransform.DOAnchorPosX(m_oPassGaugeImg.rectTransform.rect.size.x, 2.5f).SetEase(Ease.Linear));
		oSequence.AppendCallback(() => this.OnCompletePassAni(oSequence));

		this.UpdateUIsState();
		ComUtil.AssignVal(ref m_oPassAni, oSequence);
	}

	/** 구입 버튼을 눌렀을 경우 */
	public void OnTouchBuyBtn()
	{
		this.Params.m_oBuyCallback?.Invoke(this);
	}

	/** 잠금 해제 애니메이션을 시작한다 */
	public void StartUnlockAni()
	{
		m_oUnlockAnimator.SetTrigger(ComType.G_PARAMS_UNLOCK);
	}

	/** UI 상태를 갱신한다 */
	private void UpdateUIsState()
	{
		for (int i = 0; i < m_oHighlightImgList.Count; ++i)
		{
			var stColor = this.Params.m_oPopup.GetScrollerCellViewStateColor(this.Params.m_nOrder, this.State);
			m_oHighlightImgList[i].color = new Color(stColor.r, stColor.g, stColor.b, m_oHighlightImgList[i].color.a);
		}

		m_oLockUIs?.SetActive(this.State == PopupMissionAdventure.EScrollerCellViewState.LOCK);
		m_oOverlayInfoUIs?.SetActive(m_bIsEnableOverlayInfoUIs && this.State == PopupMissionAdventure.EScrollerCellViewState.ACTIVE);

		m_oBossIconImg.gameObject.SetActive(this.Params.m_oTable.ChapterType != 0);
		m_oLockIconImg.gameObject.SetActive(this.Params.m_oTable.ChapterType == 0);

#if DISABLE_THIS
		m_oBuyUIs?.SetActive(!m_bIsPassing && GameManager.Singleton.user.m_nCurrentAdventureKeyCount <= 0);
		m_oPassUIs?.SetActive(m_bIsPassing && GameManager.Singleton.user.m_nCurrentAdventureKeyCount >= 1);
		m_oActiveUIs?.SetActive(!m_bIsPassing && GameManager.Singleton.user.m_nCurrentAdventureKeyCount >= 1);
#else
		m_oBuyUIs?.SetActive(false);
		m_oPassUIs?.SetActive(m_bIsPassing);
		m_oActiveUIs?.SetActive(!m_bIsPassing);
#endif // #if DISABLE_THIS
	}

	/** 패스 애니메이션이 완료 되었을 경우 */
	private void OnCompletePassAni(Sequence a_oSender)
	{
		a_oSender?.Kill();
		m_bIsPassing = false;

		this.Params.m_oPassCallback?.Invoke(this);
	}
	#endregion // 함수

	#region 접근 함수
	/** 중접 정보 UI 유효 여부를 변경한다 */
	public void SetIsEnableOverlayInfoUIs(bool a_bIsEnable)
	{
		m_bIsEnableOverlayInfoUIs = a_bIsEnable;
		this.UpdateUIsState();
	}

	/** 상태를 변경한다 */
	public void SetState(PopupMissionAdventure.EScrollerCellViewState a_eState)
	{
		this.State = a_eState;
		this.UpdateUIsState();
	}
	#endregion // 접근 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(int a_nGroup,
		int a_nOrder, PopupMissionAdventure a_oPopup, MissionAdventureTable a_oTable, System.Action<PopupMissionAdventureFGScrollerCellView> a_oBuyCallback, System.Action<PopupMissionAdventureFGScrollerCellView> a_oPlayCallback, System.Action<PopupMissionAdventureFGScrollerCellView> a_oPassCallback)
	{
		return new STParams()
		{
			m_nGroup = a_nGroup,
			m_nOrder = a_nOrder,

			m_oTable = a_oTable,
			m_oPopup = a_oPopup,

			m_oBuyCallback = a_oBuyCallback,
			m_oPlayCallback = a_oPlayCallback,
			m_oPassCallback = a_oPassCallback
		};
	}
	#endregion // 클래스 팩토리 함수
}

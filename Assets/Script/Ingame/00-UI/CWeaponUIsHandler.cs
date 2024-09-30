using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

/** 무기 UI 처리자 */
public class CWeaponUIsHandler : MonoBehaviour, IPointerExitHandler
{
	/** 상태 */
	public enum EState
	{
		NONE = -1,
		SEL,
		UNSEL,
		[HideInInspector] MAX_VAL
	}

	/** 매개 변수 */
	public struct STParams
	{
		public int m_nSlotIdx;

		public System.Action<CWeaponUIsHandler, int> m_oTouchCallback;
		public System.Action<CWeaponUIsHandler, int> m_oReloadCallback;
	}

	#region 변수
	private bool m_bIsTouch = false;
	private float m_fReloadSkipTime = 0.0f;
	private EState m_eCurState = EState.UNSEL;
	private Vector3 m_stOriginPos = Vector3.zero;

	private Tween m_oSelAni = null;
	private Tween m_oReloadAni = null;

	[Header("=====> UIs <=====")]
	[SerializeField] private Image m_oIconImg = null;
	[SerializeField] private Image m_oFrameImg = null;
	[SerializeField] private Image m_oSelFrameImg = null;
	[SerializeField] private Image m_oReloadGaugeImg = null;
	[SerializeField] private Image m_oMagazineInactiveGaugeImg = null;

	[SerializeField] private TMP_Text m_oMagazineText = null;
	[SerializeField] private TMP_Text m_oReloadTimeText = null;

	[SerializeField] private Slider m_oLockGaugeSlider = null;
	[SerializeField] private Slider m_oMagazineGaugeSlider = null;
	[SerializeField] private Slider m_oMenualReloadGaugeSlider = null;

	[SerializeField] private SoundButton m_oEquipBtn = null;
	[SerializeField] private SoundButton m_oReloadBtn = null;

	[Header("=====> Game Objects <=====")]
	[SerializeField] private GameObject m_oBodyUIs = null;
	[SerializeField] private GameObject m_oBottomUIs = null;
	[SerializeField] private GameObject m_oStatusUIs = null;

	[SerializeField] private GameObject m_oLockUIs = null;
	[SerializeField] private GameObject m_oReloadUIs = null;
	[SerializeField] private GameObject m_oMenualReloadUIs = null;
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }
	public STLockInfo LockInfo => this.PlayerController.LockInfoList[this.SlotIdx];
	public STReloadInfo ReloadInfo => this.PlayerController.ReloadInfoList[this.SlotIdx];
	public STMagazineInfo MagazineInfo => this.PlayerController.MagazineInfoList[this.SlotIdx];

	public int SlotIdx => this.Params.m_nSlotIdx;
	public PageBattle PageBattle => MenuManager.Singleton.GetPage<PageBattle>(EUIPage.PageBattle);
	public PlayerController PlayerController => this.PageBattle.PlayerController;
	public GameObject ReloadUIs => m_oReloadUIs;
	#endregion // 프로퍼티

	#region IPointerExitHandler
	/** 영역에서 벗어났을 경우 */
	public void OnPointerExit(PointerEventData a_oEventData)
	{
		this.HandleOnTouchEnd(null, a_oEventData);
	}
	#endregion // IPointerExitHandler

	#region 함수
	/** 초기화 */
	public void Awake()
	{
		m_stOriginPos = this.transform.localPosition;

		// 버튼을 설정한다
		var oSndBtn = m_oReloadBtn.GetComponent<SoundButton>();
		oSndBtn.onClick.AddListener(this.OnTouchReloadBtn);
	}

	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;

		// 전달자를 설정한다 {
		var oDispatchers = this.GetComponentsInChildren<CTouchDispatcher>();

		for (int i = 0; i < oDispatchers.Length; ++i)
		{
			oDispatchers[i].SetBeginCallback(this.HandleOnTouchBegin);
			oDispatchers[i].SetEndCallback(this.HandleOnTouchEnd);
		}
		// 전달자를 설정한다 }
	}

	/** 제거 되었을 경우 */
	public void OnDestroy()
	{
		ComUtil.AssignVal(ref m_oSelAni, null);
		ComUtil.AssignVal(ref m_oReloadAni, null);
	}

	/** 상태를 갱신한다 */
	public void Update()
	{
		bool bIsReload = m_bIsTouch && this.PlayerController.IsEnableReloadWeapon(this.SlotIdx);
		m_fReloadSkipTime = bIsReload ? m_fReloadSkipTime + Time.deltaTime : 0.0f;

		// 무기 재장전이 가능 할 경우
		if (bIsReload)
		{
			float fReloadDeltaTime = GlobalTable.GetData<int>(ComType.G_TIME_FOR_RELOAD_PRESS) * ComType.G_UNIT_MS_TO_S;

			// 재장전 대기 시간이 지났을 경우
			if (m_fReloadSkipTime.ExIsGreatEquals(fReloadDeltaTime))
			{
				m_bIsTouch = false;
				m_fReloadSkipTime = 0.0f;

				this.Params.m_oReloadCallback?.Invoke(this, this.SlotIdx);
			}

			this.PageBattle.SetIsDirtyUpdateUIsState(true);
		}
	}

	/** UI 상태를 갱신한다 */
	public void UpdateUIsState()
	{
		// 객체를 설정한다 {
		bool bIsReload = this.PlayerController.IsEnableReloadWeapon(this.SlotIdx);
		bool bIsEmptySlot = this.PlayerController.IsEmptySlot(this.SlotIdx);

		m_oBodyUIs.SetActive(!bIsEmptySlot);
		m_oBottomUIs.SetActive(!bIsEmptySlot);
		m_oStatusUIs.SetActive(!bIsEmptySlot);

		m_oLockUIs.SetActive(this.PlayerController.IsLockWeapon(this.SlotIdx));
		m_oReloadUIs.SetActive(!bIsEmptySlot && this.ReloadInfo.m_fRemainTime.ExIsGreat(0.0f));
		m_oMenualReloadUIs.SetActive(m_bIsTouch && bIsReload);

		m_oSelFrameImg.gameObject.SetActive(m_eCurState == EState.SEL);
		m_oMagazineInactiveGaugeImg.gameObject.SetActive(m_eCurState != EState.SEL);
		m_oReloadBtn.gameObject.SetActive(!bIsEmptySlot && bIsReload && this.MagazineInfo.m_nMaxNumBullets > 1);
		
		m_oEquipBtn.interactable = !bIsEmptySlot;
		// 객체를 설정한다 }

		// 빈 슬롯이 아닐 경우
		if (!bIsEmptySlot)
		{
			float fPercent = this.MagazineInfo.m_nNumBullets / (float)this.MagazineInfo.m_nMaxNumBullets;
			m_oMagazineGaugeSlider.value = Mathf.Clamp01(fPercent);

			float fReloadTime = this.PlayerController.ReloadInfoList[this.SlotIdx].m_fMaxReloadTime - this.PlayerController.ReloadInfoList[this.SlotIdx].m_fRemainTime;
			m_oReloadGaugeImg.fillAmount = Mathf.Clamp01(fReloadTime / this.PlayerController.ReloadInfoList[this.SlotIdx].m_fMaxReloadTime);

			float fLockPercent = this.LockInfo.m_fRemainTime / this.LockInfo.m_fMaxLockTime;
			m_oLockGaugeSlider.value = Mathf.Clamp01(fLockPercent);

			float fReloadDeltaTime = GlobalTable.GetData<int>(ComType.G_TIME_FOR_RELOAD_PRESS) * ComType.G_UNIT_MS_TO_S;

			float fMenualReloadPercent = m_fReloadSkipTime / fReloadDeltaTime;
			m_oMenualReloadGaugeSlider.value = Mathf.Clamp01(fMenualReloadPercent);

			m_oIconImg.sprite = this.PlayerController.EquipWeapons[this.SlotIdx].GetIcon();
			m_oFrameImg.color = this.PageBattle.GradeColorWrapper.GetColor(this.PlayerController.EquipWeapons[this.SlotIdx].nGrade);
			m_oMagazineText.text = $"{this.MagazineInfo.m_nNumBullets} / {this.MagazineInfo.m_nMaxNumBullets}";
			m_oReloadTimeText.text = $"{this.PlayerController.ReloadInfoList[this.SlotIdx].m_fRemainTime:0.0}";
		}
	}

	/** 터치 시작을 처리한다 */
	public void HandleOnTouchBegin(CTouchDispatcher a_oSender, PointerEventData a_oEventData)
	{
		m_bIsTouch = this.PlayerController.IsEnableReloadWeapon(this.SlotIdx);
		m_fReloadSkipTime = 0.0f;

		this.Params.m_oTouchCallback?.Invoke(this, this.SlotIdx);
	}

	/** 터치 종료를 처리한다 */
	public void HandleOnTouchEnd(CTouchDispatcher a_oSender, PointerEventData a_oEventData)
	{
		m_bIsTouch = false;
		m_fReloadSkipTime = 0.0f;

		this.PageBattle.SetIsDirtyUpdateUIsState(true);
	}

	/** 재장전 버튼을 눌렀을 경우 */
	private void OnTouchReloadBtn()
	{
		m_bIsTouch = false;
		m_fReloadSkipTime = 0.0f;

		this.Params.m_oReloadCallback?.Invoke(this, this.SlotIdx);
		this.PageBattle.SetIsDirtyUpdateUIsState(true);
	}
	#endregion // 함수

	#region 접근 함수
	/** 상태를 변경한다 */
	public void SetState(EState a_eState)
	{
		m_eCurState = a_eState;
		m_oSelAni?.Kill();

		Tween oAni = null;
		this.UpdateUIsState();

		switch (a_eState)
		{
			case EState.SEL: oAni = this.transform.DOLocalMoveY(m_stOriginPos.y + ComType.G_OFFSET_Y_WEAPON_UIS_SEL, ComType.G_DURATION_WEAPON_UIS_SEL_ANI); break;
			case EState.UNSEL: oAni = this.transform.DOLocalMoveY(m_stOriginPos.y, ComType.G_DURATION_WEAPON_UIS_SEL_ANI); break;
		}
		
		ComUtil.AssignVal(ref m_oSelAni, oAni);
	}
	#endregion // 접근 함수
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/** 로딩 팝업 */
public class PopupLoading : UIDialog
{
	/** 로딩 타입 */
	public enum ELoadingType
	{
		NONE = -1,
		BATTLE_READY,
		MAX_VAL
	}

	/** 매개 변수 */
	public struct STParams
	{
		public ELoadingType m_eType;
		public PopupLoadingHandler m_oHandler;
		public System.Action<PopupLoading> m_oCallback;
	}

	#region 변수
	private Tween m_oProgressAni = null;

	[Header("=====> UIs <=====")]
	[SerializeField] private Slider m_oProgress = null;
	[SerializeField] private TMP_Text m_oTipText = null;

	[Header("=====> Game Objects <=====")]
	[SerializeField] private GameObject m_oLoadingUIs = null;
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }
	public PopupLoadingHandler Handler = null;
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public void Awake()
	{
		base.Initialize();
	}

	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;
		a_stParams.m_oHandler.SetOwner(this);

		// 텍스트를 설정한다
		m_oTipText.text = ComUtil.GetRandomTipText();

		// 로딩 UI 를 설정한다 {
		string oTypeStr = $"{this.Params.m_eType}";

		for (int i = 0; i < m_oLoadingUIs.transform.childCount; ++i)
		{
			var oChild = m_oLoadingUIs.transform.GetChild(i);
			oChild.gameObject.SetActive(oChild.name.Equals(oTypeStr));
		}
		// 로딩 UI 를 설정한다 }

		this.SetProgress(0.0f);
		this.Params.m_oHandler.StartLoading();
	}

	/** 제거 되었을 경우 */
	public void OnDestroy()
	{
		ComUtil.AssignVal(ref m_oProgressAni, null);
	}

	/** 진행률을 변경한다 */
	public void SetProgress(float a_fProgress)
	{
		m_oProgressAni?.Kill();
		ComUtil.AssignVal(ref m_oProgressAni, DOTween.To(() => m_oProgress.value, (a_fVal) => m_oProgress.value = a_fVal, a_fProgress, ComType.G_DURATION_LOADING_PROGRESS_ANI).SetUpdate(true));
	}

	/** 로딩이 완료 되었을 경우 */
	public void OnCompleteLoading()
	{
		this.SetProgress(1.0f);
		this.Params.m_oCallback?.Invoke(this);
	}

	/** 백 키를 처리한다 */
	public override void Escape()
	{
		// Do Something
	}
	#endregion // 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(ELoadingType a_eType, PopupLoadingHandler a_oHandler, System.Action<PopupLoading> a_oCallback)
	{
		return new STParams()
		{
			m_eType = a_eType,
			m_oHandler = a_oHandler,
			m_oCallback = a_oCallback
		};
	}
	#endregion // 클래스 팩토리 함수
}

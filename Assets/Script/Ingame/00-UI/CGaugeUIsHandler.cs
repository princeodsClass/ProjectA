using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/** 게이지 UI 처리자 */
public class CGaugeUIsHandler : MonoBehaviour
{
	#region 변수
	private Tween m_oGaugeIncrAni = null;
	private Tween m_oGaugeDecrAni = null;

	[SerializeField] private Slider m_oGaugeSlider = null;
	[SerializeField] private Slider m_oMidGaugeSlider = null;
	#endregion // 변수

	#region 프로퍼티
	public Slider GaugeSlider => m_oGaugeSlider;
	public PageBattle PageBattle => MenuManager.Singleton.GetPage<PageBattle>(EUIPage.PageBattle);
	public BattleController BattleController => this.PageBattle?.BattleController;
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public virtual void Awake()
	{
		m_oGaugeSlider.value = 1.0f;
		m_oMidGaugeSlider.value = 1.0f;
	}

	/** 제거 되었을 경우 */
	public virtual void OnDestroy()
	{
		this.ResetAni();
	}

	/** 애니메이션 상태를 리셋한다 */
	public void ResetAni()
	{
		ComUtil.AssignVal(ref m_oGaugeIncrAni, null);
		ComUtil.AssignVal(ref m_oGaugeDecrAni, null);
	}

	/** 퍼센트를 증가시킨다 */
	public void IncrPercent(float a_fPercent)
	{
		this.ResetAni();
		m_oMidGaugeSlider.value = Mathf.Clamp01(a_fPercent);

		ComUtil.AssignVal(ref m_oGaugeIncrAni, DOTween.To(() => m_oGaugeSlider.value, this.SetGaugeVal, Mathf.Clamp01(a_fPercent), 0.5f).SetDelay(0.5f));
	}

	/** 퍼센트를 감소시킨다 */
	public void DecrPercent(float a_fPercent)
	{
		this.ResetAni();
		m_oGaugeSlider.value = Mathf.Clamp01(a_fPercent);
		
		ComUtil.AssignVal(ref m_oGaugeDecrAni, DOTween.To(() => m_oMidGaugeSlider.value, this.SetMidGaugeVal, Mathf.Clamp01(a_fPercent), 0.5f).SetDelay(0.5f));
	}
	#endregion // 함수

	#region 접근 함수
	/** 게이지 값을 변경한다 */
	protected virtual void SetGaugeVal(float a_fVal)
	{
		m_oGaugeSlider.value = Mathf.Clamp01(a_fVal);
	}

	/** 중간 게이지 값을 변경한다 */
	protected virtual void SetMidGaugeVal(float a_fVal)
	{
		m_oMidGaugeSlider.value = Mathf.Clamp01(a_fVal);
	}

	/** 퍼센트를 변경한다 */
	public virtual void SetPercent(float a_fPercent)
	{
		// Do Something
	}
	#endregion // 접근 함수
}

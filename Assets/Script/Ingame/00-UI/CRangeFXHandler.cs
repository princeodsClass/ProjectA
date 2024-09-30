using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 범위 FX 처리자 */
public class CRangeFXHandler : MonoBehaviour
{
	#region 변수
	private GameObject m_oFollowTarget = null;

	[SerializeField] private ParticleSystem m_oOnParticle = null;
	[SerializeField] private ParticleSystem m_oOffParticle = null;
	[SerializeField] private ParticleSystem m_oWaveParticle = null;
	#endregion // 변수

	#region 함수
	/** 초기화 */
	public void Init(GameObject a_oFollowTarget)
	{
		this.SetIsOn(true);
		m_oFollowTarget = a_oFollowTarget;
	}

	/** 상태를 갱신한다 */
	public void LateUpdate()
	{
		this.transform.position = new Vector3(m_oFollowTarget.transform.position.x, 
			m_oFollowTarget.transform.position.y + ComType.G_OFFSET_ATTACK_RANGE_FX, m_oFollowTarget.transform.position.z);
	}
	#endregion // 함수

	#region 접근 함수
	/** 활성 여부를 변경한다 */
	public void SetIsOn(bool a_bIsOn)
	{
		m_oOnParticle.gameObject.SetActive(a_bIsOn);
		m_oOffParticle.gameObject.SetActive(!a_bIsOn);
	}

	/** 범위를 설정한다 */
	public void SetRange(float a_fRange)
	{
		var stOnShapeModule = m_oOnParticle.shape;
		stOnShapeModule.radius = a_fRange;

		var stOffShapeModule = m_oOffParticle.shape;
		stOffShapeModule.radius = a_fRange;

		this.transform.localScale = Vector3.one * (a_fRange / 100.0f);
		m_oWaveParticle.transform.localScale = new Vector3(a_fRange, a_fRange, a_fRange);

		m_oWaveParticle.ExPlay(a_eStopBehavior: ParticleSystemStopBehavior.StopEmittingAndClear);
		m_oOnParticle.ExPlay(a_eStopBehavior: ParticleSystemStopBehavior.StopEmittingAndClear);
		m_oOffParticle.ExPlay(a_eStopBehavior: ParticleSystemStopBehavior.StopEmittingAndClear);
	}
	#endregion // 접근 함수
}

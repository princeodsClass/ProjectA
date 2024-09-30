using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 조준 FX 처리자 */
public class CLockOnFXHandler : MonoBehaviour
{
	#region 변수
	[SerializeField] private ParticleSystem m_oOnParticle = null;
	[SerializeField] private ParticleSystem m_oOffParticle = null;
	
	private GameObject m_oLockOnTarget = null;
	private UnitController m_oFollowTarget = null;
	private CNPCGradeColorWrapper m_oColorWrapper = null;
	#endregion // 변수

	#region 함수
	/** 생성 되었을 경우 */
	public void Awake()
	{
		m_oColorWrapper = this.GetComponentInChildren<CNPCGradeColorWrapper>();
	}

	/** 상태를 갱신한다 */
	public void LateUpdate()
	{
		m_oOnParticle.gameObject.SetActive(m_oLockOnTarget != null);
		m_oOffParticle.gameObject.SetActive(m_oLockOnTarget == null);

		// 대상이 존재 할 경우
		if (m_oFollowTarget != null)
		{
			this.transform.position = new Vector3(m_oFollowTarget.transform.position.x,
				m_oFollowTarget.transform.position.y + ComType.G_OFFSET_LOCK_ON_TARGET_FX, m_oFollowTarget.transform.position.z);
		}
	}
	#endregion // 함수

	#region 접근 함수
	/** 조준 대상을 변경한다 */
	public void SetLockOnTarget(GameObject a_oTarget)
	{
		m_oLockOnTarget = a_oTarget;
	}

	/** 추적 대상을 변경한다 */
	public void SetFollowTarget(UnitController a_oController)
	{
		m_oFollowTarget = a_oController;

		// 추적 대상이 존재 할 경우
		if (a_oController is NonPlayerController && m_oColorWrapper != null)
		{
			var stColor = m_oColorWrapper.GetColor((a_oController as NonPlayerController).Table.Grade);
			var stMainModule = m_oOnParticle.main;

			// 색상이 다를 경우
			if (!stMainModule.startColor.color.ExIsEquals(stColor))
			{
				stMainModule.startColor = stColor;

				m_oOnParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
				m_oOnParticle.Play(true);
			}
		}
	}
	#endregion // 접근 함수
}

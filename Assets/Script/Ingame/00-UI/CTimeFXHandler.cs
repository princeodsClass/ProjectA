using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 지속 효과 처리자 */
public class CTimeFXHandler : MonoBehaviour
{
	#region 변수
	[SerializeField] private ParticleSystem m_oFX = null;
	[SerializeField] private List<ParticleSystem> m_oFXList = new List<ParticleSystem>();
	[SerializeField] private List<ParticleSystem> m_oEndFXList = new List<ParticleSystem>();
	#endregion // 변수

	#region 함수
	/** 무적 효과를 시작한다 */
	public void StartFX(float a_fDuration)
	{
		this.StopFX();

		for (int i = 0; i < m_oFXList.Count; ++i)
		{
			var stFXMainModule = m_oFXList[i].main;
			stFXMainModule.startLifetime = a_fDuration;
		}

		for (int i = 0; i < m_oEndFXList.Count; ++i)
		{
			var stEndFXMainModule = m_oEndFXList[i].main;
			stEndFXMainModule.startDelay = a_fDuration;
		}

		m_oFX.Play(true);
	}

	/** 무적 효과를 중지한다 */
	private void StopFX()
	{
		m_oFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
	}
	#endregion // 함수
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** POA 처리자 */
public class CPOAHandler : MonoBehaviour
{
	#region 변수
	private Collider m_oCollider = null;

	[SerializeField] private GameObject m_oFX = null;
	[SerializeField] private List<ParticleSystem> m_oPOAParticleList = new List<ParticleSystem>();
	#endregion // 변수

	#region 함수
	/** 초기화 */
	public void Awake()
	{
		this.StopFX();
	}

	/** 초기화 */
	public void Start()
	{
		m_oCollider = this.GetComponentInChildren<Collider>();
		m_oCollider.enabled = MenuManager.Singleton.CurScene != ESceneType.Battle;
		m_oCollider.isTrigger = true;
	}

	/** 효과를 시작한다 */
	public void StartFX()
	{
		m_oFX.SetActive(true);
		this.gameObject.SetActive(true);

		m_oCollider.enabled = true;

		for (int i = 0; i < m_oPOAParticleList.Count; ++i)
		{
			m_oPOAParticleList[i].Play(true);
		}
	}

	/** 효과를 중지한다 */
	public void StopFX()
	{
		m_oFX.SetActive(false);
		this.gameObject.SetActive(false);

		for (int i = 0; i < m_oPOAParticleList.Count; ++i)
		{
			m_oPOAParticleList[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
	}
	#endregion // 함수
}

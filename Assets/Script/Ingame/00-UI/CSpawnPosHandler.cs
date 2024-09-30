using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 스폰 위치 처리자 */
public class CSpawnPosHandler : MonoBehaviour
{
	#region 변수
	private Collider m_oCollider = null;
	[SerializeField] private List<ParticleSystem> m_oParticleList = new List<ParticleSystem>();
	#endregion // 변수

	#region 프로퍼티
	public CObjInfo ObjInfo { get; private set; } = null;
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public void Start()
	{
		m_oCollider = this.GetComponentInChildren<Collider>();
		m_oCollider.enabled = true;
		m_oCollider.isTrigger = true;
	}

	/** 효과를 중지한다 */
	public void StopFX()
	{
		m_oCollider.enabled = false;

		for (int i = 0; i < m_oParticleList.Count; ++i)
		{
			m_oParticleList[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			m_oParticleList[i].gameObject.SetActive(false);
		}
	}
	#endregion // 함수

	#region 접근 함수
	/** 객체 정보를 변경한다 */
	public void SetObjInfo(CObjInfo a_oObjInfo)
	{
		this.ObjInfo = a_oObjInfo;
	}
	#endregion // 접근 함수
}

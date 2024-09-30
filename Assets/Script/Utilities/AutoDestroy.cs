using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
	private GameObject m_goCached = null;
	private float m_fLifeTime = 0f;

	public void Init(float fLifeTime)
	{
		if (null == m_goCached) m_goCached = gameObject;
		m_fLifeTime = fLifeTime;

		StopCoroutine("CoProc");
		StartCoroutine(CoProc());
	}

	public void OnDisable()
	{
		StopCoroutine("CoProc");
	}

	IEnumerator CoProc()
	{
		yield return YieldInstructionCache.WaitForSeconds(m_fLifeTime);
		GameResourceManager.Singleton.ReleaseObject(m_goCached);
	}
}

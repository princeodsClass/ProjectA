using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 이펙트 이벤트 처리자 */
public class CFXEventHandler : MonoBehaviour
{
	/** 이펙트 이벤트를 수신했을 경우 */
	public virtual void FX(Object a_oParams)
	{
		var oPrefab = a_oParams as GameObject;

		// 프리팹이 아닐 경우
		if (oPrefab == null)
		{
			return;
		}

		this.PlayFX(oPrefab, this.gameObject);
	}

	/** 이펙트를 재생한다 */
	public GameObject PlayFX(GameObject a_oPrefab, GameObject a_oParent, GameObject a_oDummy = null, float a_fLifeTime = ComType.G_LIFE_T_FX)
	{
		// 프리팹이 없을 경우
		if (a_oPrefab == null)
		{
			return null;
		}

		var oFX = GameResourceManager.Singleton.CreateObject(a_oPrefab,
			a_oParent?.transform, a_oDummy?.transform, a_fLifeTime);

		var oParticle = oFX.GetComponentInChildren<ParticleSystem>();
		oParticle?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		oParticle?.Play(true);

		return oFX;
	}
}

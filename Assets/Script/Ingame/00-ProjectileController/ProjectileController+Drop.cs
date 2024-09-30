using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 투사체 제어자 - 낙하 */
public partial class ProjectileController : MonoBehaviour
{
	#region 함수
	/** 투사체를 떨어뜨린다 */
	public void Drop(Vector3 a_stPos, 
		EffectTable a_oFXTable, float a_fWarningFXDuration, float a_fExplosionRangeRatio, System.Action<ProjectileController, Collider> a_oCallback, bool a_bIsEnableWarning, float a_fDelay = 0.0f)
	{
		this.ResetRigidbody();

		float fRange = a_oFXTable.Value;
		float fExtraFXRange = fRange * a_fExplosionRangeRatio;

		m_fFXRange = (fRange * ComType.G_UNIT_MM_TO_M) + (fExtraFXRange * ComType.G_UNIT_MM_TO_M);
		m_fFXDuration = ComType.G_DURATION_EXPLOSION_ANI;
		m_fFXExtraRangeRatio = a_fExplosionRangeRatio;
		m_oCollisionCallback = a_oCallback;

		// 경고가 필요 할 경우
		if (a_bIsEnableWarning)
		{
			this.SetupThrowWarningFX(a_stPos, m_fFXRange);
			var oWarningParticle = m_oExplosionWarning.GetComponentInChildren<ParticleSystem>();

			var stMainModule = oWarningParticle.main;
			stMainModule.startLifetime = a_fWarningFXDuration;
		}

		m_oTrailRenderer?.Clear();
		StartCoroutine(this.CoDrop(a_stPos, a_oFXTable, a_fExplosionRangeRatio, a_oCallback, a_fDelay));
	}

	/** 낙하 타격 효과를 재생한다 */
	private void PlayHitFXDrop(Collider a_oCollider)
	{
		var oFXObj = GameResourceManager.Singleton.CreateObject(this.ProjectileModelInfo.GetExplosionFX(), 
			this.BattleController.PathObjRoot, this.transform, ComType.G_LIFE_T_HIT_FX);
			
		oFXObj.transform.rotation = Quaternion.identity;
		oFXObj.transform.localScale = Vector3.one * (m_fFXRange * 1.5f);

		oFXObj.GetComponentInChildren<ParticleSystem>()?.Play(true);
	}
	#endregion // 함수

	#region 코루틴 함수
	/** 투사체를 떨어뜨린다 */
	public IEnumerator CoDrop(Vector3 a_stPos, 
		EffectTable a_oFXTable, float a_fExplosionRangeRatio, System.Action<ProjectileController, Collider> a_oCallback, float a_fDelay)
	{
		m_oRigidbody.useGravity = false;
		yield return YieldInstructionCache.WaitForSeconds(a_fDelay);

		m_oRigidbody.useGravity = true;
		this.PlayExplosionBoundsAni(m_fFXRange, m_fFXDuration, null);
	}
	#endregion // 코루틴 함수
}

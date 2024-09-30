using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 데미지 필드 제어자 */
public class DamageFieldController : MonoBehaviour
{
	/** 매개 변수 */
	public struct STParams
	{
		public float m_fATK;
		public float m_fRange;
		public float m_fDuration;

		public EDamageType m_eDamageType;
		public EWeaponType m_eWeaponType;

		public EffectTable m_oFXTable;
		public UnitController m_oOwner;
		public System.Action<DamageFieldController, Collider> m_oCallback;
		public System.Action<DamageFieldController> m_oCompleteCallback;
	}

	#region 변수
	[SerializeField] private ParticleSystem m_oFXParticle = null;
	private int m_nTargetLayerMask = 0;

	private Collider[] m_oOverlapColliders = new Collider[ComType.G_MAX_NUM_OVERLAP_NON_ALLOC];
	private List<GameObject> m_oApplyTargetList = new List<GameObject>();
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;
		m_oFXParticle?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

		m_nTargetLayerMask = LayerMask.GetMask(ComType.G_LAYER_PLAYER, 
			ComType.G_LAYER_NON_PLAYER_01, ComType.G_LAYER_NON_PLAYER_02, ComType.G_LAYER_NON_PLAYER_03, ComType.G_LAYER_NON_PLAYER_04);

		// 유닛이 존재 할 경우
		if(a_stParams.m_oOwner != null)
		{
			string oLayer = ComType.G_UNIT_LAYER_LIST.ExIsValidIdx(a_stParams.m_oOwner.TargetGroup) ? 
				ComType.G_UNIT_LAYER_LIST[a_stParams.m_oOwner.TargetGroup] : ComType.G_LAYER_NON_PLAYER_01;

			m_nTargetLayerMask &= ~LayerMask.GetMask(oLayer);
		}
	}

	/** 데미지를 적용한다 */
	public void ApplyDamage(float a_fInterval = 0.0f)
	{
		StopCoroutine("CoApplyDamage");
		StartCoroutine(this.CoApplyDamage(a_fInterval));
	}

	/** 데미지를 적용한다 */
	private void DoApplyDamage(List<GameObject> a_oGameObjList)
	{
		a_oGameObjList.Clear();
		
		int nResult = Physics.OverlapSphereNonAlloc(this.transform.position, 
			this.Params.m_fRange - 0.25f, m_oOverlapColliders, m_nTargetLayerMask);

		for (int i = 0; i < nResult; ++i)
		{
			bool bIsValid01 = !a_oGameObjList.Contains(m_oOverlapColliders[i].gameObject);
			bool bIsValid02 = m_oOverlapColliders[i].TryGetComponent(out UnitController oController);

			// 타격이 불가능 할 경우
			if (!bIsValid01 || !bIsValid02 || oController == this.Params.m_oOwner || oController.TargetGroup == this.Params.m_oOwner.TargetGroup)
			{
				continue;
			}

			a_oGameObjList.ExAddVal(m_oOverlapColliders[i].gameObject);
			this.Params.m_oCallback?.Invoke(this, m_oOverlapColliders[i]);
		}
	}
	#endregion // 함수

	#region 코루틴 함수
	/** 데미지를 적용한다 */
	private IEnumerator CoApplyDamage(float a_fInterval)
	{
		m_oFXParticle?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		m_oFXParticle?.Play(true);

		m_oFXParticle.transform.localScale = Vector3.one * (this.Params.m_fRange * 2.0f);

		var stMainModule = m_oFXParticle.main;
		stMainModule.startLifetime = this.Params.m_fDuration;

		float fDuration = this.Params.m_fDuration;

		float fInterval = Mathf.Max(0.1f, this.Params.m_oFXTable.Inteval * ComType.G_UNIT_MS_TO_S);
		fInterval = fInterval.ExIsLessEquals(0.0f) ? 1.0f / Application.targetFrameRate : fInterval;
		fInterval = a_fInterval.ExIsLessEquals(0.0f) ? fInterval : a_fInterval;

		yield return YieldInstructionCache.WaitForSeconds(this.Params.m_oFXTable.WaitTime * ComType.G_UNIT_MS_TO_S);
		int nTimes = Mathf.CeilToInt(fDuration / fInterval);

		for (int i = 0; i < nTimes; ++i)
		{
			this.DoApplyDamage(m_oApplyTargetList);
			yield return YieldInstructionCache.WaitForSeconds(fInterval);
		}

		this.Params.m_oCompleteCallback?.Invoke(this);
	}
	#endregion // 코루틴 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(float a_fATK, 
		float a_fRange, float a_fDuration, EDamageType a_eDamageType, EWeaponType a_eWeaponType, EffectTable a_oFXTable, UnitController a_oOwner, System.Action<DamageFieldController, Collider> a_oCallback, System.Action<DamageFieldController> a_oCompleteCallback)
	{
		return new STParams()
		{
			m_fATK = a_fATK,
			m_fRange = a_fRange,
			m_fDuration = a_fDuration,

			m_eDamageType = a_eDamageType,
			m_eWeaponType = a_eWeaponType,

			m_oFXTable = a_oFXTable,
			m_oOwner = a_oOwner,

			m_oCallback = a_oCallback,
			m_oCompleteCallback = a_oCompleteCallback
		};
	}
	#endregion // 클래스 팩토리 함수
}

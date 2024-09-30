using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 효과 모델 정보 */
public class FXModelInfo : MonoBehaviour
{
	/** 기타 효과 정보 */
	[System.Serializable]
	public struct STEtcFXInfo
	{
		[HideInInspector] public GameObject m_oGrenadeWarningFX;
		public GameObject m_oProjectileHitExplosionFX;
	}

	/** 워프 효과 정보 */
	[System.Serializable]
	public struct STWarpFXInfo
	{
		[Header("* 일반")]
		public GameObject m_oWarpStartFX;
		public GameObject m_oWarpEndFX;

		[Header("* 심연")]
		public GameObject m_oAbyssWarpStartFX;
		public GameObject m_oAbyssWarpEndFX;
	}

	/** 버프 효과 정보 */
	[System.Serializable]
	public struct STBuffFXInfo
	{
		public GameObject m_oHPBuffFX;
		public GameObject m_oMoveSpeedBuffFX;
		public GameObject m_oAttackPowerBuffFX;
	}

	/** 디버프 효과 정보 */
	[System.Serializable]
	public struct STDebuffFXInfo
	{
		public GameObject m_oBleedingDebuffFX;
		public GameObject m_oMoveSpeedDebuffFX;
	}

	/** 무적 효과 정보 */
	[System.Serializable]
	public struct STUntouchableFXInfo
	{
		public GameObject m_oKnockbackFX;
		public GameObject m_oUntouchableFX;
		public GameObject m_oUntouchableFXDummy;
	}

	/** 아이스 효과 정보 */
	[System.Serializable]
	public struct STIceFXInfo
	{
		public GameObject m_oIceFX;
		public GameObject m_oIceBreakFX;
		public GameObject m_oIceFieldFX;
	}

	#region 변수
	[Header("=====> 기타 효과 <=====")]
	[SerializeField] private STEtcFXInfo m_stEtcFXInfo;

	[Header("=====> 워프 효과 <=====")]
	[SerializeField] private STWarpFXInfo m_stWarpFXInfo;

	[Header("=====> 버프 효과 <=====")]
	[SerializeField] private STBuffFXInfo m_stBuffFXInfo;

	[Header("=====> 디버프 효과 <=====")]
	[SerializeField] private STDebuffFXInfo m_stDebuffFXInfo;

	[Header("=====> 무적 효과 <=====")]
	[SerializeField] private STUntouchableFXInfo m_stUntouchableFXInfo;

	[Header("=====> 아이스 효과 <=====")]
	[SerializeField] private STIceFXInfo m_stIceFXInfo;




	[Header("오오라 효과")]
	[SerializeField] private GameObject m_oAuraFX = null;

	[Header("유탄 경고 효과")]
	[SerializeField] private GameObject m_oGrenadeWarningFX = null;

	[Header("사망 효과 트랜스 폼")]
	[SerializeField] private Transform m_oDieFXDummy = null;

	[Header("사망 효과")]
	[SerializeField] private GameObject m_oDieFX = null;

	[Header("부활 효과 트랜스 폼")]
	[SerializeField] private Transform m_oReviveFXDummy = null;

	[Header("부활 효과")]
	[SerializeField] private GameObject m_oReviveFX = null;

	[Header("소환 효과")]
	[SerializeField] private GameObject m_oSummonFX = null;

	[Header("소환 완료 효과")]
	[SerializeField] private GameObject m_oSummonCompleteFX = null;

	[Header("역장 효과")]
	[SerializeField] private GameObject m_oForceFieldFX = null;

	[Header("화염 필드 효과")]
	[SerializeField] private GameObject m_oFlameFieldFX = null;

	[Header("점프 공격 시작 효과")]
	[SerializeField] private GameObject m_oJumpAttackStartFX = null;

	[Header("점프 공격 경고 효과")]
	[SerializeField] private GameObject m_oJumpAttackWarningFX = null;

	[Header("점프 공격 종료 효과")]
	[SerializeField] private GameObject m_oJumpAttackFinishFX = null;
	#endregion // 변수

	#region 프로퍼티
	public STEtcFXInfo EtcFXInfo => m_stEtcFXInfo;
	public STWarpFXInfo WarpFXInfo => m_stWarpFXInfo;
	
	public STBuffFXInfo BuffFXInfo => m_stBuffFXInfo;
	public STDebuffFXInfo DebuffFXInfo => m_stDebuffFXInfo;

	public STUntouchableFXInfo UntouchableFXInfo => m_stUntouchableFXInfo;
	public STIceFXInfo IceFXInfo => m_stIceFXInfo;

	public Transform DieFXDummy => m_oDieFXDummy;
	public Transform ReviveFXDummy => m_oReviveFXDummy;

	public GameObject DieFX => m_oDieFX;
	public GameObject AuraFX => m_oAuraFX;
	public GameObject GrenadeWarningFX => m_oGrenadeWarningFX;

	public GameObject ReviveFX => m_oReviveFX;
	public GameObject SummonFX => m_oSummonFX;
	public GameObject SummonCompleteFX => m_oSummonCompleteFX;

	public GameObject ForceFieldFX => m_oForceFieldFX;
	public GameObject FlameFieldFX => m_oFlameFieldFX;

	public GameObject JumpAttackStartFX => m_oJumpAttackStartFX;
	public GameObject JumpAttackWarningFX => m_oJumpAttackWarningFX;
	public GameObject JumpAttackFinishFX => m_oJumpAttackFinishFX;
	#endregion // 프로퍼티
}

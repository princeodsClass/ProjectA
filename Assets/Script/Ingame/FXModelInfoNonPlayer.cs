using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** NPC 효과 모델 정보 */
public class FXModelInfoNonPlayer : MonoBehaviour
{
	/** 사망 효과 정보 */
	[System.Serializable]
	public struct STDieFXInfo
	{
		public GameObject m_oDieFX;
		public GameObject m_oDieFXDummy;
	}

	/** 오오라 효과 정보 */
	[System.Serializable]
	public struct STAuraFXInfo
	{
		public GameObject m_oAuraFX;
		public GameObject m_oAuraFXDummy;
	}

	/** 소환 효과 정보 */
	[System.Serializable]
	public struct STSummonFXInfo
	{
		public GameObject m_oSummonFX;
		public GameObject m_oSummonCompleteFX;
	}

	/** 점프 공격 효과 정보 */
	[System.Serializable]
	public struct STJumpAttackFXInfo
	{
		public GameObject m_oJumpAttackStartFX;
		public GameObject m_oJumpAttackWarningFX;
		public GameObject m_oJumpAttackFinishFX;
	}

	#region 변수
	[Header("=====> 사망 효과 <=====")]
	[SerializeField] private STDieFXInfo m_stDieFXInfo;

	[Header("=====> 오오라 효과 <=====")]
	[SerializeField] private STAuraFXInfo m_stAuraFXInfo;

	[Header("=====> 소환 효과 <=====")]
	[SerializeField] private STSummonFXInfo m_stSummonFXInfo;

	[Header("=====> 점프 공격 효과 <=====")]
	[SerializeField] private STJumpAttackFXInfo m_stJumpAttackFXInfo;
	#endregion // 변수

	#region 프로퍼티
	public STDieFXInfo DieFXInfo => m_stDieFXInfo;
	public STAuraFXInfo AuraFXInfo => m_stAuraFXInfo;
	public STSummonFXInfo SummonFXInfo => m_stSummonFXInfo;
	public STJumpAttackFXInfo JumpAttackFXInfo => m_stJumpAttackFXInfo;
	#endregion // 프로퍼티
}

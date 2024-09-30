using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 플레이어 효과 모델 정보 */
public class FXModelInfoPlayer : MonoBehaviour
{
	/** 부활 효과 정보 */
	[System.Serializable]
	public struct STReviveFXInfo
	{
		public GameObject m_oReviveFX;
		public GameObject m_oReviveFXDummy;
	}

	/** 액티브 스킬 효과 정보 */
	[System.Serializable]
	public struct STActiveSkillFXInfo
	{
		public GameObject m_oActiveSkillFX;
		public GameObject m_oActiveSkillFXDummy;
	}

	#region 변수
	[Header("=====> 부활 효과 <=====")]
	[SerializeField] private STReviveFXInfo m_stReviveFXInfo;

	[Header("=====> 스킬 효과 <=====")]
	[SerializeField] private STActiveSkillFXInfo m_stActiveSkillFXInfo;
	#endregion // 변수

	#region 프로퍼티
	public STReviveFXInfo ReviveFXInfo => m_stReviveFXInfo;
	public STActiveSkillFXInfo ActiveSkillFXInfo => m_stActiveSkillFXInfo;
	#endregion // 프로퍼티
}

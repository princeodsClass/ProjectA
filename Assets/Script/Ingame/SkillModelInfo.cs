using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 스킬 모델 정보 */
public class SkillModelInfo : MonoBehaviour
{
	/** 발사체 정보 */
	[System.Serializable]
	public struct STProjectileInfo
	{
		[Header("탄두")]
		public Transform m_oWarheadDummy;
		public GameObject m_oWarhead;

		[Header("탄피")]
		public Transform m_oShellDummy;
		public GameObject m_oShell;

		[Header("총구 화염 효과")]
		public Transform m_oMuzzleFlashDummy;
		public GameObject m_oMuzzleFlash;
	}

	#region 변수
	[Header("유탄")]
	[SerializeField] private STProjectileInfo m_stGrenadeProjectileInfo;

	[Header("부채꼴 연사")]
	[SerializeField] private STProjectileInfo m_stFanShotProjectileInfo;

	[Header("분열 유탄")]
	[SerializeField] private STProjectileInfo m_stSplitGrenadeProjectileInfo;

	[Header("폭격 요청")]
	[SerializeField] private STProjectileInfo m_stBombingRequestProjectileInfo;
	#endregion // 변수

	#region 프로퍼티
	public STProjectileInfo GrenadeProjectileInfo => m_stGrenadeProjectileInfo;
	public STProjectileInfo FanShotProjectileInfo => m_stFanShotProjectileInfo;
	public STProjectileInfo SplitGrenadeProjectileInfo => m_stSplitGrenadeProjectileInfo;
	public STProjectileInfo BombingRequestProjectileInfo => m_stBombingRequestProjectileInfo;
	#endregion // 프로퍼티
}

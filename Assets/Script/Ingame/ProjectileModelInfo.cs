using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileModelInfo : MonoBehaviour
{
	/** 기타 정보 */
	[SerializeField]
	public struct STEtcInfo
	{
		public GameObject m_oDecal;
	}

	/** 타격 정보 */
	[SerializeField]
	public struct STHitInfo
	{
		public GameObject m_oWoodHitFX;
		public GameObject m_oHumanHitFX;
		public GameObject m_oStoneHitFX;
	}

	/** 폭발 정보 */
	[SerializeField]
	public struct STExplosionInfo
	{
		public GameObject m_oExplosionFX;
		public GameObject m_oDamageFieldFX;
	}

	#region 변수
	[Header("=====> 기타 <=====")]
	[HideInInspector][SerializeField] private STEtcInfo m_stEtcInfo;

	[Header("=====> 타격 <=====")]
	[HideInInspector][SerializeField] private STHitInfo m_stHitInfo;

	[Header("=====> 폭발 <=====")]
	[HideInInspector][SerializeField] private STExplosionInfo m_stExplosionInfo;
	#endregion // 변수

	#region 프로퍼티
	public STEtcInfo EtcInfo => m_stEtcInfo;
	public STHitInfo HitInfo => m_stHitInfo;
	public STExplosionInfo ExplosionInfo => m_stExplosionInfo;
	#endregion // 프로퍼티





	[Header("폭발 (FX)")]
	[SerializeField] private GameObject _goExplosionFX;

	[Header("데미지 필드 (FX)")]
	[SerializeField] private GameObject _goDamageFieldFX;

	[Header("사람 피격 프리팹 ( FX )")]
	[SerializeField]
	GameObject _goImpactHuman;

	[Header("나무 피격 프리팹 ( FX )")]
	[SerializeField]
	GameObject _goImpactWood;

	[Header("돌 피격 프리팹 ( FX )")]
	[SerializeField]
	GameObject _goImpactStone;

	[Header("피격 흔적 ( 데칼 )")]
	[SerializeField]
	GameObject _goImpactDecal;

	public enum EImpactType
	{
		Common,
		Human,
		Wood,
		Stone,
		End,
	};

	public GameObject GetExplosionFX()
	{
		return _goExplosionFX;
	}

	public GameObject GetDamageFieldFX()
	{
		return _goDamageFieldFX;
	}

	public GameObject GetImpactFXObject(EImpactType type)
	{
		switch (type)
		{
			case EImpactType.Human:
				return _goImpactHuman;
			case EImpactType.Wood:
				return _goImpactWood;
			case EImpactType.Common:
			case EImpactType.Stone:
			default:
				return _goImpactStone;
		}
	}

	public GameObject GetImpactDecalObject(EImpactType type)
	{
		switch (type)
		{
			case EImpactType.Human:
			case EImpactType.Wood:
			case EImpactType.Common:
			case EImpactType.Stone:
			default:
				return _goImpactDecal;
		}
	}
}
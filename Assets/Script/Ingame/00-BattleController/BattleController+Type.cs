using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/** 전투 제어자 - 타입 */
public partial class BattleController : MonoBehaviour
{
	/** 웨이브 상태 */
	private enum EWaveState
	{
		NONE = -1,
		PLAY,
		INTERVAL,
		FINISH,
		[HideInInspector] MAX_VAL
	}

	/** 타격 UI 정보 */
	public struct STHitUIsInfo
	{
		public int m_nDamage;
		public EHitType m_eHitType;
		public Vector3 m_stPos;
	}

	/** 릴리즈 유닛 정보 */
	public struct STReleaseUnitInfo
	{
		public GameObject m_oUnitObj;
		public GameObject m_oWeaponObj;
	}
}

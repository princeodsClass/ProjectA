using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/** 전투 제어자 - 접근 */
public partial class BattleController : MonoBehaviour
{
	#region 접근 함수
	/** 소환 위치를 반환한다 */
	public Vector3 GetSummonPos(NavMeshAgent a_oNavMeshAgent, int a_nExtraOffset = 0, float a_fFilterRange = 0.0f)
	{
		bool bIsSuccess = false;
		return this.GetSummonPos(a_oNavMeshAgent, ref bIsSuccess, a_nExtraOffset, a_fFilterRange);
	}

	/** 소환 위치를 반환한다 */
	public Vector3 GetSummonPos(NavMeshAgent a_oNavMeshAgent, ref bool a_bIsSuccess, int a_nExtraOffset = 0, float a_fFilterRange = 0.0f)
	{
		for (int i = 0; i < ComType.G_MAX_TRY_TIMES_FIND_SUMMON_POS; ++i)
		{
			var stPos = new Vector3(Random.Range(this.PlayerNavMeshBounds.min.x - a_nExtraOffset, this.PlayerNavMeshBounds.max.x + a_nExtraOffset),
				0.0f, Random.Range(this.PlayerNavMeshBounds.min.z - a_nExtraOffset, this.PlayerNavMeshBounds.max.z + a_nExtraOffset));

			// 내비게이션 영역을 벗어났을 경우
			if (!NavMesh.SamplePosition(stPos + (Vector3.up * 1.5f), out NavMeshHit stNavMeshHit, float.MaxValue / 2.0f, m_nWalkableAreaMask))
			{
				continue;
			}

			bool bIsValidA = a_oNavMeshAgent.CalculatePath(stNavMeshHit.position, m_oNavMeshPath);
			bool bIsValidB = Vector3.Distance(a_oNavMeshAgent.transform.position, stNavMeshHit.position).ExIsGreatEquals(a_fFilterRange);

			// 이동이 가능 할 경우
			if (bIsValidA && bIsValidB && m_oNavMeshPath.status == NavMeshPathStatus.PathComplete)
			{
				a_bIsSuccess = true;
				return stNavMeshHit.position;
			}
		}

		a_bIsSuccess = false;
		return Vector3.zero;
	}

	/** 효과 적용 여부를 변경한다 */
	public void SetIsApplyEffects(bool a_bIsApply)
	{
		this.IsApplyEffects = a_bIsApply;
	}

	/** 지속 스킬 적용 여부를 변경한다 */
	public void SetIsApplyContinueSkill(bool a_bIsApply)
	{
		this.IsApplyContinueSkill = a_bIsApply;
	}

	/** 갱신 가능 여부를 변경한다 */
	public void SetIsEnableUpdate(bool a_bIsEnable)
	{
		this.IsEnableUpdate = a_bIsEnable;
	}

	/** NPC 제거 여부를 변경한다 */
	public void SetIsKillNonPlayers(bool a_bIsKill)
	{
		m_nNumKillNonPlayers += 1;
		this.IsKillNonPlayers = a_bIsKill;
	}

	/** 내비게이션 메쉬 제어자를 설정한다 */
	public void SetNavMeshController(CNavMeshController a_oNavMeshController)
	{
		this.NavMeshController = a_oNavMeshController;
	}

	/** 사냥 클리어 경험치를 반환한다 */
	private int GetHuntClearExp(uint a_nKey, int a_nExp)
	{
		var oHuntTable = HuntTable.GetData(a_nKey);
		int nClearLevel = (oHuntTable.Episode - 1) * 10 + oHuntTable.Order;
		float fHuntClearExpRatio = GlobalTable.GetData<float>(ComType.G_RATIO_HUNT_REWARD);

		float fTotalHuntClearExpRatio = (nClearLevel - GameManager.Singleton.user.m_nHuntLevel) * fHuntClearExpRatio;
		return Mathf.Max(0, a_nExp + (int)(a_nExp * fTotalHuntClearExpRatio));
	}

	/** 재료 아이템 객체 프리팹 이름을 반환한다 */
	private string GetGroundItemObjMaterialPrefabName(uint a_nKey)
	{
		var oMaterialTable = MaterialTable.GetData(a_nKey);

		// 프리팹 이름이 존재 할 경우
		if (ComType.G_PREFAB_N_GROUND_ITEM_MATERIAL_DICT.TryGetValue((EItemType)oMaterialTable.Type, out Dictionary<int, string> oNameDict))
		{
			return oNameDict.GetValueOrDefault(oMaterialTable.SubType, ComType.G_BG_N_ITEM_GROUND_MATERIAL);
		}

		return ((EItemType)oMaterialTable.Type == EItemType.Material) ? ComType.G_BG_N_ITEM_GROUND_MATERIAL : string.Empty;
	}
	#endregion // 접근 함수
}

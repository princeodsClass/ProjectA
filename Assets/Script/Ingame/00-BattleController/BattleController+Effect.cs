using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 전투 제어자 - 효과 */
public partial class BattleController : MonoBehaviour
{
	#region 함수
	/** 데미지 필드 효과를 적용한다 */
	public void ApplyDamageFieldEffect(EffectTable a_oEffectTable, bool a_bIsIgnoreDelay = true)
	{
		StartCoroutine(this.DoApplyDamageFieldEffect(a_oEffectTable, a_bIsIgnoreDelay));
	}

	/** 데미지 필드 효과를 적용한다 */
	private IEnumerator DoApplyDamageFieldEffect(EffectTable a_oEffectTable, bool a_bIsIgnoreDelay = true)
	{
		yield return a_bIsIgnoreDelay ? 
			YieldInstructionCache.WaitForEndOfFrame : new WaitForSeconds(3.0f);

		float fRange = a_oEffectTable.Value * ComType.G_UNIT_MM_TO_M;
		float fDuration = a_oEffectTable.Duration * ComType.G_UNIT_MS_TO_S;

		var stTargetPos = this.PlayerController.transform.position;
		var stDamageFieldPos = stTargetPos + new Vector3(Random.Range(-fRange, fRange), 0.0f, Random.Range(-fRange, fRange));

		var stParams = DamageFieldController.MakeParams(0.0f,
			fRange, fDuration, EDamageType.NONE, EWeaponType.Unknown, a_oEffectTable, null, this.HandleOnApplyDamageField, this.OnCompleteApplyDamage);

		var oDamageFieldController = GameResourceManager.Singleton.CreateObject<DamageFieldController>(this.FXModelInfo.ForceFieldFX,
			this.PathObjRoot, null, fDuration * 2.0f);

		oDamageFieldController.transform.position = stDamageFieldPos + (Vector3.up * 0.1f);
		oDamageFieldController.transform.localScale = Vector3.one;
		oDamageFieldController.transform.localEulerAngles = Vector3.zero;

		oDamageFieldController.Init(stParams);
		oDamageFieldController.ApplyDamage(0.1f);
	}

	/** 데미지 필드 적용을 처리한다 */
	public void HandleOnApplyDamageField(DamageFieldController a_oSender, Collider a_oCollider)
	{
		// 플레이어 제어자가 없을 경우
		if (!a_oCollider.gameObject.TryGetComponent(out PlayerController oController))
		{
			return;
		}

		ComUtil.AddEffect(EEquipEffectType.FORCE_WALK, 
			EEquipEffectType.FORCE_WALK, 0.35f, 0.25f, 0.0f, oController.ActiveEffectStackInfoList, 1);
			
		oController.SetupAbilityValues(true);
	}

	/** 데미지 적용이 완료 되었을 경우 */
	public void OnCompleteApplyDamage(DamageFieldController a_oSender)
	{
		StopCoroutine("CoApplyDamage");
		StartCoroutine(this.CoApplyDamage(a_oSender));
	}
	#endregion // 함수

	#region 코루틴 함수
	/** 데미지를 적용한다 */
	private IEnumerator CoApplyDamage(DamageFieldController a_oSender)
	{
		yield return YieldInstructionCache.WaitForSeconds(1.0f);
		this.ApplyDamageFieldEffect(a_oSender.Params.m_oFXTable);
	}
	#endregion // 코루틴 함수
}

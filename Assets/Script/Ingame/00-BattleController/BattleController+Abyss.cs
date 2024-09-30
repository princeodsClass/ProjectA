using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/** 전투 제어자 - 심연 */
public partial class BattleController : MonoBehaviour
{
	#region 함수
	/** 심연 포인트를 증가시킨다 */
	public void IncrAbyssPoint(int a_nPoint)
	{
		// 포인트 증가가 불가능 할 경우
		if(this.MaxAbyssPoint <= 0)
		{
			return;
		}

		this.AbyssPoint = Mathf.Clamp(this.AbyssPoint + a_nPoint, 0, this.MaxAbyssPoint);
		this.PageBattle.GaugeAbyssUIsHandler.IncrPercent(this.AbyssPoint / (float)this.MaxAbyssPoint);
	}

	/** 플레이어 워프 위치를 설정한다 */
	private void SetupPlayerWarpPos()
	{
		this.PlayerController.transform.SetParent(this.PlayMapObjsRoot.transform, false);
		this.PlayerController.transform.localPosition = GameDataManager.Singleton.PlayMapInfo.m_stPlayerPos;
		this.PlayerController.transform.localEulerAngles = GameDataManager.Singleton.PlayMapInfo.m_stPlayerRotate;
	}

	/** 플레이어 워프가 완료 되었을 경우 */
	private void OnCompletePlayerWarp()
	{
		this.PlayerController.SetUntouchableTime(0.0f);
		this.PlayerController.StateMachine.SetIsEnable(true);
		this.PlayerController.ContactStateMachine.SetState(this.PlayerController.CreateContactGroundState());
	}

	/** 보스 등장을 등장시킨다 */
	private void TryMakeAppearBoss()
	{
		// 보스 등장이 불가능 할 경우
		if (this.AbyssPoint < this.MaxAbyssPoint || this.IsMakeAppearBossNPC)
		{
			return;
		}

		StopCoroutine("CoMakeAppearBoss");
		StartCoroutine(this.CoMakeAppearBoss());
	}

	/** 다음 스테이지로 워프한다 */
	private void WarpToNextStage(CMapInfo a_oMapInfo, int a_nPlayStageID, Collider a_oCollider)
	{
		// 보스가 등장했을 경우
		if (this.IsMakeAppearBossNPC)
		{
			return;
		}

		this.PlayerController.StateMachine.SetIsEnable(false);
		this.PlayerController.ContactStateMachine.SetState(this.PlayerController.CreateContactWarpState());

		this.PlayerController.SetUntouchableTime(byte.MaxValue);
		this.PlayerController.gameObject.ExSetLayer(LayerMask.NameToLayer("Default"));

		StopCoroutine("CoHandleWarpToNextStage");
		StartCoroutine(this.CoHandleWarpToNextStage(a_oMapInfo, a_nPlayStageID, a_oCollider));
	}
	#endregion // 함수
}

/** 전투 제어자 - 심연 코루틴 */
public partial class BattleController : MonoBehaviour
{
	#region 함수
	/** 보스 NPC 를 등장시킨다 */
	private IEnumerator CoMakeAppearBoss()
	{
		this.PlayerController.NavMeshAgent.enabled = true;

		var stSummonPos = this.GetSummonPos(this.PlayerController.NavMeshAgent, -5, 10.0f);
		this.PlayerController.NavMeshAgent.enabled = false;

		// 소환 위치가 유효하지 않을 경우
		if (stSummonPos.ExIsEquals(Vector3.zero))
		{
			yield break;
		}

		// 보스 NPC 가 없을 경우
		if (!this.BossNPCObjInfoList.ExIsValid())
		{
			this.FinishBattle(true);
			yield break;
		}

		for (int i = 0; i < this.POAHandlerList.Count; ++i)
		{
			this.POAHandlerList[i].StopFX();
		}

		for (int i = 0; i < this.WarpGateHandlerList.Count; ++i)
		{
			this.WarpGateHandlerList[i].StopFX();
		}

		this.IsBossStage = true;
		this.IsMakeAppearBossNPC = true;

		yield return YieldInstructionCache.WaitForSeconds(0.5f);
		this.PlayerController.StartUntouchableFX(2.0f);

		var oNonPlayerController = this.CreateNonPlayer(this.BossNPCTableList[0],
			stSummonPos, this.PlayMapObjsRoot, null, a_oEffectTableList: this.NPCEffectTableList);

		oNonPlayerController.transform.localScale = this.BossNPCObjInfoList[0].m_stTransInfo.m_stScale;
		oNonPlayerController.StartUntouchableFX(2.0f);

		yield return YieldInstructionCache.WaitForEndOfFrame;

		oNonPlayerController.LookTarget(this.PlayerController);
		oNonPlayerController.SetExtraGaugeUIsHandler(this.PageBattle.GaugeBossUIsHandler);

		this.PageBattle.GaugeBossUIsHandler.Init(CGaugeBossUIsHandler.MakeParams(oNonPlayerController.Table));
		this.StopAllNonPlayerRagdolls();

		float fPrevTimeScale = Time.timeScale;
		ComUtil.SetTimeScale(0.01f);

		// TODO: 중복 코드 개선 필요
		var fOriginCameraHeight = this.CameraMove._fCamHeight;
		var fOriginCameraForward = this.CameraMove._fCamForward;
		var fOriginCamDummyDistance = this.CamDummy.GetComponent<CamDummy>()._fDistance;

		this.StartBossGaugeDirecting();
		oNonPlayerController.SetupOpenAni();

		var oAnimator = this.PageBattle.GaugeBossUIsHandler.GetComponent<Animator>();
		oAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

		this.StartCameraDirecting(ComType.G_OFFSET_CAMERA_HEIGHT_FOR_FOCUS,
			ComType.G_OFFSET_CAMERA_FORWARD_FOR_FOCUS, ComType.G_OFFSET_CAMERA_DISTANCE_FOR_FOCUS, oNonPlayerController.gameObject, true, true);

		yield return YieldInstructionCache.WaitForSecondsRealtime(3.5f);
		ComUtil.SetTimeScale(fPrevTimeScale);

		var oState = this.StateMachine.State as CStateBattleController;
		oState?.SetIsCompleteCameraDirecting(true);

		oNonPlayerController.SetTrackingTarget(this.PlayerController);
		oNonPlayerController.OnBattlePlay();

		this.StartCameraDirecting(fOriginCameraHeight,
			fOriginCameraForward, fOriginCamDummyDistance, this.PlayerController.gameObject, true);
	}

	/** 다음 스테이지로 워프를 처리한다 */
	private IEnumerator CoHandleWarpToNextStage(CMapInfo a_oMapInfo, int a_nPlayStageID, Collider a_oCollider)
	{
		float fWarpDuration = 0.5f;
		float fWarpReadyDuration = 0.5f;
		float fFinalWarpDuration = fWarpReadyDuration + fWarpDuration + 1.0f;

		var oCamDummy = this.CamDummy.GetComponent<CamDummy>();
		var stWarpReadyPos = a_oCollider.transform.position + (Vector3.up * 0.5f);

		m_oPlayerWarpAni?.ExKill();
		this.PlayerController.Canvas.SetActive(false);

		var oSequence = DOTween.Sequence();
		oSequence.Append(this.PlayerController.transform.DOMove(stWarpReadyPos, fWarpReadyDuration));
		oSequence.Append(this.PlayerController.transform.DOMove(stWarpReadyPos + (Vector3.up * -25.0f), 1.5f));
		oSequence.AppendCallback(() => oSequence?.Kill());

		// 워프 효과가 존재 할 경우
		if (this.FXModelInfo.WarpFXInfo.m_oAbyssWarpStartFX != null)
		{
			var oWarpStartFX = GameResourceManager.Singleton.CreateObject<ParticleSystem>(this.FXModelInfo.WarpFXInfo.m_oAbyssWarpStartFX,
				null, null, 5.0f);

			oWarpStartFX.transform.position = a_oCollider.transform.position;

			oWarpStartFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			oWarpStartFX.Play(true);
		}

		ComUtil.AssignVal(ref m_oPlayerWarpAni, oSequence);
		yield return YieldInstructionCache.WaitForSeconds(fWarpReadyDuration);

		oCamDummy.SetIsEnableUpdate(false);
		yield return YieldInstructionCache.WaitForSeconds(fFinalWarpDuration);

		int nNextStageID = (a_nPlayStageID + 1) % GameDataManager.Singleton.PlayMapInfoDict.Count;
		GameResourceManager.Singleton.Clear();

		this.PlayerController.Canvas.SetActive(true);
		oCamDummy.SetIsEnableUpdate(true);

		this.PlayerController.gameObject.ExSetLayer(LayerMask.NameToLayer("Player"));
		this.PlayerController.Canvas.gameObject.ExSetLayer(LayerMask.NameToLayer("OverlayUIs"));

		// 보스 NPC 가 등장했을 경우
		if (this.IsMakeAppearBossNPC)
		{
			this.OnCompletePlayerWarp();
			yield break;
		}

		GameDataManager.Singleton.SetPlayStageID(nNextStageID);
		this.SetupNonPlayers(GameDataManager.Singleton.PlayMapInfo, this.PlayMapObjsRoot);

		GameResourceManager.Singleton.Clear();
		this.SetupPlayerWarpPos();

		yield return YieldInstructionCache.WaitForEndOfFrame;
		this.PlayerController.transform.localEulerAngles = GameDataManager.Singleton.PlayMapInfo.m_stPlayerRotate;

		// 워프 효과가 존재 할 경우
		if (this.FXModelInfo.WarpFXInfo.m_oAbyssWarpEndFX != null)
		{
			var oWarpEndFX = GameResourceManager.Singleton.CreateObject<ParticleSystem>(this.FXModelInfo.WarpFXInfo.m_oAbyssWarpEndFX,
				this.PlayerController.transform, null, 5.0f);

			oWarpEndFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			oWarpEndFX.Play(true);
		}

		this.IsMoveToNextStage = false;
		this.SetupNavMeshCameraBounds();

		yield return YieldInstructionCache.WaitForSeconds(0.5f);
		this.OnCompletePlayerWarp();

		for (int i = 0; i < this.DropWeaponList.Count; ++i)
		{
			GameResourceManager.Singleton.ReleaseObject(this.DropWeaponList[i]);
		}

		foreach(var stKeyVal in this.OriginUnitControllerDictContainer)
		{
			// 플레이어 그룹 일 경우
			if(stKeyVal.Key == (int)ETargetGroup.PLAYER || stKeyVal.Key == (int)ETargetGroup.COMMON)
			{
				continue;
			}

			for(int i = 0; i < stKeyVal.Value.Count; ++i)
			{
				stKeyVal.Value[i].OnBattlePlay();
			}
		}
	}
	#endregion // 함수
}

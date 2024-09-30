using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 터렛 제어자 */
public partial class TurretController : NonPlayerController
{
	#region 변수
	[Header("=====> Turret Controller - Etc <=====")]
	[SerializeField] private bool m_bIsResetSpineRotate = false;
	[SerializeField] private bool m_bIsForceRotateTarget = false;

	[SerializeField] private float m_fUnderlayOffset = 0.0f;
	[SerializeField] private float m_fWeaponFixDistance = 0.0f;

	private Vector3 m_stOriginUnderlayForward = Vector3.zero;

	[Header("=====> Turret Controller - Game Objects <=====")]
	[SerializeField] private GameObject m_oUnderlay = null;
	private GameObject m_oUnderlayObj = null;
	#endregion // 변수

	#region 프로퍼티
	public override bool IsEnableMove => false;
	public override bool IsEnableTracking => false;
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public override void Start()
	{
		base.Start();

		// 밑받침이 존재 할 경우
		if (m_oUnderlay != null)
		{
			m_oUnderlayObj = GameResourceManager.Singleton.CreateObject(m_oUnderlay, this.transform, null);
			m_stOriginUnderlayForward = m_oUnderlayObj.transform.forward;
		}

		// 무기 위치 고정이 필요 없을 경우
		if (this.WeaponTrans != null || this.CurWeaponObj == null)
		{
			return;
		}

		// 무기를 설정한다 {
		var stOffset = this.transform.forward * m_fWeaponFixDistance;

		this.CurWeaponObj.transform.forward = this.transform.forward;
		this.CurWeaponObj.transform.position = this.transform.position + stOffset;
		// 무기를 설정한다 }
	}

	/** 초기화 */
	public override void Init(NPCTable a_oTable, NPCStatTable a_oStatTable, CObjInfo a_oObjInfo)
	{
		base.Init(a_oTable, a_oStatTable, a_oObjInfo);

		this.CurWeaponObj.transform.localRotation = GameResourceManager.Singleton.LoadPrefabs(EResourceType.Weapon,
			this.Table.WeaponPrefab).transform.localRotation;
	}

	/** 상태를 갱신한다 */
	public override void OnLateUpdate(float a_fDeltaTime)
	{
		base.OnLateUpdate(a_fDeltaTime);

		// 밑받침 객체 갱신이 필요 없을 경우
		if (m_oUnderlayObj == null || MenuManager.Singleton.CurScene != ESceneType.Battle)
		{
			return;
		}

		m_oUnderlayObj.transform.forward = m_stOriginUnderlayForward;
	}

	/** 지정 위치를 바라본다 */
	public override void LookAt(Vector3 a_stPos, bool a_bIsLerp = false, float a_fLerpTime = 0.0f)
	{
#if DISABLE_THIS
		bool bIsInvalidLookAt = this.WeaponAniType == EWeaponAnimationType.Mortar && 
			(this.IsFire || this.MagazineInfo.m_nNumBullets <= 0);

		// 무기 위치 고정이 필요 없을 경우
		if (bIsInvalidLookAt || this.WeaponTrans != null || this.CurWeaponObj == null)
		{
			return;
		}
#endif // #if DISABLE_THIS

		base.LookAt(a_stPos, a_bIsLerp, a_fLerpTime * 0.1f);

		// 무기 고정 타입 일 경우
		if (this.IsWeaponFixType)
		{
			this.CurWeaponObj.transform.forward = this.transform.forward;
		}
	}

	/** 스파인를 회전한다 */
	public override void RotateSpine(UnitController a_oTarget, float a_fWeight)
	{
		// 스파인 회전 리셋 모드 일 경우
		if (m_bIsResetSpineRotate)
		{
			this.Spine.transform.localRotation = Quaternion.identity;
		}

		base.RotateSpine(a_oTarget, a_fWeight);
	}

	/** 렉돌 애니메이션을 처리한다 */
	protected override void HandleRagdollAni(Vector3 a_stDirection, STHitInfo a_stHitInfo)
	{
		// 제거 상태 일 경우
		if(this.IsDestroy || !this.gameObject.activeSelf)
		{
			return;
		}

		a_stHitInfo.m_fRagdollMinForce = a_stHitInfo.m_fRagdollMinForce * 2.0f;
		a_stHitInfo.m_fRagdollMaxForce = Mathf.Max(a_stHitInfo.m_fRagdollMinForce, a_stHitInfo.m_fRagdollMaxForce);

		base.HandleRagdollAni(a_stDirection, a_stHitInfo);

		// 렉돌 제어자가 없을 경우
		if (!this.CurWeaponObj.TryGetComponent(out RagdollController oController))
		{
			return;
		}

		oController.StartRagdollAni(Vector3.one * 5.0f);
	}
	#endregion // 함수
}

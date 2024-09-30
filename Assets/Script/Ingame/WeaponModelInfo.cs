using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class WeaponModelInfo : MonoBehaviour
{
	[Header("hand 트랜스폼")]
	[SerializeField]
	Transform _tHandDummy;

	[Header("warhead 트랜스폼")]
	[SerializeField]
	Transform _tWarheadDummy;

	[Header("warhead ( 탄두 ) 프리팹")]
	[SerializeField]
	GameObject _goWaehead;

	[Header("shell ( 탄피 ) 트랜스폼")]
	[SerializeField]
	Transform _tShellDummy;

	[Header("shell ( 탄피 ) 프리팹")]
	[SerializeField]
	GameObject _goShell;

	[Header("muzzle flash 트랜스폼")]
	[SerializeField]
	Transform _tMuzzleFlashDummy;

	[Header("muzzleflash ( 화염 ) 프리팹")]
	[SerializeField]
	GameObject _goMuzzleFlash;

	[Header("sight line 트랜스폼")]
	[SerializeField]
	Transform _tSightLineDummy;

	[Header("sight line 프리팹")]
	[SerializeField]
	GameObject _goSightLine;

	[SerializeField]
	GameObject _goSightLinePlayer;

	[Header("ui 중심 게임 오브젝트")]
	[SerializeField]
	GameObject _goUIDummy;

	public Transform GetHandDummyTransform()
	{
		return _tHandDummy;
	}

	public Transform GetWarheadDummyTransform()
	{
		return _tWarheadDummy;
	}

	public Transform GetShellDummyTransform()
	{
		return _tShellDummy;
	}

	/// <summary>
	/// ui 중심으로 돌릴 ventor3 ui 에서 보여질 scale / rotation 모두 처리.
	/// </summary>
	/// <returns></returns>
	public GameObject GetUIDummy()
	{
		return _goUIDummy;
	}
}

#region 추가
/** 무기 모델 정보 */
public partial class WeaponModelInfo : MonoBehaviour
{
	#region 함수
	/** 초기화 */
	public void Awake()
	{
		var oRigidbody = this.GetComponent<Rigidbody>();

		// 리지드 바디가 존재 할 경우
		if (oRigidbody != null)
		{
			oRigidbody.useGravity = false;
			oRigidbody.isKinematic = true;
			oRigidbody.constraints = RigidbodyConstraints.FreezeAll;
		}
	}

	/** 화염 효과 트랜스 폼을 반환한다 */
	public Transform GetMuzzleFlashDummyTransform()
	{
		return _tMuzzleFlashDummy;
	}

	/** 조준선 트랜스 폼을 반환한다 */
	public Transform GetSightLineDummyTransform()
	{
		return _tSightLineDummy;
	}

	/** 탄피를 반환한다 */
	public GameObject GetShell()
	{
		return _goShell;
	}

	/** 탄두를 반환한다 */
	public GameObject GetWarhead()
	{
		return _goWaehead;
	}

	/** 화염 효과를 반환한다 */
	public GameObject GetMuzzleFlash()
	{
		return _goMuzzleFlash;
	}

	/** 조준선을 반환한다 */
	public GameObject GetSightLine()
	{
		return _goSightLine;
	}

	/** 조준선을 반환한다 */
	public GameObject GetSightLinePlayer()
	{
		return _goSightLinePlayer;
	}
	#endregion // 함수
}
#endregion // 추가

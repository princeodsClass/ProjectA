using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CamDummy : MonoBehaviour
{
	[SerializeField]
	CameraMove _cam;

	[SerializeField] GameObject _Player;
	Transform _targetTransform;

	Bounds m_stBounds;
	Vector3 _vec = Vector3.zero;
	public float _fDistance = 3.5f;
	public float ttt = 0.6f;
	public EMapInfoType m_eMapInfoType = EMapInfoType.NONE;

	#region 프로퍼티
	public GameObject Target => _Player;
	
	public bool bIsRealtime { get; set; } = false;
	public bool IsEnableUpdate { get; private set; } = true;
	public bool IsIgnoreDirection { get; set; } = false;
	#endregion // 프로퍼티

	private void Awake()
	{
#if DISABLE_THIS
		_fDistance = 3.5f;
		ttt = 0.6f;
#endif // #if DISABLE_THIS
	}

	public void SetBounds(Bounds a_stBounds)
	{
		m_stBounds = a_stBounds;
	}

	public void SetTarget(GameObject go)
	{
		_Player = go;
		_cam.SetPlayer(go);
	}

	public void Update()
	{
		// 플레이어가 없을 경우
		if (_Player == null || !this.IsEnableUpdate)
		{
			return;
		}

		this.SetupCameraPos(false);
	}

	#region 접근 함수
	/** 갱신 가능 여부를 변경한다 */
	public void SetIsEnableUpdate(bool a_bIsEnable)
	{
		this.IsEnableUpdate = a_bIsEnable;
	}

	/** 카메라 위치를 설정한다 */
	public void SetupCameraPos(bool a_bIsImmediate)
	{
		_targetTransform = _Player.transform;
		var stDirection = this.IsIgnoreDirection ? Vector3.forward : _targetTransform.forward;

		var stDestPos = _targetTransform.position + stDirection * _fDistance;
		stDestPos.x = Mathf.Clamp(stDestPos.x, m_stBounds.min.x, m_stBounds.max.x);
		stDestPos.z = Mathf.Clamp(stDestPos.z, m_stBounds.min.z, m_stBounds.max.z);

		// 즉시 적용 모드 일 경우
		if (a_bIsImmediate)
		{
			transform.SetPositionAndRotation(stDestPos, Quaternion.identity);
		}
		else
		{
			float fDeltaTime = this.bIsRealtime ? Time.unscaledDeltaTime : Time.deltaTime;
			transform.SetPositionAndRotation(Vector3.SmoothDamp(transform.position, stDestPos, ref _vec, ttt, Mathf.Infinity, fDeltaTime), Quaternion.identity);
		}
	}
	#endregion // 접근 함수
}

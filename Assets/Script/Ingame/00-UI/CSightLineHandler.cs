using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 조준선 처리자 */
public class CSightLineHandler : MonoBehaviour
{
	#region 변수
	private float m_fWidthRate = 1.0f;
	private float m_fOriginWidth = 0.0f;

	private LineRenderer m_oLineRenderer = null;

	[Header("=====> Game Objects <=====")]
	private GameObject m_oTarget = null;
	private GameObject m_oOriginDirectionTarget = null;
	#endregion // 변수

	#region 프로퍼티
	public int TargetLayerMask { get; private set; } = 0;
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public void Awake()
	{
		m_oLineRenderer = this.GetComponentInChildren<LineRenderer>();
		m_fOriginWidth = m_oLineRenderer.startWidth;
	}

	/** 조준선을 갱신한다 */
	public void UpdateSightLine()
	{
		// 대상이 없을 경우
		if (m_oTarget == null)
		{
			return;
		}

		var stDirection = m_oOriginDirectionTarget.transform.forward.ExToWorld(m_oOriginDirectionTarget.transform.parent.gameObject, false);
		var stPhysicsDirection = m_oOriginDirectionTarget.transform.forward;

		bool bIsValid = Physics.Raycast(new Ray(this.transform.position, stPhysicsDirection.normalized),
			out RaycastHit stRaycastHit, int.MaxValue / 2.0f, this.TargetLayerMask);

		this.transform.forward = stDirection.normalized.ExToLocal(this.transform.parent.gameObject, false);
		this.transform.localScale = new Vector3(1.0f, 1.0f, bIsValid ? stRaycastHit.distance + 0.25f : byte.MaxValue * 2.0f);

		float fWidth = m_fOriginWidth * m_fWidthRate;
		m_oLineRenderer.startWidth = m_oLineRenderer.endWidth = fWidth;
	}
	#endregion // 함수

	#region 접근 함수
	/** 너비 비율을 변경한다 */
	public void SetWidthRate(float a_fRate)
	{
		m_fWidthRate = a_fRate;
	}

	/** 대상 레이어 마스크를 변경한다 */
	public void SetTargetLayerMask(int a_nLayerMask)
	{
		this.TargetLayerMask = a_nLayerMask;
	}

	/** 색상을 변경한다 */
	public void SetColor(Color a_stColor)
	{
		m_oLineRenderer.material.color = a_stColor;
	}

	/** 대상을 변경한다 */
	public void SetTarget(GameObject a_oTarget)
	{
		m_oTarget = a_oTarget;
	}

	/** 기준 방향 대상을 변경한다 */
	public void SetOriginDirectionTarget(GameObject a_oTarget)
	{
		m_oOriginDirectionTarget = a_oTarget;
	}
	#endregion // 접근 함수
}

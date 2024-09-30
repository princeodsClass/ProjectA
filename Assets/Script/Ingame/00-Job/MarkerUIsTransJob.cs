using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Burst;

/** 마커 UI 트랜스 폼 잡 */
[BurstCompile]
public struct STMarkerUIsTransJob : IJobParallelForTransform
{
	#region 변수
	public NativeArray<Vector3> m_stDeltas;
	public NativeArray<RaycastHit2D> m_stRaycastHit2Ds;
	#endregion // 변수

	#region IJobParallelForTransform
	/** 트랜스 폼을 갱신한다 */
	public void Execute(int a_nIdx, TransformAccess a_stTrans)
	{
		// 인덱스가 유효하지 않을 경우
		if(a_nIdx < 0 || a_nIdx >= m_stDeltas.Length)
		{
			return;
		}

		var stDelta = m_stDeltas[a_nIdx].normalized;

		a_stTrans.position = m_stRaycastHit2Ds[a_nIdx].point;
		a_stTrans.localRotation = Quaternion.Euler(0.0f, 0.0f, Vector3.SignedAngle(Vector3.right, stDelta, Vector3.forward));
	}
	#endregion // IJobParallelForTransform
}

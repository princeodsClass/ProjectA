using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;

/** 내비게이션 메쉬 제어자 */
public partial class CNavMeshController : MonoBehaviour
{
	/** 매개 변수 */
	public struct STParams
	{
		public NavMeshTriangulation m_stTriangulation;
	}

	#region 프로퍼티
	public STParams Params { get; private set; }
	public Dictionary<int, List<Bounds>> BoundsDictContainer = new Dictionary<int, List<Bounds>>();
	public Dictionary<int, List<List<int>>> NavMeshIdxDictContainer = new Dictionary<int, List<List<int>>>();
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;

		// 인덱스를 설정한다 {
		for (int i = 0; i < this.Params.m_stTriangulation.indices.Length; i += ComType.G_NUM_VERTICES_ON_FACE)
		{
			var oIdxListContainer = this.NavMeshIdxDictContainer.GetValueOrDefault(this.Params.m_stTriangulation.areas[i / ComType.G_NUM_VERTICES_ON_FACE]) ?? new List<List<int>>();
			this.NavMeshIdxDictContainer.TryAdd(this.Params.m_stTriangulation.areas[i / ComType.G_NUM_VERTICES_ON_FACE], oIdxListContainer);

			var oIdxList = CCollectionPoolManager.Singleton.SpawnList<int>();

			try
			{
				oIdxList.Add(this.Params.m_stTriangulation.indices[i + 0]);
				oIdxList.Add(this.Params.m_stTriangulation.indices[i + 1]);
				oIdxList.Add(this.Params.m_stTriangulation.indices[i + 2]);

				this.SetupAdjacencyIndices(this.Params.m_stTriangulation.vertices, oIdxList, oIdxListContainer);
			}
			finally
			{
				CCollectionPoolManager.Singleton.DespawnList(oIdxList);
			}
		}

		foreach (var stKeyVal in this.NavMeshIdxDictContainer)
		{
			this.OptimizeAdjacencyIndices(this.Params.m_stTriangulation.vertices, stKeyVal.Value);
		}
		// 인덱스를 설정한다 }

		// 경계 영역을 설정한다
		foreach (var stKeyVal in this.NavMeshIdxDictContainer)
		{
			var oBoundsList = this.BoundsDictContainer.GetValueOrDefault(stKeyVal.Key) ?? new List<Bounds>();
			this.BoundsDictContainer.TryAdd(stKeyVal.Key, oBoundsList);

			this.SetupBounds(stKeyVal.Value, oBoundsList);
		}
	}

	/** 영역을 설정한다 */
	private void SetupBounds(List<List<int>> a_oIdxListContainer, List<Bounds> a_oOutBoundsList)
	{
		{
			for (int i = 0; i < a_oIdxListContainer.Count; ++i)
			{
				var stBounds = new Bounds();
				stBounds.min = new Vector3(ComType.B_MAX_VAL_REAL, ComType.B_MAX_VAL_REAL, ComType.B_MAX_VAL_REAL);
				stBounds.max = new Vector3(ComType.B_MIN_VAL_REAL, ComType.B_MIN_VAL_REAL, ComType.B_MIN_VAL_REAL);

				for (int j = 0; j < a_oIdxListContainer[i].Count; ++j)
				{
					int nIdx = a_oIdxListContainer[i][j];

					stBounds.min = Vector3.Min(stBounds.min, this.Params.m_stTriangulation.vertices[nIdx]);
					stBounds.max = Vector3.Max(stBounds.max, this.Params.m_stTriangulation.vertices[nIdx]);
				}

				a_oOutBoundsList.Add(stBounds);
			}
		}
	}

	/** 인접 인덱스를 설정한다 */
	private void SetupAdjacencyIndices(Vector3[] a_stVertices, List<int> a_oIdxList, List<List<int>> a_oOutIdxListContainer)
	{
		var oAdjacencyIdxList = this.FindAdjacencyIndices(a_stVertices, a_oIdxList, a_oOutIdxListContainer) ?? new List<int>();
		oAdjacencyIdxList.AddRange(a_oIdxList);

		a_oOutIdxListContainer.ExAddVal(oAdjacencyIdxList);
	}

	/** 인접 인덱스를 최적화한다 */
	private void OptimizeAdjacencyIndices(Vector3[] a_stVertices, List<List<int>> a_oIdxListContainer)
	{
		var oRemoveIdxListContainer = CCollectionPoolManager.Singleton.SpawnList<List<int>>();

		try
		{
			do
			{
				oRemoveIdxListContainer.Clear();

				for (int i = 0; i < a_oIdxListContainer.Count - 1; ++i)
				{
					for (int j = i + 1; j < a_oIdxListContainer.Count; ++j)
					{
						// 인접 인덱스 일 경우
						if (this.IsAdjacencyIndices(a_stVertices, a_oIdxListContainer[i], a_oIdxListContainer[j]))
						{
							a_oIdxListContainer[i].AddRange(a_oIdxListContainer[j]);
							a_oIdxListContainer[j].Clear();

							oRemoveIdxListContainer.ExAddVal(a_oIdxListContainer[j]);
						}
					}
				}

				for (int i = 0; i < oRemoveIdxListContainer.Count; ++i)
				{
					a_oIdxListContainer.Remove(oRemoveIdxListContainer[i]);
				}
			} while (oRemoveIdxListContainer.Count >= 1);
		}
		finally
		{
			CCollectionPoolManager.Singleton.DespawnList(oRemoveIdxListContainer);
		}
	}

	/** 인접 인덱스를 탐색한다 */
	private List<int> FindAdjacencyIndices(Vector3[] a_stVertices, List<int> a_oIdxList, List<List<int>> a_oIdxListContainer)
	{
		for (int i = 0; i < a_oIdxListContainer.Count; ++i)
		{
			// 인접 인덱스 일 경우
			if (this.IsAdjacencyIndices(a_stVertices, a_oIdxList, a_oIdxListContainer[i]))
			{
				return a_oIdxListContainer[i];
			}
		}

		return null;
	}
	#endregion // 함수

	#region 접근 함수
	/** 포함 여부를 검사한다 */
	public bool IsContains(List<Vector3> a_oVertexList, Vector3 a_stPos)
	{
		// 정점이 부족 할 경우
		if (a_oVertexList.Count <= 2)
		{
			return false;
		}

		a_stPos.y = 0.0f;

		for(int i = 0; i < a_oVertexList.Count; ++i) {
			var stVertex = a_oVertexList[i];
			stVertex.y = 0.0f;

			a_oVertexList[i] = stVertex;
		}

		var stDelta01 = a_oVertexList[2] - a_oVertexList[0];
		var stDelta02 = a_oVertexList[1] - a_oVertexList[0];
		var stDelta03 = a_stPos - a_oVertexList[0];

		float fDot01 = Vector3.Dot(stDelta01, stDelta01);
		float fDot02 = Vector3.Dot(stDelta01, stDelta02);
		float fDot03 = Vector3.Dot(stDelta01, stDelta03);

		float fDot11 = Vector3.Dot(stDelta02, stDelta02);
		float fDot12 = Vector3.Dot(stDelta02, stDelta03);

		float fInverse = 1.0f / (fDot01 * fDot11 - fDot02 * fDot02);

		float fU = (fDot11 * fDot03 - fDot02 * fDot12) * fInverse;
		float fV = (fDot01 * fDot12 - fDot02 * fDot03) * fInverse;

		return fU.ExIsGreatEquals(0.0f) && fV.ExIsGreatEquals(0.0f) && (fU + fV).ExIsLessEquals(1.0f);
	}

	/** 인접 인덱스 여부를 검사한다 */
	private bool IsAdjacencyIndices(Vector3[] a_stVertices, List<int> a_oIdxList, List<int> a_oAdjacencyIdxList)
	{
		for (int i = 0; i < a_oIdxList.Count; ++i)
		{
			int nIdx = a_oIdxList[i];
			int nResult = a_oAdjacencyIdxList.FindIndex((a_nIdx) => a_stVertices[a_nIdx].ExIsEquals(a_stVertices[nIdx]));

			// 인덱스가 존재 할 경우
			if (nResult >= 0 && nResult < a_oAdjacencyIdxList.Count)
			{
				return true;
			}
		}

		return false;
	}
	#endregion // 접근 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(NavMeshTriangulation a_stTriangulation)
	{
		return new STParams()
		{
			m_stTriangulation = a_stTriangulation
		};
	}
	#endregion // 클래스 팩토리 함수
}

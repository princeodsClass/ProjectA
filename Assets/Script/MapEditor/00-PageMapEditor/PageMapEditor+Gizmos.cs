using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
using RTG;

namespace MapEditor
{
	/** 맵 에디터 페이지 - 프리팹 */
	public partial class PageMapEditor : UIDialog
	{
		/** 트랜스 폼 기즈모 타입 */
		private enum ETransGizmosType
		{
			NONE = -1,
			MOVE,
			SCALE,
			ROTATE,
			[HideInInspector] MAX_VAL
		}

		/** 트랜스 폼 수정 타입 */
		[System.Flags]
		private enum ETransModifyType
		{
			NONE = -1,
			EMPTY,
			POS = 1 << 1,
			SCALE = 1 << 2,
			ROTATE = 1 << 3,
			[HideInInspector] MAX_VAL
		}

		/** 트랜스 폼 입력 필드 수정 타입 */
		[System.Flags]
		private enum ETransModifyInputType
		{
			NONE = -1,
			X,
			Y,
			Z,
			[HideInInspector] MAX_VAL
		}

		#region 변수
		private GizmoSpace m_eSelGizmosSpace = GizmoSpace.Local;
		private ETransGizmosType m_eSelTransGizmosType = ETransGizmosType.MOVE;
		private GizmoObjectTransformPivot m_eSelGizmosPivot = GizmoObjectTransformPivot.ObjectMeshPivot;

		private ObjectTransformGizmo m_oMoveTransGizmos = null;
		private ObjectTransformGizmo m_oScaleTransGizmos = null;
		private ObjectTransformGizmo m_oRotateTransGizmos = null;

		private List<int> m_oSelTransGizmosTargetObjIdxList = new List<int>();
		private List<GameObject> m_oMoveTransGizmosTargetObjList = new List<GameObject>();
		private List<GameObject> m_oScaleTransGizmosTargetObjList = new List<GameObject>();
		private List<GameObject> m_oRotateTransGizmosTargetObjList = new List<GameObject>();
		#endregion // 변수

		#region 프로퍼티
		public int SelTransGizmosTargetObjIdx => (m_oSelTransGizmosTargetObjIdxList.Count >= 1) ? 
			m_oSelTransGizmosTargetObjIdxList[0] : -1;
		#endregion // 프로퍼티

		#region 함수
		/** 트랜스 폼 기즈모 타겟 객체를 추가한다 */
		private void AddTransGizmosTargetObj(GameObject a_oGameObj, bool a_bIsReset = false)
		{
			// 리셋 모드 일 경우
			if (a_bIsReset)
			{
				m_oSelTransGizmosTargetObjIdxList.Clear();
			}

			int nResult = m_oPrefabObjInfoList.FindIndex((a_stPrefabObjInfo) => a_stPrefabObjInfo.m_oGameObj == a_oGameObj);

			// 인덱스가 유효 할 경우
			if (m_oPrefabObjInfoList.ExIsValidIdx(nResult))
			{
				// 이미 선택 된 객체 일 경우
				if (m_oSelTransGizmosTargetObjIdxList.Contains(nResult))
				{
					m_oSelTransGizmosTargetObjIdxList.Remove(nResult);
				}
				else
				{
					m_oSelTransGizmosTargetObjIdxList.ExAddVal(nResult);
				}

				// 선택 된 트랜스 폼 기즈모 타겟 객체가 존재 할 경우
				if (m_oSelTransGizmosTargetObjIdxList.ExIsValid())
				{
					int nNPCResult = this.NPCTableList.FindIndex((a_oTable) => a_oTable.Prefab.Equals(a_oGameObj.name));

					m_oMEUIsSelPrefabText.text = this.NPCTableList.ExIsValidIdx(nNPCResult) ? NameTable.GetValue(this.NPCTableList[nNPCResult].NameKey) : a_oGameObj.name;
					m_oMEUIsSelPrefabImg.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.MapEditor, a_oGameObj.name);

					this.UpdateREUIsVec3InputUIs(m_stREUIsPosInputUIs, this.SelTransGizmosTargetObj.transform.localPosition);
					this.UpdateREUIsVec3InputUIs(m_stREUIsScaleInputUIs, this.SelTransGizmosTargetObj.transform.localScale);
					this.UpdateREUIsVec3InputUIs(m_stREUIsRotateInputUIs, this.SelTransGizmosTargetObj.transform.localEulerAngles);
				}
			}

			m_oSelTransGizmosTargetObjIdxList.ExCopyTo(m_oMoveTransGizmosTargetObjList, (a_nIdx) => m_oPrefabObjInfoList[a_nIdx].m_oGameObj);
			m_oSelTransGizmosTargetObjIdxList.ExCopyTo(m_oScaleTransGizmosTargetObjList, (a_nIdx) => m_oPrefabObjInfoList[a_nIdx].m_oGameObj);
			m_oSelTransGizmosTargetObjIdxList.ExCopyTo(m_oRotateTransGizmosTargetObjList, (a_nIdx) => m_oPrefabObjInfoList[a_nIdx].m_oGameObj);

			m_oMoveTransGizmos.SetTargetPivotObject(this.SelTransGizmosTargetObj);
			m_oScaleTransGizmos.SetTargetPivotObject(this.SelTransGizmosTargetObj);
			m_oRotateTransGizmos.SetTargetPivotObject(this.SelTransGizmosTargetObj);

			m_oMoveTransGizmos.SetTargetObjects(m_oMoveTransGizmosTargetObjList);
			m_oScaleTransGizmos.SetTargetObjects(m_oScaleTransGizmosTargetObjList);
			m_oRotateTransGizmos.SetTargetObjects(m_oRotateTransGizmosTargetObjList);

			m_oMEUIsSelPrefabInfo.SetActive(this.SelTransGizmosTargetObj != null);
			m_oMoveTransGizmos.Gizmo.MoveGizmo.SetVertexSnapTargetObjects(m_oMoveTransGizmosTargetObjList);

			m_nREUIsSelInspectorInputIdx = 0;
			m_bIsDirtyUpdateUIsState = true;
			m_ePrefabSelMode = EPrefabSelMode.CREATE;
		}
		#endregion // 함수

		#region 접근 함수
		/** 수정 된 벡터를 반환한다 */
		private Vector3 GetModifyVec3(Vector3 a_stVec3, Vector3 a_stOriginVec3, ETransModifyInputType a_eInputType)
		{
			switch (a_eInputType)
			{
				case ETransModifyInputType.X: return new Vector3(a_stVec3.x, a_stOriginVec3.y, a_stOriginVec3.z);
				case ETransModifyInputType.Y: return new Vector3(a_stOriginVec3.x, a_stVec3.y, a_stOriginVec3.z);
				case ETransModifyInputType.Z: return new Vector3(a_stOriginVec3.x, a_stOriginVec3.y, a_stVec3.z);
			}

			return a_stVec3;
		}

		/** 선택 된 트랜스 폼 기즈모 타겟 객체 트랜스 폼을 변경한다 */
		private void SetSelTransGizmosTargetObjsTrans(
			Vector3 a_stPos, Vector3 a_stScale, Vector3 a_stRotate, ETransModifyType a_eTransModifyType, ETransModifyInputType a_eInputType)
		{
			for (int i = 0; i < m_oSelTransGizmosTargetObjIdxList.Count; ++i)
			{
				int nIdx = m_oSelTransGizmosTargetObjIdxList[i];

				m_oPrefabObjInfoList[nIdx].m_oGameObj.transform.localPosition =
					a_eTransModifyType.HasFlag(ETransModifyType.POS) ? this.GetModifyVec3(a_stPos, m_oPrefabObjInfoList[nIdx].m_oGameObj.transform.localPosition, a_eInputType) : m_oPrefabObjInfoList[nIdx].m_oGameObj.transform.localPosition;

				m_oPrefabObjInfoList[nIdx].m_oGameObj.transform.localScale =
					a_eTransModifyType.HasFlag(ETransModifyType.SCALE) ? this.GetModifyVec3(a_stScale, m_oPrefabObjInfoList[nIdx].m_oGameObj.transform.localScale, a_eInputType) : m_oPrefabObjInfoList[nIdx].m_oGameObj.transform.localScale;

				m_oPrefabObjInfoList[nIdx].m_oGameObj.transform.localEulerAngles =
					a_eTransModifyType.HasFlag(ETransModifyType.ROTATE) ? this.GetModifyVec3(a_stRotate, m_oPrefabObjInfoList[nIdx].m_oGameObj.transform.localEulerAngles, a_eInputType) : m_oPrefabObjInfoList[nIdx].m_oGameObj.transform.localEulerAngles;

				// 광원 객체 일 경우
				if (m_oPrefabObjInfoList[nIdx].m_oGameObj == m_oLight.gameObject)
				{
					this.SelMapInfo.m_stLightInfo.m_stTransInfo = new STTransInfo(a_stPos, a_stScale, a_stRotate);
				}
				// 플레이어 위치 일 경우
				else if(m_oPrefabObjInfoList[nIdx].m_oGameObj == m_oPlayerPos)
				{
					this.SelMapInfo.m_stPlayerPos = a_stPos;
					this.SelMapInfo.m_stPlayerRotate = a_stRotate;
				}
				else
				{
					m_oPrefabObjInfoList[nIdx].m_oObjInfo.m_stTransInfo.m_stPos =
						a_eTransModifyType.HasFlag(ETransModifyType.POS) ? this.GetModifyVec3(a_stPos, m_oPrefabObjInfoList[nIdx].m_oObjInfo.m_stTransInfo.m_stPos, a_eInputType) : m_oPrefabObjInfoList[nIdx].m_oObjInfo.m_stTransInfo.m_stPos;

					m_oPrefabObjInfoList[nIdx].m_oObjInfo.m_stTransInfo.m_stScale =
						a_eTransModifyType.HasFlag(ETransModifyType.SCALE) ? this.GetModifyVec3(a_stScale, m_oPrefabObjInfoList[nIdx].m_oObjInfo.m_stTransInfo.m_stScale, a_eInputType) : m_oPrefabObjInfoList[nIdx].m_oObjInfo.m_stTransInfo.m_stScale;

					m_oPrefabObjInfoList[nIdx].m_oObjInfo.m_stTransInfo.m_stRotate =
						a_eTransModifyType.HasFlag(ETransModifyType.ROTATE) ? this.GetModifyVec3(a_stRotate, m_oPrefabObjInfoList[nIdx].m_oObjInfo.m_stTransInfo.m_stRotate, a_eInputType) : m_oPrefabObjInfoList[nIdx].m_oObjInfo.m_stTransInfo.m_stRotate;
				}
			}
		}
		#endregion // 접근 함수
	}
}
#endif // #if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTG;

#if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
namespace MapEditor
{
	// 기즈모 풀 관리자
	public class CGizmosPoolManager : CPoolManager<CGizmosPoolManager, System.Type, ObjectTransformGizmo>
	{
		#region 함수
		/** 이동 기즈모를 활성화한다 */
		public ObjectTransformGizmo SpawnMoveGizmos()
		{
			return this.Spawn(typeof(MoveGizmo), () => RTGizmosEngine.Get.CreateObjectMoveGizmo());
		}

		/** 비율 기즈모를 활성화한다 */
		public ObjectTransformGizmo SpawnScaleGizmos()
		{
			return this.Spawn(typeof(ScaleGizmo), () => RTGizmosEngine.Get.CreateObjectScaleGizmo());
		}

		/** 회전 기즈모를 활성화한다 */
		public ObjectTransformGizmo SpawnRotateGizmos()
		{
			return this.Spawn(typeof(RotationGizmo), () => RTGizmosEngine.Get.CreateObjectRotationGizmo());
		}

		/** 이동 기즈모를 비활성화한다 */
		public void DespawnMoveGizmos(ObjectTransformGizmo a_oGizmos)
		{
			this.Despawn(typeof(MoveGizmo), a_oGizmos);
		}

		/** 비율 기즈모를 비활성화한다 */
		public void DespawnScaleGizmos(ObjectTransformGizmo a_oGizmos)
		{
			this.Despawn(typeof(ScaleGizmo), a_oGizmos);
		}

		/** 회전 기즈모를 비활성화한다 */
		public void DespawnRotateGizmos(ObjectTransformGizmo a_oGizmos)
		{
			this.Despawn(typeof(RotationGizmo), a_oGizmos);
		}
		#endregion // 함수	
	}
}
#endif // #if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)

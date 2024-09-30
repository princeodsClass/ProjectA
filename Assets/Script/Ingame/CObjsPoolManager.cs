using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 객체 풀 관리자 */
public partial class CObjsPoolManager : CPoolManager<CObjsPoolManager, System.Type, CObj>
{
	#region 제네릭 함수
	/** 객체를 활성화한다 */
	public T SpawnObj<T>() where T : CObj, new()
	{
		var oObj = this.Spawn(typeof(T), () => new T());
		oObj.SetIsPooling(true);

		return oObj as T;
	}

	/** 객체를 비활성화한다 */
	public void DespawnObj<T>(T a_oObj, bool a_bIsEnableAssert = true) where T : CObj, new()
	{
		this.Despawn(typeof(T), a_oObj, a_bIsEnableAssert);
	}
	#endregion // 제네릭 함수
}

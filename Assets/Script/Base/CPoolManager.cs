using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 풀 관리자 */
public abstract partial class CPoolManager<T, K, V> : SingletonMono<T> where T : CPoolManager<T, K, V> where V : class
{
	#region 변수
	private Dictionary<K, CPoolListWrapper<V>> m_oPoolListWrapperDict = new Dictionary<K, CPoolListWrapper<V>>();
	#endregion // 변수

	#region 함수
	/** 상태를 리셋한다 */
	public virtual void Reset()
	{
		m_oPoolListWrapperDict.Clear();
	}

	/** 활성화한다 */
	protected V Spawn(K a_tKey, System.Func<V> a_oCreator)
	{
		var oPoolListWrapper = m_oPoolListWrapperDict.GetValueOrDefault(a_tKey) ?? new CPoolListWrapper<V>();
		m_oPoolListWrapperDict.TryAdd(a_tKey, oPoolListWrapper);

		var oObj = oPoolListWrapper.m_oQueue.ExGetVal(null) ?? a_oCreator();
		return oObj;
	}

	/** 비활성화한다 */
	protected void Despawn(K a_tKey, V a_oObj, bool a_bIsEnableAssert = true)
	{
		// 객체가 존재 할 경우
		if (a_oObj != null && m_oPoolListWrapperDict.TryGetValue(a_tKey, out CPoolListWrapper<V> oPoolListWrapper))
		{
			oPoolListWrapper.m_oQueue.Enqueue(a_oObj);
		}
	}
	#endregion // 함수
}

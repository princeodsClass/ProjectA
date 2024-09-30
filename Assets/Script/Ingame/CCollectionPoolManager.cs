using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/** 컬렉션 풀 관리자 */
public partial class CCollectionPoolManager : CPoolManager<CCollectionPoolManager, System.Type, ICollection>
{
	#region 제네릭 함수
	/** 리스트를 활성화한다 */
	public List<T> SpawnList<T>(List<T> a_oDefValList = null)
	{
		var oList = this.Spawn(typeof(List<T>), () => new List<T>()) as List<T>;
		a_oDefValList?.ExCopyTo(oList, (a_tVal) => a_tVal, a_bIsEnableAssert: false);

		return oList;
	}

	/** 딕셔너리를 활성화한다 */
	public Dictionary<K, V> SpawnDict<K, V>(Dictionary<K, V> a_oDefValDict = null)
	{
		var oDict = this.Spawn(typeof(Dictionary<K, V>), () => new Dictionary<K, V>()) as Dictionary<K, V>;
		a_oDefValDict?.ExCopyTo(oDict, (_, a_tVal) => a_tVal, a_bIsEnableAssert: false);

		return oDict;
	}

	/** 스택을 활성화한다 */
	public Stack<T> SpawnStack<T>(Stack<T> a_oDefValStack = null)
	{
		var oStack = this.Spawn(typeof(Stack<T>), () => new Stack<T>()) as Stack<T>;
		a_oDefValStack?.ExCopyTo(oStack, (a_tVal) => a_tVal, a_bIsEnableAssert: false);

		return oStack;
	}

	/** 큐를 활성화한다 */
	public Queue<T> SpawnQueue<T>(Queue<T> a_oDefValQueue = null)
	{
		var oQueue = this.Spawn(typeof(Queue<T>), () => new Queue<T>()) as Queue<T>;
		a_oDefValQueue?.ExCopyTo(oQueue, (a_tVal) => a_tVal, a_bIsEnableAssert: false);

		return oQueue;
	}

	/** 리스트를 비활성화한다 */
	public void DespawnList<T>(List<T> a_oList, bool a_bIsEnableAssert = true)
	{
		// 리스트가 존재 할 경우
		if (a_oList != null)
		{
			a_oList.Clear();
			this.Despawn(typeof(List<T>), a_oList, a_bIsEnableAssert);
		}
	}

	/** 딕셔너리를 비활성화한다 */
	public void DespawnDict<K, V>(Dictionary<K, V> a_oDict, bool a_bIsEnableAssert = true)
	{
		// 딕셔너리가 존재 할 경우
		if (a_oDict != null)
		{
			a_oDict.Clear();
			this.Despawn(typeof(Dictionary<K, V>), a_oDict, a_bIsEnableAssert);
		}
	}

	/** 스택을 비활성화한다 */
	public void DespawnStack<T>(Stack<T> a_oStack, bool a_bIsEnableAssert = true)
	{
		// 스택이 존재 할 경우
		if (a_oStack != null)
		{
			a_oStack.Clear();
			this.Despawn(typeof(Stack<T>), a_oStack, a_bIsEnableAssert);
		}
	}

	/** 큐가 비활성화한다 */
	public void DespawnQueue<T>(Queue<T> a_oQueue, bool a_bIsEnableAssert = true)
	{
		// 큐가 존재 할 경우
		if (a_oQueue != null)
		{
			a_oQueue.Clear();
			this.Despawn(typeof(Queue<T>), a_oQueue, a_bIsEnableAssert);
		}
	}
	#endregion // 제네릭 함수
}

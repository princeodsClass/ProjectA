using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/** 비동기 작업 관리자 */
public partial class CTaskManager : SingletonMono<CTaskManager>
{
	#region 함수
	/** 비동기 작업을 대기한다 */
	public async void WaitAsyncTask(Task a_oTask, System.Action<Task> a_oCallback)
	{
		await a_oTask;
		a_oCallback?.Invoke(a_oTask);
	}
	#endregion // 함수

	#region 제네릭 함수
	/** 비동기 작업을 대기한다 */
	public void WaitAsyncTask<T>(Task<T> a_oTask, System.Action<Task<T>> a_oCallback)
	{
		this.WaitAsyncTask(a_oTask as Task, (a_oAsyncTask) => a_oCallback?.Invoke(a_oAsyncTask as Task<T>));
	}
	#endregion // 제네릭 함수
}

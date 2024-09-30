using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class UIRootBattle : UIRootBase
{
	protected override void Initialize()
	{
		base.Initialize();

		m_MenuMgr.OpenPage<PageBattle>(EUIPage.PageBattle);
	}

	public override void Clear()
	{
		base.Clear();
	}

	private void Start()
	{
		StartCoroutine(this.DoStart());
	}

	private IEnumerator DoStart()
	{
		yield return new WaitForEndOfFrame();
		YieldInstructionCache.Clear();

#if DISABLE_THIS
		yield return new WaitForEndOfFrame();
		GameResourceManager.Singleton.Clear();
#endif // #if DISABLE_THIS

		yield return new WaitForEndOfFrame();
		System.GC.Collect(System.GC.MaxGeneration, System.GCCollectionMode.Optimized, true, true);
	}
}

#region 추가
/** 최상단 전투 UI */
public partial class UIRootBattle : UIRootBase
{
	#region 프로퍼티
	public override ESceneType SceneType => ESceneType.Battle;
	#endregion // 프로퍼티

#if UNITY_EDITOR || UNITY_STANDALONE
	/** 상태를 갱신한다 */
	protected override void UpdatePerFrame()
	{
		base.UpdatePerFrame();

		// 에디터 이동 키를 눌렀을 경우
		if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.E))
		{
			m_MenuMgr.SceneEnd();
			m_MenuMgr.SceneNext(ESceneType.MapEditor);
		}
	}
#endif // #if UNITY_EDITOR || UNITY_STANDALONE
}
#endregion // 추가

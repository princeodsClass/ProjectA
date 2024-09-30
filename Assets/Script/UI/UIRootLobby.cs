using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class UIRootLobby : UIRootBase
{
	protected override void Initialize()
	{
		base.Initialize();

		m_MenuMgr.OpenPage<PageLobby>(EUIPage.PageLobby);
	}

	public override void Clear()
	{
		base.Clear();
	}
}

#region 추가
/** 최상단 로비 UI */
public partial class UIRootLobby : UIRootBase
{
	#region 프로퍼티
	public override ESceneType SceneType => ESceneType.Lobby;
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

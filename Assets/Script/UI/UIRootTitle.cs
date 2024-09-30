using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class UIRootTitle : UIRootBase
{
	[SerializeField]
	GameObject dev;

	[SerializeField]
	TMP_Dropdown dropdown;

	bool _isEditor = false;

	public void RemoveAccount()
	{
		PlayerPrefs.DeleteAll();
	}

	public void Continue()
	{
		m_MenuMgr.OpenPage<PageTitle>(EUIPage.PageTitle);

		dev.SetActive(false);
	}

	protected override void Initialize()
	{
		base.Initialize();

#if DEBUG || DEVELOPMENT_BUILD
		SRDebug.Init();
#endif // #if #if DEBUG || DEVELOPMENT_BUILD

#if UNITY_EDITOR
		_isEditor = true;

		dev.SetActive(true);
		dropdown.ClearOptions();

		List<string> op = new List<string>();

		for (int i = 1; i < (int)ELanguage.END; i++)
		{
			op.Add(((ELanguage)i).ToString());
		}

		dropdown.AddOptions(op);
		dropdown.onValueChanged.AddListener(SetLanguage);
		dropdown.value = PlayerPrefs.GetInt(ComType.STORAGE_LANGUAGE_INT) - 1;
#else
        dev.SetActive(false);
#endif

		if (!_isEditor) Continue();
	}

	public override void Clear()
	{
		base.Clear();
	}

	public void SetLanguage(int index)
	{
		PlayerPrefs.SetInt(ComType.STORAGE_LANGUAGE_INT, index + 1);

		m_GameMgr.SetLanguage();
	}
}

#region 추가
/** 최상단 타이틀 UI */
public partial class UIRootTitle : UIRootBase
{
	#region 프로퍼티
	public override ESceneType SceneType => ESceneType.Title;
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

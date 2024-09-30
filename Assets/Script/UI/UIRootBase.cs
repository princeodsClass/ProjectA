using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

public abstract partial class UIRootBase : MonoBehaviour
{
	[SerializeField] GameObject m_goBackground = null;
	[SerializeField] GameObject m_goPopupDimmed = null;
	[SerializeField] List<Camera> m_listCamera = null;

	protected GameManager m_GameMgr = null;
	protected MenuManager m_MenuMgr = null;
	protected UserAccount m_Account = null;
	protected InventoryData<ItemWeapon> m_InvenWeapon = null;
	protected InventoryData<ItemGear> m_InvenGear = null;
	protected InventoryData<ItemMaterial> m_InvenMaterial = null;
	protected InventoryData<ItemCharacter> m_InvenCharacter = null;
	protected InventoryData<ItemBox> m_InvenBox = null;
	protected Canvas m_Canvas = null;
	protected CanvasScaler m_CanvasScaler = null;
	protected RectTransform m_RectTransform = null;

	public Canvas GetCanvas() { return m_Canvas; }
	public Vector2 GetRectSize() { return m_RectTransform.sizeDelta; }

	private void Awake()
	{
		if (null == m_GameMgr) m_GameMgr = GameManager.Singleton;
		if (null == m_MenuMgr) m_MenuMgr = MenuManager.Singleton;
		if (null == m_Account) m_Account = GameManager.Singleton.user;
		if (null == m_InvenWeapon) m_InvenWeapon = GameManager.Singleton.invenWeapon;
		if (null == m_InvenGear) m_InvenGear = GameManager.Singleton.invenGear;
		if (null == m_InvenMaterial) m_InvenMaterial = GameManager.Singleton.invenMaterial;
		if (null == m_InvenCharacter) m_InvenCharacter = GameManager.Singleton.invenCharacter;
		if (null == m_InvenBox) m_InvenBox = GameManager.Singleton.invenBox;

		m_MenuMgr.Initialize();
		m_MenuMgr.SetUIRoot(this);

		Initialize();

		#region 추가
		UIRootBase.FirstAwakeSceneType = (UIRootBase.FirstAwakeSceneType != ESceneType.NONE) ? UIRootBase.FirstAwakeSceneType : this.SceneType;

		// 초기화가 필요 할 경우
		if (!UIRootBase.IsInit && this.SceneType != ESceneType.Title)
		{
			MenuManager.Singleton.SceneEnd();
			MenuManager.Singleton.SceneNext(ESceneType.Title);
		}
		#endregion // 추가
	}

	protected virtual void Initialize()
	{
		m_Canvas = GetComponent<Canvas>();
		m_CanvasScaler = GetComponent<CanvasScaler>();
		m_RectTransform = GetComponent<RectTransform>();

		SetCameraOption();
	}

	protected virtual void UpdatePerFrame()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
			m_MenuMgr.IsCheckEscape();
	}

	private void Update() { UpdatePerFrame(); }
	public virtual void Clear() { }

	protected virtual void OnCallbackTopMenu() { }

	public virtual void ScreenCapture() { }

	public virtual void SetActiveBackground(bool bShow)
	{
		if (null != m_goBackground) m_goBackground.SetActive(bShow);
	}

	public virtual void SetPopupDimmed(bool bOpen)
	{
		if (null != m_goPopupDimmed) m_goPopupDimmed.SetActive(bOpen);
	}

	public void SetCameraOption()
	{
		if (null == m_listCamera) return;

		for (int i = 0; i < m_listCamera.Count; ++i)
		{
			if (null == m_listCamera[i]) continue;

			UniversalAdditionalCameraData uacd = m_listCamera[i].GetComponent<UniversalAdditionalCameraData>();
			uacd.antialiasing = m_GameMgr.GetOption().m_Game.bAntiAliasing ? AntialiasingMode.FastApproximateAntialiasing : AntialiasingMode.None;
			uacd.renderPostProcessing = m_GameMgr.GetOption().m_Game.bPost;
		}
	}
}

/** 최상단 UI - 추가 */
public abstract partial class UIRootBase : MonoBehaviour
{
	#region 추상
	public abstract ESceneType SceneType { get; }
	#endregion // 추상

	#region 클래스 프로퍼티
	public static bool IsInit { get; private set; } = false;
	public static ESceneType FirstAwakeSceneType { get; private set; } = ESceneType.NONE;
	#endregion // 클래스 프로퍼티

	#region 클래스 함수
	/** 초기화 여부를 변경한다 */
	public static void SetIsInit(bool a_bIsInit)
	{
		UIRootBase.IsInit = a_bIsInit;
	}
	#endregion // 클래스 함수
}

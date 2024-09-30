using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIDialog : MonoBehaviour
{
	[HideInInspector] public GameObject m_CachedObject = null;
	[HideInInspector] public RectTransform m_CachedTransform = null;

	private bool m_bInitialized = false;
	private bool m_bActive = false;
	private bool m_isSound = true;

	protected MenuManager m_MenuMgr = null;
	protected GameManager m_GameMgr = null;
	protected GameResourceManager m_ResourceMgr = null;
	protected GameDataManager m_DataMgr = null;
	protected GameDataTableManager m_TableMgr = null;
	protected GameAudioManager m_AudioMgr = null;

	protected InventoryData<ItemWeapon> m_InvenWeapon = null;
	protected InventoryData<ItemGear> m_InvenGear = null;
	protected InventoryData<ItemMaterial> m_InvenMaterial = null;
	protected InventoryData<ItemCharacter> m_InvenCharacter = null;
	protected InventoryData<ItemBox> m_InvenBox = null;
	protected UserAccount m_Account = null;
	protected AboveTutorial m_Tutorial = null;
	protected AboveQuestCard m_QuestCard = null;

	protected Dictionary<string, string> m_LogDataDict = new Dictionary<string, string>();
	public bool IsActivate { get { return (m_bActive && m_CachedObject.activeSelf) ? true : false; } }

	private void Start()
	{
		if (m_bInitialized)
			m_CachedObject.SetActive(m_bActive);
	}

	public virtual void Initialize()
	{
		if (!m_bInitialized)
		{
			m_bInitialized = true;
			m_bActive = false;
		}

		m_CachedObject = gameObject;
		m_CachedTransform = GetComponent<RectTransform>();

		if (null == m_MenuMgr) m_MenuMgr = MenuManager.Singleton;
		if (null == m_GameMgr) m_GameMgr = GameManager.Singleton;
		if (null == m_ResourceMgr) m_ResourceMgr = GameResourceManager.Singleton;
		if (null == m_DataMgr) m_DataMgr = GameDataManager.Singleton;
		if (null == m_TableMgr) m_TableMgr = GameDataTableManager.Singleton;
		if (null == m_AudioMgr) m_AudioMgr = GameAudioManager.Singleton;

		if (null == m_Account) m_Account = GameManager.Singleton.user;
		if (null == m_InvenWeapon) m_InvenWeapon = GameManager.Singleton.invenWeapon;
		if (null == m_InvenGear) m_InvenGear = GameManager.Singleton.invenGear;
		if (null == m_InvenMaterial) m_InvenMaterial = GameManager.Singleton.invenMaterial;
		if (null == m_InvenCharacter) m_InvenCharacter = GameManager.Singleton.invenCharacter;
		if (null == m_InvenBox) m_InvenBox = GameManager.Singleton.invenBox;

		if (null == m_Tutorial)
		{
			m_Tutorial = m_GameMgr.tutorial;
			m_GameMgr.InitializeTutorial();
		}

		if (null == m_QuestCard)
		{
			m_QuestCard = m_GameMgr.questCard;
			m_GameMgr.InitializeQuestCard();
		}
	}

	public virtual void Open()
	{
		m_bActive = true;
		if (null != m_CachedObject) m_CachedObject.SetActive(m_bActive);

		// 응답 대기 팝업 일 경우
		if ( this.gameObject.name.Equals("PopupWait4Response"))
			return;

		m_LogDataDict.ExReplaceVal("option1", this.gameObject.name);
		C3rdPartySDKManager.Singleton.SendLog("popup_open", m_LogDataDict);
	}

	public virtual void Close()
	{
		m_bActive = false;
		if (null != m_CachedObject) m_CachedObject.SetActive(m_bActive);

        // 응답 대기 팝업 일 경우
        if (this.gameObject.name.Equals("PopupWait4Response"))
            return;

        m_MenuMgr.ClosePopup(this);

		if (m_isSound)
			GameAudioManager.PlaySFX("SFX/UI/sfx_popup_close_00", 0f, false, ComType.UI_MIX);

		m_isSound = true;
	}

	public virtual void Escape()
	{
		m_GameMgr.InitializeTutorial();

		Close();
	}

	public virtual void EscapePage()
	{
		// 추가하자.
	}

	public void MuteClose()
	{
		m_isSound = false;
		Close();
	}

	public virtual void PopupAppear()
	{
		GameAudioManager.PlaySFX("SFX/UI/sfx_popup_appear_00", 0f, false, ComType.UI_MIX);
	}

	public virtual void PopupDrop()
	{
		string name = gameObject.name;

		switch (name)
		{
			case "PopupShopCrystal":
			case "PopupShopGameMoney":
			case "PopupVIP":
				GameAudioManager.PlaySFX("SFX/UI/sfx_coin_drop_00", 0f, false, ComType.UI_MIX);
				break;
			default:
				GameAudioManager.PlaySFX("SFX/UI/sfx_popup_drop_00", 0f, false, ComType.UI_MIX);
				break;
		}
	}

	public virtual void BoxLanding()
	{
		GameAudioManager.PlaySFX("SFX/UI/sfx_box_landing_00", 0f, false, ComType.UI_MIX);
	}
}

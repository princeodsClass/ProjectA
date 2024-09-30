using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

/** 에이전트 팝업 */
public partial class PopupAgent : UIDialog
{
	/** UI 모드 */
	public enum EUIsMode
	{
		NONE = -1,
		SEL,
		COSTUME,
		ENHANCE,
		[HideInInspector] MAX_VAL
	}

	/** 매개 변수 */
	public struct STParams
	{
		public System.Action<PopupAgent, CharacterTable> m_oCloseCallback;
	}

	#region 변수
	[Header("=====> Popup Agent - Etc <=====")]
	[SerializeField] private VideoPlayer m_oVideoPlayer = null;
	[SerializeField] private List<VideoClip> m_oVideoClipList = new List<VideoClip>();

	private EUIsMode m_eCurUIsMode = EUIsMode.SEL;

	private Vector3 m_stOriginTopSize = Vector3.zero;
	private Vector3 m_stOriginContentsSize = Vector3.zero;

	private CharacterTable m_oCurCharacterTable = null;
	private CharacterTable m_oSelCharacterTable = null;

	[Header("=====> Popup Agent - UIs <=====")]
	[SerializeField] private TMP_Text m_oEXPText = null;
	[SerializeField] private TMP_Text m_oNumCoinsText = null;
	[SerializeField] private TMP_Text m_oUserLevelText = null;
	[SerializeField] private TMP_Text m_oNumCrystalText = null;
	[SerializeField] private TMP_Text m_oMenuTitleText = null;

	[SerializeField] private Slider m_oEXPSlider = null;
	private PageLobbyTop m_oPageLobbyTop = null;

	[Header("=====> Popup Agent - Game Objects <=====")]
	[SerializeField] private GameObject m_oTopUIs = null;
	[SerializeField] private GameObject m_oContentsUIs = null;

	[SerializeField] private GameObject m_oEnhanceTopSizeGuide = null;
	[SerializeField] private GameObject m_oEnhanceContentsSizeGuide = null;

	[SerializeField] private GameObject m_oAgentRoot = null;
	private GameObject m_oAgent = null;
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }

	public int MaxAgentLevel => GlobalTable.GetData<int>(ComType.G_VALUE_MAX_CHARACTER_LEVEL);
	public CharacterTable SelCharacterTable => m_oSelCharacterTable;
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public void Awake()
	{
		this.Initialize();
		m_oAgentEnhanceParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

		// UI 를 설정한다
		this.SetupUIsSel();
		this.SetupUIsInfo();
		this.SetupUIsCostume();

		// 비디오 플레이어를 설정한다
		m_oVideoPlayer.isLooping = true;
		m_oVideoPlayer.skipOnDrop = false;
		m_oVideoPlayer.playOnAwake = false;
		m_oVideoPlayer.waitForFirstFrame = false;
	}

	/** 초기화 */
	public void Start()
	{
		var oTopRectTrans = m_oTopUIs.transform as RectTransform;
		var oContentsRectTrans = m_oContentsUIs.transform as RectTransform;

		m_stOriginTopSize = oTopRectTrans.rect.size;
		m_stOriginContentsSize = oContentsRectTrans.rect.size;

		this.SetupUIsEnhance();
		ComUtil.RebuildLayouts(this.gameObject);
	}

	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;
		m_oPageLobbyTop = GameObject.Find("LobbyTop")?.GetComponent<PageLobbyTop>();

		foreach(var oCharacter in GameManager.Singleton.invenCharacter)
		{
			// 선택 캐릭터가 아닐 경우
			if(oCharacter.id != GameManager.Singleton.user.m_nCharacterID)
			{
				continue;
			}

			m_oCurCharacterTable = m_oSelCharacterTable = CharacterTable.GetData(oCharacter.nKey);
			break;
		}

		var oCharacterTableList = CharacterTable.GetList();
		int nResult = oCharacterTableList.FindIndex((a_oTable) => a_oTable == m_oSelCharacterTable);

		this.SetupAgent();
		m_oVideoPlayer.clip = m_oVideoClipList[nResult];
		
		this.ExLateCallFunc((a_oSender) => 
		{
			this.SetIdxAgentInfo(1);
			this.UpdateUIsState();
		});
	}

    private void OnDisable()
    {
        FindObjectOfType<PageLobbyInventory>()?.InitializeMaterial();
    }

    /** 제거되었을 경우 */
    public void OnDestroy()
	{
		ComUtil.AssignVal(ref m_oAgentInfoSwipeAnim, null);
	}

	/** 팝업을 닫는다 */
	public override void Close()
	{
		// 선택 모드가 아닐 경우
		if (m_eCurUIsMode != EUIsMode.SEL)
		{
			m_eCurUIsMode = EUIsMode.SEL;
			this.UpdateUIsState();

			return;
		}

		base.Close();
	}

	/** UI 상태를 갱신한다 */
	public void UpdateUIsState()
	{
		switch (m_eCurUIsMode)
		{
			case EUIsMode.SEL: this.UpdateUIsStateSel(); break;
			case EUIsMode.COSTUME: this.UpdateUIsStateCostume(); break;
			case EUIsMode.ENHANCE: this.UpdateUIsStateEnhance(); break;
		}

		int clevel = GameManager.Singleton.user.m_nLevel;
        int cExp = GameManager.Singleton.user.m_nExp;
        int tExp = AccountLevelTable.GetData((uint)(0x01000000 + clevel)).Exp;
        int tGolden = AccountLevelTable.GetData((uint)(0x01000000 + clevel)).BonusTargetPoint;

        m_oEXPText.text = clevel == GlobalTable.GetData<int>("levelMaxAccount") ? UIStringTable.GetValue("ui_max") : $"{cExp} / {tExp}";
		m_oMenuTitleText.text = this.GetMenuTitle();
		m_oUserLevelText.text = GameManager.Singleton.user.m_nLevel.ToString();

		m_oNumCoinsText.text = ComUtil.ChangeNumberFormat(GameManager.Singleton.invenMaterial.CalcTotalMoney());
		m_oNumCrystalText.text = ComUtil.ChangeNumberFormat(GameManager.Singleton.invenMaterial.CalcTotalCrystal());

		m_oEXPSlider.value = (float)cExp / tExp;
		m_oVideoPlayer.Play();
		
		this.UpdateUIsStateInfo();
		// m_oPageLobbyTop.InitializeCurrency();

		var oTopRectTrans = m_oTopUIs.transform as RectTransform;
		var oContentsRectTrans = m_oContentsUIs.transform as RectTransform;

		var oEnhanceTopRectTrans = m_oEnhanceTopSizeGuide.transform as RectTransform;
		var oEnhanceContentsRectTrans = m_oEnhanceContentsSizeGuide.transform as RectTransform;

		var stTopSize = oTopRectTrans.rect.size;
		var stContentsSize = oContentsRectTrans.rect.size;

		var stEnhanceTopSize = oEnhanceTopRectTrans.rect.size;
		var stEnhanceContentsSize = oEnhanceContentsRectTrans.rect.size;

		// 강화 모드 일 경우
		if (m_eCurUIsMode == EUIsMode.ENHANCE)
		{
			stTopSize = stEnhanceTopSize;
			stContentsSize = stEnhanceContentsSize;
		}
		else
		{
			stTopSize = m_stOriginTopSize;
			stContentsSize = m_stOriginContentsSize;
		}

		oTopRectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, stTopSize.y);
		oContentsRectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, stContentsSize.y);

		// UI 를 갱신한다 {
		m_oAgentSelUIs.SetActive(m_eCurUIsMode == EUIsMode.SEL);
		m_oAgentSelMenuUIs.SetActive(m_eCurUIsMode == EUIsMode.SEL);

		m_oAgentCostumeUIs.SetActive(m_eCurUIsMode == EUIsMode.COSTUME);
		m_oAgentCostumeMenuUIs.SetActive(m_eCurUIsMode == EUIsMode.COSTUME);

		m_oAgentEnhanceUIs.SetActive(m_eCurUIsMode == EUIsMode.ENHANCE);
		m_oAgentEnhanceMenuUIs.SetActive(m_eCurUIsMode == EUIsMode.ENHANCE);
		// UI 를 갱신한다 }

		FindObjectOfType<PageLobbyInventory>().InitializeCharacterStats();
	}

	/** 골드 버튼을 눌렀을 경우 */
	public void OnClickAddGameMoney()
    {
        if ( GameManager.Singleton.user.m_nLevel >= GlobalTable.GetData<int>("valueShopOpenLevel") )
        {
            PopupShopGameMoney ga = MenuManager.Singleton.OpenPopup<PopupShopGameMoney>(EUIPopup.PopupShopGameMoney, true);
        }
        else
        {
            PopupSysMessage pop = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
            pop.InitializeInfo("ui_error_title", "ui_error_notenoughlevel_shop", "ui_common_close", null, "TutorialShop");
        }
    }

	/** 크리스탈 버튼을 눌렀을 경우 */
    public void OnClickAddCrystal()
    {
        if ( GameManager.Singleton.user.m_nLevel >= GlobalTable.GetData<int>("valueShopOpenLevel") )
        {
            PopupShopCrystal ct = MenuManager.Singleton.OpenPopup<PopupShopCrystal>(EUIPopup.PopupShopCrystal, true);
        }
        else
        {
            PopupSysMessage pop = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
            pop.InitializeInfo("ui_error_title", "ui_error_notenoughlevel_shop", "ui_common_close", null, "TutorialShop");
        }
    }

	/** 현재 에이전트 여부를 검사한다 */
	public bool IsCurAgent(CharacterTable a_oCharacterTable)
	{
		return m_oSelCharacterTable == a_oCharacterTable;
	}

	/** 에이전트 선택 여부를 검사한다 */
	public bool IsSelAgent(CharacterTable a_oCharacterTable)
	{
		return m_oCurCharacterTable == a_oCharacterTable;
	}

	/** 에이전트 잠금 해제 여부를 검사한다 */
	public bool IsOpenAgent(CharacterTable a_oCharacterTable)
	{
		var oItemCharacter = ComUtil.GetItemCharacter(a_oCharacterTable);
		return oItemCharacter != null;
	}

	/** 에이전트를 설정한다 */
	private void SetupAgent()
	{
		// 에이전트가 존재 할 경우
		if (m_oAgent != null)
		{
			GameResourceManager.Singleton.ReleaseObject(m_oAgent, false);
		}

		m_oAgent = GameResourceManager.Singleton.CreateObject(EResourceType.Character, 
			m_oSelCharacterTable.Prefab, m_oAgentRoot.transform);
	}
	#endregion // 함수

	#region 접근 함수
	/** 메뉴 제목을 반환한다 */
	public string GetMenuTitle()
	{
		switch(m_eCurUIsMode)
		{
			case EUIsMode.SEL: return UIStringTable.GetValue("ui_item_type_name_character");
			case EUIsMode.ENHANCE: return UIStringTable.GetValue("ui_popup_weapon_button_upgrade_caption");
			case EUIsMode.COSTUME: return UIStringTable.GetValue("ui_character_skin_caption");
		}

		return string.Empty;
	}

	/** UI 모드를 변경한다 */
	public void SetUIsMode(EUIsMode a_eUIsMode)
	{
		// 모드 변경이 필요 없을 경우
		if (m_eCurUIsMode == a_eUIsMode)
		{
			return;
		}

		m_eCurUIsMode = a_eUIsMode;
		this.ExLateCallFunc((a_oSender) => ComUtil.RebuildLayouts(this.gameObject));
	}
	#endregion // 접근 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(System.Action<PopupAgent, CharacterTable> a_oCloseCallback)
	{
		return new STParams()
		{
			m_oCloseCallback = a_oCloseCallback
		};
	}
	#endregion // 클래스 팩토리 함수
}

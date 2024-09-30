using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/** 에이전트 강화 팝업 */
public partial class PopupAgentEnhance : UIDialog
{
	/** 재화 키 */
	private enum ECurrencyKey
	{
		NONE = -1,
		CURRENCY_A,
		CURRENCY_B,
		[HideInInspector] MAX_VAL
	}

	/** 매개 변수 */
	public struct STParams
	{
		public int m_nIdx;
		public bool m_bIsSelMode;
		public PopupAgent m_oPopupAgent;
		public CharacterTable m_oCharacterTable;

		public System.Action<PopupAgentEnhance> m_oCloseCallback;
	}

	#region 변수
	[Header("=====> Popup Agent Enhance - UIs <=====")]
	[SerializeField] private TMP_Text m_oNumText = null;
	[SerializeField] private TMP_Text m_oTitleText = null;
	[SerializeField] private TMP_Text m_oPriceText = null;
	[SerializeField] private TMP_Text m_oProgressText = null;

	[SerializeField] private Image m_oNumIconImg = null;
	[SerializeField] private Image m_oPriceIconImg = null;

	[SerializeField] private Slider m_oSlider = null;
	[SerializeField] private SoundButton m_oCloseBtn = null;
	[SerializeField] private List<PopupAgentEnhanceBonusUIs> m_oEnhanceBonusUIsList = new List<PopupAgentEnhanceBonusUIs>();

	[Header("=====> Popup Agent Enhance - Game Objects <=====")]
	[SerializeField] private GameObject m_oOpenUIs = null;
	[SerializeField] private GameObject m_oLockUIs = null;
	[SerializeField] private GameObject m_oEnhanceUIs = null;
	[SerializeField] private GameObject m_oCurrencyUIs = null;

	[SerializeField] private List<GameObject> m_oEnhanceCurrencyUIsList = new List<GameObject>();
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public void Awake()
	{
		base.Initialize();

		for (int i = 0; i < m_oEnhanceBonusUIsList.Count; ++i)
		{
			int nIdx = i;

			m_oEnhanceBonusUIsList[i].ActiveBtn.onClick.RemoveAllListeners();
			m_oEnhanceBonusUIsList[i].ActiveBtn.onClick.AddListener(() => this.OnTouchActiveBtn(nIdx));
		}

		for (int i = 0; i < m_oEnhanceCurrencyUIsList.Count; ++i)
		{
			int nIdx = i;
			var oCurrencyBtn = m_oEnhanceCurrencyUIsList[i].GetComponent<SoundButton>();

			oCurrencyBtn.onClick.AddListener(() => this.OnTouchAgentEnhanceCurrencyBtn(nIdx));
		}
	}

	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;

		this.UpdateUIsState();
		ComUtil.RebuildLayouts(this.gameObject);
	}

	/** 팝업이 닫힐 경우 */
	public override void Close()
	{
		// 선택 모드 일 경우
		if(this.IsSelMode())
		{
			return;
		}

		base.Close();
		this.Params.m_oCloseCallback?.Invoke(this);
	}

	/** 강화 버튼을 눌렀을 경우 */
	public void OnTouchEnhanceBtn()
	{
		// 강화 가능 할 경우
		if (this.Params.m_oPopupAgent.IsEnableEnhanceSkill(this.Params.m_oCharacterTable, this.Params.m_nIdx))
		{
			StartCoroutine(this.CoEnhanceSkill());
			return;
		}

		int nOrderVal = ComUtil.GetAgentEnhanceOrderVal(this.Params.m_oCharacterTable, this.Params.m_nIdx);
		var oSkillTable = ComUtil.GetAgentEnhanceSkillTable(this.Params.m_oCharacterTable, this.Params.m_nIdx, 0, nOrderVal);

		int nNumItemsA = GameManager.Singleton.invenMaterial.GetItemCount(oSkillTable.UpgradeMaterialKey00);
		int nNumItemsB = GameManager.Singleton.invenMaterial.GetItemCount(oSkillTable.UpgradeMaterialKey01);

		// 카드가 부족 할 경우
		if (nNumItemsA < oSkillTable.UpgradeMaterialCount00)
		{
			PopupBoxMaterial pbm = MenuManager.Singleton.OpenPopup<PopupBoxMaterial>(EUIPopup.PopupBoxMaterial, true);
			pbm.InitializeInfo(true);
		}
		// 골드가 부족 할 경우
		else if(nNumItemsB < oSkillTable.UpgradeMaterialCount01)
		{
			this.OnClickAddGameMoney();
		}
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

	/** UI 상태를 갱신한다 */
	private void UpdateUIsState()
	{
		bool bIsOpen = this.IsOpen();
		bool bIsMaxEnhance = ComUtil.IsMaxEnhance(this.Params.m_oCharacterTable, this.Params.m_nIdx);
		bool bIsEnhanceSkill = this.Params.m_oPopupAgent.IsEnhanceSkill(this.Params.m_oCharacterTable, this.Params.m_nIdx);

		int nOrderVal = ComUtil.GetAgentEnhanceOrderVal(this.Params.m_oCharacterTable, this.Params.m_nIdx);
		int nNextOrderVal = (nOrderVal < 0) ? Mathf.Max(nOrderVal - 1, -ComType.G_MAX_AGENT_SKILL_LEVEL - 1) : Mathf.Min(ComType.G_MAX_AGENT_SKILL_LEVEL + 1, nOrderVal + 1);

		var oSkillTableList = ComUtil.GetAgentEnhanceSkillTables(this.Params.m_oCharacterTable, this.Params.m_nIdx);
		var oItemCharacter = ComUtil.GetItemCharacter(this.Params.m_oCharacterTable);

		m_oOpenUIs.SetActive(bIsOpen && bIsEnhanceSkill && !this.Params.m_bIsSelMode);
		m_oLockUIs.SetActive(!bIsOpen);

		m_oEnhanceUIs.SetActive(!bIsMaxEnhance);
		m_oCurrencyUIs.SetActive(!bIsMaxEnhance);

		m_oCloseBtn.gameObject.SetActive(!this.IsSelMode());

		for (int i = 0; i < m_oEnhanceBonusUIsList.Count; ++i)
		{
			bool bIsSel = this.IsSel(i);

			var oEnhanceBonusUIs = m_oEnhanceBonusUIsList[i];
			oEnhanceBonusUIs.gameObject.SetActive(oSkillTableList[i] != null);

			oEnhanceBonusUIs.CheckImg.gameObject.SetActive(bIsSel && bIsOpen);
			oEnhanceBonusUIs.SeparateImg.gameObject.SetActive(i >= 1);

			oEnhanceBonusUIs.ActiveUIsSel.SetActive(bIsSel);
			oEnhanceBonusUIs.ActiveUIsUnsel.SetActive(!bIsSel);

			// 선택 모드 일 경우
			if(this.IsSelMode())
			{
				oEnhanceBonusUIs.ActiveUIsUnsel.SetActive(true);
				oEnhanceBonusUIs.CheckImg.gameObject.SetActive(false);
			}

			oEnhanceBonusUIs.DescText.gameObject.SetActive(bIsOpen);

			// 스킬 테이블이 없을 경우
			if (oSkillTableList[i] == null)
			{
				continue;
			}

			var oSkillTable = ComUtil.GetAgentEnhanceSkillTable(this.Params.m_oCharacterTable, this.Params.m_nIdx, i, nOrderVal);
			var oNextSkillTable = bIsEnhanceSkill ? ComUtil.GetAgentEnhanceSkillTable(this.Params.m_oCharacterTable, this.Params.m_nIdx, i, nNextOrderVal) : null;

			var oEffectTableList = EffectTable.GetGroup(oSkillTable.HitEffectGroup);
			var oNextEffectTableList = (oNextSkillTable != null) ? EffectTable.GetGroup(oNextSkillTable.HitEffectGroup) : null;

			float fSign = Mathf.Sign(oEffectTableList[0].Value);
			float fEnhanceVal = oEffectTableList[0].Value.ExIsLess(1.0f) ? oEffectTableList[0].Value * 100.0f : oEffectTableList[0].Value;

			string oPostfixStr = oEffectTableList[0].Value.ExIsLess(1.0f) ? "%" : string.Empty;

			string oDescStr = oEffectTableList[0].Value.ExIsLess(1.0f) ? 
				$"{fEnhanceVal:0.00}{oPostfixStr}" : $"{fEnhanceVal}{oPostfixStr}";

			m_oTitleText.text = ComUtil.GetAgentEnhanceName(oSkillTable);

			oEnhanceBonusUIs.ActiveUIs.SetActive(bIsOpen);
			oEnhanceBonusUIs.EnhanceUIs.SetActive(bIsOpen && bIsEnhanceSkill && nOrderVal != nNextOrderVal && !this.Params.m_bIsSelMode);

			oEnhanceBonusUIs.ValText.text = $"{oEffectTableList[0].Value:0.00}";
			oEnhanceBonusUIs.IconImg.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Common, oEffectTableList[0].Icon);

			oEnhanceBonusUIs.NameText.text = NameTable.GetValue(oEffectTableList[0].NameKey);
			oEnhanceBonusUIs.DescText.text = (oEffectTableList[0].DescKey > 0) ? DescTable.GetValue(oEffectTableList[0].DescKey) : oDescStr;

			// 다음 스킬이 존재 할 경우
			if(oNextSkillTable != null)
			{
				float fDeltaEnhanceVal = Mathf.Abs(oNextEffectTableList[0].Value) - Mathf.Abs(oEffectTableList[0].Value);
				float fAdjustEnhanceVal = fDeltaEnhanceVal.ExIsLess(1.0f) ? fDeltaEnhanceVal * 100.0f : fDeltaEnhanceVal;

				oEnhanceBonusUIs.ValText.text = fDeltaEnhanceVal.ExIsLess(1.0f) ? 
					$"{fAdjustEnhanceVal * fSign:0.00}{oPostfixStr}" : $"{fAdjustEnhanceVal * fSign}{oPostfixStr}";
			}
		}

		// 강화 가능한 스킬이 아닐 경우
		if (!bIsEnhanceSkill)
		{
			return;
		}

		int nCurrencyOrderVal = ComUtil.GetAgentEnhanceOrderVal(this.Params.m_oCharacterTable, this.Params.m_nIdx);
		var nCurrencySkillTable = ComUtil.GetAgentEnhanceSkillTable(this.Params.m_oCharacterTable, this.Params.m_nIdx, 0, nCurrencyOrderVal);

		int nNumItemsA = nCurrencySkillTable.UpgradeMaterialCount00;
		int nCurNumItemsA = GameManager.Singleton.invenMaterial.GetItemCount(nCurrencySkillTable.UpgradeMaterialKey00);

		int nNumItemsB = nCurrencySkillTable.UpgradeMaterialCount01;
		int nCurNumItemsB = GameManager.Singleton.invenMaterial.GetItemCount(nCurrencySkillTable.UpgradeMaterialKey01);

		var oCurrencyNumTextA = ComUtil.FindComponentByName<TMP_Text>("NumText", m_oEnhanceCurrencyUIsList[(int)ECurrencyKey.CURRENCY_A].transform);
		oCurrencyNumTextA.text = $"{nCurNumItemsA}/{nNumItemsA}";
		oCurrencyNumTextA.color = (nCurNumItemsA >= nNumItemsA) ? Color.white : Color.red;

		var oCurrencyNumTextB = ComUtil.FindComponentByName<TMP_Text>("NumText", m_oEnhanceCurrencyUIsList[(int)ECurrencyKey.CURRENCY_B].transform);
		oCurrencyNumTextB.text = $"{nNumItemsB}";
		oCurrencyNumTextB.color = (nCurNumItemsB >= nNumItemsB) ? Color.white : Color.red;

		var oCurrencyIconImgA = ComUtil.FindComponentByName<Image>("IconImg", m_oEnhanceCurrencyUIsList[(int)ECurrencyKey.CURRENCY_A].transform);
		oCurrencyIconImgA.sprite = ComUtil.GetIcon(nCurrencySkillTable.UpgradeMaterialKey00);

		var oCurrencyIconImgB = ComUtil.FindComponentByName<Image>("IconImg", m_oEnhanceCurrencyUIsList[(int)ECurrencyKey.CURRENCY_B].transform);
		oCurrencyIconImgB.sprite = ComUtil.GetIcon(nCurrencySkillTable.UpgradeMaterialKey01);

		// XXX: 코드 순서 변경 금지 {
		nCurrencyOrderVal = Mathf.Abs(nCurrencyOrderVal);

		m_oProgressText.text = $"{nCurrencyOrderVal - 1}/{ComType.G_MAX_AGENT_SKILL_LEVEL}";
		m_oSlider.value = (nCurrencyOrderVal - 1) / (float)ComType.G_MAX_AGENT_SKILL_LEVEL;
		// XXX: 코드 순서 변경 금지 }
	}

	/** 활성 버튼을 눌렀을 경우 */
	private void OnTouchActiveBtn(int a_nIdx)
	{
		StartCoroutine(this.CoActiveSkill(a_nIdx));
	}

	/** 에이전트 강화 재화 버튼을 눌렀을 경우 */
	public void OnTouchAgentEnhanceCurrencyBtn(int a_nIdx)
	{
		int nOrderVal = ComUtil.GetAgentEnhanceOrderVal(this.Params.m_oCharacterTable, this.Params.m_nIdx);
		var oSkillTableA = ComUtil.GetAgentEnhanceSkillTable(this.Params.m_oCharacterTable, this.Params.m_nIdx, 0, nOrderVal);

		uint nMaterialKey = (a_nIdx <= 0) ? oSkillTableA.UpgradeMaterialKey00 : oSkillTableA.UpgradeMaterialKey01;

		PopupMaterial oPopupMaterial = MenuManager.Singleton.OpenPopup<PopupMaterial>(EUIPopup.PopupMaterial, true);
		oPopupMaterial.InitializeInfo(new ItemMaterial(0, nMaterialKey, 0));
	}

	/** 선택 여부를 검사한다 */
	private bool IsSel(int a_nOrder)
	{
		int nOrderVal = ComUtil.GetAgentEnhanceOrderVal(this.Params.m_oCharacterTable, this.Params.m_nIdx);
		return (a_nOrder == 0 && nOrderVal > 0) || (a_nOrder == 1 && nOrderVal < 0);
	}

	/** 잠금 해제 여부를 반환한다 */
	private bool IsOpen()
	{
		return ComUtil.IsOpenAgentEnhance(this.Params.m_oCharacterTable, this.Params.m_nIdx);
	}

	/** 선택 모드 여부를 검사한다 */
	private bool IsSelMode()
	{
		var oSkillTableList = ComUtil.GetAgentEnhanceSkillTables(this.Params.m_oCharacterTable, this.Params.m_nIdx);
		return this.Params.m_bIsSelMode && oSkillTableList.All((a_oSkillTable) => a_oSkillTable != null);
	}
	#endregion // 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(PopupAgent a_oPopupAgent,
		CharacterTable a_oCharacterTable, int a_nIdx, System.Action<PopupAgentEnhance> a_oCloseCallback, bool a_bIsSelMode = false)
	{
		return new STParams()
		{
			m_nIdx = a_nIdx,
			m_bIsSelMode = a_bIsSelMode,
			m_oPopupAgent = a_oPopupAgent,
			m_oCharacterTable = a_oCharacterTable,

			m_oCloseCallback = a_oCloseCallback
		};
	}
	#endregion // 클래스 팩토리 함수
}

/** 에이전트 스킬 팝업 - 코루틴 */
public partial class PopupAgentEnhance : UIDialog
{
	#region 함수
	/** 스킬을 활성한다 */
	private IEnumerator CoActiveSkill(int a_nIdx)
	{
		var oWaitPopup = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);
		var oItemCharacter = ComUtil.GetItemCharacter(this.Params.m_oCharacterTable);
		var stSkillUpgrade = oItemCharacter._stSkillUpgrade;

		int nLevel = (this.Params.m_nIdx + 1) * ComType.G_OFFSET_AGENT_SKILL_LEVEL;
		int nOrderVal = ComUtil.GetAgentEnhanceOrderVal(this.Params.m_oCharacterTable, this.Params.m_nIdx);

		var oType = stSkillUpgrade.GetType();

		var oFieldInfo = oType.GetField(string.Format("Upgrade{0:00}", nLevel));
		oFieldInfo.SetValueDirect(__makeref(stSkillUpgrade), (a_nIdx <= 0) ? Mathf.Abs(nOrderVal) : -Mathf.Abs(nOrderVal));

		yield return GameDataManager.Singleton.CharacterSkillUpgrade(oItemCharacter, stSkillUpgrade, new Dictionary<long, int>());
		oWaitPopup.Close();

		this.UpdateUIsState();
		this.Params.m_oPopupAgent.UpdateUIsState();

		// 선택 모드 일 경우
		if(this.Params.m_bIsSelMode)
		{
			var stParams = this.Params;
			stParams.m_bIsSelMode = false;

			this.Params = stParams;
			this.Close();
		}
	}

	/** 스킬을 강화한다 */
	private IEnumerator CoEnhanceSkill()
	{
		var oWaitPopup = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);
		var oItemCharacter = ComUtil.GetItemCharacter(this.Params.m_oCharacterTable);

		int nOrderVal = ComUtil.GetAgentEnhanceOrderVal(this.Params.m_oCharacterTable, this.Params.m_nIdx);
		var oSkillTable = ComUtil.GetAgentEnhanceSkillTable(this.Params.m_oCharacterTable, this.Params.m_nIdx, 0, nOrderVal);

		long nItemIDA = GameManager.Singleton.invenMaterial.GetItemID(oSkillTable.UpgradeMaterialKey00);
		long nItemIDB = GameManager.Singleton.invenMaterial.GetItemID(oSkillTable.UpgradeMaterialKey01);

		int nNumItemsA = GameManager.Singleton.invenMaterial.GetItemCount(nItemIDA);
		int nNumItemsB = GameManager.Singleton.invenMaterial.GetItemCount(nItemIDB);

		int nLevel = (this.Params.m_nIdx + 1) * ComType.G_OFFSET_AGENT_SKILL_LEVEL;
		var stSkillUpgrade = oItemCharacter._stSkillUpgrade;

		var oType = stSkillUpgrade.GetType();
		var oFieldInfo = oType.GetField(string.Format("Upgrade{0:00}", nLevel));

		nOrderVal = (int)oFieldInfo.GetValue(stSkillUpgrade);
		oFieldInfo.SetValueDirect(__makeref(stSkillUpgrade), (nOrderVal < 0) ? nOrderVal - 1 : nOrderVal + 1);

		yield return GameDataManager.Singleton.CharacterSkillUpgrade(oItemCharacter, stSkillUpgrade, new Dictionary<long, int>()
		{
			[nItemIDA] = nNumItemsA - oSkillTable.UpgradeMaterialCount00,
			[nItemIDB] = nNumItemsB - oSkillTable.UpgradeMaterialCount01
		});

		GameManager.Singleton.invenMaterial.ModifyItem(nItemIDA,
			InventoryData<ItemMaterial>.EItemModifyType.Volume, nNumItemsA - oSkillTable.UpgradeMaterialCount00);

		GameManager.Singleton.invenMaterial.ModifyItem(nItemIDB,
			InventoryData<ItemMaterial>.EItemModifyType.Volume, nNumItemsB - oSkillTable.UpgradeMaterialCount01);

		oWaitPopup.Close();

		this.UpdateUIsState();
		this.Params.m_oPopupAgent.UpdateUIsState();

		ComUtil.RebuildLayouts(this.gameObject);
	}
	#endregion // 함수
}

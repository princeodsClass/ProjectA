using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/** 에이전트 팝업 - 강화 */
public partial class PopupAgent : UIDialog
{
	/** 재화 키 */
	private enum ECurrencyKey
	{
		NONE = -1,
		CURRENCY_A,
		CURRENCY_B,
		[HideInInspector] MAX_VAL
	}

	#region 변수
	[Header("=====> Popup Agent - UIs (Enhance) <=====")]
	[SerializeField] private Image m_oEnhanceBtnBlindImg = null;
	[SerializeField] private SoundButton m_oAgentEnhanceBtn = null;

	private List<PopupAgentScrollerCellViewEnhanceBG> m_oPopupAgentScrollerCellViewEnhanceBGList = new List<PopupAgentScrollerCellViewEnhanceBG>();
	private List<PopupAgentScrollerCellViewEnhanceFG> m_oPopupAgentScrollerCellViewEnhanceFGList = new List<PopupAgentScrollerCellViewEnhanceFG>();

	[Header("=====> Popup Agent - Game Objects (Enhance) <=====")]
	[SerializeField] private GameObject m_oOriginScrollerCellViewEnhanceBG = null;
	[SerializeField] private GameObject m_oOriginScrollerCellViewEnhanceFG = null;

	[SerializeField] private GameObject m_oAgentEnhanceUIs = null;
	[SerializeField] private GameObject m_oAgentEnhanceMenuUIs = null;
	[SerializeField] private GameObject m_oEnhanceScrollViewContents = null;
	[SerializeField] private GameObject m_oEnhanceScrollViewContentsBG = null;
	[SerializeField] private GameObject m_oEnhanceScrollViewContentsFG = null;

	[SerializeField] private GameObject m_oOriginEnhanceFX = null;
	[SerializeField] private List<GameObject> m_oEnhanceCurrencyUIsList = new List<GameObject>();
	#endregion // 변수

	#region 함수
	/** 에이전트 강화 버튼을 눌렀을 경우 */
	public void OnTouchAgentEnhanceBtn()
	{
		// 에이전트 강화가 불가능 할 경우
		if (ComUtil.IsEnableEnhanceAgent(m_oSelCharacterTable))
		{
			StartCoroutine(this.CoEnhanceAgent(m_oSelCharacterTable));
			return;
		}

		var oLevelTable = ComUtil.GetAgentLevelTable(m_oSelCharacterTable);

		// 레벨 테이블이 없을 경우
		if (oLevelTable == null)
		{
			return;
		}

		int nNumItemsA = GameManager.Singleton.invenMaterial.GetItemCount(oLevelTable.RequireItemKey00);
		int nNumItemsB = GameManager.Singleton.invenMaterial.GetItemCount(oLevelTable.RequireItemKey01);

		// 카드가 부족 할 경우
		if (nNumItemsA < oLevelTable.RequireItemCount00)
		{
			PopupBoxMaterial pbm = MenuManager.Singleton.OpenPopup<PopupBoxMaterial>(EUIPopup.PopupBoxMaterial, true);
			pbm.InitializeInfo(true);
		}
		// 골드가 부족 할 경우
		else if(nNumItemsB < oLevelTable.RequireItemCount01)
		{
			this.OnClickAddGameMoney();
		}
	}

	public void OnTouchAgentEnhanceBtn(bool isTutorial)
	{
		OnTouchAgentEnhanceBtn();

        GameManager.Singleton.tutorial.Activate(false);
    }

    /** 에이전트 강화 재화 버튼을 눌렀을 경우 */
    public void OnTouchAgentEnhanceCurrencyBtn(int a_nIdx)
	{
		var oItemCharacter = ComUtil.GetItemCharacter(m_oSelCharacterTable);

		var oLevelTable = CharacterLevelTable.GetTable(m_oSelCharacterTable.PrimaryKey,
			(oItemCharacter != null) ? oItemCharacter.nCurUpgrade : 0);

		uint nMaterialKey = (a_nIdx <= 0) ? oLevelTable.RequireItemKey00 : oLevelTable.RequireItemKey01;

		PopupMaterial oPopupMaterial = MenuManager.Singleton.OpenPopup<PopupMaterial>(EUIPopup.PopupMaterial, true);
		oPopupMaterial.InitializeInfo(new ItemMaterial(0, nMaterialKey, 0));
	}

	/** 에이전트 강화 활성 여부를 검사한다 */
	public bool IsActiveAgentEnhance(CharacterTable a_oCharacterTable, int a_nIdx, int a_nOrder)
	{
		int nLevel = (a_nIdx + 1) * ComType.G_OFFSET_AGENT_SKILL_LEVEL;
		var oItemCharacter = ComUtil.GetItemCharacter(a_oCharacterTable);

		var oType = oItemCharacter._stSkillUpgrade.GetType();
		var oFieldInfo = oType.GetField(string.Format("Upgrade{0:00}", nLevel));

		int nOrderVal = (oFieldInfo != null) ? (int)oFieldInfo.GetValue(oItemCharacter._stSkillUpgrade) : 0;
		return (a_nOrder == 0 && nOrderVal > 0) || (a_nOrder == 1 && nOrderVal < 0);
	}

	/** 강화 가능 스킬 여부를 검사한다 */
	public bool IsEnhanceSkill(CharacterTable a_oCharacterTable, int a_nIdx)
	{
		int nOrderVal = ComUtil.GetAgentEnhanceOrderVal(a_oCharacterTable, a_nIdx);
		var oSkillTable = ComUtil.GetAgentEnhanceSkillTable(a_oCharacterTable, a_nIdx, 0, nOrderVal);

		return oSkillTable.UpgradeMaterialKey00 != 0 && oSkillTable.UpgradeMaterialKey01 != 0;
	}

	/** 스킬 강화 가능 여부를 검사한다 */
	public bool IsEnableEnhanceSkill(CharacterTable a_oCharacterTable, int a_nIdx)
	{
		int nOrderVal = ComUtil.GetAgentEnhanceOrderVal(a_oCharacterTable, a_nIdx);
		var oSkillTable = ComUtil.GetAgentEnhanceSkillTable(a_oCharacterTable, a_nIdx, 0, nOrderVal);

		int nNumItemsA = GameManager.Singleton.invenMaterial.GetItemCount(oSkillTable.UpgradeMaterialKey00);
		int nNumItemsB = GameManager.Singleton.invenMaterial.GetItemCount(oSkillTable.UpgradeMaterialKey01);

		return nNumItemsA >= oSkillTable.UpgradeMaterialCount00 &&
			nNumItemsB >= oSkillTable.UpgradeMaterialCount01 && !ComUtil.IsMaxEnhance(a_oCharacterTable, a_nIdx);
	}

	/** 강화 UI 를 설정한다 */
	private void SetupUIsEnhance()
	{
		int nNumSteps = this.MaxAgentLevel / ComType.G_OFFSET_AGENT_SKILL_LEVEL;

		for (int i = 0; i < nNumSteps; ++i)
		{
			var oScrollerCellViewEnhanceBG = GameResourceManager.Singleton.CreateObject<PopupAgentScrollerCellViewEnhanceBG>(m_oOriginScrollerCellViewEnhanceBG,
				null, null);

			var oScrollerCellViewEnhanceFG = GameResourceManager.Singleton.CreateObject<PopupAgentScrollerCellViewEnhanceFG>(m_oOriginScrollerCellViewEnhanceFG,
				null, null);

			oScrollerCellViewEnhanceBG.Init(PopupAgentScrollerCellViewEnhanceBG.MakeParams(i,
				this, this.OnReceiveAgentSelCallbackEnhance));

			oScrollerCellViewEnhanceFG.Init(PopupAgentScrollerCellViewEnhanceFG.MakeParams(i,
				this, this.OnReceiveAgentSelCallbackEnhance));

			oScrollerCellViewEnhanceBG.transform.SetParent(m_oEnhanceScrollViewContentsBG.transform, false);
			oScrollerCellViewEnhanceFG.transform.SetParent(m_oEnhanceScrollViewContentsFG.transform, false);

			m_oPopupAgentScrollerCellViewEnhanceBGList.Add(oScrollerCellViewEnhanceBG);
			m_oPopupAgentScrollerCellViewEnhanceFGList.Add(oScrollerCellViewEnhanceFG);
		}

		for (int i = 0; i < m_oEnhanceCurrencyUIsList.Count; ++i)
		{
			int nIdx = i;
			var oCurrencyBtn = m_oEnhanceCurrencyUIsList[i].GetComponent<SoundButton>();

			oCurrencyBtn.onClick.AddListener(() => this.OnTouchAgentEnhanceCurrencyBtn(nIdx));
		}
	}

	/** 강화 UI 상태를 갱신한다 */
	private void UpdateUIsStateEnhance()
	{
		var oItemCharacter = ComUtil.GetItemCharacter(m_oSelCharacterTable);

		var oLevelTable = CharacterLevelTable.GetTable(m_oSelCharacterTable.PrimaryKey,
			(oItemCharacter != null) ? oItemCharacter.nCurUpgrade : 0);

		int nNumItemsA = oLevelTable.RequireItemCount00;
		int nCurNumItemsA = GameManager.Singleton.invenMaterial.GetItemCount(oLevelTable.RequireItemKey00);

		int nNumItemsB = oLevelTable.RequireItemCount01;
		int nCurNumItemsB = GameManager.Singleton.invenMaterial.GetItemCount(oLevelTable.RequireItemKey01);

		m_oAgentEnhanceBtn.interactable = oItemCharacter.nCurUpgrade < ComUtil.GetAgentMaxLevel(m_oSelCharacterTable);
		m_oEnhanceBtnBlindImg.gameObject.SetActive(oItemCharacter.nCurUpgrade >= ComUtil.GetAgentMaxLevel(m_oSelCharacterTable));

		var oCurrencyNumTextA = ComUtil.FindComponentByName<TMP_Text>("NumText", m_oEnhanceCurrencyUIsList[(int)ECurrencyKey.CURRENCY_A].transform);
		oCurrencyNumTextA.text = $"{nCurNumItemsA}/{nNumItemsA}";
		oCurrencyNumTextA.color = (nCurNumItemsA >= nNumItemsA) ? Color.white : Color.red;

		var oCurrencyNumTextB = ComUtil.FindComponentByName<TMP_Text>("NumText", m_oEnhanceCurrencyUIsList[(int)ECurrencyKey.CURRENCY_B].transform);
		oCurrencyNumTextB.text = $"{nNumItemsB}";
		oCurrencyNumTextB.color = (nCurNumItemsB >= nNumItemsB) ? Color.white : Color.red;

		var oCurrencyIconImgA = ComUtil.FindComponentByName<Image>("IconImg", m_oEnhanceCurrencyUIsList[(int)ECurrencyKey.CURRENCY_A].transform);
		oCurrencyIconImgA.sprite = ComUtil.GetIcon(oLevelTable.RequireItemKey00);

		var oCurrencyIconImgB = ComUtil.FindComponentByName<Image>("IconImg", m_oEnhanceCurrencyUIsList[(int)ECurrencyKey.CURRENCY_B].transform);
		oCurrencyIconImgB.sprite = ComUtil.GetIcon(oLevelTable.RequireItemKey01);

		for (int i = 0; i < m_oPopupAgentScrollerCellViewEnhanceBGList.Count; ++i)
		{
			m_oPopupAgentScrollerCellViewEnhanceBGList[i].UpdateUIsState();
		}

		for (int i = 0; i < m_oPopupAgentScrollerCellViewEnhanceFGList.Count; ++i)
		{
			m_oPopupAgentScrollerCellViewEnhanceFGList[i].UpdateUIsState();
		}
	}

	/** 에이전트 강화 팝업이 닫혔을 경우 */
	private void OnClosePopupAgentEnhance(PopupAgentEnhance a_oSender)
	{
		this.UpdateUIsState();
	}

	/** 에이전트 강화 선택 콜백을 수신했을 경우 */
	private void OnReceiveAgentSelCallbackEnhance(PopupAgentScrollerCellView a_oSender, int a_nIdx)
	{
		var oScrollerCellViewEnhanceBG = a_oSender as PopupAgentScrollerCellViewEnhanceBG;
		var stParams = PopupAgentEnhance.MakeParams(this, m_oSelCharacterTable, a_nIdx, this.OnClosePopupAgentEnhance);

		var oPopupAgentEnhance = MenuManager.Singleton.OpenPopup<PopupAgentEnhance>(EUIPopup.PopupAgentEnhance, true);
		oPopupAgentEnhance.Init(stParams);
	}
	#endregion // 함수
}

/** 에이전트 팝업 - 강화 (코루틴) */
public partial class PopupAgent : UIDialog
{
	#region 함수
	/** 에이전트를 강화한다 */
	private IEnumerator CoEnhanceAgent(CharacterTable a_oCharacterTable)
	{
		var oWaitPopup = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);
		var oItemCharacter = ComUtil.GetItemCharacter(a_oCharacterTable);
		var oLevelTable = CharacterLevelTable.GetTable(a_oCharacterTable.PrimaryKey, oItemCharacter.nCurUpgrade);

		long nItemIDA = GameManager.Singleton.invenMaterial.GetItemID(oLevelTable.RequireItemKey00);
		long nItemIDB = GameManager.Singleton.invenMaterial.GetItemID(oLevelTable.RequireItemKey01);

		int nNumItemsA = GameManager.Singleton.invenMaterial.GetItemCount(nItemIDA);
		int nNumItemsB = GameManager.Singleton.invenMaterial.GetItemCount(nItemIDB);

		yield return GameDataManager.Singleton.ItemUpgrade(oItemCharacter, oItemCharacter.nCurUpgrade + 1, new Dictionary<long, int>()
		{
			[nItemIDA] = nNumItemsA - oLevelTable.RequireItemCount00,
			[nItemIDB] = nNumItemsB - oLevelTable.RequireItemCount01
		});

		m_oAgentEnhanceParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		m_oAgentEnhanceParticle.Play(true);

		GameManager.Singleton.invenMaterial.ModifyItem(nItemIDA,
			InventoryData<ItemMaterial>.EItemModifyType.Volume, nNumItemsA - oLevelTable.RequireItemCount00);

		GameManager.Singleton.invenMaterial.ModifyItem(nItemIDB,
			InventoryData<ItemMaterial>.EItemModifyType.Volume, nNumItemsB - oLevelTable.RequireItemCount01);

		GameManager.Singleton.invenCharacter.ModifyItem(oItemCharacter.id,
			InventoryData<ItemCharacter>.EItemModifyType.Upgrade, oItemCharacter.nCurUpgrade + 1);

		// 신규 강화가 잠금 해제되었을 경우
		if ((oItemCharacter.nCurUpgrade + 1) % ComType.G_OFFSET_AGENT_SKILL_LEVEL == 0)
		{
			int nIdx = oItemCharacter.nCurUpgrade / ComType.G_OFFSET_AGENT_SKILL_LEVEL;
			var stParams = PopupAgentEnhance.MakeParams(this, m_oSelCharacterTable, nIdx, this.OnClosePopupAgentEnhance, true);

			var oPopupAgentEnhance = MenuManager.Singleton.OpenPopup<PopupAgentEnhance>(EUIPopup.PopupAgentEnhance, true);
			oPopupAgentEnhance.Init(stParams);
		}

#if DISABLE_THIS
		var oParticleSystem = GameResourceManager.Singleton.CreateObject<ParticleSystem>(m_oOriginEnhanceFX, 
			m_oAgent.transform, null, ComType.G_LIFE_T_COMMON_FX);

		oParticleSystem.transform.localScale = Vector3.one * 1.5f;

		oParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		oParticleSystem.Play(true);
#endif // #if DISABLE_THIS

		oWaitPopup.Close();
		this.UpdateUIsState();
	}
	#endregion // 함수
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/** 에이전트 구입 팝업 */
public partial class PopupAgentBuy : UIDialog
{
	/** 매개 변수 */
	public struct STParams
	{
		public PopupAgent m_oPopupAgent;
		public CharacterTable m_oCharacterTable;

		public System.Action<PopupAgentBuy, CharacterTable> m_oBuyCallback;
	}

	#region 변수
	[Header("=====> Popup Agent Buy - UIs <=====")]
	[SerializeField] private TMP_Text m_oNumText = null;
	[SerializeField] private TMP_Text m_oPriceText = null;

	[SerializeField] private TMP_Text m_oTitleText = null;
	[SerializeField] private TMP_Text m_oAgentDescText = null;

	[SerializeField] private TMP_Text m_oSkillNameText = null;
	[SerializeField] private TMP_Text m_oSkillDescText = null;

	[SerializeField] private Image m_oAgentIconImg = null;
	[SerializeField] private Image m_oSkillIconImg = null;

	[Header("=====> Popup Agent Buy - Game Objects <=====")]
	[SerializeField] private GameObject m_oAgentRoot = null;

	private GameObject m_oAgent = null;
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public void Awake()
	{
		base.Initialize();
	}

	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;

		// 에이전트가 존재 할 경우
		if (m_oAgent != null)
		{
			GameResourceManager.Singleton.ReleaseObject(m_oAgent, false);
		}
		
		m_oAgent = GameResourceManager.Singleton.CreateObject(EResourceType.Character, 
			a_stParams.m_oCharacterTable.Prefab, m_oAgentRoot.transform);

		this.UpdateUIsState();
		ComUtil.RebuildLayouts(this.gameObject);
	}

	/** UI 상태를 갱신한다 */
	public void UpdateUIsState()
	{
		var stSkillTableInfo = this.GetAgentActiveSkillInfo(this.Params.m_oCharacterTable);
		var oEffectTableList = EffectTable.GetGroup(stSkillTableInfo.Item2.HitEffectGroup);

		string oAgentDescFmt = (this.Params.m_oCharacterTable.DescKey > 0) ? 
			DescTable.GetValue(this.Params.m_oCharacterTable.DescKey) : string.Empty;

		m_oTitleText.text = NameTable.GetValue(this.Params.m_oCharacterTable.NameKey);
		m_oAgentDescText.text = string.Format(oAgentDescFmt, (stSkillTableInfo.Item1 + 1) * ComType.G_OFFSET_AGENT_SKILL_LEVEL);

		m_oSkillNameText.text = NameTable.GetValue(oEffectTableList[0].NameKey);
		m_oSkillDescText.text = DescTable.GetValue(oEffectTableList[0].DescKey);

		m_oPriceText.text = $"{this.Params.m_oCharacterTable.PreRequireItemCount}";
		m_oSkillIconImg.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Common, oEffectTableList[0].Icon);
	}

	/** 에이전트 구입 버튼을 눌렀을 경우 */
	public void OnTouchBuyAgentBtn()
	{
		// 구입 가능 할 경우
		if (this.IsEnableBuyAgent())
		{
			StartCoroutine(this.CoBuyAgent());
			return;
		}
		
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

	/** 구입 가능 여부를 검사한다 */
	private bool IsEnableBuyAgent()
	{
		return GameManager.Singleton.invenMaterial.GetItemCount(this.Params.m_oCharacterTable.PreRequireItemKey) >= this.Params.m_oCharacterTable.PreRequireItemCount;
	}
	#endregion // 함수

	#region 접근 함수
	/** 에이전트 액티브 스킬 정보를 반환한다 */
	public (int, SkillTable) GetAgentActiveSkillInfo(CharacterTable a_oCharacterTable)
	{
		int nMaxAgentLevel = GlobalTable.GetData<int>(ComType.G_VALUE_MAX_CHARACTER_LEVEL);

		for (int i = 0; i * ComType.G_OFFSET_AGENT_SKILL_LEVEL < nMaxAgentLevel; ++i)
		{
			var oSkillTable = ComUtil.GetAgentEnhanceSkillTable(a_oCharacterTable, i, 0, 1);

			// 액티브 스킬 일 경우
			if (oSkillTable.UseType == (int)ESkillUseType.ACTIVE)
			{
				return (i, oSkillTable);
			}
		}

		return default;
	}
	#endregion // 접근 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(PopupAgent a_oPopupAgent,
		CharacterTable a_oCharacterTable, System.Action<PopupAgentBuy, CharacterTable> a_oBuyCallback)
	{
		return new STParams()
		{
			m_oPopupAgent = a_oPopupAgent,
			m_oCharacterTable = a_oCharacterTable,

			m_oBuyCallback = a_oBuyCallback
		};
	}
	#endregion // 클래스 팩토리 함수
}

/** 에이전트 구입 팝업 - 코루틴 */
public partial class PopupAgentBuy : UIDialog
{
	#region 함수
	/** 에이전트를 구입한다 */
	private IEnumerator CoBuyAgent()
	{
		var oWaitPopup = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);
		var stSkillTableInfo = this.GetAgentActiveSkillInfo(this.Params.m_oCharacterTable);

		yield return GameManager.Singleton.AddItemCS(this.Params.m_oCharacterTable.PrimaryKey, 1, null);
		yield return GameManager.Singleton.invenMaterial.ConsumeCrystal(this.Params.m_oCharacterTable.PreRequireItemCount);

		var oItemCharacter = ComUtil.GetItemCharacter(this.Params.m_oCharacterTable);
		yield return GameDataManager.Singleton.ItemUpgrade(oItemCharacter, (stSkillTableInfo.Item1 + 1) * ComType.G_OFFSET_AGENT_SKILL_LEVEL - 1, new Dictionary<long, int>());

		GameManager.Singleton.invenCharacter.ModifyItem(oItemCharacter.id, 
			InventoryData<ItemCharacter>.EItemModifyType.Upgrade, (stSkillTableInfo.Item1 + 1) * ComType.G_OFFSET_AGENT_SKILL_LEVEL - 1);

		oWaitPopup.Close();
		this.Params.m_oBuyCallback?.Invoke(this, this.Params.m_oCharacterTable);
	}
	#endregion // 함수
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PopupBattleUpgradeGuide : UIDialog
{
	/** 매개 변수 */
	public struct STParams {
		public System.Action<PopupBattleUpgradeGuide> m_oCallback;
	}

    #region 변수
	[Header("=====> Etc <=====")]
	[SerializeField] private Animation m_oAnimation = null;

	[Header("=====> UIs <=====")]
    [SerializeField] private TMP_Text m_oTitleText = null;
    [SerializeField] private TMP_Text m_oMoveToInventoryBtnText = null;

    [SerializeField] private TMP_Text m_oSubTitleCharacterText = null;

    [SerializeField] private TMP_Text m_oCharacterEnhanceTitleText = null;
    [SerializeField] private TMP_Text m_oCharacterEnhanceDescText = null;

    [SerializeField] private TMP_Text m_oSubTitleWeaponText = null;

	[SerializeField] private TMP_Text m_oEnhanceTitleText = null;
	[SerializeField] private TMP_Text m_oEnhanceDescText = null;

	[SerializeField] private TMP_Text m_oLimitBreakTitleText = null;
	[SerializeField] private TMP_Text m_oLimitBreakDescText = null;

	[SerializeField] private TMP_Text m_oAwakeningTitleText = null;
	[SerializeField] private TMP_Text m_oAwakeningDescText = null;

	[Header("=====> Game Objects <=====")]
	[SerializeField] private GameObject m_oBottomUIs = null;
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }
	#endregion // 프로퍼티

	#region 함수
	private void Awake()
	{
		Initialize();
		m_oBottomUIs.SetActive(GameManager.Singleton.user.m_nLevel >= GlobalTable.GetData<int>("valueInventoryOpenLevel"));
	}

	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;
		m_oAnimation.Play();

		// 텍스트를 갱신한다
        m_oTitleText.text = UIStringTable.GetValue("ui_popup_upgradeguide_title");
		m_oMoveToInventoryBtnText.text = UIStringTable.GetValue("ui_popup_upgradeguide_button_caption");

        m_oSubTitleCharacterText.text = UIStringTable.GetValue("ui_popup_upgradeguide_character_title");

        m_oCharacterEnhanceTitleText.text = UIStringTable.GetValue("ui_popup_upgradeguide_characterenchant_title");
        m_oCharacterEnhanceDescText.text = UIStringTable.GetValue("ui_popup_upgradeguide_characterenchant_desc");

        m_oSubTitleWeaponText.text = UIStringTable.GetValue("ui_popup_upgradeguide_item_title");

        m_oEnhanceTitleText.text = UIStringTable.GetValue("ui_popup_upgradeguide_enchant_title");
		m_oEnhanceDescText.text = UIStringTable.GetValue("ui_popup_upgradeguide_enchant_desc");

		m_oLimitBreakTitleText.text = UIStringTable.GetValue("ui_popup_upgradeguide_limitbreak_title");
		m_oLimitBreakDescText.text = UIStringTable.GetValue("ui_popup_upgradeguide_limitbreak_desc");

		m_oAwakeningTitleText.text = UIStringTable.GetValue("ui_popup_upgradeguide_awaken_title");
		m_oAwakeningDescText.text = UIStringTable.GetValue("ui_popup_upgradeguide_awaken_desc");
	}

	/** 인벤토리 이동 버튼을 눌렀을 경우 */
	public void OnTouchMoveToInventoryBtn()
	{
		MenuManager.Singleton.IsMoveToInventory = true;
		this.Params.m_oCallback?.Invoke(this);
	}

	public override void Initialize()
	{
		base.Initialize();
	}

	public override void Open()
	{
		base.Open();
	}
	#endregion // 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(System.Action<PopupBattleUpgradeGuide> a_oCallback) {
		return new STParams() {
			m_oCallback = a_oCallback
		};
	}
	#endregion // 클래스 팩토리 함수
}

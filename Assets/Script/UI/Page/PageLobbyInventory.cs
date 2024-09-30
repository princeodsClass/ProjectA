using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Security.Cryptography;

public partial class PageLobbyInventory : MonoBehaviour
{
    [SerializeField]
    GameObject _goCheat;

    [SerializeField]
    TMP_Dropdown dropdownWeapon, dropdownGear, dropdownMaterial;

    [SerializeField]
    Transform[] _slotEquipWeapon, _slotEquipGear;

    [SerializeField]
    GameObject[] _RepositoryPage, _RepositoryButtonActiveBG, _goRepositoryNewTag, _RepositoryIcon;

    [SerializeField]
    SoundButton[] _RepositorySelectButton;

    [SerializeField]
    TextMeshProUGUI[] _RepositoryButtonCaption, _RepositoryButtonSelectedCaption;

    [SerializeField]
    TextMeshProUGUI _txtButtonWorkshop, _txtButtonRecycle;

    [SerializeField]
    GameObject _CharacterModelRoot, _InvenCounter;

    [Header("자원 세부 탭 오브젝트")]
    [SerializeField]
    GameObject[] _goRepositoryMaterialPage;

    [Header("자원 세부 탭 오브젝트 이름")]
    [SerializeField]
    TextMeshProUGUI[] _txtRepositoryMaterialPage;

    [Header("Contents Lock 오브젝트 루트")]
    [SerializeField]
    Transform[] _tRepositoryMaterialPage;

    [SerializeField]
    GameObject _goWorkshopLock, _goRecycleLock,
               _goHeadUnlock, _goUpperUnlock, _goHandsUnlock, _goLowerUnlock,
               _goHeadLock, _goUpperLock, _goHandsLock, _goLowerLock;

    [SerializeField]
    TextMeshProUGUI _txtHeadLock, _txtUpperLock, _txtHandsLock, _txtLowerLock;

    [Header("캐릭터 스탯")]
    [SerializeField]
    TextMeshProUGUI _txtCharacterName, _txtCharacterHP, _txtCharacterDP;

    [SerializeField]
    GameObject[] _objResizeTarget;

    [SerializeField]
    PageLobbyMission _pageMission;

    MenuManager m_MenuMgr = null;
    UserAccount m_Account = null;

    InventoryData<ItemWeapon> m_InvenWeapon = null;
    InventoryData<ItemMaterial> m_InvenMaterial = null;
    InventoryData<ItemGear> m_InvenGear = null;
    InventoryData<ItemCharacter> m_InvenCharacter = null;

    public Dictionary<long, GameObject> _dicWeapon;
    public Dictionary<long, GameObject>[] _dicMaterial;
    public Dictionary<long, GameObject> _dicGear;

    GameObject _objCharacter = null;

    PageLobby _lobby = null;
    PageLobbyShop _shop = null;
    PageLobbyTop _top = null;

    int _curPage = 0;

    public Transform _tempSlot;

    private void Awake()
    {
        if ( null == m_MenuMgr ) m_MenuMgr = MenuManager.Singleton;
        if ( null == m_Account ) m_Account = GameManager.Singleton.user;
        if ( null == m_InvenWeapon ) m_InvenWeapon = GameManager.Singleton.invenWeapon;
        if ( null == m_InvenMaterial ) m_InvenMaterial = GameManager.Singleton.invenMaterial;
        if ( null == m_InvenGear ) m_InvenGear = GameManager.Singleton.invenGear;
        if ( null == m_InvenCharacter) m_InvenCharacter = GameManager.Singleton.invenCharacter;

        if ( null == _top ) _top = FindObjectOfType<PageLobbyTop>();
        if ( null == _shop ) _shop = FindObjectOfType<PageLobbyShop>();
        if ( null == _lobby) _lobby = gameObject.GetComponentInParent<PageLobby>();

        _dicWeapon = new Dictionary<long, GameObject>();
        _dicMaterial = new Dictionary<long, GameObject>[_goRepositoryMaterialPage.Length];

        for ( int i = 0; i < _goRepositoryMaterialPage.Length; i++)
            _dicMaterial[i] = new Dictionary<long, GameObject>();

        _dicGear = new Dictionary<long, GameObject>();

        _goRepositoryNewTag.ToList().ForEach(t => t.SetActive(false));

        InitializeText();
        DevInitialize();
    }

    void DevInitialize()
    {
#if UNITY_EDITOR
        _goCheat.SetActive(true);

        dropdownWeapon.ClearOptions();
        dropdownGear.ClearOptions();
        dropdownMaterial.ClearOptions();

        List<WeaponTable> opWeapon = WeaponTable.GetList();
        List<string> opWeaponName = new List<string>();

        List<GearTable> opGear= GearTable.GetList();
        List<string> opGearName = new List<string>();

        List<MaterialTable> opMaterial = MaterialTable.GetList();
        List<string> opMaterialName = new List<string>();

        for (int i = 0; i < (int)opWeapon.Count; i++)
            opWeaponName.Add(opWeapon[i].Prefab);

        for (int i = 0; i < (int)opGear.Count; i++)
            opGearName.Add(opGear[i].Prefab);

        for (int i = 0; i < (int)opMaterial.Count; i++)
            opMaterialName.Add(opMaterial[i].Prefab);

        dropdownWeapon.AddOptions(opWeaponName);
        dropdownWeapon.onValueChanged.AddListener(AddWeapon);

        dropdownGear.AddOptions(opGearName);
        dropdownGear.onValueChanged.AddListener(AddGear);

        dropdownMaterial.AddOptions(opMaterialName);
        dropdownMaterial.onValueChanged.AddListener(AddMaterial);
#else
        _goCheat.SetActive(false);
#endif
    }

    void AddWeapon(int order)
    {
        PopupWait4Response wait = m_MenuMgr.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response);

        StartCoroutine(GameManager.Singleton.AddItemCS(WeaponTable.GetData(order).PrimaryKey, 1, () =>
        {
            wait.Close();
            InitializeWeapon();
        }));
    }

    void AddGear(int order)
    {
        PopupWait4Response wait = m_MenuMgr.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response);

        StartCoroutine(GameManager.Singleton.AddItemCS(GearTable.GetData(order).PrimaryKey, 1, () =>
        {
            wait.Close();
            InitializeGear();
        }));
    }

    void AddMaterial(int order)
    {
        PopupWait4Response wait = m_MenuMgr.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response);

        StartCoroutine(GameManager.Singleton.AddItemCS(MaterialTable.GetData(order).PrimaryKey, 1000, () =>
        {
            wait.Close();
            InitializeMaterial();
        }));
    }

    void InitializeText()
    {
        _RepositoryButtonCaption[0].text = _RepositoryButtonSelectedCaption[0].text =
            UIStringTable.GetValue("ui_page_lobby_inventory_tab_weapon");
        _RepositoryButtonCaption[1].text = _RepositoryButtonSelectedCaption[1].text =
            UIStringTable.GetValue("ui_page_lobby_inventory_tab_material");
        _RepositoryButtonCaption[2].text = _RepositoryButtonSelectedCaption[2].text =
            UIStringTable.GetValue("ui_page_lobby_inventory_tab_gear");

        _txtButtonWorkshop.text = UIStringTable.GetValue("ui_page_lobby_inventory_button_workshop");
        _txtButtonRecycle.text = UIStringTable.GetValue("ui_page_lobby_inventory_button_recycle");

        for ( int i = 0; i < _txtRepositoryMaterialPage.Length; i++ )
            _txtRepositoryMaterialPage[i].text = UIStringTable.GetValue($"ui_page_lobby_inventory_material_caption_{i}");
    }

    private void OnEnable()
    {
        InitializeInventory();

        OnPageButtonClick(0);

        _goWorkshopLock.SetActive(m_Account.m_nLevel < GlobalTable.GetData<int>("valueWorkshopOpenLevel"));
        _goRecycleLock.SetActive(m_Account.m_nLevel < GlobalTable.GetData<int>("valueRecycleOpenLevel"));

        _txtButtonWorkshop.transform.parent.gameObject.SetActive(!_goWorkshopLock.activeSelf);
        _txtButtonRecycle.transform.parent.gameObject.SetActive(!_goRecycleLock.activeSelf);
    }

    public void InitializeInventory()
    {
        InitializeMaterial();
        InitializeWeapon();
        InitializeGear();
        InitializeCharacter();
        InitializeCharacterStats();
        InitializeLock();
    }

    void InitializeLock()
    {
        int head = GlobalTable.GetData<int>("valueHeadOpenLevel");
        _goHeadLock.SetActive(m_Account.m_nLevel < head);
        _txtHeadLock.text = $"Lv.{head}";
        _goHeadUnlock.SetActive(m_Account.m_nLevel >= head);

        int upper = GlobalTable.GetData<int>("valueUpperOpenLevel");
        _goUpperLock.SetActive(m_Account.m_nLevel < upper);
        _txtUpperLock.text = $"Lv.{upper}";
        _goUpperUnlock.SetActive(m_Account.m_nLevel >= upper);

        int hands = GlobalTable.GetData<int>("valueHandsOpenLevel");
        _goHandsLock.SetActive(m_Account.m_nLevel < hands);
        _txtHandsLock.text = $"Lv.{hands}";
        _goHandsUnlock.SetActive(m_Account.m_nLevel >= hands);

        int lower = GlobalTable.GetData<int>("valueLowerOpenLevel");
        _goLowerLock.SetActive(m_Account.m_nLevel < lower);
        _txtLowerLock.text = $"Lv.{lower}";
        _goLowerUnlock.SetActive(m_Account.m_nLevel >= lower);
    }

    public void InitializeCharacterStats()
    {
        ItemCharacter curCharacter = null;

        if (null != m_InvenCharacter)
            foreach (ItemCharacter character in m_InvenCharacter)
                if (character.id == m_Account.m_nCharacterID)
                    curCharacter = character;

        CharacterTable ch = CharacterTable.GetData(curCharacter.nKey);
        _txtCharacterName.text = $"{NameTable.GetValue(ch.NameKey)} ({curCharacter.nCurUpgrade + 1})";

        List<CharacterLevelTable> group = CharacterLevelTable.GetGroup(ch.PrimaryKey);
        _txtCharacterHP.text = group[curCharacter.nCurUpgrade].HP.ToString();
        _txtCharacterDP.text = group[curCharacter.nCurUpgrade].DP.ToString();
    }

    public void ReSize()
    {
        StartCoroutine(CoResize());
    }

    IEnumerator CoResize()
    { 
        for ( int i = 0; i < _objResizeTarget.Length; i++ )
        { 
            if ( _objResizeTarget[i].activeSelf )
                LayoutRebuilder.ForceRebuildLayoutImmediate(_objResizeTarget[i].GetComponent<RectTransform>());

            yield return null;
        }
    }

    public Dictionary<long, GameObject> GetinvenGODic(EInventoryType type)
    {
        switch ( type )
        {
            case EInventoryType.weapon: return _dicWeapon;
            case EInventoryType.gear: return _dicGear;
        }

        return null;
    }

    void ItemCounter()
    {
        string cPre, cSuf;
        int cur = 0;
        int max = 0;

        int maxWeapon = m_Account.m_nMaxWeaponRepository;
        int maxGear = m_Account.m_nMaxGearRepository;

        switch (_curPage)
        {
            case 1:
                _InvenCounter.SetActive(false);
                return;
            case 0:
                _InvenCounter.SetActive(true);
                cur = m_InvenWeapon.GetItemCount();
                max = maxWeapon;
                break;
            case 2:
                _InvenCounter.SetActive(true);
                cur = m_InvenGear.GetItemCount();
                max = maxGear;
                break;
        }

        cPre = max <= cur ? "<color=red>" : "";
        cSuf = max <= cur ? "</color>" : "";

        _InvenCounter.GetComponentInChildren<TextMeshProUGUI>().text = $"{cPre}{cur} / {max}{cSuf}";
    }

    public void InitializeWeapon()
    {
        _dicWeapon.ToList().ForEach(slot => Destroy(slot.Value));
        _dicWeapon.Clear();

        if (null != m_InvenWeapon)
        {
            SlotWeapon slot;

            m_InvenWeapon.SortItem();

            foreach (ItemWeapon weapon in m_InvenWeapon)
            {
                _dicWeapon.Add(weapon.id, AddWeapon(weapon));

                slot = _dicWeapon[weapon.id].GetComponent<SlotWeapon>();
                slot.SetEquipReady(false);
                slot.SetTargetFX(false);
                slot.SetState(SlotWeapon.SlotState.normal);
                slot.SetBaseScrollRect(GetComponent<UIMultiScrollRect>());

                //ComUtil.SetParent(_RepositoryPage[0].transform, _dicWeapon[weapon.id].transform);

                for ( int i =  0; i < 4; i++ )
                {
                    if (weapon.id == GameManager.Singleton.user.m_nWeaponID[i])
                    {
                        ComUtil.SetParent(_slotEquipWeapon[i], _dicWeapon[weapon.id].transform);
                        slot.SetState(SlotWeapon.SlotState.equiped);
                        // slot.SetNewTag(false);
                        break;
                    }
                }

                if (weapon.bNew) SetNewTag(0, true);
            }
        }

        ReSize();
    }

    public void InitializeMaterial()
    {
        for (int i = 0; i < _dicMaterial.Length; i++)
        {
            // foreach (KeyValuePair<long, GameObject> slot in _dicMaterial[i])
            //     Destroy(slot.Value);

            ComUtil.DestroyChildren(_tRepositoryMaterialPage[i]);
            _dicMaterial[i].Clear();
        }

        if (null != m_InvenMaterial)
        {
            m_InvenMaterial.SortItem();

            foreach (ItemMaterial material in m_InvenMaterial)
            {
                if (material.nKey != ComType.KEY_ITEM_CRYSTAL_FREE &&   // 보너스 하드 재화
                    material.nKey != ComType.KEY_ITEM_CRYSTAL_PAY &&    // 구매한 하드 재화
                    material.nKey != ComType.KEY_ITEM_GOLD &&           // 게임 머니
                    material.nVolume > 0)                               // 아이템 수량이 0 인 것
                {
                    switch (material.eType)
                    {
                        case EItemType.Currency:
                            _dicMaterial[1].Add(material.id, AddMaterial(material, 1));
                            break;
                        case EItemType.Part:
                            switch(material.nSubType)
                            {
                                case 1:
                                    _dicMaterial[2].Add(material.id, AddMaterial(material, 2));
                                    break;
                                case 2:
                                    _dicMaterial[3].Add(material.id, AddMaterial(material, 3));
                                    break;
                                case 3:
                                    _dicMaterial[4].Add(material.id, AddMaterial(material, 4));
                                    break;
                                case 4:
                                    _dicMaterial[5].Add(material.id, AddMaterial(material, 5));
                                    break;
                                case 5:
                                    _dicMaterial[6].Add(material.id, AddMaterial(material, 6));
                                    break;
                                case 6:
                                    _dicMaterial[7].Add(material.id, AddMaterial(material, 7));
                                    break;
                                case 7:
                                    _dicMaterial[8].Add(material.id, AddMaterial(material, 8));
                                    break;
                            }
                            break;
                        case EItemType.GearPart:
                            switch (material.nSubType)
                            {
                                case 1:
                                    _dicMaterial[9].Add(material.id, AddMaterial(material, 9));
                                    break;
                                case 2:
                                    _dicMaterial[10].Add(material.id, AddMaterial(material, 10));
                                    break;
                                case 3:
                                    _dicMaterial[11].Add(material.id, AddMaterial(material, 11));
                                    break;
                                case 4:
                                    _dicMaterial[12].Add(material.id, AddMaterial(material, 12));
                                    break;
                            }
                            break;
                        default:
                            _dicMaterial[0].Add(material.id, AddMaterial(material, 0));
                            break;
                    }
                }

                if (material.bNew) _goRepositoryNewTag[0].SetActive(true);
            }

            for ( int i = 0; i < _goRepositoryMaterialPage.Length; i++ )
                _goRepositoryMaterialPage[i].SetActive(_dicMaterial[i].Count > 0);

            /*
                if ( material.nKey != ComType.KEY_ITEM_CRYSTAL_FREE &&
                     material.nKey != ComType.KEY_ITEM_CRYSTAL_PAY  &&
                     material.nKey != ComType.KEY_ITEM_GOLD && 
                     material.nVolume > 0 )
                    _dicMaterial.Add(material.id, AddMaterial(material));
            */
        }

        _top.InitializeCurrency();
        _shop.InitializeSlotToken();
        _pageMission.InitializeCompDefence();
        ReSize();
    }

    public void SetNewTag(int index, bool state)
    {
        bool cur = false;

        _goRepositoryNewTag[index].SetActive(state);

        for ( int i = 0; i < _goRepositoryNewTag.Length; i++ )
            if ( _goRepositoryNewTag[i].activeSelf ) cur = true;

        _lobby.SetNewTag(1, ( cur &&
                              GameManager.Singleton.user.m_nLevel >= GlobalTable.GetData<int>("valueInventoryOpenLevel") ) );
    }

    public void InitializeGear()
    {
        _dicGear.ToList().ForEach(slot => Destroy(slot.Value));
        _dicGear.Clear();

        if (null != m_InvenGear)
        {
            SlotGear slot;

            m_InvenGear.SortItem();

            foreach (ItemGear gear in m_InvenGear)
            {
                _dicGear.Add(gear.id, AddGear(gear));

                slot = _dicGear[gear.id].GetComponent<SlotGear>();
                slot.SetEquipReady(false);
                slot.SetState(SlotGear.SlotState.normal);

                //ComUtil.SetParent(_RepositoryPage[2].transform, _dicGear[gear.id].transform);

                for (int i = 0; i < 4; i++)
                {
                    if (gear.id == GameManager.Singleton.user.m_nGearID[i])
                    {
                        ComUtil.SetParent(_slotEquipGear[i], _dicGear[gear.id].transform);
                        slot.SetState(SlotGear.SlotState.equiped);
                        slot.SetNewTag(false);
                        break;
                    }
                }

                if (gear.bNew) SetNewTag(2, true);
            }
        }

        ReSize();
    }

    public void InitializeCharacter()
    {
        string cName = string.Empty;

		// 캐릭터가 존재 할 경우
		if(_objCharacter != null)
		{
			GameResourceManager.Singleton.ReleaseObject(_objCharacter, false);
			_objCharacter = null;
		}

        if (null != m_InvenCharacter)
            foreach (ItemCharacter character in m_InvenCharacter)
                if (character.id == m_Account.m_nCharacterID)
                    cName = CharacterTable.GetData(character.nKey).Prefab;

        if ( null == _objCharacter )
            _objCharacter = GameResourceManager.Singleton.CreateObject(EResourceType.Character, cName, _CharacterModelRoot.transform);
    }

    public void OnclickExtentRepository()
    {
        PopupRepositoryExtent extent = m_MenuMgr.OpenPopup<PopupRepositoryExtent>(EUIPopup.PopupRepositoryExtent);
        extent.InitializeInfo(_curPage, 1);
    }

    public void OnClickRecycle(bool isTutorial = false)
    {
        OnClickRecycle(0);
        GameManager.Singleton.tutorial.Activate(false);
    }

    public void OnClickRecycle(int index = 0)
    {
        PopupRecycle recycle = m_MenuMgr.OpenPopup<PopupRecycle>(EUIPopup.PopupRecycle);
        recycle.InitializeInfo(_dicWeapon, _dicGear, index);
    }

    public void OnClickWorkshop()
    {
        PopupWorkshop workshop = m_MenuMgr.OpenPopup<PopupWorkshop>(EUIPopup.PopupWorkshop);
        workshop.InitializeInfo();
    }

    public void OnClickWorkshopGearTutorial(bool isTutorial = false)
    {
        OnClickWorkshop();

        GameObject product = GameObject.Find("Product");
        RectTransform rt = product.GetComponent<RectTransform>();
        GameManager.Singleton.tutorial.SetFinger(product,
                                                 GameObject.Find("SlotRecipeItem").GetComponent<SlotRecipeItem>().OnClick,
                                                 rt.rect.width, rt.rect.height, 750);
    }

    public GameObject EquipWeapon(long key)
    {
        return _dicWeapon[key];
    }

    public void OnPageButtonClick(int index)
    {
        if ( index == 2 )
        {
            if ( m_Account.m_nLevel < GlobalTable.GetData<int>("valueGearOpenLevel") )
            {
                PopupSysMessage pop = m_MenuMgr.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
                pop.InitializeInfo("ui_error_title", "ui_hint_gear_desc_2", "ui_common_close", null, "TutorialGear");
                pop.AddMessage("ui_error_notenoughlevel_gear", 2);

                return;
            }
        }

        _curPage = index;

        for (int i = 0; i < _RepositorySelectButton.Length; i++)
        {
            _RepositorySelectButton[i].enabled = index != i;
            _RepositoryButtonActiveBG[i].gameObject.SetActive(index == i);
            _RepositoryIcon[i].SetActive(index == i);
            _RepositoryPage[i].gameObject.SetActive(index == i);
        }

        ItemCounter();
        ReSize();
    }

    public void SetWeaponAnimation(GameObject obj)
    {
        obj.GetComponent<SlotWeapon>().SetEquipReady(true);

        foreach(KeyValuePair<long, GameObject> weapon in _dicWeapon)
            for( int i = 0; i < 4; i++)
                if (weapon.Key == GameManager.Singleton.user.m_nWeaponID[i] )
                    weapon.Value.GetComponent<SlotWeapon>().SetEquipReady(true);
    }


    GameObject AddWeapon(ItemWeapon weapon)
    {
        SlotWeapon item = m_MenuMgr.LoadComponent<SlotWeapon>(_RepositoryPage[0].transform, EUIComponent.SlotWeapon);
        item.Initialize(weapon);
        item.SetSelected(SlotWeapon.SlotState.normal);

        ItemCounter();

        return item.gameObject;
    }

    GameObject AddMaterial(ItemMaterial material, int category)
    {
        SlotMaterial item = m_MenuMgr.LoadComponent<SlotMaterial>(_tRepositoryMaterialPage[category].transform, EUIComponent.SlotMaterial);
        item.Initialize(material);

        return item.gameObject;
    }

    GameObject AddGear(ItemGear gear)
    {
        SlotGear item = m_MenuMgr.LoadComponent<SlotGear>(_RepositoryPage[2].transform, EUIComponent.SlotGear);
        item.Initialize(gear);

        ItemCounter();

        return item.gameObject;
    }

    public void WorkshopLock()
    {
        PopupSysMessage pop = m_MenuMgr.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
        pop.InitializeInfo("ui_error_title", "ui_hint_workshop_desc_2", "ui_common_close", null, "TutorialWorkshop");
        pop.AddMessage("ui_error_notenoughlevel_workshop", 2);
    }

    public void RecycleLock()
    {
        PopupSysMessage pop = m_MenuMgr.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
        pop.InitializeInfo("ui_error_title", "ui_hint_recycle_desc_2", "ui_common_close", null, "TutorialRecycle");
        pop.AddMessage("ui_error_notenoughlevel_recycle", 2);
    }

    public void HeadLock()
    {
        PopupSysMessage pop = m_MenuMgr.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
        pop.InitializeInfo("ui_error_title", "ui_hint_gear_desc_2", "ui_common_close", null, "TutorialGear");
        pop.AddMessage("ui_error_notenoughlevel_head",2);
    }

    public void UpperLock()
    {
        PopupSysMessage pop = m_MenuMgr.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
        pop.InitializeInfo("ui_error_title", "ui_hint_gear_desc_2", "ui_common_close", null, "TutorialGear");
        pop.AddMessage("ui_error_notenoughlevel_upper", 2);
    }

    public void HandsLock()
    {
        PopupSysMessage pop = m_MenuMgr.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
        pop.InitializeInfo("ui_error_title", "ui_hint_gear_desc_2", "ui_common_close", null, "TutorialGear");
        pop.AddMessage("ui_error_notenoughlevel_hands", 2);
    }

    public void LowerLock()
    {
        PopupSysMessage pop = m_MenuMgr.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
        pop.InitializeInfo("ui_error_title", "ui_hint_gear_desc_2", "ui_common_close", null, "TutorialGear");
        pop.AddMessage("ui_error_notenoughlevel_lower", 2);
    }

    public void GearUnlock(int type)
    {
        PopupWorkshop workshop = m_MenuMgr.OpenPopup<PopupWorkshop>(EUIPopup.PopupWorkshop);
        workshop.InitializeInfo(1);

        RecipeTable recipe;

        switch ( type )
        {
            case 0:
                recipe = RecipeTable.GetData(GlobalTable.GetData<uint>("HeadRecipeKey"));
                break;
            case 1:
                recipe = RecipeTable.GetData(GlobalTable.GetData<uint>("UpperRecipeKey"));
                break;
            case 2:
                recipe = RecipeTable.GetData(GlobalTable.GetData<uint>("HandsRecipeKey"));
                break;
            default:
                recipe = RecipeTable.GetData(GlobalTable.GetData<uint>("LowerRecipeKey"));
                break;
        }    

        PopupWorkshopItem item = m_MenuMgr.OpenPopup<PopupWorkshopItem>(EUIPopup.PopupWorkshopItem, true);
        item.InitializeInfo(recipe);
    }
}

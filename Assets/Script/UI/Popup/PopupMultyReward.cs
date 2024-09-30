using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PopupMultyReward : UIDialog
{
	[SerializeField]
	Transform _tSlotRoot, _tRewardRoot, _tBoxModelRoot;

	[SerializeField]
	GameObject _goButtonSkip, _goButtonClose,
			   _goFX,
			   _goButtonBG, _goDesc;

	[SerializeField]
	GameObject _goGradeUp;

	[SerializeField]
	TextMeshProUGUI _txtGradeUp;

	Action _callback = null;

	Animator _BoxAnimator = null;

	GameObject _goBox;

	uint _boxkey;
	int _curReward;
	bool _isComplete = false;
	bool _isBoxShow = true;

	Dictionary<uint, int> _rewardsItem = new Dictionary<uint, int>();
	Dictionary<uint, int> _rewardsByBox = new Dictionary<uint, int>();

	List<SlotSimpleItemReward> _liSlotRewardsItem = new List<SlotSimpleItemReward>();
	List<SlotSimpleItemReward> _liSlotRewardsByBox = new List<SlotSimpleItemReward>();

	private void Awake()
	{
		Initialize();

		_goButtonSkip.SetActive(true);
		_goButtonClose.SetActive(false);

		_goGradeUp.SetActive(false);

		_goButtonSkip.GetComponentInChildren<TextMeshProUGUI>().text = UIStringTable.GetValue("ui_popup_boxreward_button_skip");
		_goButtonClose.GetComponentInChildren<TextMeshProUGUI>().text = UIStringTable.GetValue("ui_common_close");
		_goDesc.GetComponentInChildren<TextMeshProUGUI>().text = UIStringTable.GetValue("ui_hint_box_duplicate_desc");

		_txtGradeUp.text = UIStringTable.GetValue("ui_hint_increase_defence_new_difficulty");
	}

	public void InitializeInfo(MissionDefenceTable wave, Action callback = null)
	{
		GameManager.Singleton.StartCoroutine(m_Account.ClearDefence(wave, SetGradeUp));

		_callback = callback;

		ComUtil.DestroyChildren(_tBoxModelRoot);

		_isComplete = false;
		_curReward = 0;

		_rewardsItem = RewardTable.RandomResultInGroup(wave.RewardGroup);
		SetRewardsList();

		GameManager.Singleton.StartCoroutine(AddItem(_rewardsItem));
		GameManager.Singleton.StartCoroutine(AddItem(_rewardsByBox));

		InitializeRewards(_rewardsItem, _liSlotRewardsItem);
		InitializeRewards(_rewardsByBox, _liSlotRewardsByBox);

		SetBox();
	}

	void SetGradeUp()
    {
		_goGradeUp.SetActive(true);
	}

	void SetBox()
    {
		if ( _boxkey == 0 )
        {
			PopBox();
			return;
		}

		_goBox = m_ResourceMgr.CreateObject(EResourceType.Box, BoxTable.GetData(_boxkey).Prefeb, _tBoxModelRoot);

		if (_goBox.GetComponent<BoxRewards>() == null)
			_goBox.AddComponent<BoxRewards>();

		_BoxAnimator = _goBox.GetComponent<Animator>();
		_BoxAnimator.SetTrigger("Start");

		return;
	}

	void SetRewardsList()
	{
		_boxkey = 0;

		_rewardsItem.ToList().ForEach(el =>
		{
			if ( ComUtil.GetItemType(el.Key) == EItemType.Box )
				_boxkey = el.Key;
		});

		_rewardsItem.Remove(_boxkey);

		if ( _boxkey > 0 )
			_rewardsByBox = RewardTable.RandomResultInGroup(BoxTable.GetData(_boxkey).RewardGroup);
	}

	IEnumerator AddItem(Dictionary<uint, int> rewards)
	{
		foreach (KeyValuePair<uint, int> item in rewards)
		{
			switch ( ComUtil.GetItemType(item.Key) )
			{
				case EItemType.Weapon:
				case EItemType.Gear:
				case EItemType.Box:
				case EItemType.Character:
					for (int i = 0; i < item.Value; i++)
					{
						yield return GameManager.Singleton.StartCoroutine(m_GameMgr.AddItemCS(item.Key, 1));
					}
					break;
				default:
					yield return GameManager.Singleton.StartCoroutine(m_GameMgr.AddItemCS(item.Key, item.Value));
					break;
			}
		}
	}

	void InitializeRewards(Dictionary<uint, int> rewards, List<SlotSimpleItemReward> slotlist)
	{
		slotlist.Clear();
		SlotSimpleItemReward slot;

		uint key;
		int grade;
		string volume, name;
		Sprite icon;

		foreach (KeyValuePair<uint, int> reward in rewards)
		{
			key = reward.Key;
			volume = reward.Value.ToString();

			switch (ComUtil.GetItemType(reward.Key))
			{
				case EItemType.Weapon:
				case EItemType.Gear:
				case EItemType.Box:
				case EItemType.Character:
					icon = m_ResourceMgr.LoadSprite(EAtlasType.Icons, WeaponTable.GetData(key).Icon);
					volume = NameTable.GetValue(WeaponTable.GetData(key).NameKey);
					grade = WeaponTable.GetData(key).Grade;

					for (int i = 0; i < reward.Value; i++)
                    {
						slot = m_MenuMgr.LoadComponent<SlotSimpleItemReward>(_tSlotRoot, EUIComponent.SlotSimpleItemReward);
						slot.Initialize(icon, volume, string.Empty, grade, true, true, key);
						slot.gameObject.SetActive(false);

						slotlist.Add(slot);
					}

					break;
				default:
					icon = m_ResourceMgr.LoadSprite(EAtlasType.Icons, MaterialTable.GetData(key).Icon);
					name = NameTable.GetValue(MaterialTable.GetData(key).NameKey);
					grade = MaterialTable.GetData(key).Grade;

					slot = m_MenuMgr.LoadComponent<SlotSimpleItemReward>(_tSlotRoot, EUIComponent.SlotSimpleItemReward);
					slot.Initialize(icon, volume, name, grade, true, true, key);
					slot.gameObject.SetActive(false);

					slotlist.Add(slot);
					break;
			}
		}
	}

	public void PopCard()
	{
		if (_isComplete) return;

		for (int i = 0; i < _liSlotRewardsByBox.Count; i++)
		{
			if (i == _curReward)
			{
				_liSlotRewardsByBox[i].gameObject.SetActive(true);
				_liSlotRewardsByBox[i].SetAppear();
			}
			else
				_liSlotRewardsByBox[i].gameObject.SetActive(false);
		}

		_curReward++;
	}

	public void PopBox()
	{
		if (_isComplete) return;


		if ( _curReward < _liSlotRewardsByBox.Count )
        {
			_BoxAnimator.SetTrigger("Pop");
			//_curReward++;
		}
		else if ( _curReward < _liSlotRewardsByBox.Count + _liSlotRewardsItem.Count )
        {
			BoxDown();

			_liSlotRewardsByBox.ForEach(slot => slot.gameObject.SetActive(false));

			for (int i = 0; i < _liSlotRewardsItem.Count; i++)
			{
				if (i == _curReward - _liSlotRewardsByBox.Count)
				{
					_liSlotRewardsItem[i].gameObject.SetActive(true);
					_liSlotRewardsItem[i].SetRestart();
				}
				else
					_liSlotRewardsItem[i].gameObject.SetActive(false);
			}

			_curReward++;
		}
		else
			Skip();
	}

	void BoxDown()
    {
		if (null != _BoxAnimator && _isBoxShow)
		{
			_BoxAnimator?.SetTrigger("End");
			_isBoxShow = false;
		}
	}

	public void Skip()
	{
		BoxDown();
		StartCoroutine(ShowAll());
	}

	IEnumerator ShowAll()
	{
		_isComplete = true;
		_goButtonSkip.SetActive(false);
		_goButtonClose.SetActive(true);

		List<SlotSimpleItemReward> newlist = new List<SlotSimpleItemReward>();
		newlist.AddRange(_liSlotRewardsByBox);
		newlist.AddRange(_liSlotRewardsItem);

		newlist.ForEach(slot =>
		{
			ComUtil.SetParent(_tRewardRoot, slot.transform);
			slot.gameObject.SetActive(false);
		});

		foreach (SlotSimpleItemReward go in newlist)
		{
			go.gameObject.SetActive(true);
			go.SetRestart();

			yield return new WaitForSecondsRealtime(GlobalTable.GetData<float>("timeRewardShowGap") / 1000);
		}
	}

	public override void Initialize()
	{
		base.Initialize();
	}

	public override void Open()
	{
		base.Open();
	}

	public override void Close()
	{
		_callback?.Invoke();

		m_MenuMgr.SceneEnd();
		m_MenuMgr.SceneNext(ESceneType.Lobby);
	}
}
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PopupBoxReward : UIDialog, IPointerDownHandler, IPointerUpHandler
{
	private System.Action<PopupBoxReward> m_oCallback = null;

	[SerializeField]
	GameObject _slotRoot, _rewardRoot, _boxModelRoot;

	[SerializeField]
	TextMeshProUGUI _txtMoreCCount, _txtMoreTCount,
					_txtMoreCCount10, _txtMoreTCount10,
					_txtMoreCrystal, _txtMoreToken,
					_txtMoreCrystal10, _txtMoreToken10,
					_txtTicketCount;

	[SerializeField]
	GameObject _objButtonConfirm, _objButtonBoost, _objButtonSkip,
			   _goButtonConfirm,
			   _goButtonMoreCrystal, _goButtonMoreToken,
			   _goButtonMoreCrystal10, _goButtonMoreToken10,
			   _goMarkerAD, _goMarkerVIP, _goMarkerFree, _goMarkerTicket,
			   _objBox, _objFX,
			   _goButtonBG, _goFast, _goDesc;

	[SerializeField]
	GameObject _goNormalButton, _goShopBuyButton;

	[SerializeField]
	Image _imgTicketIcon;

	Sprite _icon;
	Animator animator = null;
	Animator _BoxAnimator = null;

	GameObject _Box;
	BoxDealTable _boxDeal;
	BoxTable _box;

	uint _key, _ticket;
	int _grade, _curReward;
	string _volume;
	bool _isGrade = false;
	bool _isComplete = false;
	bool _isBattle = false;

	Dictionary<uint, int> _rewardItem;
	List<GameObject> _goReward = new List<GameObject>();

	public enum EBoxType
	{
		normal,
		shopbuy,
	}

	EBoxType _bType;

	private void Awake()
	{
		Initialize();

		animator = GetComponent<Animator>();

		_ticket = GlobalTable.GetData<uint>("valueAdsRemoveTicketDecKey");
		_imgTicketIcon.sprite = ComUtil.GetIcon(_ticket);
		_txtTicketCount.text = m_InvenMaterial.GetItemCount(_ticket).ToString();

		_objButtonConfirm.GetComponentInChildren<TextMeshProUGUI>().text =
		_goButtonConfirm.GetComponentInChildren<TextMeshProUGUI>().text =
						UIStringTable.GetValue("ui_popup_boxreward_button_close");
		_objButtonBoost.GetComponentInChildren<TextMeshProUGUI>().text = UIStringTable.GetValue("ui_popup_boxreward_button_booster");
		_objButtonSkip.GetComponentInChildren<TextMeshProUGUI>().text = UIStringTable.GetValue("ui_popup_boxreward_button_skip");

		_goDesc.GetComponentInChildren<TextMeshProUGUI>().text = UIStringTable.GetValue("ui_hint_box_duplicate_desc");
	}

	public void InitializeInfo(ItemBox box, bool isOwn = true, bool isBattle = false, int count = 1,
							   EBoxType bType = EBoxType.normal, BoxDealTable boxDeal = null,
							   System.Action<PopupBoxReward> a_oCallback = null, bool isDesc = true)
	{
		m_oCallback = a_oCallback;

		_box = BoxTable.GetData(box.nKey);
		_boxDeal = boxDeal;

		ComUtil.DestroyChildren(_boxModelRoot.transform);

		_Box = m_ResourceMgr.CreateObject(EResourceType.Box, box.strPrefeb, _boxModelRoot.transform);

		if (_Box.GetComponent<BoxRewards>() == null)
			_Box.AddComponent<BoxRewards>();
		_BoxAnimator = _Box.GetComponent<Animator>();
		_BoxAnimator.SetTrigger("Start");

		_isComplete = false;
		_isBattle = isBattle;

		_bType = bType;

		_objButtonConfirm.SetActive(false);
		_objButtonBoost.SetActive(false);
		_objButtonSkip.SetActive(false);

		_goButtonConfirm.SetActive(false);
		_goButtonMoreCrystal.SetActive(false);
		_goButtonMoreCrystal10.SetActive(false);
		_goButtonMoreToken.SetActive(false);
		_goButtonMoreToken10.SetActive(false);

		_goDesc.SetActive(isDesc);

		_curReward = 0;

		SetRewards(box, count);
		SetMarker();
		GameManager.Singleton.StartCoroutine(AddItem( () =>
		{
            if (m_MenuMgr.CurScene == ESceneType.Lobby)
                GameObject.Find("LobbyTop").GetComponent<PageLobbyTop>().InitializeCurrency();

            if (isOwn) m_GameMgr.invenBox.DeleteBox(box.id);

			InitializeRewards();
        }));

	}

	void SetMarker()
	{
		if (_box.AbleToBoostFree == 1)
		{
			_goMarkerFree.SetActive(true);
			_goMarkerAD.SetActive(false);
			_goMarkerVIP.SetActive(false);
			_goMarkerTicket.SetActive(false);
		}
		else if (m_Account.IsVIP())
		{
			_goMarkerVIP.SetActive(true);
			_goMarkerAD.SetActive(false);
			_goMarkerFree.SetActive(false);
			_goMarkerTicket.SetActive(false);
		}
		else
		{
			_goMarkerVIP.SetActive(false);
			_goMarkerFree.SetActive(false);
			_goMarkerTicket.SetActive(int.Parse(_txtTicketCount.text) > 0);
			_goMarkerAD.SetActive(!_goMarkerTicket.activeSelf);
		}
	}

	public void OnClickTicket()
	{
		PopupMaterial pop = m_MenuMgr.OpenPopup<PopupMaterial>(EUIPopup.PopupMaterial, true);
		pop.InitializeInfo(new ItemMaterial(0, _ticket, 0));
	}

	void SetRewards(ItemBox box, int count)
	{
		Dictionary<uint, int> t = new Dictionary<uint, int>();

		for (int i = 0; i < count; i++)
		{
			t = RewardTable.RandomResultInGroup(box.nRewardGroup);
			t.ToList().ForEach(r =>
			{

				if (_rewardItem.ContainsKey(r.Key))
					_rewardItem[r.Key] += r.Value;
				else
					_rewardItem.Add(r.Key, r.Value);

			});
		}
	}

	IEnumerator AddItem(Action callback)
	{
		foreach (KeyValuePair<uint, int> item in _rewardItem)
		{
			string type = item.Key.ToString("X").Substring(0, 2);

			switch (type)
			{
				case "20":
				case "23":
				case "11":
					for (int i = 0; i < item.Value; i++)
					{
						yield return GameManager.Singleton.StartCoroutine(m_GameMgr.AddItemCS(item.Key, 1));
					}
					break;
				case "22":
					yield return GameManager.Singleton.StartCoroutine(m_GameMgr.AddItemCS(item.Key, item.Value));
					break;
			}
		}

		callback?.Invoke();
    }

	void InitializeRewards()
	{
		foreach (KeyValuePair<uint, int> reward in _rewardItem)
		{
			_key = reward.Key;
			_volume = reward.Value.ToString();

			string type = _key.ToString("X").Substring(0, 2);

			switch (type)
			{
				case "20":
					_icon = m_ResourceMgr.LoadSprite(EAtlasType.Icons, WeaponTable.GetData(_key).Icon);
					_volume = NameTable.GetValue(WeaponTable.GetData(_key).NameKey);
					_isGrade = true;
					_grade = WeaponTable.GetData(_key).Grade;

					for (int i = 0; i < reward.Value; i++)
						SetSlot();

					break;
				case "22":
					_icon = m_ResourceMgr.LoadSprite(EAtlasType.Icons, MaterialTable.GetData(_key).Icon);
					_isGrade = false;
					_grade = MaterialTable.GetData(_key).Grade;
					SetSlot();
					break;
				case "23":
				case "11":
					break;
			}
		}
	}

	void SetSlot()
	{
		SlotSimpleItemReward _slot = m_MenuMgr.LoadComponent<SlotSimpleItemReward>(_slotRoot.transform, EUIComponent.SlotSimpleItemReward);

		string name = _isGrade ? string.Empty : NameTable.GetValue(MaterialTable.GetData(_key).NameKey);

		_goReward.Add(_slot.gameObject);

		_slot.Initialize(_icon, _volume, name, _grade, _isGrade, true, _key);
		_slot.gameObject.SetActive(false);
	}

	private void OnDisable()
	{
		Destroy(_Box);

		if (!_isBattle)
		{
#if DISABLE_THIS
			GameObject.Find("PageLobby")?.gameObject.BroadcastMessage("SetupOverlayImg", SendMessageOptions.DontRequireReceiver);
			GameObject.Find("PageLobby")?.gameObject.BroadcastMessage("SetupTutorialUIs", SendMessageOptions.DontRequireReceiver);
#endif // #if DISABLE_THIS
		}
	}

	public void PopCard()
	{
		if (_isComplete) return;

		for (int i = 0; i < _goReward.Count; i++)
		{
			if (i == _curReward)
			{
				_goReward[i].SetActive(true);
				_goReward[i].GetComponent<SlotSimpleItemReward>().SetAppear();
			}
			else
				_goReward[i].SetActive(false);
		}

		_curReward++;
	}

	public void PopBox()
	{
		if (_isComplete) return;

		if (_rewardItem.Count > _curReward)
		{
			_BoxAnimator.SetTrigger("Pop");
		}
		else
		{
			Skip();
		}
	}

	public void PopReward(bool isAni = true)
	{
		if (_isComplete) return;

		if (_rewardItem.Count > _curReward)
		{
			for (int i = 0; i < _goReward.Count; i++)
			{
				if (i == _curReward)
				{
					_goReward[i].SetActive(true);
					_goReward[i].GetComponent<SlotSimpleItemReward>().SetAppear();
				}
				else
					_goReward[i].SetActive(false);
			}

			if (isAni) _BoxAnimator.SetTrigger("Pop");

			_curReward++;
		}
		else
		{
			Skip();
		}
	}

	public void Skip()
	{
		StartCoroutine(ShowAll());
	}

	IEnumerator ShowAll()
	{
		_isComplete = true;
		_objButtonSkip.SetActive(false);
		_BoxAnimator.SetTrigger("End");

		if (_goReward.Count > 1)
		{
			foreach (GameObject obj in _goReward)
			{
				ComUtil.SetParent(_rewardRoot.transform, obj.transform);
				obj.SetActive(false);
			}

			foreach (GameObject obj in _goReward)
			{
				obj.SetActive(true);
				obj.GetComponent<SlotSimpleItemReward>().SetRestart();

				yield return new WaitForSecondsRealtime(GlobalTable.GetData<float>("timeRewardShowGap") / 1000);
			}
		}

		if (_bType == EBoxType.shopbuy)
			StartCoroutine(ShowButtonShopBuy());
		else
			StartCoroutine(ShowButton());
	}

	IEnumerator ShowButtonShopBuy()
	{
		_goNormalButton.SetActive(false);
		_goShopBuyButton.SetActive(true);
		_objButtonSkip.SetActive(false);

		int t = m_InvenMaterial.GetItemCount(_boxDeal.CostTokenKey);

		if (t > _boxDeal.CostTokenCount * 10f)
		{
			_goButtonMoreToken10.SetActive(true);
			_goButtonMoreToken.SetActive(true);

			yield return new WaitForSecondsRealtime((float)(GlobalTable.GetData<int>("timeBoostToConfirm") / 1000));

			animator.SetTrigger("MoreToken10Appear");
			animator.SetTrigger("MoreTokenAppear");

			_goButtonMoreCrystal10.SetActive(false);
			_goButtonMoreCrystal.SetActive(false);
		}
		else if (t > _boxDeal.CostTokenCount)
		{
			_goButtonMoreCrystal10.SetActive(true);
			_goButtonMoreToken.SetActive(true);

			yield return new WaitForSecondsRealtime((float)(GlobalTable.GetData<int>("timeBoostToConfirm") / 1000));

			animator.SetTrigger("MoreCrystal10Appear");
			animator.SetTrigger("MoreTokenAppear");

			_goButtonMoreToken10.SetActive(false);
			_goButtonMoreCrystal.SetActive(false);
		}
		else
		{
			_goButtonMoreCrystal10.SetActive(true);
			_goButtonMoreCrystal.SetActive(true);
			yield return new WaitForSecondsRealtime((float)(GlobalTable.GetData<int>("timeBoostToConfirm") / 1000));

			animator.SetTrigger("MoreCrystal10Appear");
			animator.SetTrigger("MoreCrystalAppear");

			_goButtonMoreToken10.SetActive(false);
			_goButtonMoreToken.SetActive(false);
		}

		_txtMoreCCount.text = _boxDeal.CostItemCount.ToString();
		_txtMoreCrystal.text = $"<color=yellow>1</color> {UIStringTable.GetValue("ui_popup_box_open_button_more_desc")}";

		_txtMoreTCount.text = _boxDeal.CostTokenCount.ToString();
		_txtMoreToken.text = $"<color=yellow>1</color> {UIStringTable.GetValue("ui_popup_box_open_button_more_desc")}";

		_txtMoreCCount10.text = (_boxDeal.CostItemCount * 10 * GlobalTable.GetData<float>("ratioShopBuyTenDiscount")).ToString();
		_txtMoreCrystal10.text = $"<color=yellow>10</color> {UIStringTable.GetValue("ui_popup_box_open_button_more_desc")}";

		_txtMoreTCount10.text = (_boxDeal.CostTokenCount * 10).ToString();
		_txtMoreToken10.text = $"<color=yellow>10</color> {UIStringTable.GetValue("ui_popup_box_open_button_more_desc")}";

		yield return new WaitForSecondsRealtime((float)(GlobalTable.GetData<int>("timeBoostToConfirm") / 1000));

		_goButtonConfirm.SetActive(true);
		animator.SetTrigger("ConfirmBSAppear");
	}

	IEnumerator ShowButton()
	{
		_goNormalButton.SetActive(true);
		_goShopBuyButton.SetActive(false);
		_objButtonSkip.SetActive(false);

		if (_box.AbleToBoost == 1 || _box.AbleToBoostFree == 1)
		{
			_objButtonBoost.SetActive(true);
			animator.SetTrigger("BoostAppear");
		}

		if (_box.AbleToBoostFree == 1)
		{
			GameObject button = GameObject.Find("Boost");
			Rect rt = button.GetComponent<RectTransform>().rect;
            m_Tutorial.SetMessage("ui_tip_boost_00", -250);
			m_Tutorial.SetFinger(button, SetBoostTarget, rt.width, rt.height, 300);
        }

		yield return new WaitForSecondsRealtime((float)(GlobalTable.GetData<int>("timeBoostToConfirm") / 1000));

		_objButtonConfirm.SetActive(true);
		animator.SetTrigger("ConfirmAppear");
	}

	public void OnClickBuyCrystal()
	{
		OnEnable();

		if (!m_DataMgr.AbleToConnect()) return;

		if (m_InvenMaterial.CalcTotalCrystal() < _boxDeal.CostItemCount)
		{
			SetButtonState(true);
			PopupShopCrystal ct = MenuManager.Singleton.OpenPopup<PopupShopCrystal>(EUIPopup.PopupShopCrystal, true);
			return;
		}

		SetButtonState(false);
		StartCoroutine(BuyOne());
	}

	IEnumerator BuyOne()
	{
		yield return StartCoroutine(m_InvenMaterial.ConsumeCrystal(_boxDeal.CostItemCount));

		SetButtonState(true);

		ItemBox box = new ItemBox(0, _boxDeal.BoxKey, 0);

		ReStart(box);
	}

	public void OnClickBuyCrystal10()
	{
		OnEnable();
		if (!m_DataMgr.AbleToConnect()) return;

		if (m_InvenMaterial.CalcTotalCrystal() < (_boxDeal.CostItemCount * 10f * GlobalTable.GetData<float>("ratioShopBuyTenDiscount")))
		{
			SetButtonState(true);
			PopupShopCrystal ct = MenuManager.Singleton.OpenPopup<PopupShopCrystal>(EUIPopup.PopupShopCrystal, true);
			return;
		}

		SetButtonState(false);
		StartCoroutine(BuyTen());
	}

	IEnumerator BuyTen()
	{
		yield return StartCoroutine(m_InvenMaterial.ConsumeCrystal((int)(_boxDeal.CostItemCount * 10f * GlobalTable.GetData<float>("ratioShopBuyTenDiscount"))));

		SetButtonState(true);

		ItemBox box = new ItemBox(0, _boxDeal.BoxKey, 0);

		ReStart(box, 10);
	}

	public void OnClickBuyToken()
	{
		if (!m_DataMgr.AbleToConnect()) return;

		SetButtonState(false);
		StartCoroutine(BuyToken());
	}

	IEnumerator BuyToken()
	{
		yield return StartCoroutine(m_GameMgr.AddItemCS(_boxDeal.CostTokenKey, -_boxDeal.CostTokenCount));

		SetButtonState(true);

		ItemBox box = new ItemBox(0, _boxDeal.BoxKey, 0);

		ReStart(box);
	}

	public void OnClickBuyToken10()
	{
		if (!m_DataMgr.AbleToConnect()) return;

		SetButtonState(false);
		StartCoroutine(BuyToken10());
	}

	IEnumerator BuyToken10()
	{
		yield return StartCoroutine(m_GameMgr.AddItemCS(_boxDeal.CostTokenKey, -_boxDeal.CostTokenCount * 10));

		SetButtonState(true);

		ItemBox box = new ItemBox(0, _boxDeal.BoxKey, 0);

		ReStart(box, 10);
	}

	void SetBoostTarget(bool isTutorial)
    {
		uint tarKey = 0;

		foreach (KeyValuePair<uint, int> reward in _rewardItem)
			if (ComUtil.GetItemType(reward.Key) == EItemType.Weapon)
				tarKey = reward.Key;

		StartCoroutine(Boosting(tarKey));
		m_Tutorial.Activate(false);
	}

	public void SetBoostTarget()
	{
		List<uint> targetBoost = new List<uint>();
		uint tarKey;

		foreach (KeyValuePair<uint, int> reward in _rewardItem)
		{
			string key = reward.Key.ToString("X");

			switch (key.Substring(0, 2))
			{
				case "20":
					if (WeaponTable.IsContainsKey(reward.Key + (uint)Math.Pow(16, 5)))
						targetBoost.Add(reward.Key);
					break;
				case "23":
					break;
				case "11":
					break;
				case "22":
					targetBoost.Add(reward.Key);
					break;
			}
		}

		System.Random r = new System.Random();
		tarKey = targetBoost[r.Next(targetBoost.Count)];

		StartCoroutine(Boosting(tarKey));
	}

	public void OnClickBoost()
	{
#if UNITY_EDITOR
		SetBoostTarget();
#else
        C3rdPartySDKManager.Singleton.ShowRewardAds(ResultADS);
#endif
	}

	void ResultADS(CAdsManager cAdsManager, CAdsManager.STAdsRewardInfo sTAdsRewardInfo, bool result)
	{
		if (result)
		{
			SetBoostTarget();
			GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.ADSView(EADSViewType.Boost));
		}
		else
		{
			GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.ADSView(EADSViewType.Boost, false));
		}
	}


	IEnumerator Boosting(uint target)
	{
		_objButtonBoost.SetActive(false);
		_objButtonConfirm.SetActive(false);

		string key = target.ToString("X");

		switch (key.Substring(0, 2))
		{
			case "20":
				yield return StartCoroutine(m_GameMgr.AddItemCS(target + (uint)Math.Pow(16, 5), 1));
				break;
			case "22":
				yield return StartCoroutine(m_GameMgr.AddItemCS(target, _rewardItem[target]));
				break;
		}

		float duration = GlobalTable.GetData<int>("timeBoxBoosting") / 1000f;
		float elapsedTime = 0f;
		float interval = 0.05f;

		GameAudioManager.PlaySFX("SFX/UI/sfx_ui_machine_increasement_01", 0f, false, ComType.UI_MIX);

		while (elapsedTime < duration)
		{
			_goFast.SetActive(true);

			System.Random r = new System.Random();
			_goReward[r.Next(_goReward.Count)].GetComponent<SlotSimpleItemReward>().SetBoost();

			yield return new WaitForSeconds(interval);
			interval = interval + 0.01f;
			elapsedTime += interval;

			_goFast.SetActive(false);
		}

		OnPointerUp();
		GameAudioManager.PlaySFX("SFX/UI/sfx_ui_confirm_01", 0f, false, ComType.UI_MIX);

		_goReward.ForEach(s =>
		{
			if (s.GetComponent<SlotSimpleItemReward>()._itemKey == target)
				s.GetComponent<SlotSimpleItemReward>().Boosting();
		});

		_objButtonConfirm.SetActive(true);
	}

	void SetButtonState(bool state)
	{
		_objButtonConfirm.GetComponent<SoundButton>().interactable = state;
		_objButtonBoost.GetComponent<SoundButton>().interactable = state;
		_objButtonSkip.GetComponent<SoundButton>().interactable = state;
		_goButtonConfirm.GetComponent<SoundButton>().interactable = state;
		_goButtonMoreCrystal.GetComponent<SoundButton>().interactable = state;
		_goButtonMoreToken.GetComponent<SoundButton>().interactable = state;
		_goButtonMoreCrystal10.GetComponent<SoundButton>().interactable = state;
		_goButtonMoreToken10.GetComponent<SoundButton>().interactable = state;
	}

	void ReStart(ItemBox box, int count = 1)
	{
		animator.Play("PopupBoxRewardStart");

		_goReward.ForEach(go => Destroy(go));
		_goReward.Clear();

		_rewardItem.Clear();

		_curReward = 0;

		InitializeInfo(box, false, false, count, EBoxType.shopbuy, _boxDeal);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		Time.timeScale = GlobalTable.GetData<float>("valueFasterTimeScale");
	}

	public void OnPointerUp(PointerEventData eventData = null)
	{
		Time.timeScale = 1f;
	}

	private void OnEnable()
	{
		_rewardItem = new Dictionary<uint, int>();
		_goFast.SetActive(false);
		// _goButtonBG.GetComponent<SoundButton>().interactable = false;
	}

	public void CompleteBoxOpenAnimation()
	{
		_goButtonBG.GetComponent<SoundButton>().interactable = true;
		if (_curReward < _goReward.Count && _goReward.Count > 1) _objButtonSkip.SetActive(true);
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
		GameObject.Find("BattlePage")?.GetComponent<PageLobbyBattle>().CheckBoxState();
		GameObject.Find("LobbyTop")?.GetComponent<PageLobbyTop>().InitializeCurrency();
		GameObject.Find("PopupAgent")?.GetComponent<PopupAgent>().UpdateUIsState();
		GameObject.Find("PopupMissionAdventure")?.GetComponent<PopupMissionAdventure>().UpdateUIsState();
		GameObject.Find("PageBattle")?.GetComponent<PageBattle>().UpdateUIsState();

		// 콜백이 존재 할 경우
		if (m_oCallback != null)
		{
			m_GameMgr.RefreshInventory(GameManager.EInvenType.All);

			foreach (GameObject obj in _goReward)
				Destroy(obj);

			_goReward.Clear();

			base.Close();
			var oCallback = m_oCallback;

			m_oCallback = null;
			oCallback(this);

			if(m_MenuMgr.CurScene == ESceneType.Lobby)
                FindObjectOfType<PageLobbyInventory>()?.InitializeInventory();

            return;
		}

		if (_isBattle)
		{
			m_MenuMgr.SceneEnd();
			m_MenuMgr.SceneNext(ESceneType.Lobby);
		}
		else
		{
			m_GameMgr.RefreshInventory(GameManager.EInvenType.All);

			foreach (GameObject obj in _goReward)
				Destroy(obj);

			_goReward.Clear();
			FindObjectOfType<PageLobbyInventory>()?.InitializeInventory();
		}

		base.Close();
	}
}

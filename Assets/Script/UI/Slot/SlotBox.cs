using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotBox : MonoBehaviour
{
	[SerializeField]
	GameObject[] _ObjState;

	[SerializeField]
	Image _imgIcon, _imgOpenMaterialIcon;

	[SerializeField]
	TextMeshProUGUI _txtName, _txtRemainTime, _txtOpentime, _txtOpenMaterialCount, _txtLevel;

	[SerializeField]
	GameObject _goHeader, _goLevel, _goNewTag;

	[SerializeField]
	GameObject[] _goNametag;

	Animator _Animator;
	ItemBox _box;
	Coroutine _coTicketCounter;

	public ERewardBoxState _rewardBoxState;
	bool _isButton = true;
	public int _SlotNumber;
	int _nTicketCount = 0;
	uint _unTicketKey = 0;
	string D, h, m, s;

	public enum placeState
	{
		normal,
		exchange_main,
		exchange_target,
	}

	placeState _palcestate = placeState.normal;

    private void Awake()
    {
		_unTicketKey = GlobalTable.GetData<uint>("valueAdsRemoveTicketDecKey");
	}

    public void Initialize(ItemBox box, bool isButton = true, bool header = false, bool hideName = false, bool hideLevel = true, placeState state = placeState.normal)
	{
		_box = box;
		_isButton = isButton;
		_palcestate = state;

		_Animator = GetComponent<Animator>();
		_SlotNumber = box.nSlotNumber;
		_imgIcon.sprite = box.GetIcon();

		_coTicketCounter = StartCoroutine(CheckTicket());

		InitalizeText();

		TimeSpan tempTime = TimeSpan.FromMilliseconds(box.nOpenDatetime);

		if ( box.nOpenDatetime > 1 )
        {
			int tempTimeD = tempTime.Days;
			int tempTimeH = tempTime.Hours;
			int tempTimeM = tempTime.Minutes;
			int tempTimeS = tempTime.Seconds;

			_txtOpentime.text = (tempTimeD > 0 ? $"{tempTimeD}{D}" : "");
			_txtOpentime.text = _txtOpentime.text + (tempTimeH > 0 ? $"{tempTimeH}{h}" : "");
			_txtOpentime.text = _txtOpentime.text + (tempTimeM > 0 ? $"{tempTimeM}{m}" : "");
			_txtOpentime.text = _txtOpentime.text + (tempTimeS > 0 ? $"{tempTimeS}{s}" : "");
		}
		else
        {
			_txtOpentime.text = UIStringTable.GetValue("ui_popup_battlepass_purchase_free_caption");
		}

		_txtOpenMaterialCount.text = box.nOpenMaterialCount.ToString();

		SetOtherBoxState();
		if (_box.bNew) SetAnimation();

		_goNametag.ToList().ForEach(go => go.SetActive(!hideName));
		_goHeader.SetActive(header);
		_goLevel.SetActive(!hideLevel);
	}

	IEnumerator CheckTicket()
	{
		WaitForSecondsRealtime w = new WaitForSecondsRealtime(1f);

		while (true)
		{
			_nTicketCount = GameManager.Singleton.invenMaterial.GetItemCount(_unTicketKey);
			SetOtherBoxState();
			yield return w;
		}
	}

	void SetOtherBoxState()
	{
		if (_box._openStartDatetime > DateTime.MinValue)
		{
			if ( _box._openStartDatetime.AddMilliseconds(_box.nOpenDatetime) > DateTime.UtcNow )
            {
				_rewardBoxState = ERewardBoxState.Opening;
				StartCoroutine(SetTime());
			}
			else
            {
				_rewardBoxState = ERewardBoxState.Complete;
			}
		}
		else
		{
			if (_box.nOpenMaterialCount == 0)
			{
				_rewardBoxState = ERewardBoxState.Free;
			}
			else
			{
				bool isOpeningBox = GameManager.Singleton.IsExistOpeningBox() != -1;
				bool ableToADOpen = BoxTable.GetData(_box.nKey).AbleToADOpen == 1;
				bool isVIP = GameManager.Singleton.user.IsVIP();

				if (ableToADOpen)
				{
					if (isVIP)
					{
						_rewardBoxState = ERewardBoxState.ReadyVIP;
					}
					else if (_nTicketCount > 0)
					{
						_rewardBoxState = ERewardBoxState.ReadyTicket;
					}
					else if (isOpeningBox)
					{
						_rewardBoxState = ERewardBoxState.ReadyAd;
					}
					else
					{
						_rewardBoxState = ERewardBoxState.Ready;
					}
				}
				else
				{
					_rewardBoxState = isOpeningBox ? ERewardBoxState.ReadyCrystal : ERewardBoxState.Ready;
				}
			}
		}

		ChangeState();
	}

	public void SetNewTag(bool state)
    {
		_goNewTag.SetActive(state);
	}

	void InitalizeText()
	{
		D = $"<size=70%>{UIStringTable.GetValue("ui_day")}</size>";
		h = $"<size=70%>{UIStringTable.GetValue("ui_hour")}</size>";
		m = $"<size=70%>{UIStringTable.GetValue("ui_minute")}</size>";
		s = $"<size=70%>{UIStringTable.GetValue("ui_second")}</size>";

		_txtName.text = UIStringTable.GetValue("ui_slot_box_complete_caption");
		_txtLevel.text = $"{UIStringTable.GetValue("ui_level")} {_box.nLevel}";
	}

	IEnumerator SetTime()
	{
		TimeSpan time;
		yield return null;

		while (_box._openStartDatetime.AddMilliseconds(_box.nOpenDatetime) > DateTime.UtcNow)
		{
			_txtRemainTime.text = string.Empty;
			time = RemainTime();

			if (time.TotalDays > 1)
			{
				_txtRemainTime.text = $"{time.Days}{D}";
				_txtRemainTime.text = _txtRemainTime.text + (time.Hours > 0 ? $"{time.Hours}{h}" : "");

				yield return new WaitForSecondsRealtime(3600f);
			}
			else if (time.TotalHours > 1)
			{
				_txtRemainTime.text = $"{time.Hours}{h}";
				_txtRemainTime.text = _txtRemainTime.text + (time.Minutes > 0 ? $"{time.Minutes}{m}" : "");

				yield return new WaitForSecondsRealtime(60f);
			}
			else if (time.TotalSeconds > 1)
			{
				_txtRemainTime.text = _txtRemainTime.text + (time.Minutes > 0 ? $"{time.Minutes}{m}" : "");
				_txtRemainTime.text = _txtRemainTime.text + (time.Seconds > 0 ? $"{time.Seconds}{s}" : "");

				yield return new WaitForSecondsRealtime(1f);
			}
		}

		if (MenuManager.Singleton.CurScene == ESceneType.Lobby)
			if (GameManager.Singleton.invenBox.GetItemCount() > 0)
				GameObject.Find("BattlePage").GetComponent<PageLobbyBattle>().CheckBoxState();

		GameManager.Singleton.RefreshBox();
	}

	TimeSpan RemainTime()
	{
		return _box._openStartDatetime.AddMilliseconds(_box.nOpenDatetime) - DateTime.UtcNow;
	}

	int OpenMaterialCount()
	{
		if (_box._openStartDatetime == DateTime.MinValue)
		{
			return _box.nOpenMaterialCount;
		}
		else
		{
			return (int)(_box.nOpenMaterialCount * (RemainTime().TotalMilliseconds / _box.nOpenDatetime));
		}
	}

	public void SetAnimation()
	{
		if (_box.bNew)
		{
			_Animator.SetInteger("slotNumber", _SlotNumber);
			_Animator.SetTrigger("entrance");

			GameObject lup = FindObjectOfType<PopupLevelUp>()?.gameObject;
            GameObject eproad = FindObjectOfType<PopupEPRoadMap>()?.gameObject;
            GameObject wrewards = FindObjectOfType<PopupWeaponReward>()?.gameObject;

            if ( GameManager.Singleton.user.m_nEpisode == 0 && GameManager.Singleton.user.m_nChapter == 0 )
			{
				PopupDefault pop = MenuManager.Singleton.OpenPopup<PopupDefault>(EUIPopup.PopupDefault);
                pop.SetTitle("ui_error_title");
                pop.SetMessage("ui_hint_box_desc");
                pop.SetButtonText("ui_slot_box_complete_caption", "ui_popup_button_later", OpenBox1st, null, "TutorialBox");
            }
			else if ( GameManager.Singleton.user.m_nLevel < 4 &&
				      GameManager.Singleton.invenBox.GetItemCount() == 4 )
			{
				if ( ( null == lup || false == lup.activeSelf ) && 
					 ( null == eproad || false == eproad.activeSelf ) && 
					 ( null == wrewards || false == wrewards.activeSelf ) )
                    OpenBoxFull();
			}

			GameManager.Singleton._liPreBoxID.Add(_box.id);
		}

		_box.bNew = false;
	}

	public void SetHeader(bool state)
	{
		_goHeader.SetActive(state);
	}

	public void ChangeState()
	{
		for (int i = 0; i < (int)ERewardBoxState.END; i++)
		{
			if (i == (int)_rewardBoxState)
				_ObjState[i].SetActive(true);
			else
				_ObjState[i].SetActive(false);
		}
	}

	public long GetID()
	{
		return _box.id;
	}

	public void OpenPopup()
	{
		if (!_isButton) return;

		if (_palcestate == placeState.normal)
		{
			if (_rewardBoxState != ERewardBoxState.Complete)
			{
				switch (_box.nGrade)
				{
					case 1:
					case 2:
					case 3:
					case 4:
						PopupBoxNormal popupNormal = MenuManager.Singleton.OpenPopup<PopupBoxNormal>(EUIPopup.PopupBoxNormal);
						popupNormal.InitializeInfo(_box, GameManager.Singleton.IsExistOpeningBox() > -1);
						break;
				}
			}
			else
			{
				PopupBoxReward popupReward = MenuManager.Singleton.OpenPopup<PopupBoxReward>(EUIPopup.PopupBoxReward);
				popupReward.InitializeInfo(_box);
			}
		}
		else if (_palcestate == placeState.exchange_target)
		{
			GameObject.Find("PopupBoxExchange").GetComponent<PopupBoxExchange>().ChangeSelectSlot(_box.id, _SlotNumber);
		}
		else if (_palcestate == placeState.exchange_main)
		{
			PopupBoxNormal popupNormal = MenuManager.Singleton.OpenPopup<PopupBoxNormal>(EUIPopup.PopupBoxNormal, true);
			popupNormal.InitializeInfo(_box, false, false);
		}
	}

	void OpenBox1st()
	{
		Rect rt = transform.parent.GetComponent<RectTransform>().rect;
		GameManager.Singleton.tutorial.SetFinger(gameObject, OpenTutorial, rt.width, rt.height, 750);
    }

    void OpenBoxFull()
    {
        Rect rt = transform.parent.GetComponent<RectTransform>().rect;
        GameManager.Singleton.tutorial.SetMessage("ui_tip_box_00", 200);
        GameManager.Singleton.tutorial.SetFinger(gameObject, OpenTutorial, rt.width, rt.height, 750);
    }

    public void OpenTutorial(bool isTutorial = false)
    {
		PopupBoxNormal popupNormal = MenuManager.Singleton.OpenPopup<PopupBoxNormal>(EUIPopup.PopupBoxNormal);
		popupNormal.InitializeInfo(_box, GameManager.Singleton.IsExistOpeningBox() > -1, true, isTutorial);		
	}

    private void OnDisable()
    {
		StopAllCoroutines();
    }
}

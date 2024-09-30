using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupInputMail : UIDialog
{
	[SerializeField]
	TextMeshProUGUI _txtTitle, _txtMessage,
					_txtAccountTitle, _txtPasswordTitle, _txtNewPasswordTitle, _txtConfirmTitle,
					_txtButtonLinkCaption, _txtButtonUnlinkCaption, _txtButtonChangePWCaption, _txtButtonConfirmCaption;

	[SerializeField]
	GameObject _goMailInput, _goPasswordInput, _goNewPasswordInput, _goConfirmInput,
			   _goButtonLink, _goButtonUnlink, _goButtonChangePW, _goButtonConfirm;

	[SerializeField]
	TMP_InputField _ifAccount, _ifPassword, _ifNewPassword, _ifConfirm;

	PopupOption _option;

	public void InitializeInfo()
	{
		this.InitializeInfo(null);
	}

	public void InitializeInfo(PopupOption option)
	{
		_option = option;
		Resize();
	}

	void InitializeText()
	{
		_txtTitle.text = UIStringTable.GetValue("ui_popup_email_title");

	}

	void Resize()
	{
		// LayoutRebuilder.ForceRebuildLayoutImmediate(_rtNPCInfo);
	}

	private void OnEnable()
	{
		_txtAccountTitle.text = _txtPasswordTitle.text = _txtNewPasswordTitle.text = _txtConfirmTitle.text = string.Empty;
		_ifAccount.text = _ifPassword.text = _ifNewPassword.text = _ifConfirm.text = string.Empty;

		if (m_Account.m_strNickname == ComType.DEFAULT_NICKNAME)
			InitilizeDefault();
		else
			InitializeLinked();
	}

	void InitilizeDefault()
	{
		_txtMessage.text = $"<color=red>{UIStringTable.GetValue("ui_popup_email_message_default")}</color>";

		_goMailInput.SetActive(true);
		_goPasswordInput.SetActive(true);
		_goNewPasswordInput.SetActive(false);
		_goConfirmInput.SetActive(true);

		_goButtonLink.SetActive(true);
		_goButtonUnlink.SetActive(false);
		_goButtonChangePW.SetActive(false);
		_goButtonConfirm.SetActive(false);

		_txtAccountTitle.text = UIStringTable.GetValue("ui_popup_email_account_title");
		_txtPasswordTitle.text = UIStringTable.GetValue("ui_popup_email_password_title");
		_txtConfirmTitle.text = UIStringTable.GetValue("ui_popup_email_confirm_title");

		_txtButtonLinkCaption.text = UIStringTable.GetValue("ui_popup_email_button_link_caption");
	}

	void InitializeLinked()
	{
		_txtMessage.text = m_Account.m_strNickname;

		_goMailInput.SetActive(false);
		_goPasswordInput.SetActive(true);
		_goNewPasswordInput.SetActive(true);
		_goConfirmInput.SetActive(true);

		_goButtonLink.SetActive(false);
		_goButtonUnlink.SetActive(true);
		_goButtonChangePW.SetActive(true);
		_goButtonConfirm.SetActive(false);

		_txtPasswordTitle.text = UIStringTable.GetValue("ui_popup_email_password_title_old");
		_txtNewPasswordTitle.text = UIStringTable.GetValue("ui_popup_email_newpassword_title");
		_txtConfirmTitle.text = UIStringTable.GetValue("ui_popup_email_confirm_title");

		_txtButtonUnlinkCaption.text = UIStringTable.GetValue("ui_popup_email_button_unlink_caption");
		_txtButtonChangePWCaption.text = UIStringTable.GetValue("ui_popup_email_button_changepw_caption");
	}

	public void OnClickLink()
	{
		if (string.IsNullOrEmpty(_ifAccount.text)) return;

		if (_ifPassword.text != _ifConfirm.text)
		{
			Exception("ui_error_passworddoesnotmatch_new");
		}
		else
		{
			StartCoroutine(Link());
		}
	}

	IEnumerator Link()
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "nickname", _ifAccount.text },
			{ "password", _ifPassword.text },
		};

		yield return StartCoroutine(m_DataMgr.SendRequestByPost(EAccountPostType.link_account, fields, (result) =>
		{
			if (result.ToLower() == "ok")
			{
				m_Account.m_strNickname = _ifAccount.text;
				_option?.InitializeAccount();
				GameObject.Find("LobbyTop")?.GetComponent<PageLobbyTop>().InitializeAccountInfo();
				Close();
			}
			else if (result.ToLower() == "nop")
			{
				Exception("ui_error_passworddoesnotmatch");
			}
			else
			{
				m_Account.m_nUID = int.Parse(result);
				PlayerPrefs.SetInt(ComType.STORAGE_UID, (int)m_Account.m_nUID);

				// 옵션 팝업이 존재 할 경우
				if (_option != null)
				{
					_option.ReStart();
				}
				else
				{
					GameObject.Find("LobbyTop")?.GetComponent<PageLobbyTop>().ReStart();
				}
			}
		}));
	}

	public void OnClickUnlink()
	{
		if (_ifPassword.text == string.Empty)
		{
			Exception("ui_error_password_null");
			return;
		}
		else
		{
			StartCoroutine(Unlink());
		}
	}

	IEnumerator Unlink()
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "nickname", ComType.DEFAULT_NICKNAME },
			{ "password", _ifPassword.text },
		};

		yield return StartCoroutine(m_DataMgr.SendRequestByPost(EAccountPostType.unlink_account, fields, (result) =>
		{
			if (result.ToLower() == "ok")
			{
				m_Account.m_strNickname = ComType.DEFAULT_NICKNAME;
				_option?.InitializeAccount();
				GameObject.Find("LobbyTop")?.GetComponent<PageLobbyTop>().InitializeAccountInfo();
				Close();
			}
			else if (result.ToLower() == "nop")
			{
				Exception("ui_error_passworddoesnotmatch");
			}
		}));
	}

	void Exception(string message)
	{
		PopupSysMessage m = m_MenuMgr.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
		m.InitializeInfo("ui_error_title", message, "ui_popup_button_confirm");
	}

	public void OnClickChangePW()
	{
		StartCoroutine(CheckPW());

		if (_ifPassword.text != _ifConfirm.text)
		{
			Exception("ui_error_passworddoesnotmatch_new");
		}
		else
		{
			StartCoroutine(Unlink());
		}
	}

	IEnumerator CheckPW()
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "password", _ifPassword.text },
		};

		yield return StartCoroutine(m_DataMgr.SendRequestByPost(EAccountPostType.check_password, fields, (result) =>
		{
			if (result.ToLower() == "ok")
				StartCoroutine(ChangePW());
			else
			{
				Exception("ui_error_passworddoesnotmatch");
			}
		}));
	}

	IEnumerator ChangePW()
	{
		if (_ifNewPassword.text != _ifConfirm.text)
		{
			Exception("ui_error_passworddoesnotmatch_new");
			yield break;
		}

		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "password", _ifNewPassword.text },
		};

		yield return StartCoroutine(m_DataMgr.SendRequestByPost(EAccountPostType.change_password, fields, (result) =>
		{
			if (result.ToLower() == "ok")
			{
				Close();
				Exception("ui_error_changepassword_ok");
			}
		}));
	}

	private void Awake()
	{
		Initialize();
	}

	public override void Initialize()
	{
		base.Initialize();
		InitializeText();
	}

	public override void Open()
	{
		base.Open();

		m_MenuMgr.ShowPopupDimmed(true);
	}

	public override void Close()
	{
		base.Close();

		m_MenuMgr.ShowPopupDimmed(false);
	}

	#region 추가
	/** 시작 버튼을 눌렀을 경우 */
	public void OnClickStartBtn()
	{
		// GameDataManager.Singleton.SetPlayHuntLV(_curLevel);
		// GameDataManager.Singleton.SetupPlayMapInfo(
	}
	#endregion // 추가
}
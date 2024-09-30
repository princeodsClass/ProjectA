using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class GameDataManager : SingletonMono<GameDataManager>
{
	bool _isSignin = false;

	public bool IsSignin()
	{
		return _isSignin;
	}

	public bool AbleToConnect()
	{
		string title = UIStringTable.IsContainsKey("ui_error_title") ?
												   "ui_error_title" :
												   "Infomation";

		string message = UIStringTable.IsContainsKey("ui_error_networkisnotavailable") ?
													 "ui_error_networkisnotavailable" :
													 "Cannot communicate with the server";

		string exit = UIStringTable.IsContainsKey("ui_popup_button_confirm") ?
												  "ui_popup_button_confirm" :
												  "Quit";

		if (Application.internetReachability == NetworkReachability.NotReachable)
		{
			PopupSysMessage sys = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
			sys.InitializeInfo(title, message, exit, GameDataTableManager.Singleton.ExitApp);

			return false;
		}
		else
		{
			return true;
		}
	}

	public IEnumerator SendRequestByPost<T>(T control, Dictionary<string, string> param, Action<string> result = null, Action a_oFailCallback = null) where T : Enum
	{
		if (!AbleToConnect()) yield break;

		int maxRetryCount = GlobalTable.GetData<int>("countMaxRetryPost");
		int retryCount = 0;

		WWWForm form = new WWWForm();

		form.AddField("control", control.ToString());

		foreach (KeyValuePair<string, string> field in param)
			form.AddField(field.Key, field.Value);

		do
		{
			using (UnityWebRequest www = UnityWebRequest.Post(GetURL(control), form))
			{
				www.timeout = GlobalTable.GetData<int>("timeRequest");
				yield return www.SendWebRequest();

				if (www.result != UnityWebRequest.Result.ConnectionError)
				{
					if (www.downloadHandler.text == "under_maintenance")
					{
						UnderMaintenance();
						yield break;
					}

					if (www.downloadHandler.text == ComType.EXCEPTION_ACCESSTOKEN)
					{
						PopupSysMessage pop = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
						pop.InitializeInfo("ui_error_title", "ui_error_connect_accesstoken", "ui_reconnect", GameManager.Singleton.Restart);
					}
					else
					{
						result?.Invoke(www.downloadHandler.text);
						break;
					}
				}
				else
				{
					GameManager.Log($"HTTP Error: {(int)www.responseCode}, {www.error}", "red");
					GameManager.Log($"Server Response: {www.downloadHandler.text}", "red");
				}
			}

			retryCount++;
			yield return new WaitForSeconds(2.0f);

		} while (retryCount < maxRetryCount);

		if (retryCount >= maxRetryCount)
		{
			PopupSysMessage fail = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
			fail.InitializeInfo("ui_error_title",
								"ui_error_networkisnotavailable",
								"ui_reconnect",
								GameManager.Singleton.Restart);

			a_oFailCallback?.Invoke();
		}
	}

	void UnderMaintenance()
	{
		string title = UIStringTable.IsContainsKey("ui_error_title") ?
												   "ui_error_title" :
												   "Infomation";

		string message = UIStringTable.IsContainsKey("ui_error_undermaintenance") ?
													 "ui_error_undermaintenance" :
													 "ui_error_undermaintenance";

		string exit = UIStringTable.IsContainsKey("ui_popup_button_confirm") ?
												  "ui_popup_button_confirm" :
												  "Quit";

		PopupSysMessage sys = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
		sys.InitializeInfo(title, message, exit, GameDataTableManager.Singleton.ExitApp);
	}

	string GetURL<T>(T control)
	{
		string path = null;

		switch (control)
		{
			case EAccountPostType:
				path = ComType.SERVER_ACCOUNT_POST_PATH;
				break;
			case EInventoryPostType:
				path = ComType.SERVER_INVENTORY_POST_PATH;
				break;
			case EShopPostType:
				path = ComType.SERVER_SHOP_POST_PATH;
				break;
			case EMapPostType:
				path = ComType.SERVER_MAP_POST_PATH;
				break;
			case EETCPostType:
				path = ComType.SERVER_ETC_POST_PATH;
				break;
			case EAbyssPostType:
				path = ComType.SERVER_ABYSS_POST_PATH;
				break;
		}

		return $"{GameDataTableManager.m_URL}{path}";
	}

	/// <summary>
	/// auid 로 계정 정보 받아오기.
	/// </summary>
	/// <param name="auid"></param>
	/// <returns></returns>
	public IEnumerator GetAccountInfoByAuid(int auid)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", auid.ToString() }
		};

		yield return StartCoroutine(SendRequestByPost(EAccountPostType.get_info_auid, fields));
	}

	/// <summary>
	/// 닉네임으로 계정 정보 받아오기.
	/// </summary>
	/// <param name="nickname"></param>
	/// <returns></returns>
	public IEnumerator GetAccountInfoByNickname(string nickname)
	{
		// 공용 모듈화를 위해 필요한 파라미터는 딕셔너리로 정의해서 넘기자.
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "nickname", nickname }
		};

		yield return StartCoroutine(SendRequestByPost(EAccountPostType.get_info_nickname, fields, (result) =>
		{
			GameManager.Log(result);
		}));
	}

	public IEnumerator GetOtherUserInfo(long auid)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", auid.ToString() }
		};

		PopupWait4Response wait = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

		yield return StartCoroutine(SendRequestByPost(EAccountPostType.get_info_otheruser, fields, (result) =>
		{
			wait.Close();

			if (result == "no exist auid")
			{
				PopupSysMessage message = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
				message.InitializeInfo("ui_error_title", "ui_error_noexistuser", "ui_common_close");
			}
			else
			{
				PopupOtherUser other = MenuManager.Singleton.OpenPopup<PopupOtherUser>(EUIPopup.PopupOtherUser, true);
				other.InitializeInfo(result);
			}
		}));
	}

	/// <summary>
	/// 가입하기. 계정 정보가 없을 때에만 사용하자.
	/// 필수 요소 : nickname / osType / deviceInfo
	/// </summary>
	/// <returns></returns>
	public IEnumerator Signup(PageTitle title)
	{
		PlayerPrefs.DeleteAll();

		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "nickname", ComType.DEFAULT_NICKNAME },
			{ "osType", ((int)ComUtil.CheckOS()).ToString() },
			{ "deviceInfo", ComUtil.GetUniqueID() },
			{ "country", CultureInfo.CurrentCulture.Name },
			{ "deviceModel", SystemInfo.deviceModel }
		};

		yield return StartCoroutine(SendRequestByPost(EAccountPostType.signup, fields, (result) =>
		{
			PlayerPrefs.SetInt(ComType.STORAGE_UID, int.Parse(result));
			PlayerPrefs.SetString("isNew", "true");
		}));

		yield return StartCoroutine(Signin(title));
	}

	/// <summary>
	/// 로그인. PlayerPrefs 에 저장되어 있는 accessToken 과 서버의 것 비교해서 다르면 갱신.
	/// 필수 요소 : auid / accessToken / osType / deviceInfo
	/// </summary>
	/// <returns></returns>
	public IEnumerator Signin(PageTitle title)
	{
		string accessToken = PlayerPrefs.GetString(ComType.STORAGE_TOKEN);

		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "accessToken", accessToken },
			{ "osType", ((int)ComUtil.CheckOS()).ToString() },
			{ "deviceInfo", ComUtil.GetUniqueID() },
			{ "deviceModel", SystemInfo.deviceModel },
			{ "country", CultureInfo.CurrentCulture.Name},
			{ "clientVersion", Application.version},
			{ "genuine", Application.genuine ? "1" : "0"},
			{ "genuineCheckAvailable", Application.genuineCheckAvailable ? "1" : "0"},
		};

		yield return StartCoroutine(SendRequestByPost(EAccountPostType.signin, fields, (result) =>
		{
			if (result.ToLower() == "blacklisted")
			{
				PopupSysMessage sys = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
				sys.InitializeInfo("ui_error_title", "ui_error_blacklised", "ui_popup_button_confirm", GameDataTableManager.Singleton.ExitApp);
			}
			else if (result.ToLower() == "tampered" ||
				(Application.genuineCheckAvailable &&
				  !Application.genuine))
			{
				string title = UIStringTable.GetValue("ui_error_title");
				string message = UIStringTable.GetValue("ui_error_networkisnotavailable");
				string exit = UIStringTable.GetValue("ui_popup_button_confirm");

				PopupSysMessage sys = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
				sys.InitializeInfo(title, message, exit, GameDataTableManager.Singleton.ExitApp);
			}
			else
			{
				try
				{
					JObject jResult = JObject.Parse(result);

					GameManager.Singleton._nPreLevel = (int)jResult.GetValue("currentLevel");
					GameManager.Singleton._nPreEpisode = (int)jResult.GetValue("currentEpisode");
					GameManager.Singleton._nPreChapter = (int)jResult.GetValue("currentChapter");

					PlayerPrefs.SetInt(ComType.STORAGE_UID, (int)jResult.GetValue(ComType.STORAGE_UID));
					PlayerPrefs.SetString(ComType.STORAGE_TOKEN, jResult.GetValue("accessToken").ToString());

					PlayerPrefs.Save();

					GameManager.Singleton.user.Initialize(jResult);

					StartCoroutine(title.LateSign());
				}
				catch (Exception ex)
				{
					string msg = $"{ex.Message}\n\n{ex.StackTrace}";

					PopupSysMessage exPop = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
					exPop.InitializeInfo("Error", msg, "Exit", GameDataTableManager.Singleton.ExitApp);
				}
			}
		}));
	}

	public IEnumerator SetBattlePassDatetime(DateTime dt)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "passStartDatetime", dt.ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EAccountPostType.modify_account, fields));
	}


	public IEnumerator GetBattlePassInfo(PopupBattlePass popup = null)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EETCPostType.get_battlepassinfo, fields, (result) =>
		{
			GameManager.Singleton.user.InitializeBattlePass(result);
			if (null != popup) popup.InitializeInfo();
		}));
	}

	public IEnumerator SetBattlePassInfo(int level, int type)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "level", level.ToString() },
			{ "type", type.ToString() }
		};

		yield return StartCoroutine(SendRequestByPost(EETCPostType.set_battlepassinfo, fields));
	}

	public IEnumerator SkipBattlePass(PopupWait4Response wait, PopupBattlePass pass)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "passLevel", ( GameManager.Singleton.user.m_nPassLevel + 1 ).ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EAccountPostType.modify_account, fields, (result) =>
		{
			GameManager.Singleton.user.m_nPassLevel++;
			GameObject.Find("LobbyTop")?.GetComponent<PageLobbyTop>()?.InitializeCurrency();
			wait.Close();
			pass.InitializePassInfo();
			pass.SetFocus();
        }));
	}

	public IEnumerator GetAttendanceRewards(AttendanceTable attendance)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "attendanceContinuouslyCount", (GameManager.Singleton.user.m_nAttendanceContinuouslyCount + 1).ToString() }
		};

		yield return StartCoroutine(SendRequestByPost(EAccountPostType.get_attendance_rewards, fields, (result) =>
		{
			if (result == "ok")
			{
				GameManager.Singleton.user.m_nAttendanceContinuouslyCount++;
				GameManager.Singleton.user.m_bIsAbleToAttendanceReward = false;

				if (ComUtil.GetItemType(attendance.RewardKey) != EItemType.Box)
					StartCoroutine(GameManager.Singleton.AddItemCS(attendance.RewardKey, attendance.RewardCount));
			}
		}));
	}

	public IEnumerator GetQuestInfo()
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EETCPostType.get_questinfo, fields, (result) =>
		{
			GameManager.Singleton.user.InitializeQuestInfo(result);
		}));
	}

	public IEnumerator CreateQuest(int index, uint key)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "order", index.ToString() },
			{ "type", ( index == 4 || index == 5 ) ? "1" : "0" },
			{ "questindex", key.ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EETCPostType.create_questinfo, fields, (result) =>
		{
			if (result == "ok")
			{
				GameManager.Singleton.user.m_nQuestID[index] = 0;
				GameManager.Singleton.user.m_nQuestKey[index] = key;
				GameManager.Singleton.user.m_nQuestCount[index] = 0;
				GameManager.Singleton.user.m_bQuestIsComplete[index] = false;
			}
			else
			{
				PopupSysMessage pop = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
				pop.InitializeInfo("ui_error_title", "ui_error_quest_notexist", "ui_common_close");
			}
		}));
	}

	public IEnumerator CompleteQuest(long id, int index, Action callback = null)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "id", id.ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EETCPostType.complete_quest, fields, (result) =>
		{
			GetRewards(index, callback);
		}));
	}

	void GetRewards(int index, Action callback)
	{
		QuestTable quest = QuestTable.GetData(GameManager.Singleton.user.m_nQuestKey[index]);
		StartCoroutine(AddQuestRewards(quest, callback));
	}

	IEnumerator AddQuestRewards(QuestTable quest, Action callback)
	{
		yield return StartCoroutine(GameManager.Singleton.AddItemCS(quest.RewardsKey00, quest.RewardsCount00));
		yield return StartCoroutine(GameManager.Singleton.AddItemCS(quest.RewardsKey01, quest.RewardsCount01));

        GameObject.Find("LobbyTop")?.GetComponent<PageLobbyTop>()?.InitializeCurrency();
        if (null != callback) callback.Invoke();
	}

	public IEnumerator GetQuestAccountRewardsInfo()
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EETCPostType.get_questrewardsinfo, fields, (result) =>
		{
			GameManager.Singleton.user.InitializeQuestRewardsInfo(result);
		}));
	}

	public IEnumerator SetQuestAccountRewardsInfo(SlotQuestReward.ERewardType type, Action callback = null)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "type", ((int)type).ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EETCPostType.set_questrewardsinfo, fields, (result) =>
		{
			if (result == "ok")
			{
				if (type == SlotQuestReward.ERewardType.Half)
					GameManager.Singleton.user.m_bIsRecieveQuestAccouuntRewardsMiddle = true;
				else if (type == SlotQuestReward.ERewardType.Full)
					GameManager.Singleton.user.m_bIsRecieveQuestAccouuntRewardsFull = true;
			}

			if (null != callback) callback.Invoke();
		}));
	}

	public IEnumerator GetAbyssInfo()
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EAbyssPostType.get_abyss_info, fields, (result) =>
		{
			GameManager.Singleton.user.InitializeAbyssInfo(result);
			StartCoroutine(GetAbyssRecordInfo());
		}));
	}

	public IEnumerator GetAbyssRecordInfo()
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
		};

		yield return StartCoroutine(SendRequestByPost(EAbyssPostType.get_abyss_recordinfo, fields, (result) =>
		{
			GameManager.Singleton.user.InitializeAbyssRecordInfo(result);
		}));
	}

	public IEnumerator SetAbyssSeasonRewardsGet(ComCommunityAbyss comp)
	{
		PopupWait4Response wait = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response);

		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "season", GameManager.Singleton.user.m_nAbyssSeason.ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EAbyssPostType.set_abyss_season_rewards_get, fields, (result) =>
		{
			GameManager.Singleton.user.m_bIsAbyssRewardsReceived = true;

			comp.SetAdjustCover(true);
			wait.Close();

			if ("already get" == result)
			{
				PopupSysMessage sys = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
				sys.InitializeInfo("ui_error_title", "ui_error_abyss_rewards_already_get", "ui_common_close");
			}
			else
			{
				StartCoroutine(ClaimAbyssSeasonRewards());
			}
		}));
	}

	IEnumerator ClaimAbyssSeasonRewards()
	{
		LeagueTable league = LeagueTable.GetDataWithRank(GameManager.Singleton.user.m_nAbyssCurRank);
		Dictionary<uint, int> rewards = new Dictionary<uint, int>();

		rewards = RewardTable.RandomResultInGroup(league.RewardsGroup);

		foreach (KeyValuePair<uint, int> temp in rewards)
		{
			switch (ComUtil.GetItemType(temp.Key))
			{
				case EItemType.Weapon:
				case EItemType.Gear:
					yield return StartCoroutine(GameManager.Singleton.AddItemCS(temp.Key, 1));
					break;
				case EItemType.Material:
				case EItemType.MaterialG:
					yield return StartCoroutine(GameManager.Singleton.AddItemCS(temp.Key, temp.Value));
                    GameObject.Find("LobbyTop")?.GetComponent<PageLobbyTop>()?.InitializeCurrency();
                    break;
				case EItemType.Box:
					PopupBoxReward re = MenuManager.Singleton.OpenPopup<PopupBoxReward>(EUIPopup.PopupBoxReward, true);
					re.InitializeInfo(new ItemBox(0, temp.Key, 0), false);
					break;
			}
		}
	}

	public IEnumerator GetAbyssRankInfo(PopupAbyss popup = null)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
		};

		yield return StartCoroutine(SendRequestByPost(EAbyssPostType.get_abyss_rankinfo, fields, (result) =>
		{
			popup?.SetRankList(result);
		}));
	}

	/// <summary>
	/// auid 기준으로 최근 광고 구매 정보 받아오기.
	/// 필수 요소 : auid
	/// </summary>
	/// <returns></returns>
	public IEnumerator GetADSDealInfo()
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EShopPostType.get_adsdeal_list, fields, (result) =>
		{
			GameManager.Singleton.user.InitializeADSDeal(result);
		}));
	}

	public IEnumerator SetADSDealInfo(int slotNumber, uint dealKey)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "slotNumber", slotNumber.ToString() },
			{ "dealKey", dealKey.ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EShopPostType.buy_adsdeal, fields, (result) =>
		{
			JObject jResult = JObject.Parse(result);
			int slotNumber = (int)jResult.GetValue("slotNumber");
		}));
	}

	/// <summary>
	/// auid 기준으로 최근 데일리 구매 정보 받아오기.
	/// 필수 요소 : auid
	/// </summary>
	/// <returns></returns>
	public IEnumerator GetDailyDealInfo()
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EShopPostType.get_dailydeal_list, fields, (result) =>
		{
			GameManager.Singleton.user.InitializeDailyDeal(result);
		}));
	}

	public IEnumerator SetDailyDealInfo(int slotNumber, uint dealKey)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "slotNumber", slotNumber.ToString() },
			{ "dealKey", dealKey.ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EShopPostType.buy_dailydeal, fields, (result) =>
		{
			JObject jResult = JObject.Parse(result);
			int slotNumber = (int)jResult.GetValue("slotNumber");

			GameManager.Singleton.user.SetDailyDealCover(slotNumber, true);
		}));
	}

	/// <summary>
	/// auid 기준으로 최근 게임머니 구매 정보 받아오기.
	/// 필수 요소 : auid
	/// </summary>
	/// <returns></returns>
	public IEnumerator GetGameMoneyDealInfo()
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EShopPostType.get_gamemoneydeal_list, fields, (result) =>
		{
			GameManager.Singleton.user.InitializeGameMoneyDeal(result);
		}));
	}

	public IEnumerator SetGameMoneyDealInfo(uint dealKey)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "dealKey", dealKey.ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EShopPostType.buy_gamemoneydeal, fields, (result) =>
		{
			GameManager.Singleton.user.InitializeGameMoneyDeal(result);
		}));
	}

	/// <summary>
	/// auid 기준으로 최근 게임머니 구매 정보 받아오기.
	/// 필수 요소 : auid
	/// </summary>
	/// <returns></returns>
	public IEnumerator GetCrystalDealInfo()
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EShopPostType.get_crystaldeal_list, fields, (result) =>
		{
			RecieveCrystal(result);
		}));
	}

	public IEnumerator SetCrystalDealInfo(uint dealKey)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "dealKey", dealKey.ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EShopPostType.buy_crystaldeal, fields, (result) =>
		{
			RecieveCrystal(result);
		}));
	}

	void RecieveCrystal(string result)
	{
		JArray jResult = JArray.Parse(result);

		List<uint> deallist = new List<uint>();

		for (int i = 0; i < jResult.Count; i++)
		{
			JObject item = (JObject)jResult[i];
			deallist.Add((uint)item.GetValue("dealKey"));
		}

		ComShopCrystal popup = GameObject.Find("PopupShopCrystal")?.GetComponentInChildren<ComShopCrystal>();
		ComShopCrystal compo = GameObject.Find("ShopPage")?.GetComponentInChildren<ComShopCrystal>();

		if (null != popup) popup.SetGoods(deallist);
		if (null != compo) compo.SetGoods(deallist);

        GameObject.Find("LobbyTop")?.GetComponent<PageLobbyTop>()?.InitializeCurrency();
	}

	public IEnumerator SetViplDealInfo(VIPDealTable deal, PopupWait4Response wait)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "dealKey", deal.PrimaryKey.ToString() },
			{ "vipEndDatetime", ComUtil.EnUTC((double)deal.Period * 1000) },
		};

		yield return StartCoroutine(SendRequestByPost(EShopPostType.buy_vipdeal, fields, (result) =>
		{
			if (result == "ok")
				StartCoroutine(LateSetVip(deal, wait));
		}));
	}

	IEnumerator LateSetVip(VIPDealTable deal, PopupWait4Response wait)
	{
		// to do : 이건 나중에 접속 보상으로 돌리자.
		yield return GameManager.Singleton.StartCoroutine(GameManager.Singleton.AddItemCS(deal.RewardItemKey0, deal.RewardItemCount0));
		yield return GameManager.Singleton.StartCoroutine(GameManager.Singleton.AddItemCS(deal.RewardItemKey1, deal.RewardItemCount1));
		yield return GameManager.Singleton.StartCoroutine(GameManager.Singleton.AddItemCS(deal.RewardItemKey2, deal.RewardItemCount2));

		GameManager.Singleton.user.m_dtEndVip = DateTime.UtcNow.AddSeconds((double)deal.Period);

		FindObjectOfType<ButtonVIP>()?.gameObject.SetActive(false);

		FindObjectOfType<PageLobbyInventory>()?.InitializeInventory();
		FindObjectOfType<PageLobbyBattle>()?.CheckQuestState();
		FindObjectOfType<PageLobbyShop>()?.SetComShopADS();
		FindObjectOfType<ComShopVIP>()?.Initialize();

		wait.Close();
	}

	public IEnumerator AddBuyCount(EDealType type, int count = 1)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
		};

		switch (type)
		{
			case EDealType.Weapon:
				count += GameManager.Singleton.user.m_nCountBuyWeaponBox;
				fields.Add("countBuyWeaponBox", count.ToString());
				break;
			case EDealType.PremiumWeapon:
			case EDealType.Material:
				count += GameManager.Singleton.user.m_nCountBuyMateerialBox;
				fields.Add("countBuyMaterialBox", count.ToString());
				break;
			case EDealType.PremiumMaterial:
				count += GameManager.Singleton.user.m_nCountBuyPremiumMaterialBox;
				fields.Add("countBuyPremiumMaterialBox", count.ToString());
				break;
		}

		yield return StartCoroutine(SendRequestByPost(EAccountPostType.modify_account, fields, (result) =>
		{
			JObject jResult = JObject.Parse(result);

			GameManager.Singleton.user.Initialize(jResult);
		}));
	}

	/// <summary>
	/// 계정 정보 수정. 모듈화를 위해 필요한 파라미터는 딕셔너리로 정의해서 넘기자.
	/// 필수 요소 : accessToken
	/// </summary>
	/// <returns></returns>
	public IEnumerator ModifyAccount(Dictionary<string, string> fields)
	{
		yield return StartCoroutine(SendRequestByPost(EAccountPostType.modify_account, fields, (result) =>
		{
			GameManager.Log(result);

			JObject jResult = JObject.Parse(result);

			GameManager.Singleton.user.Initialize(jResult);
		}));
	}

	/// <summary>
	/// 인벤 아이템 추가.
	/// 필수 요소 : accessToken, auid, primarykey, table(EInventoryType) , count ( item 일 경우에만 )
	/// </summary>
	/// <returns></returns>
	public IEnumerator InsertItem(Dictionary<string, string> fields)
	{
		yield return StartCoroutine(SendRequestByPost(EInventoryPostType.insert_item, fields, (result) =>
		{
			GameManager.Log(result);

			JObject jResult = JObject.Parse(result);

			CallbackAddItem(result, true);
		}));
	}

	/// <summary>
	/// 인벤 아이템 삭제.
	/// 아이템 항목 자체를 삭제시키는 것. 수량 조절은 modify 로 처리.
	/// 필수 요소 : accessToken, auid, id, table ( inventory / box )
	/// </summary>
	/// <returns></returns>
	public IEnumerator DeleteItem(Dictionary<string, string> fields)
	{
		yield return StartCoroutine(SendRequestByPost(EInventoryPostType.delete_item, fields, (result) =>
		{
			GameManager.Log(result);
			CallbackDelItem(result);
		}));
	}

	/// <summary>
	/// 캐릭터 생성은 별도로 하자 ( 서버에서 캐릭터 스킬 정보 생성을 위해 )
	/// </summary>
	/// <param name="fields"></param>
	/// <returns></returns>
	public IEnumerator InsertCharacter(Dictionary<string, string> fields)
	{
		yield return StartCoroutine(SendRequestByPost(EInventoryPostType.insert_character, fields, (result) =>
		{
			GameManager.Log(result);

			JObject jResult = JObject.Parse(result);

			CallbackAddItem(result, true);
		}));
	}

	/// <summary>
	/// 캐릭터 업그레이드 정보 받아오기
	/// </summary>
	/// <param name="id"></param>
	/// <param name="callback"></param>
	/// <returns></returns>
	public IEnumerator GetCharacterSkillUpgradeInfo(long id, Action<string> callback = null)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "id", id.ToString() },
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EInventoryPostType.get_characterskill, fields, (result) =>
		{
			callback?.Invoke(result);
		}));
	}

	public IEnumerator CharacterSkillUpgrade(ItemCharacter character,
											 ItemCharacter.stSkillUpgrade upInfo,
											 Dictionary<long, int> material)
	{
		yield return StartCoroutine(ConsumnMaterial(material));

		Type type = typeof(ItemCharacter.stSkillUpgrade);
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "id", character.id.ToString() },
		};

		foreach (FieldInfo field in type.GetFields())
			fields.Add(field.Name.ToLower(), field.GetValue(upInfo).ToString());

		yield return StartCoroutine(SendRequestByPost(EInventoryPostType.upgrade_characterskill, fields, (result) =>
		{
			if (result == "ok")
				character._stSkillUpgrade = upInfo;
		}));
	}

	/// <summary>
	/// 인벤토리에서 아이템 리스트 전체 받아오기.
	/// 필수 요소 : accessToken, auid, table(EInventoryType)
	/// </summary>
	/// <returns></returns>
	public IEnumerator GetInventoryList(EDatabaseType table)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "table", table.ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EInventoryPostType.get_item_list, fields, (result) =>
		{
			if (fields["table"] == EDatabaseType.box.ToString())
			{
				JArray jResult = JArray.Parse(result);

				foreach (JObject item in jResult)
				{
					GameManager.Singleton._liPreBoxID.Add((long)item.GetValue($"id"));
				}
			}

			GameManager.Singleton.InitializeInventory(result);
		}));
	}

	/// <summary>
	/// 인벤토리 db 아이템 수정.
	/// 필수 요소 : accessToken, auid, id ( 수정하려는 것 ), 수정 내용
	/// </summary>
	/// <returns></returns>
	public IEnumerator ModifyItem(Dictionary<string, string> fields)
	{
		yield return StartCoroutine(SendRequestByPost(EInventoryPostType.modify_item, fields, (result) =>
		{
			GameManager.Log(result);

			CallbackAddItem(result);
		}));
	}

	void CallbackAddItem(string result, bool isNew = false)
	{
		JObject jResult = JObject.Parse(result);

		GameManager.Singleton.AddItemSC(jResult, isNew);
	}

	void CallbackDelItem(string result)
	{
		JObject jResult = JObject.Parse(result);
		GameManager.Singleton.DeleteItem(jResult);
	}

	public void DeleteItem(long id, EDatabaseType type)
	{
		if (type == EDatabaseType.box && id > 0)
		{
			GameManager.Singleton._liPreBoxID.Remove(id);
		}

		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "table", type.ToString() },
			{ "id", id.ToString() }
		};

		StartCoroutine(DeleteItem(fields));
	}

	public IEnumerator StartBattlePass(DateTime dt = default)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "passStartDatetime", dt == default ? ComUtil.EnUTC() : ComUtil.EnDatetime(dt) },
			{ "isElite", "0" },
			{ "isPlus", "0" },
			{ "passLevel", "1" },
			{ "passExp", "0" },
		};

		yield return StartCoroutine(SendRequestByPost(EAccountPostType.modify_account, fields, (result) =>
		{
			JObject joResult = JObject.Parse(result);
            dt = ComUtil.String2Datetime((string)joResult.GetValue("passStartDatetime"));

            GameManager.Singleton.user.m_dtPass = dt;
			GameManager.Singleton.user.m_bIsPlus = false;
			GameManager.Singleton.user.m_bIsElite = false;
			GameManager.Singleton.user.m_nPassLevel = 1;
			GameManager.Singleton.user.m_nPassExp = 0;
		}));
	}

	public IEnumerator ActiveBattlePass(string type)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ type, 1.ToString() }
		};

		yield return StartCoroutine(SendRequestByPost(EShopPostType.set_battlepassactive, fields, (result) =>
		{
			if (result == "ok")
			{
				if (type == "isElite")
					GameManager.Singleton.user.m_bIsElite = true;
				else if (type == "isPlus")
					GameManager.Singleton.user.m_bIsPlus = true;

				GameObject.Find("PopupBattlePass").GetComponent<PopupBattlePass>().InitializeInfo();
			}
			else
			{
				GameManager.Log(result, "red");
			}
		}));
	}

	public IEnumerator SetBattlePass(PopupBattlePass pop, BattlePassTable pass, int type)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "level", pass.Lv.ToString() },
			{ "type", type.ToString() }
		};

		yield return StartCoroutine(SendRequestByPost(EETCPostType.set_battlepassinfo, fields, (result) =>
		{
			if (result == "ok")
			{
				StartCoroutine(CallbackSetBattlePass(pop, pass, type));
			}
			else
				GameManager.Log(result, "red");
		}));
	}

	IEnumerator CallbackSetBattlePass(PopupBattlePass pop, BattlePassTable pass, int type)
	{
		uint key = (type == 0) ? pass.PlusRewardItemKey : (type == 1) ? pass.EliteRewardItemKey : pass.NormalRewardItemKey;
		int count = (type == 0) ? pass.PlusRewardItemCount : (type == 1) ? pass.EliteRewardItemCount : pass.NormalRewardItemCount;

		GameManager.Singleton.user.m_bpPass[pass.Lv - 1][type] = true;

		switch (key.ToString("X").Substring(0, 2))
		{
			case "20":  // weapon
			case "23":  // gear
				yield return StartCoroutine(GameManager.Singleton.AddItemCS(key, 1));
				break;
			case "22":  // material
				yield return StartCoroutine(GameManager.Singleton.AddItemCS(key, count));
                GameObject.Find("LobbyTop")?.GetComponent<PageLobbyTop>()?.InitializeCurrency();
                break;
			case "24":  // box
				PopupBoxReward re = MenuManager.Singleton.OpenPopup<PopupBoxReward>(EUIPopup.PopupBoxReward, true);
				re.InitializeInfo(new ItemBox(0, key, 0), false);
				break;
		}
	}

	public IEnumerator ItemUpgrade(ItemBase item, int upgrade, Dictionary<long, int> material)
	{
		yield return StartCoroutine(ConsumnMaterial(material));

		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "table", EDatabaseType.inventory.ToString() },
			{ "id", item.id.ToString() },
			{ "upgrade", upgrade.ToString() }
		};

		yield return StartCoroutine(SendRequestByPost(EInventoryPostType.modify_item, fields, (a_oResult) =>
		{
			GameManager.Log(a_oResult);
		}));
	}

	public IEnumerator ItemLimitbreak(ItemBase item, int limitbreak, Dictionary<long, int> material)
	{
		yield return StartCoroutine(ConsumnMaterial(material));

		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "table", EDatabaseType.inventory.ToString() },
			{ "id", item.id.ToString() },
			{ "upgrade", "0" },
			{ "reinforce", "0" },
			{ "limitbreak", limitbreak.ToString() }
		};

		yield return StartCoroutine(SendRequestByPost(EInventoryPostType.modify_item, fields));
	}

	public IEnumerator ItemReinforce(ItemBase item, int reinforce, Dictionary<long, int> material)
	{
		yield return StartCoroutine(ConsumnMaterial(material));

		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "table", EDatabaseType.inventory.ToString() },
			{ "id", item.id.ToString() },
			{ "reinforce", reinforce.ToString() }
		};

		yield return StartCoroutine(SendRequestByPost(EInventoryPostType.modify_item, fields));
	}

	public IEnumerator ItemReset(ItemBase item)
	{
		Dictionary<string, string> weaponfields = new Dictionary<string, string>
		{
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "table", EDatabaseType.inventory.ToString() },
			{ "id", item.id.ToString() },
			{ "upgrade", "0" },
			{ "reinforce", "0" },
			{ "limitbreak", "0" }
		};

		yield return StartCoroutine(SendRequestByPost(EInventoryPostType.modify_item, weaponfields));
	}

	public IEnumerator ItemMaxUpgrade(ItemBase item, int upgrade, int reinforce, int limitbreak, Dictionary<long, int> material)
	{
		yield return StartCoroutine(ConsumnMaterial(material));

		Dictionary<string, string> weaponfields = new Dictionary<string, string>
		{
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "table", EDatabaseType.inventory.ToString() },
			{ "id", item.id.ToString() },
			{ "upgrade", upgrade.ToString() },
			{ "reinforce", reinforce.ToString() },
			{ "limitbreak", limitbreak.ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EInventoryPostType.modify_item, weaponfields));
	}

	public IEnumerator ConsumnMaterial(Dictionary<long, int> material)
	{
		foreach (KeyValuePair<long, int> m in material)
		{
			Dictionary<string, string> materialFields = new Dictionary<string, string>
			{
				{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
				{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
				{ "table", EDatabaseType.inventory.ToString() },
				{ "id", m.Key.ToString() },
				{ "count", m.Value.ToString() }
			};

			if (m.Value == 0)
				yield return StartCoroutine(SendRequestByPost(EInventoryPostType.delete_item, materialFields, (result) =>
				{
					JObject jResult = JObject.Parse(result);
					GameManager.Singleton.DeleteItem(jResult);
				}));
			else
				yield return StartCoroutine(SendRequestByPost(EInventoryPostType.modify_item, materialFields, (result) =>
				{
					GameManager.Log(result);
				}));
		}
	}

	public IEnumerator ItemLock(ItemBase item)
	{
		Dictionary<string, string> weaponfields = new Dictionary<string, string>
		{
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "table", EDatabaseType.inventory.ToString() },
			{ "id", item.id.ToString() },
			{ "isLock", ( item.bIsLock == true ? 1 : 0 ).ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EInventoryPostType.modify_item, weaponfields));
	}

	public IEnumerator AddBox(int slotNumber, uint key)
	{
		foreach (ItemBox box in GameManager.Singleton.invenBox)
		{
			if (box.nSlotNumber == slotNumber)
			{
				GameManager.Log("There's already a box in slot.", "red");
				yield break;
			}
		}

		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "table", EDatabaseType.box.ToString() },
			{ "slotNumber", slotNumber.ToString() },
			{ "primaryKey", key.ToString() },
			{ "new", "true" },
		};

		yield return StartCoroutine(SendRequestByPost(EInventoryPostType.insert_item, fields, (result) =>
		{
			GameManager.Log(result);

			JObject jResult = JObject.Parse(result); //.ToString());

			CallbackAddItem(result);
		}));
	}

	public IEnumerator ExchangeBox(long id, uint key)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "table", EDatabaseType.box.ToString() },
			{ "id", id.ToString() },
			{ "primaryKey", key.ToString() },
			{ "openDatetime", "" },
			{ "new", "true" },
		};

		yield return StartCoroutine(SendRequestByPost(EInventoryPostType.modify_item, fields, (result) =>
		{
			GameManager.Log(result);

			JObject jResult = JObject.Parse(result); //.ToString());

			CallbackAddItem(result);

			MenuManager.Singleton.SceneEnd();
			MenuManager.Singleton.SceneNext(ESceneType.Lobby);
		}));
	}

	public IEnumerator IncreaseRepository(int index, int count)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
		};

		if (index == 0)
			fields.Add("maxWeapon", count.ToString());
		else if (index == 2)
			fields.Add("maxGear", count.ToString());

		yield return StartCoroutine(SendRequestByPost(EAccountPostType.modify_account, fields, (result) =>
		{
			JObject jResult = JObject.Parse(result);

			GameManager.Singleton.user.Initialize(jResult);
		}));
	}

	public IEnumerator SetMapVersion(EMapInfoType type, int episode, int chapter, int version, System.Action<JObject, bool> a_oCallback)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "mapType", ((int)type).ToString() },
			{ "episode", episode.ToString() },
			{ "chapter", chapter.ToString() },
			{ "version", version.ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EMapPostType.set_map_version, fields, (a_oResult) =>
		{
			a_oCallback?.Invoke(JObject.Parse(a_oResult), true);
		}));
	}

	public IEnumerator GetMapVersion(EMapInfoType type, int episode, int chapter, System.Action<JObject, bool> a_oCallback)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "episode", episode.ToString() },
			{ "chapter", chapter.ToString() },
			{ "mapType", ((int)type).ToString() }
		};

		yield return StartCoroutine(SendRequestByPost(EMapPostType.get_map_version, fields, (a_oResult) =>
		{
			a_oCallback?.Invoke(JObject.Parse(a_oResult), true);
		}, () =>
		{
			a_oCallback?.Invoke(null, false);
		}));
	}

	public IEnumerator GetNotice(PopupNotice no)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EETCPostType.get_notice, fields, (result) =>
		{
			JArray jResult = JArray.Parse(result);
			no.SetMessage(jResult);
		}));
	}

	public IEnumerator ADSView(EADSViewType type, bool isSuccess = true)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "viewType", ( (int)type ).ToString() },
			{ "logType", ( isSuccess? 1:2 ).ToString() },
			{ "vipType", ( GameManager.Singleton.user.IsVIP() ? 1 : 0 ).ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EETCPostType.ads_view, fields));
	}

	public IEnumerator StartAdventure(int adventureGroup, DateTime time)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "adventureGroup", adventureGroup.ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EAccountPostType.start_adventure, fields, (result) =>
		{
			if (result == "ok")
			{
				GameManager.Singleton.user.m_nCurrentAdventureKeyCount = GlobalTable.GetData<int>("countMaxAdventureKey");
				GameManager.Singleton.user.m_dtStartAdventure = GameManager.Singleton.user.m_dtAdventureKeyStart = time;
				GameManager.Singleton.user.m_nAdventureLevel = 0;
				GameManager.Singleton.user.m_nAdventureGroup = adventureGroup;
			}
			else
			{
				GameManager.Log(result, "red");
			}
		}));
	}

    public IEnumerator StartAdventureChapter(long id, int count, Action callback)
    {
        callback?.Invoke();
		yield break;

        var stKeyStartTime = GameManager.Singleton.IsEnableChargeAdventureKey() ?
			GameManager.Singleton.user.m_dtAdventureKeyStart : System.DateTime.UtcNow;

		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "osType", ((int)ComUtil.CheckOS()).ToString() },
            { "id", id.ToString() },
            { "count", count.ToString() },
        };

		yield return StartCoroutine(SendRequestByPost(EMapPostType.start_adventureChapter, fields, (result) =>
		{
			if (result == "ok")
			{
				callback?.Invoke();

				// GameManager.Singleton.IncrAdventureKeyCount(-1);
			}
			else if (result == "lack of key")
			{
				PopupSysMessage lok = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
				lok.InitializeInfo("ui_error_title", "ui_error_lackofkey", "ui_common_close");
			}
			else
			{
				GameManager.Log(result, "red");
			}
		}));
	}

	public IEnumerator EndAdventureChapter(ECompleteMapType type)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "winState", ((int)type).ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EMapPostType.end_adventureChapter, fields, (result) =>
		{
			if (result == "ok")
			{
				GameManager.Singleton.user.RefreshQuestCount(EQuestActionType.ClearAdventure, 1);
			}
		}));
	}

	public IEnumerator SkipAdventureChapter(System.Action<bool> a_oCallback)
	{
		DateTime stKeyStartTime = GameManager.Singleton.IsEnableChargeAdventureKey() ?
					 			  GameManager.Singleton.user.m_dtAdventureKeyStart : DateTime.UtcNow;

		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "adventureKeyStart", ComUtil.EnDatetime(stKeyStartTime) }
		};

		yield return StartCoroutine(SendRequestByPost(EMapPostType.skip_adventureChapter, fields, (result) =>
		{
			if (result == "ok")
			{
				// GameManager.Singleton.IncrAdventureKeyCount(-1);
				GameManager.Singleton.user.m_nAdventureLevel += 1;
				GameManager.Singleton.user.RefreshQuestCount(EQuestActionType.ClearAdventure, 1);

				a_oCallback?.Invoke(true);
			}
			else if (result == "lack of ticket")
			{
				PopupSysMessage lok = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
				lok.InitializeInfo("ui_error_title", "ui_error_lackofticket", "ui_common_close");

				a_oCallback?.Invoke(false);
			}
			else if (result == "lack of key")
			{
				PopupSysMessage lok = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
				lok.InitializeInfo("ui_error_title", "ui_error_lackofticket_defence", "ui_common_close");

				a_oCallback?.Invoke(false);
			}
			else
			{
				GameManager.Log(result, "red");
				a_oCallback?.Invoke(false);
			}
		}));
	}

	public IEnumerator VerifyReceipt(string receipt, string productID, Action callback)
	{
		if (receipt == null)
		{
			PopupSysMessage err = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
			err.InitializeInfo("ui_error_title", "ui_error_receipt_info", "ui_popup_button_confirm");

			yield break;
		}

		if (string.Empty != receipt)
			receipt = JObject.Parse(receipt)["TransactionID"].ToString();

		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "osType", ((int)ComUtil.CheckOS()).ToString() },
			{ "productID", productID },
			{ "receipt", receipt },
		};

		yield return StartCoroutine(SendRequestByPost(EShopPostType.verify_receipt, fields, (result) =>
		{
			if (result == "0")
			{
				C3rdPartySDKManager.Singleton.ConfirmPurchase(productID);
				callback?.Invoke();
			}
			else if (result == "255")
			{
				GameManager.Log("duplicate receipt", "red");
				C3rdPartySDKManager.Singleton.RejectPurchase(productID);

			}
			else
			{
				GameManager.Log("fail", "red");
				C3rdPartySDKManager.Singleton.RejectPurchase(productID);
			}
		},
		() =>
		{
			C3rdPartySDKManager.Singleton.RejectPurchase(productID);
		}));
	}

	public IEnumerator StartDefence(int difficulty, long id, int count, Action callback)
	{
		PopupWait4Response wait = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "osType", ((int)ComUtil.CheckOS()).ToString() },
			{ "id",  id.ToString() },
			{ "count",  count.ToString() },
			{ "difficulty", difficulty.ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EMapPostType.start_defence, fields, (result) =>
		{
			wait.Close();

			if (result == "ok")
			{
				callback?.Invoke();
			}
			else if (result == "lack of key")
			{
				PopupSysMessage lok = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
				lok.InitializeInfo("ui_error_title", "ui_error_lackofkey", "ui_common_close");
			}
			else
			{
				GameManager.Log(result, "red");
			}
		}));
	}

	public IEnumerator StartZombie(long id, int count, Action callback)
	{
		PopupWait4Response wait = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "osType", ((int)ComUtil.CheckOS()).ToString() },
			{ "id",  id.ToString() },
			{ "count",  count.ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EMapPostType.start_zombie, fields, (result) =>
		{
			wait.Close();

			if (result == "ok")
			{
				callback?.Invoke();
			}
			else if (result == "lack of key")
			{
				PopupSysMessage lok = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
				lok.InitializeInfo("ui_error_title", "ui_error_lackofkey", "ui_common_close");
			}
			else
			{
				GameManager.Log(result, "red");
			}
		}));
	}

	public IEnumerator SetBonusPoint(float point)
	{
		AccountLevelTable tar = AccountLevelTable.GetData((uint)(0x01000000 + GameManager.Singleton.user.m_nLevel));
		float tempPoint = GameManager.Singleton.user.m_nCurrrentBonusPoint + point;

		PopupWait4Response wait = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
		};

		if (tempPoint >= tar.BonusTargetPoint)
		{
			fields.Add("currentBonusPoint", "0");
			fields.Add("endBonusDatetime", ComUtil.EnUTC((double)tar.BonusMaintain));
			fields.Add("type", "start");
		}
		else
		{
			fields.Add("currentBonusPoint", tempPoint.ToString());
			fields.Add("type", "increase");
		}

		yield return StartCoroutine(SendRequestByPost(EAccountPostType.set_bonuspoint, fields, (result) =>
		{
			if (result == "ok")
			{
				GameManager.Singleton.user.m_nCurrrentBonusPoint = tempPoint;
				GameManager.Singleton.user.m_dtBonusEnd = default;
				GameManager.Singleton.user.m_bIsBonusTime = false;
			}
			else if (result == "start")
			{
				GameManager.Singleton.user.m_nCurrrentBonusPoint = 0;
				GameManager.Singleton.user.m_dtBonusEnd = DateTime.UtcNow.AddMilliseconds(tar.BonusMaintain);
				GameManager.Singleton.user.m_bIsBonusTime = true;
			}
		}));
	}

	public IEnumerator ObtainChapterWeapon(Action callback = null)
	{
		yield break;

		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "episode", GameManager.Singleton.user.m_nEpisode.ToString() },
			{ "chapter", GameManager.Singleton.user.m_nChapter.ToString() },
		};

		yield return StartCoroutine(SendRequestByPost(EAccountPostType.set_obtain_chapter_weapon, fields, (result) =>
		{
			if (result == "ok")
				// GameManager.Singleton.user.m_bIsObtainChapterWeapon = true;

				if (null != callback) callback.Invoke();
		}));
	}

	#region 프로퍼티
	public bool IsHuntBoost { get; private set; } = false;
	public bool IsContinuePlay { get; private set; } = false;

	public int ExtraInstHP { get; private set; } = 0;
	public int ExtraInstDEF { get; private set; } = 0;

	public int PlayHuntLV { get; private set; } = 0;
	public int StageInstLV { get; private set; } = 1;
	public int PlayStageID { get; private set; } = 0;
	public int BonusNPCAppearStageID { get; private set; } = -1;

	public int PlayerHP { get; private set; } = 0;
	public int ContinueTimes { get; private set; } = 0;
	public int EquipWeaponIdx { get; private set; } = 0;

	public int AcquireStageEXP { get; private set; } = 0;
	public int AcquireStagePassEXP { get; private set; } = 0;

	public float GoldenPoint { get; private set; } = 0.0f;
	public float AccumulateStageInstEXP { get; private set; } = 0;
	public float PlayerActiveSkillPoint { get; private set; } = 0.0f;

	public EPlayMode PlayMode { get; private set; } = EPlayMode.NONE;
	public EBattleType BattleType { get; private set; } = EBattleType.NONE;
	public EMapInfoType PlayMapInfoType { get; private set; } = EMapInfoType.NONE;

	public STReloadInfo[] ReloadInfos { get; } = new STReloadInfo[ComType.G_MAX_NUM_EQUIP_WEAPONS];
	public STMagazineInfo[] MagazineInfos { get; }= new STMagazineInfo[ComType.G_MAX_NUM_EQUIP_WEAPONS];

	public List<uint> HintGroupKeyList { get; } = new List<uint>();
	public List<(EffectTable, float)> BuffEffectTableInfoList { get; } = new List<(EffectTable, float)>();
	public List<STEffectStackInfo> PassiveEffectStackInfoList { get; } = new List<STEffectStackInfo>();
	public List<STEffectStackInfo> NonPlayerPassiveEffectStackInfoList { get; } = new List<STEffectStackInfo>();

	public List<uint> AcquireWeaponList = new List<uint>();
	public List<uint> OriginAcquireWeaponList = new List<uint>();

	public Dictionary<uint, int> AcquireItemInfoDict { get; } = new Dictionary<uint, int>();
	public Dictionary<uint, int> MissingItemInfoDict { get; } = new Dictionary<uint, int>();
	public Dictionary<uint, int> OriginAcquireItemInfoDict { get; } = new Dictionary<uint, int>();

	public Dictionary<int, CMapInfo> PlayMapInfoDict { get; } = new Dictionary<int, CMapInfo>();
	public CMapInfo PlayMapInfo => PlayMapInfoDict.GetValueOrDefault(this.PlayStageID);
	#endregion // 프로퍼티

	#region 함수
	/** 플레이어 정보를 리셋한다 */
	public void ResetPlayerInfos()
	{
		this.IsContinuePlay = false;
		this.EquipWeaponIdx = 0;

		this.HintGroupKeyList.Clear();
		this.BuffEffectTableInfoList.Clear();
		this.PassiveEffectStackInfoList.Clear();
		this.NonPlayerPassiveEffectStackInfoList.Clear();
	}

	/** 플레이 맵 정보를 설정한다 */
	public void SetupPlayMapInfo(EMapInfoType a_eMapInfoType,
		EPlayMode a_ePlayMode, Dictionary<int, CMapInfo> a_oMapInfoDict, bool a_bIsInfiniteMode = false)
	{
		this.PlayMode = a_ePlayMode;
		this.BattleType = a_bIsInfiniteMode ? EBattleType.INFINITE : EBattleType.NORM;

		this.PlayMapInfoType = a_eMapInfoType;
		this.BonusNPCAppearStageID = a_oMapInfoDict.Count - 1;

		this.StageInstLV = 1;
		this.ExtraInstHP = 0;
		this.ExtraInstDEF = 0;
		this.AcquireStageEXP = 0;
		this.AcquireStagePassEXP = 0;
		this.AccumulateStageInstEXP = 0;

		this.ResetPlayerInfos();

		this.AcquireWeaponList.Clear();
		this.AcquireItemInfoDict.Clear();

		this.MissingItemInfoDict.Clear();
		this.OriginAcquireWeaponList.Clear();
		this.OriginAcquireItemInfoDict.Clear();

		a_oMapInfoDict.ExCopyTo(this.PlayMapInfoDict, (_, a_oMapInfo) => a_oMapInfo);
	}

	/** 웨이브 모드 여부를 검사한다 */
	public bool IsWaveMode()
	{
		return this.IsInfiniteWaveMode() || this.PlayMapInfoType == EMapInfoType.DEFENCE;
	}

	/** 무한 웨이브 모드 여부를 검사한다 */
	public bool IsInfiniteWaveMode()
	{
		bool bIsWaveMode = this.BattleType == EBattleType.INFINITE;
		bIsWaveMode = bIsWaveMode || this.PlayMapInfoType == EMapInfoType.INFINITE;

		return bIsWaveMode;
	}

	/** 캠페인 무한 웨이브 모드 여부를 검사한다 */
	public bool IsCampaignInfiniteWaveMode()
	{
		bool bIsWaveMode = this.BattleType == EBattleType.INFINITE;
		bIsWaveMode = bIsWaveMode && this.PlayMapInfoType == EMapInfoType.CAMPAIGN;

		return bIsWaveMode;
	}
	#endregion // 함수

	#region 접근 함수
	/** 사냥 부스트 여부를 변경한다 */
	public void SetIsHuntBoost(bool a_bIsBoost)
	{
		this.IsHuntBoost = a_bIsBoost;
	}

	/** 지속 플레이 여부를 변경한다 */
	public void SetIsContinuePlay(bool a_bIsContinuePlay)
	{
		this.IsContinuePlay = a_bIsContinuePlay;
	}

	/** 추가 인스턴스 체력을 변경한다 */
	public void SetExtraInstHP(int a_nHP)
	{
		this.ExtraInstHP = a_nHP;
	}

	/** 추가 인스턴스 방어력을 변경한다 */
	public void SetExtraInstDEF(int a_nDEF)
	{
		this.ExtraInstDEF = a_nDEF;
	}

	/** 플레이 사냥 레벨을 변경한다 */
	public void SetPlayHuntLV(int a_nLV)
	{
		this.PlayHuntLV = a_nLV;
	}

	/** 플레이 스테이지 식별자를 변경한다 */
	public void SetPlayStageID(int a_nStageID)
	{
		this.PlayStageID = Mathf.Max(0, a_nStageID);
	}

	/** 스테이지 인스턴스 레벨을 변경한다 */
	public void SetStageInstLV(int a_nLV)
	{
		this.StageInstLV = a_nLV;
	}

	/** 누적 스테이지 EXP 를 변경한다 */
	public void SetAccumulateStageInstEXP(float a_fEXP)
	{
		this.AccumulateStageInstEXP = a_fEXP;
	}

	/** 보너스 NPC 등장 스테이지 식별자를 변경한다 */
	public void SetBonusNPCAppearStageID(int a_nStageID)
	{
		this.BonusNPCAppearStageID = a_nStageID;
	}

	/** 플레이어 체력을 변경한다 */
	public void SetPlayerHP(int a_nHP)
	{
		this.PlayerHP = a_nHP;
	}

	/** 이어하기 횟수를 변경한다 */
	public void SetContinueTimes(int a_nTimes)
	{
		this.ContinueTimes = Mathf.Max(0, a_nTimes);
	}

	/** 장착 무기 인덱스를 변경한다 */
	public void SetEquipWeaponIdx(int a_nIdx)
	{
		this.EquipWeaponIdx = Mathf.Clamp(a_nIdx, 0, 3);
	}

	/** 스테이지 획득 경험치를 변경한다 */
	public void SetAcquireStageEXP(int a_nEXP)
	{
		this.AcquireStageEXP = a_nEXP;
	}

	/** 스테이지 획득 패스 경험치를 변경한다 */
	public void SetAcquireStagePassEXP(int a_nEXP)
	{
		this.AcquireStagePassEXP = a_nEXP;
	}

	/** 보너스 포인트를 변경한다 */
	public void SetGoldenPoint(float a_fPoint)
	{
		this.GoldenPoint = Mathf.Max(0, a_fPoint);
	}

	/** 플레이어 액티브 스킬 포인트를 변경한다 */
	public void SetPlayerActiveSkillPoint(float a_fPoint)
	{
		this.PlayerActiveSkillPoint = a_fPoint;
	}
	#endregion // 접근 함수

#if DEBUG || DEVELOPMENT_BUILD
	public float MovingShootSpeedRatio { get; set; } = 1.0f;
	public float MovingShootJoystickRatio { get; set; } = 1.0f;
#endif // #if DEBUG || DEVELOPMENT_BUILD
}

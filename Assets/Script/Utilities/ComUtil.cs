using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Text;
using System.Linq;
using DG.Tweening;
using System.Security.Cryptography;


#if UNITY_IOS
using UnityEngine.iOS;
#endif

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public static class ComUtil
{
	static System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

	public static void SetParent(Transform tParent, Transform tChild)
	{
		if (null == tParent) return;

		RectTransform tRect = tChild.GetComponent<RectTransform>();

		if (null == tRect)
		{
			tChild.SetParent(tParent);
			tChild.Reset();
		}
		else
		{
			SetParent(tParent, tRect);
		}

		tChild.SetAsLastSibling();
	}

	public static void SetParent(Transform tParent, RectTransform tChild)
	{
		//if (null == tParent) return;

		tChild.SetParent(tParent);
		tChild.Reset();
	}

	public static Sprite GetWeaponSubtypeIcon(EWeaponType type)
	{
		string iconname = $"icon_type_weapon_{type.ToString()}";
		return GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons, iconname);
	}

	public static Sprite GetGearTypeIcon(EGearType type)
	{
		string iconname = $"icon_type_gear_{type.ToString()}";
		return GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons, iconname);
	}

	public static string GetGradeIconName(int nGrade, int nSub)
	{
		string strName = string.Empty;
		switch (nGrade)
		{
			case 0: strName = "Icon_Class_D"; break;
			case 3: strName = "Icon_Class_C"; break;
			case 6: strName = "Icon_Class_B"; break;
			case 9: strName = "Icon_Class_A"; break;
			case 12: strName = "Icon_Class_S"; break;
			case 13: strName = "Icon_Class_SS"; break;
		}

		switch (nSub)
		{
			case 1: strName = $"{strName}L"; break;
			case 2: strName = $"{strName}R"; break;
			case 3: strName = $"{strName}U"; break;
		}

		return strName;
	}

	public static void DetectLanguage()
	{
		ELanguage languageCode = ELanguage.English;

		switch (Application.systemLanguage)
		{
			case SystemLanguage.Korean: languageCode = ELanguage.Korean; break;
			case SystemLanguage.English: languageCode = ELanguage.English; break;
			case SystemLanguage.Japanese: languageCode = ELanguage.Japanese; break;
			case SystemLanguage.ChineseSimplified:
			case SystemLanguage.ChineseTraditional:
			case SystemLanguage.Chinese: languageCode = ELanguage.Chinese; break;
			case SystemLanguage.German: languageCode = ELanguage.German; break;
			case SystemLanguage.Spanish: languageCode = ELanguage.Spanish; break;
			case SystemLanguage.Portuguese: languageCode = ELanguage.Portuguese; break;
			case SystemLanguage.French: languageCode = ELanguage.French; break;
			case SystemLanguage.Italian: languageCode = ELanguage.Italian; break;
			case SystemLanguage.Dutch: languageCode = ELanguage.Dutch; break;
			case SystemLanguage.Russian: languageCode = ELanguage.Russian; break;
			case SystemLanguage.Hungarian: languageCode = ELanguage.Hungarian; break;
			case SystemLanguage.Greek: languageCode = ELanguage.Greek; break;
			case SystemLanguage.Turkish: languageCode = ELanguage.Turkish; break;
			case SystemLanguage.Vietnamese: languageCode = ELanguage.Vietnamese; break;
			//case SystemLanguage.Arabic: languageCode = ELanguage.Arabic; break;
			case SystemLanguage.Indonesian: languageCode = ELanguage.Indonesian; break;
			//case SystemLanguage.Hebrew: languageCode = ELanguage.Hebrew; break;
			default: languageCode = ELanguage.English; break;
		}

		SetLanguage((int)languageCode);
	}

	public static ItemBase GetItemBase(uint nItemKey, int nCount = 1)
	{
		ItemBase cItem = null;

		switch (GetItemType(nItemKey))
		{
			case EItemType.Weapon: cItem = new ItemWeapon(0, nItemKey, nCount, 0, 0, 0, false); break;
			case EItemType.Gear: cItem = new ItemGear(0, nItemKey, nCount, 0, 0, 0, false); break;
			case EItemType.MaterialG: cItem = new ItemMaterial(0, nItemKey, nCount); break;
			case EItemType.Box: cItem = new ItemBox(0, nItemKey, nCount); break;
			case EItemType.Character: cItem = new ItemCharacter(0, nItemKey, 0); break;
		}

		return cItem;
	}

	public static void SavePreference(string strKey, object obj)
	{
		string strJson = JsonConvert.SerializeObject(obj);
		PlayerPrefs.SetString(strKey, strJson);
		PlayerPrefs.Save();
	}

	public static T LoadPreference<T>(string strKey)
	{
		if (!PlayerPrefs.HasKey(strKey)) return default;

		string strJson = PlayerPrefs.GetString(strKey);
		return JsonConvert.DeserializeObject<T>(strJson);
	}

	public static void SaveData<T>(T data, string strFileName)
	{
		try
		{
			//string strJson = JsonUtility.ToJson(data);
			string strJson = JsonConvert.SerializeObject(data);
			if (strJson.Equals("{}"))
			{
				GameManager.Log("data is null..", "red");
				return;
			}

			string strPath = $"{Application.persistentDataPath}/{strFileName}";
			FileStream fs = new FileStream(strPath, FileMode.Create);
			BinaryFormatter bf = new BinaryFormatter();
			bf.Serialize(fs, strJson);
			fs.Close();
		}
		catch (FileNotFoundException e)
		{
			GameManager.Log($"File error : {e.Message}", "red");
		}
		catch (DirectoryNotFoundException e)
		{
			GameManager.Log($"Directory error : {e.Message}", "red");
		}
		catch (IOException e)
		{
			GameManager.Log($"IO error : {e.Message}", "red");
		}
	}

	public static T LoadData<T>(string strFileName)
	{
		string strPath = $"{Application.persistentDataPath}/{strFileName}";
		try
		{
			if (File.Exists(strPath))
			{
				FileStream fs = new FileStream(strPath, FileMode.Open);
				BinaryFormatter bf = new BinaryFormatter();

				string strJson = bf.Deserialize(fs) as string;
				T data = JsonConvert.DeserializeObject<T>(strJson);

				fs.Close();
				//Debug.Log(strJson);
				return data;
			}

		}
		catch (FileNotFoundException e)
		{
			GameManager.Log($"File error : {e.Message}", "red");
		}
		catch (DirectoryNotFoundException e)
		{
			GameManager.Log($"Directory error : {e.Message}", "red");
		}
		catch (IOException e)
		{
			GameManager.Log($"IO error : {e.Message}", "red");
		}
		return default;
	}

	public static List<T> Unique<T>(IList<T> source, Func<T, T, bool> match)
	{
		if (source == null)
			return null;

		List<T> uniques = new List<T>();

		foreach (T item in source)
		{
			int idx = uniques.FindIndex(x => match(item, x));

			if (idx == -1)
				uniques.Add(item);
		}

		return uniques;
	}

	public static void Shuffle<T>(List<T> source, System.Random rand = null)
	{
		if (rand == null)
			rand = new System.Random();
		for (var i = 0; i < source.Count; i++)
			Swap(source, i, rand.Next(i, source.Count));
	}

	public static void Swap<T>(List<T> source, int i, int j)
	{
		var temp = source[i];
		source[i] = source[j];
		source[j] = temp;
	}

	public static Color HexToColor(string hex)
	{
		hex = hex.Replace("0x", "");
		hex = hex.Replace("#", "");
		byte a = 255;
		byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
		byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
		byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

		if (hex.Length == 8)
		{
			a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
		}
		return new Color32(r, g, b, a);
	}

	public static string EnUTC(double ms = 0)
	{
		return DateTime.UtcNow.AddMilliseconds(ms).ToString("yyyy-MM-dd HH:mm:ss");
	}

	public static string EnDatetime(DateTime dt)
	{
		return dt.ToString("yyyy-MM-dd HH:mm:ss");
	}

	public static string Number(float n, string color = null, int size = 0, string subString = null)
	{
		string number = "";

		if (n == (float)((int)n))
			number = string.Format("{0:N0}", (int)n);
		else
			number = string.Format("{0:N}", n);

		if (!string.IsNullOrEmpty(subString)) number = subString + number;

		if (!string.IsNullOrEmpty(color)) number = "<color=#" + color + ">" + number + "</color>";

		if (size > 0) number = "<size=" + size + ">" + number + "</size>";

		return number;
	}

#if DISABLE_THIS
	public static void OpenAppSetting()
	{
		try
		{
#if UNITY_ANDROID
			using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			using (AndroidJavaObject currentActivityObject = unityClass.GetStatic<AndroidJavaObject>("currentActivity"))
			{
				string packageName = currentActivityObject.Call<string>("getPackageName");

				using (var uriClass = new AndroidJavaClass("android.net.Uri"))
				using (AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("fromParts", "package", packageName, null))
				using (var intentObject = new AndroidJavaObject("android.content.Intent", "android.settings.APPLICATION_DETAILS_SETTINGS", uriObject))
				{
					intentObject.Call<AndroidJavaObject>("addCategory", "android.intent.category.DEFAULT");
					intentObject.Call<AndroidJavaObject>("setFlags", 0x10000000);
					currentActivityObject.Call("startActivity", intentObject);
				}
			}
#elif UNITY_IOS
            string url = GetIOSSettingsURL();
            GameManager.Log("the settings url is:" + url);
            Application.OpenURL(url);
#endif
		}
		catch (Exception ex)
		{
			GameManager.Log(ex.ToString());
		}
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	public static extern string GetIOSSettingsURL();
#endif
#endif // #if DISABLE_THIS

	public static Vector2 GetTouchUIPos()
	{
		Vector2 vPos;
#if UNITY_EDITOR
		vPos = Input.mousePosition;
#else
		vPos = Input.GetTouch(0).position;
#endif
		return vPos;
	}

	public static string GetFullFilePath(string strURL, string strFolderName)
	{
		return $"{Application.persistentDataPath}/{strFolderName}/{GetSafeFilePath(strURL)}";
	}

	public static string GetSafeFilePath(string strURL)
	{
		UnityEngine.Debug.Assert(GetAbsolutePath(strURL).Length < ComType.FILE_NAME_LEN_MAX, "FileName must be less than name max");
		return string.Join("_", GetAbsolutePath(strURL).Split(Path.GetInvalidFileNameChars()));
	}

	public static string GetAbsolutePath(string strURL)
	{
		Uri uri = new Uri(strURL);
		return uri.AbsoluteUri;
	}

	public static void DeleteFile(string strFolderName)
	{
		string strDir = $"{Application.persistentDataPath}/{strFolderName}";
		string[] strFiles = Directory.GetFiles(strDir);
		for (int i = 0; i < strFiles.Length; ++i)
			File.Delete(strFiles[i]);
	}

	public static bool ContainsParam(this Animator _Anim, string _ParamName)
	{
		for (int i = 0; i < _Anim.parameters.Length; ++i)
		{
			if (_Anim.parameters[i].name.Equals(_ParamName, StringComparison.OrdinalIgnoreCase))
				return true;
		}
		return false;
	}

	public static bool AlmostEquals(this float fTarget, float fSecond, float fDiff)
	{
		return Mathf.Abs(fTarget - fSecond) < fDiff;
	}

	public static bool AlmostEquals(this Quaternion qTarget, Quaternion qSecond, float fAngle)
	{
		return Quaternion.Angle(qTarget, qSecond) < fAngle;
	}

	public static bool AlmostEquals(this Vector3 qTarget, Vector3 qSecond, float fDistance)
	{
		return Vector3.Distance(qTarget, qSecond) < fDistance;
	}

	public static T GetComponentInChildren<T>(this GameObject obj, bool includeInactive) where T : Component
	{
		T[] ret = obj.GetComponentsInChildren<T>(includeInactive);
		if (null == ret || ret.Length == 0) return null;
		return ret[0];
	}

	static public void DestroyChildren(this Transform tf, bool bInsertPool = true)
	{
		if ( null == tf ) return;

		while (0 != tf.childCount)
		{
			Transform tfChild = tf.GetChild(0);
			tfChild.SetParent(null);
			GameResourceManager.Singleton.ReleaseObject(tfChild.gameObject, bInsertPool);
		}
	}

	static public void DestroyChildren(this Transform tf, int nIndex)
	{
		if (nIndex < tf.childCount)
		{
			Transform tfChild = tf.GetChild(nIndex);
			tfChild.SetParent(null);
			GameResourceManager.Singleton.ReleaseObject(tfChild.gameObject);
		}
	}

	static public void Reset(this Transform tf)
	{
		tf.localPosition = Vector3.zero;
		tf.localRotation = Quaternion.identity;
		tf.localScale = Vector3.one;
	}

	static public void Reset(this RectTransform tf)
	{
		tf.anchoredPosition3D = Vector3.zero;
		tf.sizeDelta = Vector2.zero;
		tf.localScale = Vector3.one;
	}

	/// <summary>
	/// 실행중인 os 확인.
	/// </summary>
	public static EOSType CheckOS()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
            return EOSType.Android;
#elif UNITY_IOS && !UNITY_EDITOR
            return EOSType.iOS;
#else
		return EOSType.Editor;
#endif
	}

	/// <summary>
	/// 디바이스 고유 값 획득.
	/// </summary>
	public static string GetUniqueID()
	{
		switch (CheckOS())
		{
			case EOSType.Editor:
				string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
				path = Path.Combine(path, "ninetap");

				string filePath = Path.Combine(path, "uuid");
				string uuid = "";

				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}

				if (File.Exists(filePath))
				{
					using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
					using (var reader = new BinaryReader(stream))
					{
						uuid = Encoding.UTF8.GetString(reader.ReadBytes((int)stream.Length));
					}
				}
				else
				{
					uuid = Guid.NewGuid().ToString();
					using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
					using (var writer = new BinaryWriter(stream))
					{
						writer.Write(Encoding.UTF8.GetBytes(uuid));
					}
				}

				return uuid;

			case EOSType.Android:
				string androidId = "";

				try
				{
					AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
					AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity").Call<AndroidJavaObject>("getApplicationContext");
					AndroidJavaClass settingsSecure = new AndroidJavaClass("android.provider.Settings$Secure");
					androidId = settingsSecure.CallStatic<string>("getString", context.Call<AndroidJavaObject>("getContentResolver"), "android_id");
				}
				catch (AndroidJavaException e)
				{
					GameManager.Log("AndroidJavaException: " + e.Message, "red");
				}

				return androidId;
#if UNITY_IOS
			case EOSType.iOS:
				return Device.vendorIdentifier;
#endif
		}

		return null;
	}

	/// <summary>
	/// min ~ max 사이의 랜덤 영문 스트링
	/// </summary>
	/// <param name="min"></param>
	/// <param name="max"></param>
	/// <returns></returns>
	public static string RandString(int min, int max)
	{
		string result = null;

		int length = new System.Random().Next(min, max + 1);

		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
		StringBuilder sb = new StringBuilder();

		for (int i = 0; i < length; i++)
		{
			int index = new System.Random().Next(chars.Length);
			result += chars[index];
		}

		return result;
	}

	/// <summary>
	/// csv 형태로 스트링 스플릿
	/// </summary>
	/// <param name="line"></param>
	/// <returns></returns>
	public static string[] SplitCsvLine(string line)
	{
		bool inQuotes = false;
		var columns = new List<string>();
		var currentColumn = "";

		foreach (char c in line)
		{
			if (c == '"')
			{
				inQuotes = !inQuotes;
			}
			else if (c == ',' && !inQuotes)
			{
				columns.Add(currentColumn.Trim('"'));
				currentColumn = "";
			}
			else
			{
				currentColumn += c;
			}
		}

		columns.Add(currentColumn.Trim('"'));

		return columns.ToArray();
	}

	/// <summary>
	/// 저장된 에피소드 값으로 데이터 테이블의 primaryKey 찾기.
	/// </summary>
	/// <param name="order"></param>
	/// <returns></returns>
	public static uint GetEpisodeKey(int order)
	{
		string hex = $"301{order.ToString("X3")}00";

		return Convert.ToUInt32(hex, 16);
	}

	public static uint GetChapterKey(int episode, int chapter)
	{
		string hex = $"311{episode.ToString("X3")}{chapter.ToString("X2")}";

		return Convert.ToUInt32(hex, 16);
	}

	public static uint GetAdventureKey(int a_nGroup, int a_nOrder)
	{
		string hex = $"363{a_nGroup.ToString("X3")}{a_nOrder.ToString("X2")}";

		return Convert.ToUInt32(hex, 16);
	}

	public static uint GetAbyssKey(int a_nGroup, int a_nOrder)
	{
		string hex = $"378{a_nGroup.ToString("X3")}{a_nOrder.ToString("X2")}";

		return Convert.ToUInt32(hex, 16);
	}

	public static uint GetDefenceGroupKey(int a_nDifficulty)
	{
		string hex = $"384{a_nDifficulty.ToString("X1")}{0.ToString("X4")}";

		return Convert.ToUInt32(hex, 16);
	}

	public static uint GetInstLevelKey(int a_nLevel)
	{
		string hex = $"04{a_nLevel.ToString("X6")}";

		return Convert.ToUInt32(hex, 16);
	}

	public static void SetLanguage(int languageCode)
	{
		PlayerPrefs.SetInt(ComType.STORAGE_LANGUAGE_INT, languageCode);
	}

	public static void ReStart()
	{
		MenuManager.Singleton.SceneEnd();
		MenuManager.Singleton.SceneNext(ESceneType.Title);
	}

	/// <summary>
	/// 두 개의 딕셔너리 합치기. key 가 같으면, value 더하기.
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	/// <param name="dict1"></param>
	/// <param name="dict2"></param>
	/// <returns></returns>
	public static Dictionary<TKey, TValue> MergeDictionaries<TKey, TValue>(Dictionary<TKey, TValue> dict1, Dictionary<TKey, TValue> dict2)
	{
		Dictionary<TKey, TValue> mergedDict = new Dictionary<TKey, TValue>(dict1);

		foreach (KeyValuePair<TKey, TValue> kvp in dict2)
		{
			TKey key = kvp.Key;
			TValue value = kvp.Value;

			if (mergedDict.ContainsKey(key))
			{
				TValue existingValue = mergedDict[key];
				TValue newValue = AddValues(existingValue, value);
				mergedDict[key] = newValue;
			}
			else
			{
				mergedDict.Add(key, value);
			}
		}

		return mergedDict;
	}

	private static TValue AddValues<TValue>(TValue value1, TValue value2)
	{
		if (typeof(TValue) == typeof(int))
		{
			int result = Convert.ToInt32(value1) + Convert.ToInt32(value2);
			return (TValue)Convert.ChangeType(result, typeof(TValue));
		}
		else if (typeof(TValue) == typeof(long))
		{
			long result = Convert.ToInt64(value1) + Convert.ToInt64(value2);
			return (TValue)Convert.ChangeType(result, typeof(TValue));
		}

		return value1;
	}

	public static string ChangeNumberFormat(int number)
	{
		if (number >= 10000000)
		{
			float shortenedNumber = number / 1000000f;
			return shortenedNumber.ToString("0.###") + "M";
		}
		else if (number >= 10000)
		{
			float shortenedNumber = number / 1000f;
			return shortenedNumber.ToString("0.#") + "K";
		}
		else
		{
			return number.ToString();
		}
	}

	/// <summary>
	/// 재료 보유 여부를 bool 로 반환
	/// </summary>
	/// <param name="material"></param>
	/// <returns></returns>
	public static bool CheckMaterial(Dictionary<uint, int> material)
	{
		foreach (KeyValuePair<uint, int> m in material)
		{
			if (m.Value > GameManager.Singleton.invenMaterial.GetItemCount(m.Key))
				return false;
		}

		return true;
	}

	public static string GetItemTypeName(int type)
	{
		string strName = string.Empty;

		strName = ((EItemType)(type)).ToString().ToLower();
		strName = $"ui_item_type_name_{strName}";

		return strName;
	}

	public static void FindFileFromFolder(string path, ref List<string> list)
	{

		string[] directories = Directory.GetDirectories(path);
		for (int i = 0, ii = directories.Length; ii > i; ++i)
		{
			FindFileFromFolder(directories[i], ref list);
		}

		string[] files = Directory.GetFiles(path);
		for (int i = 0, ii = files.Length; ii > i; ++i)
		{
			string ext = Path.GetExtension(files[i]);
			if (ext == ".meta") continue;

			list.Add(files[i]);
		}
	}

	public static string[] DivideString(string word)
	{
		char[] tok = new char[1] { '|' };

		string[] arrWord = word.Split(tok);

		return arrWord;
	}

	public static string GetText(string path)
	{
		string directory = Path.GetDirectoryName(path);

		if (!Directory.Exists(directory))
		{

			Directory.CreateDirectory(directory);

			return string.Empty;
		}

		if (!File.Exists(path))
		{

			return string.Empty;
		}

		string text = string.Empty;

		using (FileStream reader = File.OpenRead(path))
		{
			byte[] temp = new byte[reader.Length];
			reader.Read(temp, 0, (int)reader.Length);
			reader.Close();

			text = System.Text.Encoding.UTF8.GetString(temp);
			GameManager.Log(text);
		}

		return text;
	}

	public static Sprite GetIcon(uint key)
	{
		switch (key.ToString("X").Substring(0, 2))
		{
			case "20": return GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons, WeaponTable.GetData(key).Icon);
			case "23": return GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons, GearTable.GetData(key).Icon);
			case "22": return GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons, MaterialTable.GetData(key).Icon);
			case "24": return GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons, BoxTable.GetData(key).Icon);
			case "25": return GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons, DiceTable.GetData(key).Icon);
			case "11": return GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons, CharacterTable.GetData(key).Icon);
			default: return null;
		}
	}

	public static int GetItemGrade(uint key)
	{
		switch (key.ToString("X").Substring(0, 2))
		{
			case "20": return WeaponTable.GetData(key).Grade;
			case "23": return GearTable.GetData(key).Grade;
			case "22": return MaterialTable.GetData(key).Grade;
			case "24": return BoxTable.GetData(key).Grade;
			case "25": return DiceTable.GetData(key).Grade;
			default: return 0;
		}
	}

	public static string GetItemName(uint key)
	{
		switch (key.ToString("X").Substring(0, 2))
		{
			case "20": return NameTable.GetValue(WeaponTable.GetData(key).NameKey);
			case "23": return NameTable.GetValue(GearTable.GetData(key).NameKey);
			case "22": return NameTable.GetValue(MaterialTable.GetData(key).NameKey);
			case "24": return NameTable.GetValue(BoxTable.GetData(key).NameKey);
			default: return string.Empty;
		}
	}

	public static string GetItemDesc(uint key)
	{
		switch (key.ToString("X").Substring(0, 2))
		{
			case "20": return DescTable.GetValue(WeaponTable.GetData(key).NameKey);
			case "23": return DescTable.GetValue(GearTable.GetData(key).NameKey);
			case "22": return DescTable.GetValue(MaterialTable.GetData(key).NameKey);
			case "24": return DescTable.GetValue(BoxTable.GetData(key).NameKey);
			default: return string.Empty;
		}
	}

	public static EItemType GetItemType(uint key)
	{
		switch (key.ToString("X").Substring(0, 2))
		{
			case "2F": return EItemType.FieldObject;
			case "20": return EItemType.Weapon;
			case "23": return EItemType.Gear;
			case "22": return EItemType.Material;
			case "24": return EItemType.Box;
			case "11": return EItemType.Character;
			default: return 0;
		}
	}

	public static int GetItemSubType(uint key)
	{
		string temp = key.ToString("X");

		switch (temp.Substring(0, 2))
		{
			case "20":
			case "23": return Convert.ToInt32(temp.Substring(3, 1));
			case "22": return Convert.ToInt32(temp.Substring(4, 1));
			case "24": return Convert.ToInt32(temp.Substring(4, 1));
			default: return 0;
		}
	}

	public static string EncodingToString(string path)
	{
		return string.Empty;
	}

	public static Transform Search(Transform target, string name)
	{
		if (target.name == name) return target;

		for (int i = 0; i < target.childCount; ++i)
		{
			Transform result = Search(target.GetChild(i), name);

			if (result != null) return result;
		}

		return null;
	}

	public static Transform[] SearchAll(Transform target, string name)
	{
		List<Transform> list = new List<Transform>();

		for (int i = 0; i < target.childCount; ++i)
		{
			Transform result = Search(target.GetChild(i), name);

			if (result != null)
				list.Add(result);
		}

		return list.ToArray();
	}

	public static void ChangeLayer(Transform t, int layer)
	{
		t.gameObject.layer = layer;

		for (int i = 0, ii = t.childCount; ii > i; ++i)
		{
			Transform tmp = t.GetChild(i);
			tmp.gameObject.layer = layer;

			if (tmp.childCount != 0)
				ChangeLayer(tmp, layer);
		}
	}

	public static string LoadFile(string output)
	{
		if (true == Application.isMobilePlatform)
		{
			TextAsset _textAsset = Resources.Load<TextAsset>(output);
			return _textAsset.text;
		}

		string str = string.Empty;

		if (File.Exists(output) == false)
			return str;

		using (FileStream reader = File.OpenRead(output))
		{
			byte[] temp = new byte[reader.Length];
			reader.Read(temp, 0, (int)reader.Length);
			reader.Close();

			str = System.Text.Encoding.UTF8.GetString(temp);
		}

		return str;
	}

	public static void SaveFile(string output, string str)
	{
		if (!Directory.Exists(Path.GetDirectoryName(output)))
			Directory.CreateDirectory(Path.GetDirectoryName(output));

		if (File.Exists(output))
			File.Delete(output);

		byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
		using (FileStream writer = File.Create(output))
		{
			writer.Write(bytes, 0, bytes.Length);
			writer.Close();
		}
	}

	public static long GetCurrentTimestamp()
	{
		System.TimeSpan t = System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1));
		return System.Convert.ToInt64(t.TotalSeconds * 1000);
	}

	public static System.DateTime ConvertTimestamp(long timestamp)
	{
		return (new System.DateTime(1970, 1, 1, 0, 0, 0)).AddSeconds(timestamp * 0.001);
	}

	public static void StartCheckTimer(string functionName = "")
	{
		sw.Start();

		if (string.IsNullOrEmpty(functionName) == true)
			GameManager.Log("시간 측정을 시작합니다.");
		else
			GameManager.Log(functionName + "의 시간 측정을 시작합니다.");
	}

	public static void StopCheckTimer()
	{
		sw.Stop();
		GameManager.Log(string.Format("총 {0} 초가 걸렸습니다.", sw.ElapsedMilliseconds * 0.001f));
		sw.Reset();
	}

	public static float GetNowStopWatchTime()
	{
		return sw.ElapsedMilliseconds;
	}

	public static DateTime String2Datetime(string value)
	{
		if (DateTime.TryParse(value, out DateTime result))
			return result;
		else
			return default(DateTime);
	}

	public static void GetChildrenNames(ref List<string> childrenNames, Transform root)
	{
		foreach (Transform child in root)
		{
			childrenNames.Add(child.name);
			GetChildrenNames(ref childrenNames, child);
		}
	}

	public static void GetComponentsInChildren<T>(GameObject a_oGameObj, List<T> a_oOutComponentList)
	{
		for (int i = 0; i < a_oGameObj.transform.childCount; ++i)
		{
			// 컴포넌트가 존재 할 경우
			if (a_oGameObj.transform.GetChild(i).TryGetComponent(out T oComponent))
			{
				a_oOutComponentList.Add(oComponent);
			}

			ComUtil.GetComponentsInChildren(a_oGameObj.transform.GetChild(i).gameObject, a_oOutComponentList);
		}
	}

	public static Transform FindChildByName(string ThisName, Transform ThisObj)
	{
		Transform ReturnObj;

		if (ThisObj.name == ThisName)
			return ThisObj.transform;

		foreach (Transform child in ThisObj)
		{
			ReturnObj = FindChildByName(ThisName, child);

			if (ReturnObj != null)
				return ReturnObj;
		}

		return null;
	}

	public static T FindComponentByName<T>(string ThisName, Transform ThisObj) where T : Component
	{
		return ComUtil.FindChildByName(ThisName, ThisObj)?.GetComponentInChildren<T>();
	}

	public static void Collect<T>(List<T> container, Transform parent) where T : Component
	{
		foreach (Transform tr in parent)
		{
			T[] actorModels = tr.gameObject.GetComponents<T>();
			if (null != actorModels)
			{
				container.AddRange(actorModels);
			}

			Collect(container, tr);
		}
	}

	public static void ChangeMobileParticleShader(Transform tr)
	{
		if (false == Application.isMobilePlatform)
			return;

		List<Renderer> targetRenderers = new List<Renderer>();

		Collect(targetRenderers, tr);

		foreach (var render in targetRenderers)
		{
			if (true == render.material.shader.name.Equals("Particles/Additive"))
			{
				render.material.shader = Shader.Find("Mobile/Particles/Additive");
			}
			else if (render.material.shader.name.Equals("Particles/Alpha Blended"))
			{
				render.material.shader = Shader.Find("Mobile/Particles/Alpha Blended");
			}
		}
	}

    public static string Decrypt(byte[] encryptedData, byte[] iv)
    {
        try
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(GameManager.Singleton._enKey);
                aesAlg.IV = iv;
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Mode = CipherMode.CBC;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(encryptedData))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Decryption error: " + ex.Message);
        }
    }

    public static byte[] HexStringToByteArray(string hexString)
    {
        try
        {
            if (string.IsNullOrEmpty(hexString))
                throw new ArgumentNullException(nameof(hexString), "Hex string is null or empty");

            if (hexString.Length % 2 != 0)
                throw new ArgumentException("Hex string length must be even");

            byte[] bytes = new byte[hexString.Length / 2];

            for (int i = 0; i < bytes.Length; i++)
            {
                string byteValue = hexString.Substring(i * 2, 2);
                bytes[i] = Convert.ToByte(byteValue, 16);
            }

            return bytes;
        }
        catch (Exception ex)
        {
            GameManager.Log($"Hex to byte conversion error: {ex.Message}", "red");
            return null;
        }
    }

    public static void ChangeLayersRecursively(Transform trans, string name)
	{
		trans.gameObject.layer = LayerMask.NameToLayer(name);
		foreach (Transform child in trans)
		{
			ChangeLayersRecursively(child, name);
		}
	}

	public static void ChangeLayersRecursively(Transform trans, int layer)
	{
		trans.gameObject.layer = layer;
		foreach (Transform child in trans)
		{
			ChangeLayersRecursively(child, layer);
		}
	}

	//============================================================================================
	// GetFunctionName
	// - 문자열로 타겟 인스턴스의 함수를 찾아준다
	//============================================================================================
	public static Action<T> StringFunctionToAction<T>(object target, string functionName)
	{
		Action<T> action = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), target, functionName);
		return action;
	}

	//============================================================================================
	// GetFunctionName
	// - 문자열로 타겟 인스턴스의 함수를 찾아준다
	//============================================================================================
	public static Action StringFunctionToAction(object target, string functionName)
	{
		Action action = (Action)Delegate.CreateDelegate(typeof(Action), target, functionName);
		return action;
	}

	//============================================================================================
	// GetAnimationClip
	// - 
	//============================================================================================
	public static AnimationClip GetAnimationClip(Animator anim, string stateName)
	{
		RuntimeAnimatorController ac = anim.runtimeAnimatorController;
		for (int i = 0; i < ac.animationClips.Length; i++)
		{
			if (true == ac.animationClips[i].name.Contains(stateName))
			{
				return ac.animationClips[i];
			}
		}

		GameManager.Log("Not found animation clip : " + stateName);
		return null;
	}

	///string type Vector3( 1,1,1) -> 1#1#1
	public static bool ChangeStrToVector(string str, out Vector3 pos)
	{
		char[] tok = new char[1] { '#' };

		string[] arrWord = str.Split(tok);

		pos = new Vector3(float.Parse(arrWord[0]), float.Parse(arrWord[1]), float.Parse(arrWord[2]));

		return arrWord.Length >= 3;
	}

	public static void ChangeVectorToStr(Vector3 pos, ref string str)
	{
		str = string.Format("{0}#{1}#{2}", pos.x, pos.y, pos.z);
	}

	/// <summary>
	/// 해당 카메라가 타겟을 그리고 있는지 여부
	/// </summary>
	/// <param name="cam"></param>
	/// <param name="target"></param>
	/// <returns></returns>
	public static bool IsInCamera(Camera cam, GameObject target)
	{
		Vector3 screenPos = cam.WorldToViewportPoint(target.transform.position);
		return screenPos.z > 0 && screenPos.x > 0 && screenPos.x < 1 && screenPos.y > 0 && screenPos.y < 1;
	}

	public static float Random(float min, float max)
	{
		return UnityEngine.Random.Range(min, max);
	}

	public static int Random(int min, int max)
	{
		return UnityEngine.Random.Range(min, max);
	}

	public static int ChangeHex(string hex)
	{
		return int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
	}

	public static string ConvertByteToHexString(byte[] convertArr)
	{
		string convertArrString = string.Empty;
		convertArrString = string.Concat(Array.ConvertAll(convertArr, byt => byt.ToString("X2")));
		return convertArrString;
	}

	public static byte[] ConvertByteArray(int value)
	{
		byte[] intByte = BitConverter.GetBytes(value);
		if (BitConverter.IsLittleEndian)
			Array.Reverse(intByte);

		return intByte;
	}

	#region 추가
	/** 맵 정보 접두어를 반환한다 */
	public static string GetMapInfoPrefix(EMapInfoType a_eType)
	{
		return ComType.G_PREFIX_MAP_INFO_DICT.GetValueOrDefault(a_eType, ComType.G_PREFIX_MAP_INFO);
	}

	/** 비율 증가 값을 반환한다 */
	public static float GetPercentIncrVal(float a_fVal, float a_fPercent)
	{
		return a_fVal * (1.0f + a_fPercent);
	}

	/** 문자열 => Zip 문자열로 변환한다 */
	public static string ExToZipStr(this string a_oSender, System.Text.Encoding a_oEncoding = null)
	{
		using (var oMemoryStream = new MemoryStream())
		{
			using (var oGZipStream = new GZipStream(oMemoryStream, CompressionMode.Compress))
			{
				var oBytes = (a_oEncoding ?? System.Text.Encoding.Default).GetBytes(a_oSender);
				oGZipStream.Write(oBytes, 0, oBytes.Length);
			}

			var oZipBytes = oMemoryStream.ToArray();
			return System.Convert.ToBase64String(oZipBytes, 0, oZipBytes.Length);
		}
	}

	/** Zip 문자열 => 문자열로 변환한다 */
	public static string ExZipStrToStr(this string a_oSender, System.Text.Encoding a_oEncoding = null)
	{
		var oBytes = System.Convert.FromBase64String(a_oSender);

		using (var oResultStream = new MemoryStream())
		{
			using (var oMemoryStream = new MemoryStream(oBytes, 0, oBytes.Length))
			{
				using (var oGZipStream = new GZipStream(oMemoryStream, CompressionMode.Decompress))
				{
					oGZipStream.CopyTo(oResultStream);
				}

				oResultStream.Seek(0, SeekOrigin.Begin);
				return (a_oEncoding ?? System.Text.Encoding.Default).GetString(oResultStream.ToArray());
			}
		}
	}

	/** 유닛 여부를 검사한다 */
	public static bool IsUnit(GameObject a_oGameObj)
	{
		return a_oGameObj.CompareTag(ComType.G_TAG_NPC) || a_oGameObj.CompareTag(ComType.G_TAG_PLAYER);
	}

	/** 구조물 여부를 검사한다 */
	public static bool IsStructure(GameObject a_oGameObj)
	{
		return a_oGameObj.CompareTag(ComType.G_TAG_STRUCTURE) || a_oGameObj.CompareTag(ComType.G_TAG_STRUCTURE_WOOD);
	}

	/** 필드 오브젝트 아이템 여부를 검사한다 */
	public static bool IsGroundItemFieldObj(uint a_nKey)
	{
		return a_nKey.ToString("X").Substring(0, 2).Equals("2F");
	}

	/** 획득 가능 아이템 여부를 검사한다 */
	public static bool IsEnableAcquireGroundItem(uint a_nKey)
	{
		// 필드 오브젝트 아이템 일 경우
		if (ComUtil.IsGroundItemFieldObj(a_nKey))
		{
			return false;
		}

		string oType = a_nKey.ToString("X").Substring(0, 2);
		return oType.Equals("20") || MaterialTable.GetData(a_nKey).SubType != 0;
	}

	/** 랜덤하게 팁 문자열을 반환한다 */
	public static string GetRandomTipText()
	{
		// TODO: 추후 구현 필요
		return string.Empty;
	}

	/** 쓰기용 스트림을 반환한다 */
	public static FileStream GetWriteStream(string a_oFilePath)
	{
		string oDirPath = Path.GetDirectoryName(a_oFilePath).Replace("\\", "/");

		// 디렉토리가 없을 경우
		if (!string.IsNullOrEmpty(oDirPath) && !Directory.Exists(oDirPath))
		{
			Directory.CreateDirectory(oDirPath);
		}

		return File.Open(a_oFilePath, FileMode.Create, FileAccess.Write);
	}

	/** 이어하기 최종 비용을 계산한다 */
	public static int CalcContinueTotalPrice(int a_nTimes, int a_nStandardPrice)
	{
		return a_nTimes * a_nStandardPrice;
	}

	/** 고유 스테이지 식별자 생성한다 */
	public static ulong MakeUStageID(int a_nStageID, int a_nChapterID, int a_nEpisodeID)
	{
		return ComUtil.MakeUChapterID(a_nChapterID, a_nEpisodeID) + ((ulong)a_nStageID * ComType.G_UNIT_IDS_PER_IDS_01);
	}

	/** 고유 챕터 식별자를 생성한다 */
	public static ulong MakeUChapterID(int a_nStageID, int a_nChapterID)
	{
		return ComUtil.MakeUEpisodeID(a_nChapterID) + ((ulong)a_nStageID * ComType.G_UNIT_IDS_PER_IDS_02);
	}

	/** 고유 에피소드 식별자를 생성한다 */
	public static ulong MakeUEpisodeID(int a_nEpisodeID)
	{
		return (ulong)a_nEpisodeID * ComType.G_UNIT_IDS_PER_IDS_03;
	}

	/** 동일 여부를 검사한다 */
	public static bool ExIsEquals(this double a_dblSender, double a_dblRhs)
	{
		return a_dblSender >= a_dblRhs - double.Epsilon && a_dblSender <= a_dblRhs + double.Epsilon;
	}

	/** 작음 여부를 검사한다 */
	public static bool ExIsLess(this float a_fSender, float a_fRhs)
	{
		return a_fSender < a_fRhs - float.Epsilon;
	}

	/** 작음 여부를 검사한다 */
	public static bool ExIsLess(this double a_dblSender, double a_dblRhs)
	{
		return a_dblSender < a_dblRhs - double.Epsilon;
	}

	/** 작거나 같음 여부를 검사한다 */
	public static bool ExIsLessEquals(this float a_fSender, float a_fRhs)
	{
		return a_fSender.ExIsLess(a_fRhs) || Mathf.Approximately(a_fSender, a_fRhs);
	}

	/** 작거나 같음 여부를 검사한다 */
	public static bool ExIsLessEquals(this double a_dblSender, double a_dblRhs)
	{
		return a_dblSender.ExIsLess(a_dblRhs) || a_dblSender.ExIsEquals(a_dblRhs);
	}

	/** 큰 여부를 검사한다 */
	public static bool ExIsGreat(this float a_fSender, float a_fRhs)
	{
		return a_fSender > a_fRhs + float.Epsilon;
	}

	/** 큰 여부를 검사한다 */
	public static bool ExIsGreat(this double a_dblSender, double a_dblRhs)
	{
		return a_dblSender > a_dblRhs + double.Epsilon;
	}

	/** 크거나 같음 여부를 검사한다 */
	public static bool ExIsGreatEquals(this float a_fSender, float a_fRhs)
	{
		return a_fSender.ExIsGreat(a_fRhs) || Mathf.Approximately(a_fSender, a_fRhs);
	}

	/** 크거나 같음 여부를 검사한다 */
	public static bool ExIsGreatEquals(this double a_dblSender, double a_dblRhs)
	{
		return a_dblSender.ExIsGreat(a_dblRhs) || a_dblSender.ExIsEquals(a_dblRhs);
	}

	/** 화면 등장 여부를 검사한다 */
	public static bool ExIsVisible(this Camera a_oSender, Vector3 a_stWorldPos)
	{
		var stViewportPos = a_oSender.WorldToViewportPoint(a_stWorldPos);
		return stViewportPos.x.ExIsGreatEquals(-0.01f) && stViewportPos.x.ExIsLessEquals(1.01f) && stViewportPos.y.ExIsGreatEquals(-0.01f) && stViewportPos.y.ExIsLessEquals(1.01f);
	}

	/** 범위 포함 여부를 검사한다 */
	public static bool ExIsInRange(this float a_fSender, float a_fMinVal, float a_fMaxVal)
	{
		return a_fSender.ExIsGreatEquals(a_fMinVal) && a_fSender.ExIsLessEquals(a_fMaxVal);
	}

	/** 포함 여부를 검사한다 */
	public static bool ExIsContainsAABB(this Bounds a_stSender, Vector3 a_stPos)
	{
		return a_stPos.x.ExIsInRange(a_stSender.min.x, a_stSender.max.x) && a_stPos.z.ExIsInRange(a_stSender.min.z, a_stSender.max.z);
	}

	/** 바이트를 읽어들인다 */
	public static byte[] ReadBytes(string a_oFilePath, bool a_bIsBase64, System.Text.Encoding a_oEncoding = null)
	{
		// 파일이 존재 할 경우
		if (File.Exists(a_oFilePath))
		{
			var oBytes = File.ReadAllBytes(a_oFilePath);
			return a_bIsBase64 ? System.Convert.FromBase64String((a_oEncoding ?? System.Text.Encoding.Default).GetString(oBytes)) : oBytes;
		}

		return null;
	}

	/** 바이트를 읽어들인다 */
	public static byte[] ReadBytes(Stream a_oStream, bool a_bIsBase64, System.Text.Encoding a_oEncoding = null)
	{
		var oBytes = new byte[a_oStream.Length];
		a_oStream.Read(oBytes);

		return a_bIsBase64 ? System.Convert.FromBase64String((a_oEncoding ?? System.Text.Encoding.Default).GetString(oBytes)) : oBytes;
	}

	/** 바이트를 읽어들인다 */
	public static byte[] ReadBytesFromRes(string a_oFilePath, bool a_bIsBase64, System.Text.Encoding a_oEncoding = null)
	{
		var oTextAsset = Resources.Load<TextAsset>(a_oFilePath);

		// 텍스트 에셋이 존재 할 경우
		if (oTextAsset != null)
		{
			return a_bIsBase64 ? System.Convert.FromBase64String((a_oEncoding ?? System.Text.Encoding.Default).GetString(oTextAsset.bytes)) : oTextAsset.bytes;
		}

		return null;
	}

	/** 문자열을 읽어들인다 */
	public static string ReadStr(string a_oFilePath, bool a_bIsBase64, System.Text.Encoding a_oEncoding = null)
	{
		// 파일이 존재 할 경우
		if (File.Exists(a_oFilePath))
		{
			var oBytes = ComUtil.ReadBytes(a_oFilePath, a_bIsBase64, a_oEncoding ?? System.Text.Encoding.Default);
			return a_bIsBase64 ? (a_oEncoding ?? System.Text.Encoding.Default).GetString(oBytes) : File.ReadAllText(a_oFilePath, a_oEncoding ?? System.Text.Encoding.Default);
		}

		return string.Empty;
	}

	/** 문자열을 읽어들인다 */
	public static string ReadStrFromRes(string a_oFilePath, bool a_bIsBase64, System.Text.Encoding a_oEncoding = null)
	{
		var oTextAsset = Resources.Load<TextAsset>(a_oFilePath);

		// 텍스트 에셋이 존재 할 경우
		if (oTextAsset != null)
		{
			var oBytes = ComUtil.ReadBytesFromRes(a_oFilePath, a_bIsBase64, a_oEncoding ?? System.Text.Encoding.Default);
			return a_bIsBase64 ? (a_oEncoding ?? System.Text.Encoding.Default).GetString(oBytes) : oTextAsset.text;
		}

		return string.Empty;
	}

	/** 문자열을 읽어들인다 */
	public static string ReadStr(Stream a_oStream, bool a_bIsBase64, System.Text.Encoding a_oEncoding = null)
	{
		var oBytes = ComUtil.ReadBytes(a_oStream, a_bIsBase64, a_oEncoding ?? System.Text.Encoding.Default);
		return (a_oEncoding ?? System.Text.Encoding.Default).GetString(oBytes);
	}

	/** 바이트를 기록한다 */
	public static void WriteBytes(string a_oFilePath, byte[] a_oBytes, bool a_bIsBase64, System.Text.Encoding a_oEncoding = null, bool a_bIsEnableAssert = true)
	{
		// 기록이 가능 할 경우
		if (a_oBytes != null && !string.IsNullOrEmpty(a_oFilePath))
		{
			using (var oWStream = ComUtil.GetWriteStream(a_oFilePath))
			{
				ComUtil.WriteBytes(oWStream, a_oBytes, a_bIsBase64, a_oEncoding ?? System.Text.Encoding.Default, a_bIsEnableAssert);
			}
		}
	}

	/** 바이트를 기록한다 */
	public static void WriteBytes(FileStream a_oWStream, byte[] a_oBytes, bool a_bIsBase64, System.Text.Encoding a_oEncoding = null, bool a_bIsEnableAssert = true)
	{
		// 스트림이 존재 할 경우
		if (a_oWStream != null && a_oBytes != null)
		{
			string oBase64Str = System.Convert.ToBase64String(a_oBytes, 0, a_oBytes.Length);
			a_oWStream.Write(a_bIsBase64 ? (a_oEncoding ?? System.Text.Encoding.Default).GetBytes(oBase64Str) : a_oBytes);

			a_oWStream.Flush(true);
		}
	}

	/** 문자열을 기록한다 */
	public static void WriteStr(string a_oFilePath, string a_oStr, bool a_bIsBase64, System.Text.Encoding a_oEncoding = null, bool a_bIsEnableAssert = true)
	{
		// 기록이 가능 할 경우
		if (a_oStr != null && !string.IsNullOrEmpty(a_oFilePath))
		{
			using (var oWStream = ComUtil.GetWriteStream(a_oFilePath))
			{
				ComUtil.WriteStr(oWStream, a_oStr, a_bIsBase64, a_oEncoding ?? System.Text.Encoding.Default, a_bIsEnableAssert);
			}
		}
	}

	/** 문자열을 기록한다 */
	public static void WriteStr(FileStream a_oWStream, string a_oStr, bool a_bIsBase64, System.Text.Encoding a_oEncoding = null, bool a_bIsEnableAssert = true)
	{
		// 스트림이 존재 할 경우
		if (a_oWStream != null && a_oStr != null)
		{
			ComUtil.WriteBytes(a_oWStream, (a_oEncoding ?? System.Text.Encoding.Default).GetBytes(a_oStr), a_bIsBase64, a_oEncoding ?? System.Text.Encoding.Default, a_bIsEnableAssert);
		}
	}

	/** 맵 식별자 => 스테이지 식별자로 변환한다 */
	public static int ExMapIDToStageID(this ulong a_nSender)
	{
		return (int)(a_nSender % ComType.G_UNIT_IDS_PER_IDS_02);
	}

	/** 맵 식별자 => 챕터 식별자로 변환한다 */
	public static int ExMapIDToChapterID(this ulong a_nSender)
	{
		return (int)((a_nSender % ComType.G_UNIT_IDS_PER_IDS_03) / ComType.G_UNIT_IDS_PER_IDS_02);
	}

	/** 맵 식별자 => 에피소드 식별자로 변환한다 */
	public static int ExMapIDToEpisodeID(this ulong a_nSender)
	{
		return (int)(a_nSender / ComType.G_UNIT_IDS_PER_IDS_03);
	}

	/** 값을 제거한다 */
	public static void ExRemoveVal<K, V>(this Dictionary<K, V> a_oSender, K a_tKey, bool a_bIsEnableAssert = true)
	{
		// 값 제거가 가능 할 경우
		if (a_oSender != null && a_oSender.ContainsKey(a_tKey))
		{
			a_oSender.Remove(a_tKey);
		}
	}

	/** 값을 대체한다 */
	public static void ExReplaceVal<K, V>(this Dictionary<K, V> a_oSender, K a_tKey, V a_tVal, bool a_bIsEnableAssert = true)
	{
		// 딕셔너리가 존재 할 경우
		if (a_oSender != null)
		{
			// 값 대체가 가능 할 경우
			if (a_oSender.ContainsKey(a_tKey))
			{
				a_oSender[a_tKey] = a_tVal;
			}
			else
			{
				a_oSender.Add(a_tKey, a_tVal);
			}
		}
	}

	/** JSON 객체를 읽어들인다 */
	public static T ReadJSONObj<T>(string a_oFilePath, bool a_bIsBase64, bool a_bIsZip = false, System.Text.Encoding a_oEncoding = null)
	{
		string oJSONStr = ComUtil.ReadStr(a_oFilePath, a_bIsBase64, a_oEncoding ?? System.Text.Encoding.Default);
		oJSONStr = a_bIsZip ? oJSONStr.ExZipStrToStr() : oJSONStr;

		return oJSONStr.ExJSONStrToObj<T>();
	}

	/** JSON 객체를 기록한다 */
	public static void WriteJSONObj<T>(string a_oFilePath, T a_oObj, bool a_bIsBase64, bool a_bIsZip, System.Text.Encoding a_oEncoding = null, bool a_bIsNeedsRoot = false, bool a_bIsPretty = false, bool a_bIsEnableAssert = true)
	{
		// 경로가 유효 할 경우
		if (!string.IsNullOrEmpty(a_oFilePath))
		{
			string oJSONStr = a_oObj.ExToJSONStr(a_bIsNeedsRoot, a_bIsPretty);
			ComUtil.WriteStr(a_oFilePath, a_bIsZip ? oJSONStr.ExToZipStr() : oJSONStr, a_bIsBase64, a_oEncoding ?? System.Text.Encoding.Default, a_bIsEnableAssert);
		}
	}

	/** JSON 객체를 읽어들인다 */
	public static T ReadJSONObjFromRes<T>(string a_oFilePath, bool a_bIsBase64, System.Text.Encoding a_oEncoding = null)
	{
		return ComUtil.ReadStrFromRes(a_oFilePath, a_bIsBase64, a_oEncoding ?? System.Text.Encoding.Default).ExJSONStrToObj<T>();
	}

	/** 객체 => JSON 문자열로 변환한다 */
	public static string ExToJSONStr<T>(this T a_tSender, bool a_bIsNeedsRoot = false, bool a_bIsPretty = false)
	{
		object oObj = !a_bIsNeedsRoot ? a_tSender as object : new Dictionary<string, object>()
		{
			["Root"] = a_tSender
		};

		return JsonConvert.SerializeObject(oObj, a_bIsPretty ? Formatting.Indented : Formatting.None);
	}

	/** JSON 문자열 => 객체로 변환한다 */
	public static T ExJSONStrToObj<T>(this string a_oSender)
	{
		return JsonConvert.DeserializeObject<T>(a_oSender);
	}

	/** 값을 교환한다 */
	public static void Swap<T>(ref T a_tOutLhs, ref T a_tOutRhs)
	{
		T tTemp = a_tOutLhs; a_tOutLhs = a_tOutRhs; a_tOutRhs = tTemp;
	}

	/** 값을 교환한다 */
	public static void Swap<T>(ref T a_tOutLhs, ref T a_tOutRhs, ESwapType a_eSwapType) where T : System.IComparable<T>
	{
		switch (a_eSwapType)
		{
			case ESwapType.LESS: ComUtil.LessSwap(ref a_tOutLhs, ref a_tOutRhs); break;
			case ESwapType.GREAT: ComUtil.GreatSwap(ref a_tOutLhs, ref a_tOutRhs); break;
			default: ComUtil.Swap(ref a_tOutLhs, ref a_tOutRhs); break;
		}
	}

	/** 값을 교환한다 */
	private static void LessSwap<T>(ref T a_tOutLhs, ref T a_tOutRhs) where T : System.IComparable<T>
	{
		// 보정이 필요 할 경우
		if (a_tOutLhs.CompareTo(a_tOutRhs) > 0)
		{
			ComUtil.Swap(ref a_tOutLhs, ref a_tOutRhs);
		}
	}

	/** 값을 교환한다 */
	private static void GreatSwap<T>(ref T a_tOutLhs, ref T a_tOutRhs) where T : System.IComparable<T>
	{
		// 보정이 필요 할 경우
		if (a_tOutLhs.CompareTo(a_tOutRhs) < 0)
		{
			ComUtil.Swap(ref a_tOutLhs, ref a_tOutRhs);
		}
	}

	/** 인덱스 유효 여부를 검사한다 */
	public static bool ExIsValid_Idx(this int a_nSender)
	{
		return a_nSender > -1;
	}

	/** 값을 추가한다 */
	public static void ExAddVal<T>(this List<T> a_oSender, T a_tVal, bool a_bIsEnableAssert = true)
	{
		// 값 추가가 가능 할 경우
		if (a_oSender != null && !a_oSender.Contains(a_tVal))
		{
			a_oSender.Add(a_tVal);
		}
	}

	/** 값을 추가한다 */
	public static void ExAddVal<T>(this List<T> a_oSender, 
		T a_tVal, System.Predicate<T> a_oCompare, bool a_bIsAssert = true)
	{
		// 값 추가가 불가능 할 경우
		if(a_oSender == null || a_oCompare == null || a_oSender.FindIndex(a_oCompare).ExIsValid_Idx())
		{
			return;
		}

		a_oSender.Add(a_tVal);
	}

	/** 값을 제거한다 */
	public static void ExRemoveVal<T>(this List<T> a_oSender, T a_tVal, bool a_bIsEnableAssert = true)
	{
		// 값 제거가 가능 할 경우
		if (a_oSender != null && a_oSender.Contains(a_tVal))
		{
			a_oSender.ExRemoveVal((a_tCompareVal) => a_tCompareVal.Equals(a_tVal), a_bIsEnableAssert);
		}
	}

	/** 값을 제거한다 */
	public static void ExRemoveVal<T>(this List<T> a_oSender, System.Predicate<T> a_oCompare, bool a_bIsEnableAssert = true)
	{
		// 값 제거가 가능 할 경우
		if (a_oSender != null && a_oCompare != null)
		{
			a_oSender.ExRemoveValAt(a_oSender.FindIndex(a_oCompare), a_bIsEnableAssert);
		}
	}

	/** 값을 제거한다 */
	public static void ExRemoveValAt<T>(this List<T> a_oSender, int a_nIdx, bool a_bIsEnableAssert = true)
	{
		// 값 제거가 가능 할 경우
		if (a_oSender != null && a_nIdx >= 0 && a_nIdx < a_oSender.Count)
		{
			a_oSender.RemoveAt(a_nIdx);
		}
	}

	/** 값을 반환한다 */
	public static T ExGetVal<T>(this Queue<T> a_oSender, T a_tDefVal)
	{
		return (a_oSender.Count > 0) ? a_oSender.Dequeue() : a_tDefVal;
	}

	/** 맵 정보를 생성한다 */
	public static CMapInfo MakeMapInfo(EMapInfoType a_eMapInfoType, int a_nStageID, int a_nChapterID, int a_nEpisodeID)
	{
		var oMapInfo = new CMapInfo()
		{
			m_eMapInfoType = a_eMapInfoType,
			m_stIDInfo = new STIDInfo(a_nStageID, a_nChapterID, a_nEpisodeID)
		};

		oMapInfo.OnAfterDeserialize();
		return oMapInfo;
	}

	/** 객체 정보를 생성한다 */
	public static CObjInfo MakeObjInfo(STTransInfo a_stTransInfo, STPrefabInfo a_stPrefabInfo)
	{
		var oObjInfo = new CObjInfo()
		{
			m_stTransInfo = a_stTransInfo,
			m_stPrefabInfo = a_stPrefabInfo
		};

		oObjInfo.OnAfterDeserialize();
		return oObjInfo;
	}

	/** 이동 지점 정보를 생성한다 */
	public static CWayPointInfo MakeWayPointInfo(STTransInfo a_stTransInfo, STPrefabInfo a_stPrefabInfo)
	{
		var oWayPointInfo = new CWayPointInfo()
		{
			m_stTransInfo = a_stTransInfo,
			m_stPrefabInfo = a_stPrefabInfo
		};

		oWayPointInfo.OnAfterDeserialize();
		return oWayPointInfo;
	}

	/** 애니메이션을 제거한다 */
	public static void ExKill(this DG.Tweening.Tween a_oSender, int a_nID = -1, bool a_bIsComplete = false)
	{
		a_oSender?.Kill(a_bIsComplete);
		DOTween.Kill((a_nID > -1) ? a_nID : a_oSender);
	}

	/** 값을 할당한다 */
	public static void AssignVal(ref DG.Tweening.Tween a_rLhs, DG.Tweening.Tween a_oRhs, int a_nID = -1, DG.Tweening.Tween a_oDefVal = null)
	{
		a_rLhs?.ExKill(a_nID);
		a_rLhs = a_oRhs ?? a_oDefVal;

		// 식별자가 유효 할 경우
		if (a_nID > -1)
		{
			a_rLhs?.SetId(a_nID);
		}
	}

	/** 값을 할당한다 */
	public static void AssignVal(ref DG.Tweening.Sequence a_rLhs, DG.Tweening.Tween a_oRhs, int a_nID = -1, DG.Tweening.Tween a_oDefVal = null)
	{
		a_rLhs?.ExKill(a_nID);
		a_rLhs = (a_oRhs ?? a_oDefVal) as Sequence;

		// 식별자가 유효 할 경우
		if (a_nID > -1)
		{
			a_rLhs?.SetId(a_nID);
		}
	}

	/** 함수를 지연 호출한다 */
	public static void ExLateCallFunc(this MonoBehaviour a_oSender, System.Action<MonoBehaviour> a_oCallback, bool a_bIsEnableAssert = true)
	{
		// 컴포넌트가 존재 할 경우
		if (a_oSender != null)
		{
			a_oSender.StartCoroutine(a_oSender.ExCoLateCallFunc(a_oCallback));
		}
	}

	/** 함수를 지연 호출한다 */
	public static void ExLateCallFunc(this MonoBehaviour a_oSender, System.Action<MonoBehaviour> a_oCallback, float a_fDelay, bool a_bIsEnableAssert = true)
	{
		// 컴포넌트가 존재 할 경우
		if (a_oSender != null)
		{
			a_oSender.StartCoroutine(a_oSender.ExCoLateCallFunc(a_oCallback, a_fDelay, false));
		}
	}

	/** 함수를 지연 호출한다 */
	public static void ExLateCallFuncRealtime(this MonoBehaviour a_oSender, System.Action<MonoBehaviour> a_oCallback, float a_fDelay, bool a_bIsEnableAssert = true)
	{
		// 컴포넌트가 존재 할 경우
		if (a_oSender != null)
		{
			a_oSender.StartCoroutine(a_oSender.ExCoLateCallFunc(a_oCallback, a_fDelay, true));
		}
	}

	/** 효과를 실행한다 */
	public static void ExPlay(this ParticleSystem a_oSender, bool a_bIsPlayChildren = true, bool a_bIsStopChildren = true, ParticleSystemStopBehavior a_eStopBehavior = ParticleSystemStopBehavior.StopEmitting, bool a_bIsEnableAssert = true)
	{
		// 파티클 효과가 존재 할 경우
		if (a_oSender != null)
		{
			a_oSender.Stop(a_bIsStopChildren, a_eStopBehavior);
			a_oSender.Play(a_bIsPlayChildren);
		}
	}

	/** 리스트를 복사한다 */
	public static void ExCopyTo<T01, T02>(this List<T01> a_oSender, List<T02> a_oDestValList, System.Func<T01, T02> a_oCallback, bool a_bIsClear = true, bool a_bIsEnableAssert = true)
	{
		// 복사가 가능 할 경우
		if (a_oSender != null && (a_oDestValList != null && a_oCallback != null))
		{
			// 클리어 모드 일 경우
			if (a_bIsClear)
			{
				a_oDestValList.Clear();
			}

			for (int i = 0; i < a_oSender.Count; ++i)
			{
				a_oDestValList.Add(a_oCallback(a_oSender[i]));
			}
		}
	}

	/** 딕셔너리를 복사한다 */
	public static void ExCopyTo<K, V01, V02>(this Dictionary<K, V01> a_oSender, Dictionary<K, V02> a_oDestValDict, System.Func<K, V01, V02> a_oCallback, bool a_bIsClear = true, bool a_bIsEnableAssert = true)
	{
		// 복사가 가능 할 경우
		if (a_oSender != null && (a_oDestValDict != null && a_oCallback != null))
		{
			// 클리어 모드 일 경우
			if (a_bIsClear)
			{
				a_oDestValDict.Clear();
			}

			foreach (var stKeyVal in a_oSender)
			{
				a_oDestValDict.TryAdd(stKeyVal.Key, a_oCallback(stKeyVal.Key, stKeyVal.Value));
			}
		}
	}

	/** 스택을 복사한다 */
	public static void ExCopyTo<T01, T02>(this Stack<T01> a_oSender, Stack<T02> a_oDestValStack, System.Func<T01, T02> a_oCallback, bool a_bIsClear = true, bool a_bIsEnableAssert = true)
	{
		// 복사가 가능 할 경우
		if (a_oSender != null && (a_oDestValStack != null && a_oCallback != null))
		{
			// 클리어 모드 일 경우
			if (a_bIsClear)
			{
				a_oDestValStack.Clear();
			}

			var oValStack = new Stack<T01>();

			while (a_oSender.Count >= 1)
			{
				oValStack.Push(a_oSender.Pop());
			}

			while (oValStack.Count >= 1)
			{
				a_oDestValStack.Push(a_oCallback(oValStack.Pop()));
			}
		}
	}

	/** 큐를 복사한다 */
	public static void ExCopyTo<T01, T02>(this Queue<T01> a_oSender, Queue<T02> a_oDestValQueue, System.Func<T01, T02> a_oCallback, bool a_bIsClear = true, bool a_bIsEnableAssert = true)
	{
		// 복사가 가능 할 경우
		if (a_oSender != null && (a_oDestValQueue != null && a_oCallback != null))
		{
			// 클리어 모드 일 경우
			if (a_bIsClear)
			{
				a_oDestValQueue.Clear();
			}

			while (a_oSender.Count >= 1)
			{
				a_oDestValQueue.Enqueue(a_oCallback(a_oSender.Dequeue()));
			}
		}
	}

	/** 값을 제거한다 */
	public static void ExRemoveVals<T>(this List<T> a_oSender, List<T> a_oValList, bool a_bIsEnableAssert = true)
	{
		// 값 제거가 가능 할 경우
		if (a_oSender != null && a_oValList != null)
		{
			for (int i = 0; i < a_oValList.Count; ++i)
			{
				a_oSender.Remove(a_oValList[i]);
			}
		}
	}

	/** 문자열 => 색상으로 변환한다 */
	public static Color ConvertToColor(string a_oColor)
	{
		return ColorUtility.TryParseHtmlString(a_oColor, out Color stColor) ? stColor : Color.white;
	}

	/** 유효 여부를 검사한다 */
	public static bool ExIsValid(this string a_oSender)
	{
		return !string.IsNullOrEmpty(a_oSender);
	}

	/** 유효 여부를 검사한다 */
	public static bool ExIsValid<T>(this T[] a_oSender)
	{
		return a_oSender != null && a_oSender.Length > 0;
	}

	/** 유효 여부를 검사한다 */
	public static bool ExIsValid(this TextAsset a_oSender)
	{
		return a_oSender != null && (a_oSender.text.ExIsValid() || a_oSender.bytes.ExIsValid());
	}

	/** 유효 여부를 검사한다 */
	public static bool ExIsValid<T>(this List<T> a_oSender)
	{
		return a_oSender != null && a_oSender.Count > 0;
	}

	/** 인덱스 유효 여부를 검사한다 */
	public static bool ExIsValidIdx<T>(this List<T> a_oSender, int a_nIdx)
	{
		return a_nIdx > -1 && a_nIdx < a_oSender.Count;
	}

	/** 인덱스 유효 여부를 검사한다 */
	public static bool ExIsValidIdx(this int a_nSender)
	{
		return a_nSender > -1;
	}

	/** 인덱스 유효 여부를 검사한다 */
	public static bool ExIsValidIdx(this (int, int, int) a_oSender)
	{
		return a_oSender.Item1.ExIsValidIdx() && a_oSender.Item2.ExIsValidIdx() && a_oSender.Item3.ExIsValidIdx();
	}

	/** 동일 여부를 검사한다 */
	public static bool ExIsEquals(this float a_fSender, float a_fRhs, float a_fEpsilon = float.Epsilon)
	{
		return a_fSender >= a_fRhs - 0.00001f && a_fSender <= a_fRhs + 0.00001f;
	}

	/** 동일 여부를 검사한다 */
	public static bool ExIsEquals(this Color a_stSender, Color a_stRhs, float a_fEpsilon = float.Epsilon)
	{
		return a_stSender.r.ExIsEquals(a_stRhs.r, a_fEpsilon) && a_stSender.g.ExIsEquals(a_stRhs.g, a_fEpsilon) && a_stSender.b.ExIsEquals(a_stRhs.b, a_fEpsilon) && a_stSender.a.ExIsEquals(a_stRhs.a, a_fEpsilon);
	}

	/** 동일 여부를 검사한다 */
	public static bool ExIsEquals(this Vector2 a_stSender, Vector2 a_stRhs)
	{
		return a_stSender.x.ExIsEquals(a_stRhs.x) && a_stSender.y.ExIsEquals(a_stRhs.y);
	}

	/** 동일 여부를 검사한다 */
	public static bool ExIsEquals(this Vector3 a_stSender, Vector3 a_stRhs)
	{
		return a_stSender.x.ExIsEquals(a_stRhs.x) && a_stSender.y.ExIsEquals(a_stRhs.y) && a_stSender.z.ExIsEquals(a_stRhs.z);
	}

	/** 리소스를 로드한다 */
	public static bool TryLoadRes<T>(string a_oFilePath, out T a_tOutRes) where T : UnityEngine.Object
	{
		a_tOutRes = Resources.Load<T>(a_oFilePath);
		return typeof(T).Equals(typeof(TextAsset)) ? (a_tOutRes as TextAsset).ExIsValid() : a_tOutRes != null;
	}

	/** 테이블 데이터를 반환한다 */
	public static EffectTable ExGetData(this List<EffectTable> a_oSender, EEffectType a_eType, ERangeType a_eRangeType, EEffectCategory a_eCategory)
	{
		int nResult = a_oSender.FindIndex((a_oFXTable) => (EEffectType)a_oFXTable.Type == a_eType && (ERangeType)a_oFXTable.RangeType == a_eRangeType && (EEffectCategory)a_oFXTable.Category == a_eCategory);
		return (nResult >= 0 && nResult < a_oSender.Count) ? a_oSender[nResult] : null;
	}

	/** 로컬 => 월드로 변환한다 */
	public static Vector3 ExToWorld(this Vector3 a_stSender, GameObject a_oParent, bool a_bIsCoord = true)
	{
		return a_oParent.transform.localToWorldMatrix * new Vector4(a_stSender.x, a_stSender.y, a_stSender.z, a_bIsCoord ? 1.0f : 0.0f);
	}

	/** 월드 => 로컬로 변환한다 */
	public static Vector3 ExToLocal(this Vector3 a_stSender, GameObject a_oParent, bool a_bIsCoord = true)
	{
		return a_oParent.transform.worldToLocalMatrix * new Vector4(a_stSender.x, a_stSender.y, a_stSender.z, a_bIsCoord ? 1.0f : 0.0f);
	}

	/** 360 각도를 반환한다 */
	public static float ExGet360Deg(this float a_fSender)
	{
		return a_fSender.ExIsLess(0.0f) ? 360.0f - a_fSender : a_fSender;
	}

	/** 360 각도를 반환한다 */
	public static Vector2 ExGet360Deg(this Vector2 a_stSender)
	{
		return new Vector2(a_stSender.x.ExGet360Deg(), a_stSender.y.ExGet360Deg());
	}

	/** 360 각도를 반환한다 */
	public static Vector3 ExGet360Deg(this Vector3 a_stSender)
	{
		return new Vector3(a_stSender.x.ExGet360Deg(), a_stSender.y.ExGet360Deg(), a_stSender.z.ExGet360Deg());
	}

	/** 360 각도를 반환한다 */
	public static Vector4 ExGet360Deg(this Vector4 a_stSender)
	{
		return new Vector4(a_stSender.x.ExGet360Deg(), a_stSender.y.ExGet360Deg(), a_stSender.z.ExGet360Deg(), a_stSender.z.ExGet360Deg());
	}

	/** 포물선 속도를 반환한다 */
	public static Vector3 GetParabolaVelocity(Vector3 a_stSrcPos, Vector3 a_stDestPos, Vector3 a_stGravity, float a_fAngle)
	{
		var stSrcPos = new Vector3(a_stSrcPos.x, 0.0f, a_stSrcPos.z);
		var stDestPos = new Vector3(a_stDestPos.x, 0.0f, a_stDestPos.z);

		float fAngle = a_fAngle * Mathf.Deg2Rad;
		float fDistance = Vector3.Distance(a_stSrcPos, a_stDestPos);

		float fRate = 1.0f / Mathf.Cos(fAngle);
		float fAngleSign = (a_stDestPos.x > a_stSrcPos.x) ? 1.0f : -1.0f;
		float fVelocityV = (fDistance * Mathf.Tan(fAngle)) + (a_stSrcPos.y - a_stDestPos.y);
		float fVelocityH = (a_stGravity.magnitude * Mathf.Pow(fDistance, 2.0f)) / 2.0f;

		float fInitVelocity = fRate * Mathf.Sqrt(fVelocityH / fVelocityV);
		float fParabolaAngle = Vector3.Angle(Vector3.forward, stDestPos - stSrcPos) * fAngleSign;

		return Quaternion.AngleAxis(fParabolaAngle, Vector3.up) * new Vector3(0.0f, fInitVelocity * Mathf.Sin(fAngle), fInitVelocity * Mathf.Cos(fAngle));
	}

	/** 스크롤 뷰 수직 정규 위치를 반환한다 */
	public static float ExGetNormPosV(this ScrollRect a_oSender, GameObject a_oViewport, GameObject a_oContents, Vector3 a_stPos)
	{
		float fContentsPosY = (a_oContents.transform as RectTransform).rect.height - a_stPos.y;
		return Mathf.Clamp01((fContentsPosY - (a_oViewport.transform as RectTransform).rect.height) / ((a_oContents.transform as RectTransform).rect.height - (a_oViewport.transform as RectTransform).rect.height));
	}

	/** 레이어를 변경한다 */
	public static void ExSetLayer(this GameObject a_oSender, int a_nLayer, bool a_bIsResetChildren = true, bool a_bIsEnableAssert = true)
	{
		// 게임 객체가 존재 할 경우
		if (a_oSender != null)
		{
			a_oSender.layer = a_nLayer;

			// 자식 객체 리셋 모드 일 경우
			if (a_bIsResetChildren)
			{
				for (int i = 0; i < a_oSender.transform.childCount; ++i)
				{
					a_oSender.transform.GetChild(i).gameObject.ExSetLayer(a_nLayer, a_bIsResetChildren);
				}
			}
		}
	}

	/** 값을 재배치한다 */
	public static void ExShuffle<T>(this List<T> a_oSender, bool a_bIsEnableAssert = true)
	{
		// 값 재배치가 가능 할 경우
		if (a_oSender != null)
		{
			for (int i = 0; i < a_oSender.Count; ++i)
			{
				int nIdx = UnityEngine.Random.Range(0, a_oSender.Count);

				T tTemp = a_oSender[i];
				a_oSender[i] = a_oSender[nIdx];
				a_oSender[nIdx] = tTemp;
			}
		}
	}

	/** 시간 비율을 변경한다 */
	public static void SetTimeScale(float a_fTimeScale, bool a_bIsWithPhysics = false)
	{
		int nPhysicsFrameRate = 60;

		Time.timeScale = a_fTimeScale;
		Time.fixedDeltaTime = (1.0f / nPhysicsFrameRate) * a_fTimeScale;

		if (a_bIsWithPhysics)
		{
			Time.fixedDeltaTime = (1.0f / nPhysicsFrameRate) / a_fTimeScale;
		}
	}

	/** 시간 문자열을 반환한다 */
	public static string ExGetTimeStr(this System.TimeSpan a_stSender)
	{
		// 60 초 미만 일 경우
		if (a_stSender.TotalSeconds.ExIsLess(60.0f))
		{
			return string.Format("{0}{1}", a_stSender.Seconds, UIStringTable.GetValue("ui_second"));
		}
		// 60 분 미만 일 경우
		else if (a_stSender.TotalSeconds.ExIsLess(3600.0f))
		{
			return string.Format("{0}{1} {2}{3}",
				a_stSender.Minutes, UIStringTable.GetValue("ui_minute"), a_stSender.Seconds, UIStringTable.GetValue("ui_second"));
		}
		// 24 시간 미만 일 경우
		else if (a_stSender.TotalSeconds.ExIsLess(24.0f * 3600.0f))
		{
			return string.Format("{0}{1} {2}{3}",
				a_stSender.Hours, UIStringTable.GetValue("ui_hour"), a_stSender.Minutes, UIStringTable.GetValue("ui_minute"));
		}

		return string.Format("{0}{1} {2}{3}",
			a_stSender.Days, UIStringTable.GetValue("ui_day"), a_stSender.Hours, UIStringTable.GetValue("ui_hour"));
	}

	/** 시간 간격을 반환한다 */
	public static double ExGetDeltaTimePerDays(this System.DateTime a_stSender, System.DateTime a_stRhs)
	{
		return (a_stSender - a_stRhs).TotalDays;
	}

	/** 완료 여부를 검사한다 */
	public static bool ExIsComplete(this Task a_oSender)
	{
		return a_oSender.IsCompleted && !a_oSender.IsFaulted && !a_oSender.IsCanceled;
	}

	/** 성공 완료 여부를 검사한다 */
	public static bool ExIsCompleteSuccess(this Task a_oSender)
	{
		return a_oSender.ExIsComplete() && a_oSender.IsCompletedSuccessfully;
	}

	/** 완료 여부를 검사한다 */
	public static bool ExIsComplete<T>(this Task<T> a_oSender)
	{
		return (a_oSender as Task).ExIsComplete() && a_oSender.Result != null;
	}

	/** 성공 완료 여부를 검사한다 */
	public static bool ExIsCompleteSuccess<T>(this Task<T> a_oSender)
	{
		return a_oSender.ExIsComplete() && a_oSender.IsCompletedSuccessfully;
	}

	/** 함수를 지연 호출한다 */
	private static IEnumerator ExCoLateCallFunc(this MonoBehaviour a_oSender, System.Action<MonoBehaviour> a_oCallback)
	{
		try
		{
			//yield return YieldInstructionCache.WaitForEndOfFrame;
			yield return new WaitForEndOfFrame();
		}
		finally
		{
			a_oCallback?.Invoke(a_oSender);
		}
	}

	/** 함수를 지연 호출한다 */
	private static IEnumerator ExCoLateCallFunc(this MonoBehaviour a_oSender, System.Action<MonoBehaviour> a_oCallback, float a_fDelay, bool a_bIsRealtime)
	{
		try
		{
			yield return a_bIsRealtime ?
				YieldInstructionCache.WaitForSecondsRealtime(a_fDelay) : YieldInstructionCache.WaitForSeconds(a_fDelay);
		}
		finally
		{
			a_oCallback?.Invoke(a_oSender);
		}
	}

#if UNITY_EDITOR || UNITY_STANDALONE
	public static bool IsAppleMSeries => SystemInfo.processorType.ToUpper().Contains("APPLE M");

	/** 커맨드 라인을 실행한다 */
	public static void ExecuteCmdLine(string a_oParams, bool a_bIsAsync = true, bool a_bIsEnableAssert = true)
	{
		// 매개 변수가 유효 할 경우
		if (a_oParams.ExIsValid())
		{
#if UNITY_EDITOR_WIN
			ComUtil.ExecuteCmdLine(ComType.G_TOOL_P_CMD_PROMPT, string.Format(ComType.G_CMD_LINE_PARAMS_FMT_CMD_PROMPT, a_oParams), a_bIsAsync, a_bIsEnableAssert);
#else
			string oParams = string.Format("{0};{1}", ComUtil.IsAppleMSeries ? ComType.G_BUILD_CMD_SILICON_EXPORT_PATH : ComType.G_BUILD_CMD_INTEL_EXPORT_PATH, a_oParams);
			ComUtil.ExecuteCmdLine(ComType.G_TOOL_P_SHELL, string.Format(ComType.G_CMD_LINE_PARAMS_FMT_SHELL, oParams), a_bIsAsync, a_bIsEnableAssert);
#endif // #if UNITY_EDITOR_WIN
		}
	}

	/** 커맨드 라인을 실행한다 */
	public static void ExecuteCmdLine(string a_oFilePath, string a_oParams, bool a_bIsAsync = true, bool a_bIsEnableAssert = true)
	{
		UnityEngine.Debug.Log($"CEditorFunc.ExecuteCmdLine: {a_oFilePath}, {a_oParams}");

		// 커맨드 라인 실행이 가능 할 경우
		if (a_oFilePath.ExIsValid() && a_oParams.ExIsValid())
		{
			var oProcess = Process.Start(ComUtil.MakeProcessStartInfo(a_oFilePath, a_oParams));

			// 동기 모드 일 경우
			if (!a_bIsAsync)
			{
				oProcess?.WaitForExit();
			}
		}
	}

	/** 프로세스 시작 정보를 생성한다 */
	public static ProcessStartInfo MakeProcessStartInfo(string a_oFilePath, string a_oParams)
	{
		var oStartInfo = new ProcessStartInfo(a_oFilePath, a_oParams);
		oStartInfo.CreateNoWindow = true;
		oStartInfo.UseShellExecute = false;

		return oStartInfo;
	}
#endif // #if UNITY_EDITOR || UNITY_STANDALONE
	#endregion // 추가

	#region 클래스 프로퍼티
	public static bool IsNeedsTrackingConsent
	{
		get
		{
#if !UNITY_EDITOR && UNITY_IOS
			var oVer = new System.Version(Device.systemVersion);
			var oCompareVer = new System.Version(14, 0, 0);

			return oVer.CompareTo(oCompareVer) >= 0;
#else
			return true;
#endif // #if !UNITY_EDITOR && UNITY_IOS
		}
	}

	/** 레이아웃을 재배치한다 */
	public static void RebuildLayouts(GameObject a_oObj)
	{
		var oLayoutGroupList = CCollectionPoolManager.Singleton.SpawnList<LayoutGroup>();
		var oContentSizeFitterList = CCollectionPoolManager.Singleton.SpawnList<ContentSizeFitter>();

		try
		{
			a_oObj?.GetComponentsInChildren(true, oLayoutGroupList);
			a_oObj?.GetComponentsInChildren(true, oContentSizeFitterList);

			ComUtil.RebuildLayouts(oLayoutGroupList);
			ComUtil.RebuildLayouts(oContentSizeFitterList);
		}
		finally
		{
			CCollectionPoolManager.Singleton.DespawnList(oLayoutGroupList);
			CCollectionPoolManager.Singleton.DespawnList(oContentSizeFitterList);
		}
	}

	/** 아이템 설명 팝업을 출력한다 */
	public static void ShowItemDescPopup(uint a_nKey, int a_nCount)
	{
		switch (ComUtil.GetItemType(a_nKey))
		{
			case EItemType.Weapon:
				PopupWeapon weaponpopup = MenuManager.Singleton.OpenPopup<PopupWeapon>(EUIPopup.PopupWeapon, true);
				weaponpopup.InitializeInfo(new ItemWeapon(0, a_nKey, 1, 0, 0, 0, false), false, true);
				break;
			case EItemType.Gear:
				PopupGear gearpopup = MenuManager.Singleton.OpenPopup<PopupGear>(EUIPopup.PopupGear, true);
				gearpopup.InitializeInfo(new ItemGear(0, a_nKey, 1, 0, 0, 0, false), false, true);
				break;
			case EItemType.Material:
				PopupMaterial matpopup = MenuManager.Singleton.OpenPopup<PopupMaterial>(EUIPopup.PopupMaterial, true);
				matpopup.InitializeInfo(new ItemMaterial(0, a_nKey, a_nCount));
				break;
			case EItemType.Box:
				PopupBoxNormal boxpopup = MenuManager.Singleton.OpenPopup<PopupBoxNormal>(EUIPopup.PopupBoxNormal, true);
				boxpopup.InitializeInfo(new ItemBox(0, a_nKey, 0), false, false);
				break;
			default:
				break;
		}
	}

	/** 어빌리티 값을 반환한다 */
	public static float GetAbilityVal(EEquipEffectType a_eType, Dictionary<EEquipEffectType, float> a_oAbilityValDict)
	{
		float fVal = a_oAbilityValDict.GetValueOrDefault(a_eType);
		return ComUtil.GetAbilityVal(a_eType, fVal, a_oAbilityValDict);
	}

	/** 어빌리티 값을 반환한다 */
	public static float GetAbilityVal(EEquipEffectType a_eType, float a_fVal, Dictionary<EEquipEffectType, float> a_oAbilityValDict)
	{
		float fAddVal = a_oAbilityValDict.GetValueOrDefault(a_eType + ((int)EOperationType.ADD * (int)EEquipEffectType.OPERATION_TYPE_VAL));
		float fRatioVal = a_oAbilityValDict.GetValueOrDefault(a_eType + ((int)EOperationType.RATIO * (int)EEquipEffectType.OPERATION_TYPE_VAL));

		return (a_fVal + fAddVal) * (1.0f + fRatioVal);
	}

	/** 에이전트 강화 오더 값을 반환한다 */
	public static int GetAgentEnhanceOrderVal(CharacterTable a_oCharacterTable, int a_nIdx)
	{
		int nLevel = (a_nIdx + 1) * ComType.G_OFFSET_AGENT_SKILL_LEVEL;
		var oItemCharacter = ComUtil.GetItemCharacter(a_oCharacterTable);

		var oType = oItemCharacter._stSkillUpgrade.GetType();
		var oFieldInfo = oType.GetField(string.Format("Upgrade{0:00}", nLevel));

		return (oFieldInfo != null) ? (int)oFieldInfo.GetValue(oItemCharacter._stSkillUpgrade) : 0;
	}

	/** 에이전트 최고 레벨을 반환한다 */
	public static int GetAgentMaxLevel(CharacterTable a_oCharacterTable)
	{
		return GlobalTable.GetData<int>(ComType.G_VALUE_MAX_CHARACTER_LEVEL) - 1;
	}

	/** 에이전트 강화 이름을 반환한다 */
	public static string GetAgentEnhanceName(SkillTable a_oSkillTable)
	{
		switch ((ESkillUseType)a_oSkillTable.UseType)
		{
			case ESkillUseType.ACTIVE: return UIStringTable.GetValue("ui_character_skilltype_active");
			case ESkillUseType.PASSIVE: return UIStringTable.GetValue("ui_character_skilltype_passive");
			case ESkillUseType.PASSIVE_GLOBAL: return UIStringTable.GetValue("ui_character_skilltype_global");
		}

		return string.Empty;
	}

	/** 캐릭터 아이템을 반환한다 */
	public static ItemCharacter GetItemCharacter(CharacterTable a_oCharacterTable)
	{
		foreach (var oItemCharacter in GameManager.Singleton.invenCharacter)
		{
			// 잠금 해제 된 캐릭터 일 경우
			if (a_oCharacterTable.PrimaryKey == oItemCharacter.nKey)
			{
				return oItemCharacter;
			}
		}

		return null;
	}

	/** 에이전트 강화 스킬 테이블을 반환한다 */
	public static SkillTable GetAgentEnhanceSkillTable(CharacterTable a_oCharacterTable, int a_nIdx, int a_nOrder, int a_nOrderVal)
	{
		var oEnhanceKeyList = ComUtil.GetAgentEnhanceKeys(a_oCharacterTable, a_nIdx);
		uint nKeyOffset = 0x0101 * (uint)(Mathf.Abs(a_nOrderVal) - 1);

		return (oEnhanceKeyList != null && oEnhanceKeyList[a_nOrder] != 0) ? SkillTable.GetData(oEnhanceKeyList[a_nOrder] + nKeyOffset) : null;
	}

	/** 에이전트 레벨 테이블을 반환한다 */
	public static CharacterLevelTable GetAgentLevelTable(CharacterTable a_oCharacterTable)
	{
		var oItemCharacter = ComUtil.GetItemCharacter(a_oCharacterTable);
		return (oItemCharacter != null) ? CharacterLevelTable.GetTable(a_oCharacterTable.PrimaryKey, oItemCharacter.nCurUpgrade) : null;
	}

	/** 에이전트 강화 스킬 테이블을 반환한다 */
	public static List<SkillTable> GetAgentEnhanceSkillTables(CharacterTable a_oCharacterTable, int a_nIdx)
	{
		var oSkillTableList = new List<SkillTable>();
		var oEnhanceKeyList = ComUtil.GetAgentEnhanceKeys(a_oCharacterTable, a_nIdx);

		for (int i = 0; i < oEnhanceKeyList.Count; ++i)
		{
			var oSkillTable = (oEnhanceKeyList != null && oEnhanceKeyList[i] != 0) ? SkillTable.GetData(oEnhanceKeyList[i]) : null;
			oSkillTableList.Add(oSkillTable);
		}

		return oSkillTableList;
	}

	/** 에이전트 강화 키를 반환한다 */
	public static List<uint> GetAgentEnhanceKeys(CharacterTable a_oCharacterTable, int a_nIdx)
	{
		var oAgentEnhanceKeyInfoList = ComUtil.GetAgentEnhanceKeyInfos(a_oCharacterTable);

		// 키 정보가 없을 경우
		if (!oAgentEnhanceKeyInfoList.ExIsValidIdx(a_nIdx))
		{
			return null;
		}

		return new List<uint>()
		{
			oAgentEnhanceKeyInfoList[a_nIdx].Item1,
			oAgentEnhanceKeyInfoList[a_nIdx].Item2
		};
	}

	/** 에이전트 강화 키 정보를 반환한다 */
	public static List<(uint, uint)> GetAgentEnhanceKeyInfos(CharacterTable a_oCharacterTable)
	{
		var oAgentEnhanceKeyInfoList = new List<(uint, uint)>();
		oAgentEnhanceKeyInfoList.Add((a_oCharacterTable.Skill0501, 0));
		oAgentEnhanceKeyInfoList.Add((a_oCharacterTable.Skill1001, 0));
		oAgentEnhanceKeyInfoList.Add((a_oCharacterTable.Skill1501, a_oCharacterTable.Skill1502));
		oAgentEnhanceKeyInfoList.Add((a_oCharacterTable.Skill2001, 0));
		oAgentEnhanceKeyInfoList.Add((a_oCharacterTable.Skill2501, a_oCharacterTable.Skill2502));
		oAgentEnhanceKeyInfoList.Add((a_oCharacterTable.Skill3001, 0));
		oAgentEnhanceKeyInfoList.Add((a_oCharacterTable.Skill3501, a_oCharacterTable.Skill3502));
		oAgentEnhanceKeyInfoList.Add((a_oCharacterTable.Skill4001, 0));
		oAgentEnhanceKeyInfoList.Add((a_oCharacterTable.Skill4501, a_oCharacterTable.Skill4502));
		oAgentEnhanceKeyInfoList.Add((a_oCharacterTable.Skill5001, 0));
		oAgentEnhanceKeyInfoList.Add((a_oCharacterTable.Skill5501, a_oCharacterTable.Skill5502));
		oAgentEnhanceKeyInfoList.Add((a_oCharacterTable.Skill6001, 0));
		oAgentEnhanceKeyInfoList.Add((a_oCharacterTable.Skill6501, a_oCharacterTable.Skill6502));

		return oAgentEnhanceKeyInfoList;
	}

	/** 최대 강화 여부를 검사한다 */
	public static bool IsMaxEnhance(CharacterTable a_oCharacterTable, int a_nIdx)
	{
		int nOrderVal = ComUtil.GetAgentEnhanceOrderVal(a_oCharacterTable, a_nIdx);
		return Mathf.Abs(nOrderVal) - 1 >= ComType.G_MAX_AGENT_SKILL_LEVEL;
	}

	/** 에이전트 잠금 해제 가능 여부를 검사한다 */
	public static bool IsEnableOpenAgent(CharacterTable a_oCharacterTable)
	{
		var oItemCharacter = ComUtil.GetItemCharacter(a_oCharacterTable);
		return oItemCharacter == null && GameManager.Singleton.invenMaterial.GetItemCount(a_oCharacterTable.RequireItemKey) >= a_oCharacterTable.RequireItemCount;
	}

	/** 에이전트 강화 가능 여부를 검사한다 */
	public static bool IsEnableEnhanceAgent(CharacterTable a_oCharacterTable)
	{
		var oLevelTable = ComUtil.GetAgentLevelTable(a_oCharacterTable);
		var oItemCharacter = ComUtil.GetItemCharacter(a_oCharacterTable);

		// 강호가 불가능 할 경우
		if (oLevelTable == null || (oItemCharacter != null && oItemCharacter.nCurUpgrade >= ComUtil.GetAgentMaxLevel(a_oCharacterTable)))
		{
			return false;
		}

		int nNumItemsA = GameManager.Singleton.invenMaterial.GetItemCount(oLevelTable.RequireItemKey00);
		int nNumItemsB = GameManager.Singleton.invenMaterial.GetItemCount(oLevelTable.RequireItemKey01);

		return nNumItemsA >= oLevelTable.RequireItemCount00 && nNumItemsB >= oLevelTable.RequireItemCount01;
	}

	/** 에이전트 강화 잠금 해제 여부를 검사한다 */
	public static bool IsOpenAgentEnhance(CharacterTable a_oCharacterTable, int a_nIdx)
	{
		int nLevel = (a_nIdx + 1) * ComType.G_OFFSET_AGENT_SKILL_LEVEL;
		var oItemCharacter = ComUtil.GetItemCharacter(a_oCharacterTable);

		return oItemCharacter != null && oItemCharacter.nCurUpgrade >= nLevel - 1;
	}

	/** 효과를 추가한다 */
	public static void AddEffect(EEquipEffectType a_eEffectType,
		EEquipEffectType a_eAbilityEffectType, float a_fVal, float a_fDuration, float a_fInterval, List<STEffectStackInfo> a_oOutEffectStackInfo, int a_nMaxStackCount = 0, bool a_bIsIgnoreStandardAbility = false)
	{
		int nResult = a_oOutEffectStackInfo.FindIndex((a_stStackInfo) =>
		{
			// 출혈 일 경우
			if (a_eEffectType == EEquipEffectType.BLEEDING || a_eEffectType == EEquipEffectType.FREEZE)
			{
				return a_eEffectType == a_stStackInfo.m_eEffectType;
			}

			bool bIsEquals01 = a_fVal.ExIsEquals(a_stStackInfo.m_fVal);
			bool bIsEquals02 = a_fDuration.ExIsEquals(a_stStackInfo.m_fDuration);

			return bIsEquals01 && bIsEquals02 && a_eEffectType == a_stStackInfo.m_eEffectType;
		});

		// 효과 스택 정보가 없을 경우
		if (!a_oOutEffectStackInfo.ExIsValidIdx(nResult))
		{
			a_oOutEffectStackInfo.Add(default);
			nResult = a_oOutEffectStackInfo.Count - 1;
		}

		var stStackInfo = a_oOutEffectStackInfo[nResult];
		stStackInfo.m_fVal = a_fVal;
		stStackInfo.m_fDuration = a_fDuration;
		stStackInfo.m_fInterval = a_fInterval;
		stStackInfo.m_fRemainTime = a_fDuration;
		stStackInfo.m_nStackCount = stStackInfo.m_nStackCount + 1;
		stStackInfo.m_nApplyTimes = 0;
		stStackInfo.m_nMaxStackCount = Mathf.Max(stStackInfo.m_nMaxStackCount, a_nMaxStackCount);
		stStackInfo.m_bIsIgnoreStandardAbility = stStackInfo.m_bIsIgnoreStandardAbility || a_bIsIgnoreStandardAbility;

		stStackInfo.m_eEffectType = a_eEffectType;
		stStackInfo.m_eAbilityEffectType = a_eAbilityEffectType;

		// 최대 스택 개수가 제한되어 있을 경우
		if (stStackInfo.m_nMaxStackCount >= 1)
		{
			stStackInfo.m_nStackCount = Mathf.Min(stStackInfo.m_nStackCount, stStackInfo.m_nMaxStackCount);
		}

		a_oOutEffectStackInfo[nResult] = stStackInfo;
	}

	/** 효과를 추가한다 */
	public static void AddEffect(EffectTable a_oEffectTable, 
		List<STEffectStackInfo> a_oOutEffectStackInfo, bool a_bIsIgnoreOperationType = true, int a_nMaxStackCount = -1, bool a_bIsIgnoreStandardAbility = false)
	{
		ComUtil.AddEffect(a_oEffectTable, a_oEffectTable.Value, a_oOutEffectStackInfo, a_bIsIgnoreOperationType, a_nMaxStackCount, a_bIsIgnoreStandardAbility);
	}

	/** 효과를 추가한다 */
	public static void AddEffect(EffectTable a_oEffectTable, 
		float a_fVal, List<STEffectStackInfo> a_oOutEffectStackInfo, bool a_bIsIgnoreOperationType = true, int a_nMaxStackCount = -1, bool a_bIsIgnoreStandardAbility = false)
	{
		int nOperationTypeVal = a_bIsIgnoreOperationType ? 0 : (int)EOperationType.ADD * (int)EEquipEffectType.OPERATION_TYPE_VAL;

		ComUtil.AddEffect((EEquipEffectType)(a_oEffectTable.Type + nOperationTypeVal),
			(EEquipEffectType)(a_oEffectTable.Type + nOperationTypeVal), a_fVal, a_oEffectTable.Duration * ComType.G_UNIT_MS_TO_S, a_oEffectTable.Inteval * ComType.G_UNIT_MS_TO_S, a_oOutEffectStackInfo, (a_nMaxStackCount < 0) ? a_oEffectTable.MaxStackCount : a_nMaxStackCount, a_bIsIgnoreStandardAbility);
	}

	/** 레이아웃을 재배치한다 */
	private static void RebuildLayouts<T>(List<T> a_oComponentList) where T : Component
	{
		for (int i = a_oComponentList.Count - 1; i >= 0; --i)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(a_oComponentList[i].transform as RectTransform);
		}

		for (int i = 0; i < a_oComponentList.Count; ++i)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(a_oComponentList[i].transform as RectTransform);
		}
	}

	/** 보너스 포인트 획득 비율을 반환한다 */
	public static float GetGoldenPointRate()
	{
		switch (GameDataManager.Singleton.PlayMode)
		{
			case EPlayMode.CAMPAIGN: 
			case EPlayMode.TUTORIAL: return GlobalTable.GetData<float>("ratioBonusNPCinCampaign");
			
			case EPlayMode.HUNT: return GlobalTable.GetData<float>("ratioBonusNPCinHunt");
			case EPlayMode.ADVENTURE: return GlobalTable.GetData<float>("ratioBonusNPCinAdventure");
			case EPlayMode.DEFENCE: return GlobalTable.GetData<float>("ratioBonusNPCinDefence");
			case EPlayMode.ABYSS: return GlobalTable.GetData<float>("ratioBonusNPCinAbyss");
		}

		return 0.0f;
	}
	#endregion // 클래스 프로퍼티
}

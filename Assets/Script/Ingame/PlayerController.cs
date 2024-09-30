using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using TMPro;
using Object = UnityEngine.Object;

public partial class PlayerController : UnitController
{
	[Header("=====> Player Controller - Etc <=====")]
	[SerializeField][Range(0.0f, 1.0f)] private float m_fMovingShootSpeedRatio = 1.0f;
	private Vector3 m_stShootingPointOffset = Vector3.zero;
	private Collider[] m_oOverlapColliders = new Collider[ComType.G_MAX_NUM_OVERLAP_NON_ALLOC];

	public float MovingShootSpeedRatio => m_fMovingShootSpeedRatio;

#if MOVING_SHOOT_ENABLE
	[SerializeField] private bool m_bIsMovingShoot = true;
	public bool IsMovingShoot => m_bIsMovingShoot;
#endif // #if MOVING_SHOOT_ENABLE

	GameManager _GameMgr;
	GameResourceManager _ResourceMgr;
	MenuManager _MenuMgr;

	FloatingJoystick _floatingJoystick;
	Rigidbody _rigidbody;
	Animator _animator;

	CharacterTable _character;
	CharacterLevelTable _characterLevelTable;
	GameObject[] _goEquipWeapon = new GameObject[4];
	ItemWeapon[] _equipWeapon = new ItemWeapon[4];
	Transform _tWeapon;
	Vector3 _vec, _ttt;

	float _hAxis, _vAxis;
	float _cSpeed, _ratioCSpeed;

	int _curSelectWeaponIndex = 0;

	bool _bWalk, _isWalk;
	bool _isBlock, _isLand;
	bool _isCopmpleteLoad = false;

	public override void Awake()
	{
		if (null == _GameMgr) _GameMgr = GameManager.Singleton;
		if (null == _ResourceMgr) _ResourceMgr = GameResourceManager.Singleton;
		if (null == _MenuMgr) _MenuMgr = MenuManager.Singleton;

		foreach (ItemCharacter character in _GameMgr.invenCharacter)
			if (_GameMgr.user.m_nCharacterID == character.id)
			{
				_character = CharacterTable.GetData(character.nKey);
				_characterLevelTable = CharacterLevelTable.GetTable(character.nKey, character.nCurUpgrade);
			}


		_rigidbody = GetComponent<Rigidbody>();
		_animator = GetComponent<Animator>();

		_tWeapon = ComUtil.FindChildByName(ComType.NAME_WEAPON_DUMMY, transform);
		this.Canvas.SetActive(MenuManager.Singleton.CurScene == ESceneType.Battle);

		if (_MenuMgr.CurScene == ESceneType.Battle)
		{
			_floatingJoystick = GameObject.Find("Joystick").GetComponent<FloatingJoystick>();
			_cSpeed = (float)_characterLevelTable.MoveSpeed / 2.0f;
			_isCopmpleteLoad = true;

			#region 추가
			base.Awake();

			_rigidbody.useGravity = false;
			_rigidbody.isKinematic = true;
			_rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

			m_fGravity = GlobalTable.GetData<float>(ComType.G_VALUE_GRAVITY_FOR_PLAYER);
			m_fMoveSpeed = _characterLevelTable.MoveSpeed;

			for (int i = 0; i < this.AbilityValDicts.Length; ++i)
			{
				this.AbilityValDicts[i] = new Dictionary<EEquipEffectType, float>();
				this.OriginAbilityValDicts[i] = new Dictionary<EEquipEffectType, float>();
				this.StandardAbilityValDicts[i] = new Dictionary<EEquipEffectType, float>();
			}

			float fStepHeight = GlobalTable.GetData<int>(ComType.G_MAX_STEP_HEIGHT) * ComType.G_UNIT_MM_TO_M;

			this.CharacterController = this.GetComponent<CharacterController>();
			this.CharacterController.stepOffset = Mathf.Max(0.0f, fStepHeight - this.CharacterController.radius);

			// 효과를 설정한다 {
			m_oRangeFXHandler = _ResourceMgr.CreateObject(EResourceType.Effect, ComType.G_FX_N_ATTACK_RANGE).GetComponent<CRangeFXHandler>();
			m_oRangeFXHandler.Init(this.gameObject);

			m_oLockOnFXHandler = _ResourceMgr.CreateObject(EResourceType.Effect, ComType.G_FX_N_LOCK_ON).GetComponent<CLockOnFXHandler>();
			m_oLockOnFXHandler.gameObject.SetActive(false);
			// 효과를 설정한다 }
			#endregion // 추가
		}

		InitializeEquipWeapon(this.IsEmptySlot(GameDataManager.Singleton.EquipWeaponIdx) ?
			0 : GameDataManager.Singleton.EquipWeaponIdx);

		// 전투 씬 일 경우
		if (MenuManager.Singleton.CurScene == ESceneType.Battle)
		{
			for (int i = 0; i < GameDataManager.Singleton.AcquireWeaponList.Count; ++i)
			{
				for (int j = 0; j < this.EquipWeaponObjs.Length; ++j)
				{
					// 빈 슬롯 일 경우
					if (this.IsEmptySlot(j))
					{
						this.TryInitializeEquipWeapon(j, GameDataManager.Singleton.AcquireWeaponList[i]);
						break;
					}
				}
			}
		}

		// 무기 장착이 가능 할 경우
		if (!this.IsEmptySlot(GameDataManager.Singleton.EquipWeaponIdx))
		{
			this.EquipWeapon(GameDataManager.Singleton.EquipWeaponIdx, true);
		}
	}

	/** 초기화 */
	public override void Start()
	{
		base.Start();

		this.NonPlayerLayerMask = LayerMask.GetMask(ComType.G_LAYER_NON_PLAYER_01,
			ComType.G_LAYER_NON_PLAYER_02, ComType.G_LAYER_NON_PLAYER_03, ComType.G_LAYER_NON_PLAYER_04);

		// 전투 씬이 아닐 경우
		if (_MenuMgr.CurScene != ESceneType.Battle)
		{
			return;
		}

		this.MaxStageInstLV = InstanceLevelTable.GetList().Count;
		this.SetStageInstLV(GameDataManager.Singleton.StageInstLV);

		this.SetAccumulateStageInstEXP(GameDataManager.Singleton.AccumulateStageInstEXP, true);

#if DISABLE_THIS
		int nHP = GameManager.Singleton.user.GetHP(GameManager.Singleton.user.m_nLevel);
		int nDEF = GameManager.Singleton.user.GetDP(GameManager.Singleton.user.m_nLevel);
#else
		int nHP = (int)((_characterLevelTable.HP + GameManager.Singleton.user.m_nAdditionalHP) * (1.0f + GameManager.Singleton.user.m_fMaxHPRatio));
		int nDEF = (int)((_characterLevelTable.DP + GameManager.Singleton.user.m_nAdditionalDP) * (1.0f + GameManager.Singleton.user.m_fDefencePowerRatio));
#endif // #if DISABLE_THIS

		m_oSkillPointRatioDict.TryAdd(EPlayMode.CAMPAIGN, GlobalTable.GetData<float>(ComType.G_RATIO_SKILL_POINT_CAMPAIGN));
		m_oSkillPointRatioDict.TryAdd(EPlayMode.TUTORIAL, GlobalTable.GetData<float>(ComType.G_RATIO_SKILL_POINT_CAMPAIGN));
		m_oSkillPointRatioDict.TryAdd(EPlayMode.HUNT, GlobalTable.GetData<float>(ComType.G_RATIO_SKILL_POINT_HUNT));
		m_oSkillPointRatioDict.TryAdd(EPlayMode.ADVENTURE, GlobalTable.GetData<float>(ComType.G_RATIO_SKILL_POINT_ADVENTURE));
		m_oSkillPointRatioDict.TryAdd(EPlayMode.DEFENCE, GlobalTable.GetData<float>(ComType.G_RATIO_SKILL_POINT_DEFENCE));
		m_oSkillPointRatioDict.TryAdd(EPlayMode.ABYSS, GlobalTable.GetData<float>(ComType.G_RATIO_SKILL_POINT_ABYSS));
		m_oSkillPointRatioDict.TryAdd(EPlayMode.INFINITE, GlobalTable.GetData<float>(ComType.G_RATIO_SKILL_POINT_INFINITE));

		for (int i = 0; i < this.StandardAbilityValDicts.Length; ++i)
		{
			this.StandardAbilityValDicts[i].ExReplaceVal(EEquipEffectType.HP, nHP + GameDataManager.Singleton.ExtraInstHP);
			this.StandardAbilityValDicts[i].ExReplaceVal(EEquipEffectType.DEF, nDEF + GameDataManager.Singleton.ExtraInstDEF);
		}

		int nMaxAgentLevel = GlobalTable.GetData<int>(ComType.G_VALUE_MAX_CHARACTER_LEVEL);
		var oGlobalPassiveEffectList = new List<EffectTable>();

		for (int i = 0; i * ComType.G_OFFSET_AGENT_SKILL_LEVEL < nMaxAgentLevel; ++i)
		{
			// 잠금 해제 상태가 아닐 경우
			if (!ComUtil.IsOpenAgentEnhance(_character, i))
			{
				continue;
			}

			int nOrderVal = ComUtil.GetAgentEnhanceOrderVal(_character, i);
			var oSkillTable = ComUtil.GetAgentEnhanceSkillTable(_character, i, (nOrderVal > 0) ? 0 : 1, nOrderVal);

			var oEffectList = EffectTable.GetGroup(oSkillTable.HitEffectGroup);

			// 액티브 스킬 일 경우
			if (oSkillTable.UseType == (int)ESkillUseType.ACTIVE)
			{
				this.ActiveSkillTable = oSkillTable;
			}
			else
			{
				this.AddPassiveEffects(oEffectList, false);
			}
		}

		foreach (ItemCharacter oCharacter in GameManager.Singleton.invenCharacter)
		{
			// 플레이어 캐릭터 일 경우
			if (GameManager.Singleton.user.m_nCharacterID == oCharacter.id)
			{
				continue;
			}

			var oCharacterTable = CharacterTable.GetData(oCharacter.nKey);
			this.SetupGlobalPassiveEffects(oCharacterTable, oGlobalPassiveEffectList);
		}

		m_stShootingPointOffset = this.ShootingPoint.transform.position - this.transform.position;
		this.AddPassiveEffects(oGlobalPassiveEffectList, false);

		this.SetupAbilityValues(true);
		this.UpdateUIsState();
	}

	/** 전역 패시브 효과를 설정한다 */
	private void SetupGlobalPassiveEffects(CharacterTable a_oCharacterTable, List<EffectTable> a_oOutGlobalPassiveEffectList)
	{
		int nMaxAgentLevel = GlobalTable.GetData<int>(ComType.G_VALUE_MAX_CHARACTER_LEVEL);

		for (int i = 0; i * ComType.G_OFFSET_AGENT_SKILL_LEVEL < nMaxAgentLevel; ++i)
		{
			// 잠금 해제 상태가 아닐 경우
			if (!ComUtil.IsOpenAgentEnhance(a_oCharacterTable, i))
			{
				continue;
			}

			int nOrderVal = ComUtil.GetAgentEnhanceOrderVal(a_oCharacterTable, i);
			var oSkillTable = ComUtil.GetAgentEnhanceSkillTable(a_oCharacterTable, i, (nOrderVal > 0) ? 0 : 1, nOrderVal);

			// 전역 패시브 스킬이 아닐 경우
			if (oSkillTable.UseType != (int)ESkillUseType.PASSIVE_GLOBAL)
			{
				continue;
			}

			var oEffectList = EffectTable.GetGroup(oSkillTable.HitEffectGroup);
			oEffectList.ExCopyTo(a_oOutGlobalPassiveEffectList, (a_oEffectTable) => a_oEffectTable, false);
		}
	}

	public void InitializeEquipWeapon(int slot = -1)
	{
		this.LockInfoList.Clear();
		this.ReloadInfoList.Clear();
		this.MagazineInfoList.Clear();

		this.AttackDelayList.Clear();

		for (long i = 0; i < 4; i++)
		{
			Destroy(_goEquipWeapon[i]);
			_goEquipWeapon[i] = null;
			_equipWeapon[i] = null;

			if (_GameMgr.user.m_nWeaponID[i] > 0)
			{
				ItemWeapon w = _GameMgr.invenWeapon.GetItem(_GameMgr.user.m_nWeaponID[i]);

				_equipWeapon[i] = w;

				if (null != w)
				{
					_goEquipWeapon[i] = _ResourceMgr.CreateObject(EResourceType.Weapon, w.strPrefab, _tWeapon);
					_goEquipWeapon[i].SetActive(i == slot);

					this.ResetEquipWeapon(_goEquipWeapon[i]);
				}

				this.EquipWeaponTables[i] = WeaponTable.GetData(w.nKey);
				this.EquipSoundModelInfos[i] = _goEquipWeapon[i].GetComponentInChildren<SoundModelInfo>();
				this.EquipWeaponModelInfos[i] = _goEquipWeapon[i].GetComponentInChildren<WeaponModelInfo>();
				this.EquipDamageTypes[i] = (EDamageType)this.EquipWeaponTables[i].DamageType;
			}

			// 조준선이 존재 할 경우
			if (this.EquipWeaponModelInfos[i] != null && this.EquipWeaponModelInfos[i].GetSightLinePlayer() != null)
			{
				var oSightLineHandler = GameResourceManager.Singleton.CreateObject<CSightLineHandler>(this.EquipWeaponModelInfos[i].GetSightLinePlayer(), this.EquipWeaponModelInfos[i].GetSightLineDummyTransform(), null);
				oSightLineHandler.gameObject.SetActive(false);
				oSightLineHandler.SetTargetLayerMask(LayerMask.GetMask(ComType.G_LAYER_NON_PLAYER_01, ComType.G_LAYER_NON_PLAYER_02, ComType.G_LAYER_NON_PLAYER_03, ComType.G_LAYER_NON_PLAYER_04, ComType.G_LAYER_STRUCTURE));
				oSightLineHandler.SetOriginDirectionTarget(this.EquipWeaponModelInfos[i].GetSightLineDummyTransform().gameObject);

				this.SightLineHandlers[i] = oSightLineHandler;
			}

			#region 추가
			slot = (slot < 0 && _equipWeapon[i] != null) ? (int)i : slot;
			float fAttackDelay = (_equipWeapon[i] != null) ? _equipWeapon[i].nAttackDelay * ComType.G_UNIT_MS_TO_S : 0.0f;

			this.AttackDelayList.Add(Mathf.Max(0.01f, fAttackDelay));

			this.LockInfoList.Add(new STLockInfo()
			{
				m_bIsLock = false,
				m_fRemainTime = 0.0f,
				m_fMaxLockTime = GlobalTable.GetData<int>(ComType.G_TIME_WEAPON_LOCK_SKILL) * ComType.G_UNIT_MS_TO_S
			});

			this.ReloadInfoList.Add(new STReloadInfo()
			{
				m_bIsReload = false,
				m_fRemainTime = 0.0f,
				m_fMaxReloadTime = this.IsEmptySlot((int)i) ? 0.0f : _equipWeapon[i].nReloadTime * ComType.G_UNIT_MS_TO_S
			});

			this.MagazineInfoList.Add(new STMagazineInfo()
			{
				m_nSlotIdx = (int)i,
				m_nNumBullets = this.IsEmptySlot((int)i) ? 0 : _equipWeapon[i].nMagazineSize,
				m_nMaxNumBullets = this.IsEmptySlot((int)i) ? 0 : _equipWeapon[i].nMagazineSize
			});
			#endregion // 추가
		}

		this.NumEquipWeapons = _equipWeapon.Count((a_oItemWeapon) => a_oItemWeapon != null);
		this.EquipWeapon(this.FindFirstEquipWeaponSlot(), true);

		CalculateAttackPower();
		CalculateHP();
	}

	/** 첫 장착 무기 슬롯을 반환한다 */
	private int FindFirstEquipWeaponSlot()
	{
		for (int i = 0; i < ComType.G_MAX_NUM_EQUIP_WEAPONS; ++i)
		{
			// 무기가 존재 할 경우
			if (!this.IsEmptySlot(i))
			{
				return i;
			}
		}

		return -1;
	}

	/** 장착 무기를 갱신한다 */
	public void UpdateEquipWeapons()
	{
		int nHP = (int)((_characterLevelTable.HP + GameManager.Singleton.user.m_nAdditionalHP) * (1.0f + GameManager.Singleton.user.m_fMaxHPRatio));
		int nDEF = (int)((_characterLevelTable.DP + GameManager.Singleton.user.m_nAdditionalDP) * (1.0f + GameManager.Singleton.user.m_fDefencePowerRatio));

		for (int i = 0; i < ComType.G_MAX_NUM_EQUIP_WEAPONS; ++i)
		{
			_goEquipWeapon[i]?.SetActive(false);
			this.SightLineHandlers[i]?.gameObject.SetActive(false);

			GameResourceManager.Singleton.ReleaseObject(_goEquipWeapon[i], false);
			GameResourceManager.Singleton.ReleaseObject(this.SightLineHandlers[i]?.gameObject, false);

			_goEquipWeapon[i] = null;
			this.SightLineHandlers[i] = null;

			this.StandardAbilityValDicts[i].Clear();
			this.StandardAbilityValDicts[i].ExReplaceVal(EEquipEffectType.HP, nHP + GameDataManager.Singleton.ExtraInstHP);
			this.StandardAbilityValDicts[i].ExReplaceVal(EEquipEffectType.DEF, nDEF + GameDataManager.Singleton.ExtraInstDEF);
		}

		for (int i = 0; i < ComType.G_MAX_NUM_EQUIP_WEAPONS; ++i)
		{
			// 장착 무기가 없을 경우
			if (GameManager.Singleton.user.m_nWeaponID[i] <= 0)
			{
				continue;
			}

			var oItemWeapon = GameManager.Singleton.invenWeapon.GetItem(_GameMgr.user.m_nWeaponID[i]);
			this.TryInitializeEquipWeapon(i, oItemWeapon.nKey, oItemWeapon);
		}

		this.EquipNextWeapon(this.CurWeaponIdx);
		this.EquipWeapon(this.CurWeaponIdx, true);

		this.SetupAbilityValues(true);
	}

	private void TryInitializeEquipWeapon(int a_nSlot, uint a_nKey, ItemWeapon a_oItemWeapon = null, bool a_bIsSetupAbility = false)
	{
		// 무기 초기화가 불가능 할 경우
		if (!this.IsEmptySlot(a_nSlot))
		{
			return;
		}

		var oItemWeapon = a_oItemWeapon ?? new ItemWeapon(0, a_nKey, 1, 0, 0, 0, false);
		_equipWeapon[a_nSlot] = oItemWeapon;

		_goEquipWeapon[a_nSlot] = _ResourceMgr.CreateObject(EResourceType.Weapon, oItemWeapon.strPrefab, _tWeapon);
		_goEquipWeapon[a_nSlot].SetActive(false);

		this.ResetEquipWeapon(_goEquipWeapon[a_nSlot]);
		this.NumEquipWeapons = _equipWeapon.Count((a_oItemWeapon) => a_oItemWeapon != null);

		this.EquipWeaponTables[a_nSlot] = WeaponTable.GetData(a_nKey);
		this.EquipSoundModelInfos[a_nSlot] = _goEquipWeapon[a_nSlot].GetComponentInChildren<SoundModelInfo>();
		this.EquipWeaponModelInfos[a_nSlot] = _goEquipWeapon[a_nSlot].GetComponentInChildren<WeaponModelInfo>();
		this.EquipDamageTypes[a_nSlot] = (EDamageType)this.EquipWeaponTables[a_nSlot].DamageType;

		float fAttackDelay = _equipWeapon[a_nSlot].nAttackDelay * ComType.G_UNIT_MS_TO_S;
		this.AttackDelayList[a_nSlot] = Mathf.Max(0.01f, fAttackDelay);

		var stReloadInfo = this.ReloadInfoList[a_nSlot];
		stReloadInfo.m_fMaxReloadTime = _equipWeapon[a_nSlot].nReloadTime * ComType.G_UNIT_MS_TO_S;

		var stMagazineInfo = this.MagazineInfoList[a_nSlot];
		stMagazineInfo.m_nNumBullets = stMagazineInfo.m_nMaxNumBullets = _equipWeapon[a_nSlot].nMagazineSize;

		this.ReloadInfoList[a_nSlot] = stReloadInfo;
		this.MagazineInfoList[a_nSlot] = stMagazineInfo;

		// 조준선이 존재 할 경우
		if (this.EquipWeaponModelInfos[a_nSlot] != null && this.EquipWeaponModelInfos[a_nSlot].GetSightLinePlayer() != null)
		{
			var oSightLineHandler = GameResourceManager.Singleton.CreateObject<CSightLineHandler>(this.EquipWeaponModelInfos[a_nSlot].GetSightLinePlayer(), this.EquipWeaponModelInfos[a_nSlot].GetSightLineDummyTransform(), null);
			oSightLineHandler.gameObject.SetActive(false);
			oSightLineHandler.SetTargetLayerMask(LayerMask.GetMask(ComType.G_LAYER_NON_PLAYER_01, ComType.G_LAYER_NON_PLAYER_02, ComType.G_LAYER_NON_PLAYER_03, ComType.G_LAYER_NON_PLAYER_04, ComType.G_LAYER_STRUCTURE));
			oSightLineHandler.SetOriginDirectionTarget(this.EquipWeaponModelInfos[a_nSlot].GetSightLineDummyTransform().gameObject);

			this.SightLineHandlers[a_nSlot] = oSightLineHandler;
		}

		// 어빌리티 설정 모드 일 경우
		if (a_bIsSetupAbility)
		{
			this.EquipWeapons[a_nSlot].GetAbilityValues(this.StandardAbilityValDicts[a_nSlot]);
			GameManager.Singleton.user.GetAbilityValues(this.StandardAbilityValDicts[a_nSlot]);
		}

		this.SetupAbilityValues(this.StandardAbilityValDicts[a_nSlot], this.EffectStackInfoList, this.AbilityValDicts[a_nSlot]);
		this.SetupAbilityValues(this.StandardAbilityValDicts[a_nSlot], this.EffectStackInfoList, this.OriginAbilityValDicts[a_nSlot]);

		this.PageBattle.SetIsDirtyUpdateUIsState(true);
	}

	/** 전투 플레이 상태가 되었을 경우 */
	public override void OnBattlePlay()
	{
		for (int i = 0; i < this.StandardAbilityValDicts.Length; ++i)
		{
			// 슬롯이 비어 있을 경우
			if (this.IsEmptySlot(i))
			{
				continue;
			}

			this.EquipWeapons[i].GetAbilityValues(this.StandardAbilityValDicts[i]);
			GameManager.Singleton.user.GetAbilityValues(this.StandardAbilityValDicts[i]);

			this.SetupAbilityValues(this.StandardAbilityValDicts[i], this.EffectStackInfoList, this.AbilityValDicts[i]);
			this.SetupAbilityValues(this.StandardAbilityValDicts[i], this.EffectStackInfoList, this.OriginAbilityValDicts[i]);
		}

		this.EquipWeapon(this.CurWeaponIdx, true);
		base.OnBattlePlay();

		GameDataManager.Singleton.PassiveEffectStackInfoList.ExCopyTo(this.PassiveEffectStackInfoList,
			(a_stStackInfo) => a_stStackInfo);

		this.SetupAbilityValues(true);

		// 지속 플레이 일 경우
		if (GameDataManager.Singleton.IsContinuePlay)
		{
			this.SetHP(GameDataManager.Singleton.PlayerHP);
			this.CurActiveSkillPoint = GameDataManager.Singleton.PlayerActiveSkillPoint;

			this.BattleController.SetGoldenPoint(GameDataManager.Singleton.GoldenPoint);
			GameDataManager.Singleton.SetIsContinuePlay(false);

			for (int i = 0; i < GameDataManager.Singleton.ReloadInfos.Length; ++i)
			{
				this.ReloadInfoList[i] = GameDataManager.Singleton.ReloadInfos[i];
			}

			for (int i = 0; i < GameDataManager.Singleton.MagazineInfos.Length; ++i)
			{
				this.MagazineInfoList[i] = GameDataManager.Singleton.MagazineInfos[i];
			}
		}

		this.UpdateUIsState();
	}

	/** 무기 장착 연출을 시작한다 */
	public void StartWeaponEquipDirecting(int a_nIdx)
	{
		m_oWeaponEquipAni?.Kill();
		var oWeaponTable = WeaponTable.GetData(this.CurWeapon.nKey);

		float fFOV = GameDataManager.Singleton.PlayMapInfo.m_stCameraInfo.m_fFOV;
		float fFinalFOV = fFOV + oWeaponTable.CorrectFOV;

		m_oWeaponEquipAni = DOTween.To(() => this.BattleController.MainCamera.fieldOfView, (a_fVal) => this.BattleController.MainCamera.fieldOfView = a_fVal, fFinalFOV, 0.5f);
	}

	public void EquipWeapon(int slotNumber, bool a_bIsForce = false)
	{
		if (_MenuMgr.CurScene == ESceneType.Lobby)
			ChangeWeaponInLobby(Mathf.Clamp(slotNumber, 0, 3));

		// 무기 장착이 가능 할 경우
		if (a_bIsForce || this.IsEnableEquipWeapon(slotNumber))
		{
			_curSelectWeaponIndex = slotNumber;
			this.SetMagazineInfo(this.MagazineInfoList[slotNumber]);

			for (int i = 0; i < 4; i++)
				if (null != _goEquipWeapon[i])
					_goEquipWeapon[i].SetActive(i == slotNumber);

			if (_MenuMgr.CurScene == ESceneType.Battle)
			{
				this.SetIsFire(false);
				this.SetIsAiming(true);
				this.SetIsDoubleShot(false);
				this.SetIsDoubleShotFire(false);

				this.SetupAbilityValues();
				m_bIsEnableAutoEquipNextWeapon = false;

				var oState = this.StateMachine.State as CStateUnitBattle;
				oState?.SetUpdateSkipTime(0.0f);

				_animator.SetFloat(ComType.G_PARAMS_WEAPON_ANI_TYPE, (int)_equipWeapon[slotNumber].eAnimationType);
				m_oRangeFXHandler.SetRange(this.AttackRange * ComType.G_UNIT_MM_TO_M);

				this.UpdateLockOnTarget();
				this.StartWeaponEquipDirecting(this.CurWeaponIdx);

				// 교환 사운드를 재생한다
				this.BattleController.PlaySwapSound(null);
			}
		}

		// 전투 씬 일 경우
		if (_MenuMgr.CurScene == ESceneType.Battle)
		{
			this.PageBattle.SetIsDirtyUpdateUIsState(true);
		}

#if NEVER_USE_THIS
		// 기존 구문
        _curSelectWeaponIndex = slotNumber;

        for (int i = 0; i < 4; i++)
            if (null != _goEquipWeapon[i])
                _goEquipWeapon[i].SetActive(i == slotNumber);

        _animator.SetFloat("WeaponAniType", (int)_equipWeapon[slotNumber].eAnimationType);
#endif // #if NEVER_USE_THIS
	}

	void ChangeWeaponInLobby(int slotNumber)
	{
		if (null == _equipWeapon[slotNumber])
		{
			int index = Array.IndexOf(_equipWeapon, _equipWeapon.FirstOrDefault(item => item != null));
			ItemWeapon w = _equipWeapon[index];

			_goEquipWeapon[slotNumber] = _ResourceMgr.CreateObject(EResourceType.Weapon, w.strPrefab, _tWeapon);
			_animator.SetFloat(ComType.G_PARAMS_WEAPON_ANI_TYPE, (int)w.eAnimationType);
		}
		else
		{
			_animator.SetFloat(ComType.G_PARAMS_WEAPON_ANI_TYPE, (int)_equipWeapon[slotNumber].eAnimationType);
		}
		_animator.SetTrigger("Reload");
	}

	public int CalculateAttackPower()
	{
		return 0;
	}

	public int CalculateHP()
	{
		return 0;
	}

	public float asdf = 0.5f;

	public void FixedUpdate()
	{
		this.OnFixedUpdate(Time.fixedDeltaTime);

#if NEVER_USE_THIS
		// 기존 구문
        _rigidbody.angularVelocity = Vector3.zero;
        
        _isBlock = Physics.Raycast(transform.position + transform.up * 0.35f,
                                   transform.forward,
                                   0.6f,
                                   LayerMask.GetMask("Structure"));
        _animator.SetBool("IsMove", _vec != Vector3.zero);

        _isLand = Physics.Raycast(transform.position + transform.up * 0.35f,
                                  Vector3.down,
                                  0.4f,
                                  LayerMask.GetMask("Structure"));
        _animator.SetBool("IsAir", !_isLand);
#endif // #if NEVER_USE_THIS
	}

	public void PlayerMove()
	{
		/*
        if (_hAxis != 0 || _vAxis != 0)
        {
            ResetAttackDelay();
            ResetAimDelay();
        }
        */

		// 스킬 사용 중 일 경우
		if (this.StateMachine.State is CStateUnitBattleSkill)
		{
			(this.StateMachine.State as CStatePlayerBattleSkill).TryApplySkill();
			return;
		}

		_vec = new Vector3(_hAxis, 0, _vAxis).normalized;
		var stVelocity = this.Velocity + (this.Acceleration * Time.deltaTime);

		this.SetVelocity(stVelocity);

#if MOVING_SHOOT_ENABLE
		if (this.IsMovingShoot)
		{
			// 조준 대상이 없을 경우
			if (this.LockOnTarget == null || this.Animator.GetFloat("Speed").ExIsGreat(this.MovingShootSpeedRatio))
			{
				this.transform.LookAt(transform.position + _vec);
			}
		}
		else
		{
			this.transform.LookAt(transform.position + _vec);
		}
#else
		this.transform.LookAt(transform.position + _vec);
#endif // #if MOVING_SHOOT_ENABLE

		this.CharacterController.Move(stVelocity * Time.deltaTime);

#if NEVER_USE_THIS
		// 기존 구문
		_vec = new Vector3(_hAxis, 0, _vAxis).normalized;
		//_ttt = (_vec * (_cSpeed / 200f) * (_isAir == true ? 0.5f : 1f)) * Time.deltaTime;
		_ttt = _vec * _cSpeed / 100f * _ratioCSpeed;

		if (!_isBlock)
			transform.position += _ttt;
		//transform.position += (_vec * (_character.MoveSpeed / 100f) * (_bWalk == true ? 0.2f : 1f) * (_isAir == true ? 0.5f : 1f)) * Time.deltaTime;

		transform.LookAt(transform.position + _vec);
#endif // #if NEVER_USE_THIS
	}

	public void PlayerControl()
	{
		// 스킬 시전 상태 일 경우
		if (this.StateMachine.State is CStateUnitBattleSkill)
		{
			_vec = Vector3.zero;
			_vAxis = _hAxis = _ratioCSpeed = 0.0f;

			return;
		}

#if DEBUG
		_vAxis = Input.GetAxisRaw("Vertical") + _floatingJoystick.Vertical;
		_hAxis = Input.GetAxisRaw("Horizontal") + _floatingJoystick.Horizontal;

		//_ratioCSpeed = MathF.Sqrt(MathF.Pow(_hAxis, 2f) + MathF.Pow(_vAxis, 2f));

#if MOVING_SHOOT_ENABLE
		if (this.IsMovingShoot)
		{
			_ratioCSpeed = new Vector3(_vAxis, 0, _hAxis).magnitude;
			_ratioCSpeed = Mathf.Clamp(_ratioCSpeed, 0.95f, 1.0f);
		}
		else
		{
			_ratioCSpeed = new Vector3(_vAxis, 0, _hAxis).normalized.magnitude;
		}
#else
		_ratioCSpeed = new Vector3(_vAxis, 0, _hAxis).normalized.magnitude;
#endif // #if MOVING_SHOOT_ENABLE
#else
		_vAxis = _floatingJoystick.Vertical;
		_hAxis = _floatingJoystick.Horizontal;

		_ratioCSpeed = MathF.Sqrt(MathF.Pow(_hAxis, 2f) + MathF.Pow(_vAxis, 2f));
#endif

		_ratioCSpeed = Mathf.Min(_ratioCSpeed, this.MaxMoveSpeedRatio);
		_animator.SetFloat("Speed", _ratioCSpeed);
	}

	/** 상태를 갱신한다 */
	public override void OnUpdate(float a_fDeltaTime)
	{
		base.OnUpdate(a_fDeltaTime);

		this.TryHandleSkill(a_fDeltaTime);
		this.TryHandleRevive(a_fDeltaTime);
	}

	/** 스킬 상태를 처리한다 */
	private void TryHandleSkill(float a_fDeltaTime)
	{
		// 스킬 사용 상태가 아닐 경우
		if (!this.IsUseSkill)
		{
			return;
		}

		this.RemainSkillUseTime = Mathf.Max(0.0f, this.RemainSkillUseTime - a_fDeltaTime);
		this.IsUseSkill = this.RemainSkillUseTime.ExIsGreat(0.0f);

		// 스킬 사용이 완료 되었을 경우
		if (!this.IsUseSkill)
		{
			this.CurActiveSkillPoint = 0.0f;
			this.OnCompleteUseSkill(this.ApplySkillTable);
		}

		this.PageBattle.SetIsDirtyUpdateUIsState(true);
	}

	/** 무적 상태를 처리한다 */
	private void TryHandleRevive(float a_fDeltaTime)
	{
		// 부활 상태가 아닐 경우
		if (!this.IsRevive || this.IsUntouchable)
		{
			return;
		}

		this.IsRevive = false;

		// 무적 스킬 정보가 존재 할 경우
		if (SkillTable.IsContainsKey(ComType.G_KEY_KNOCK_BACK_SKILL_KEY))
		{
			var oSkillTable = SkillTable.GetData(ComType.G_KEY_KNOCK_BACK_SKILL_KEY);
			this.OnCompleteUseSkillUntouchable(oSkillTable);
		}
	}

	private void Update()
	{
		// 전투 씬이 아닐 경우
		if (_MenuMgr.CurScene != ESceneType.Battle)
		{
			return;
		}

		// 전투 제어자가 존재 할 경우
		if (this.BattleController != null)
		{
			this.BattleController.TouchPointHandler.gameObject.SetActive(false);
		}

		this.OnUpdate(Time.deltaTime);
		bool bIsDirtyUpdateUIsState = false;

		for (int i = 0; i < this.LockInfoList.Count; ++i)
		{
			// 잠금 상태가 아닐 경우
			if (!this.LockInfoList[i].m_bIsLock)
			{
				continue;
			}

			var stLockInfo = this.LockInfoList[i];
			stLockInfo.m_bIsLock = stLockInfo.m_fRemainTime.ExIsGreat(0.0f);
			stLockInfo.m_fRemainTime = Mathf.Max(0.0f, stLockInfo.m_fRemainTime - Time.deltaTime);

			bIsDirtyUpdateUIsState = true;
			this.LockInfoList[i] = stLockInfo;
		}

		for (int i = 0; i < this.ReloadInfoList.Count; ++i)
		{
			// 재장전 상태가 아닐 경우
			if (!this.ReloadInfoList[i].m_bIsReload)
			{
				continue;
			}

			var stReloadInfo = this.ReloadInfoList[i];
			stReloadInfo.m_fRemainTime = Mathf.Max(0.0f, stReloadInfo.m_fRemainTime - Time.deltaTime);
			stReloadInfo.m_bIsReload = stReloadInfo.m_fRemainTime.ExIsGreat(0.0f);

			bIsDirtyUpdateUIsState = true;
			this.ReloadInfoList[i] = stReloadInfo;

			// 재장전이 완료 되었을 경우
			if (!stReloadInfo.m_bIsReload)
			{
				this.OnCompleteReloadWeapon(i);
				this.PageBattle.UpdateUIsState();

				m_oRangeFXHandler.SetIsOn(true);
			}
		}

		// UI 상태 갱신이 필요 할 경우
		if (bIsDirtyUpdateUIsState)
		{
			this.PageBattle.SetIsDirtyUpdateUIsState(true);
		}

		for (int i = 0; i < this.AttackDelayList.Count; ++i)
		{
			// 빈 슬롯 일 경우
			if (this.IsEmptySlot(i))
			{
				continue;
			}

			float fAttackDelay = this.AttackDelayList[i];
			this.AttackDelayList[i] = Mathf.Max(0.0f, fAttackDelay - Time.deltaTime);
		}

		// 전투 씬이 아닐 경우
		if (_MenuMgr.CurScene != ESceneType.Battle)
		{
			return;
		}

		if (Input.GetKeyDown(KeyCode.Alpha1)) this.PageBattle.WeaponUIsHandlerList[0].HandleOnTouchBegin(null, null);
		else if (Input.GetKeyDown(KeyCode.Alpha2)) this.PageBattle.WeaponUIsHandlerList[1].HandleOnTouchBegin(null, null);
		else if (Input.GetKeyDown(KeyCode.Alpha3)) this.PageBattle.WeaponUIsHandlerList[2].HandleOnTouchBegin(null, null);
		else if (Input.GetKeyDown(KeyCode.Alpha4)) this.PageBattle.WeaponUIsHandlerList[3].HandleOnTouchBegin(null, null);

		if (Input.GetKeyUp(KeyCode.Alpha1)) this.PageBattle.WeaponUIsHandlerList[0].HandleOnTouchEnd(null, null);
		else if (Input.GetKeyUp(KeyCode.Alpha2)) this.PageBattle.WeaponUIsHandlerList[1].HandleOnTouchEnd(null, null);
		else if (Input.GetKeyUp(KeyCode.Alpha3)) this.PageBattle.WeaponUIsHandlerList[2].HandleOnTouchEnd(null, null);
		else if (Input.GetKeyUp(KeyCode.Alpha4)) this.PageBattle.WeaponUIsHandlerList[3].HandleOnTouchEnd(null, null);

#if NEVER_USE_THIS
		// 기존 구문
        if (_MenuMgr.CurScene != ESceneType.Battle ||
            !_isCopmpleteLoad) return;

        PlayerControl();
        PlayerMove();
#endif // #if NEVER_USE_THIS
	}
}

#region 추가
/** 플레이어 제어자 */
public partial class PlayerController : UnitController
{
	#region 변수
	[Header("=====> Player Controller - Etc <=====")]
	private bool m_bIsEnableAutoEquipNextWeapon = false;
	private float m_fGravity = 0.0f;
	private float m_fMoveSpeed = 0.0f;

	[Header("=====> Player Controller - UIs <=====")]
	[SerializeField] private TMP_Text m_oHPText = null;

	[Header("=====> Player Controller - Game Objects <=====")]
	[SerializeField] private List<GameObject> m_oSetupLayerObjList = new List<GameObject>();

	private Tween m_oAni = null;
	private Tween m_oWeaponEquipAni = null;
	private CRangeFXHandler m_oRangeFXHandler = null;
	private CLockOnFXHandler m_oLockOnFXHandler = null;

	private Dictionary<EPlayMode, float> m_oSkillPointRatioDict = new Dictionary<EPlayMode, float>();
	#endregion // 변수

	#region 프로퍼티
	public int StageInstLV { get; private set; } = 1;
	public int MaxStageInstLV { get; private set; } = 1;

	public int NumEquipWeapons { get; private set; } = 0;
	public int NonPlayerLayerMask { get; private set; } = 0;

	public float RemainSkillUseTime { get; private set; } = 0.0f;
	public float CurActiveSkillPoint { get; private set; } = 0.0f;
	public float MaxRemainSkillUseTime { get; private set; } = 0.0f;

	public float AccumulateStageInstEXP { get; private set; } = 0;
	public float MaxAccumulateStageInstEXP { get; private set; } = 0;

	public bool IsRevive { get; private set; } = false;
	public bool IsUseSkill { get; private set; } = false;

	public CharacterTable Table => _character;
	public InstanceLevelTable InstLevelTable { get; private set; } = null;
	public CharacterController CharacterController { get; private set; } = null;

	public List<float> AttackDelayList { get; } = new List<float>();
	public List<STLockInfo> LockInfoList { get; } = new List<STLockInfo>();
	public List<STReloadInfo> ReloadInfoList { get; } = new List<STReloadInfo>();
	public List<STMagazineInfo> MagazineInfoList { get; } = new List<STMagazineInfo>();

	public WeaponTable[] EquipWeaponTables { get; } = new WeaponTable[4];
	public SoundModelInfo[] EquipSoundModelInfos { get; } = new SoundModelInfo[4];
	public WeaponModelInfo[] EquipWeaponModelInfos { get; } = new WeaponModelInfo[4];
	public CSightLineHandler[] SightLineHandlers { get; } = new CSightLineHandler[4];
	public EDamageType[] EquipDamageTypes { get; } = new EDamageType[4];

	public Dictionary<EEquipEffectType, float>[] AbilityValDicts { get; } = new Dictionary<EEquipEffectType, float>[4];
	public Dictionary<EEquipEffectType, float>[] OriginAbilityValDicts { get; } = new Dictionary<EEquipEffectType, float>[4];
	public Dictionary<EEquipEffectType, float>[] StandardAbilityValDicts { get; } = new Dictionary<EEquipEffectType, float>[4];

	public bool IsEnableActiveSkill => this.ActiveSkillTable != null;
	public float MaxActiveSkillPoint => this.IsEnableActiveSkill ? this.ActiveSkillTable.RequirePoint : 0.0f;

	public float VAxis => _vAxis;
	public float HAxis => _hAxis;

	public float Speed => this.GetMoveSpeed();
	public float RatioSpeed => _ratioCSpeed;

	public override int TargetGroup => (int)ETargetGroup.PLAYER;
	public override int FireFXGroup => this.CurWeaponTable.HitEffectGroup;
	public override int NumFireProjectilesAtOnce => this.CurWeapon.nProjectileCount;

	public override float Gravity => m_fGravity;
	public SkillTable ActiveSkillTable { get; private set; } = null;

#if MOVING_SHOOT_ENABLE
	public override float MoveSpeed
	{
		get
		{
			// 무빙 샷 상태 일 경우
			if(this.IsMovingShoot && this.StateMachine.State is CStateUnitBattle)
			{
				float fMoveSpeed = (_characterLevelTable.MoveSpeed / 2.0f) + (this.CurWeapon.nMoveSpeed / 2.0f);
				return fMoveSpeed * GameDataManager.Singleton.MovingShootSpeedRatio;
			}

			return (_characterLevelTable.MoveSpeed / 2.0f) + (this.CurWeapon.nMoveSpeed / 2.0f);
		}
	}
#else
	public override float MoveSpeed => (m_fMoveSpeed / 2.0f) + (this.CurWeapon.nMoveSpeed / 2.0f);
#endif // #if MOVING_SHOOT_ENABLE

	public override float AttackRangeStandard => (float)this.CurWeapon.nAttackRangeStandard;
	public override bool IsEnableFire => this.LockOnTarget != null && this.StateMachine.State is CStateUnitBattleLockOn;

	public override EAttackType AttackType => this.CurWeapon.m_eAttackType;
	public override EDamageType DamageType => this.CurDamageType;
	public override EWeaponAnimationType WeaponAniType => this.CurWeapon.eAnimationType;

	public int CurWeaponIdx => (_curSelectWeaponIndex < 0) ? 0 : _curSelectWeaponIndex;
	public EDamageType CurDamageType => this.EquipDamageTypes[this.CurWeaponIdx];
	public STMagazineInfo CurMagazineInfo => this.MagazineInfoList[this.CurWeaponIdx];

	public ItemWeapon[] EquipWeapons => _equipWeapon;
	public GameObject[] EquipWeaponObjs => _goEquipWeapon;
	public CRangeFXHandler RangeFXHandler => m_oRangeFXHandler;

	public override ItemWeapon CurWeapon => this.EquipWeapons[this.CurWeaponIdx];
	public override GameObject CurWeaponObj => this.EquipWeaponObjs[this.CurWeaponIdx];
	public override WeaponTable CurWeaponTable => this.EquipWeaponTables[this.CurWeaponIdx];
	public override SoundModelInfo CurSoundModelInfo => this.EquipSoundModelInfos[this.CurWeaponIdx];
	public override WeaponModelInfo CurWeaponModelInfo => this.EquipWeaponModelInfos[this.CurWeaponIdx];
	public override CSightLineHandler CurSightLineHandler => this.SightLineHandlers[this.CurWeaponIdx];

	public override Dictionary<EEquipEffectType, float> CurAbilityValDict => this.AbilityValDicts[this.CurWeaponIdx];
	public override Dictionary<EEquipEffectType, float> CurOriginAbilityValDict => this.OriginAbilityValDicts[this.CurWeaponIdx];
	public override Dictionary<EEquipEffectType, float> CurStandardAbilityValDict => this.StandardAbilityValDicts[this.CurWeaponIdx];
	#endregion // 프로퍼티

	#region 함수
	/** 제거 되었을 경우 */
	public override void OnDestroy()
	{
		base.OnDestroy();

		ComUtil.AssignVal(ref m_oAni, null);
		ComUtil.AssignVal(ref m_oWeaponEquipAni, null);
	}

	/** 조준선을 리셋한다 */
	public override void ResetSightLineHandler()
	{
		base.ResetSightLineHandler();

		for (int i = 0; i < this.SightLineHandlers.Length; ++i)
		{
			this.SightLineHandlers[i]?.SetColor(Color.white);
			this.SightLineHandlers[i]?.gameObject.SetActive(false);
		}
	}

	/** 상태를 갱신한다 */
	public override void OnCustomUpdate(float a_fDeltaTime)
	{
		base.OnCustomUpdate(a_fDeltaTime);
		this.TryAcquireGroundItemsAround();
	}

	/** 이동 상태를 갱신한다 */
	public override void UpdateMoveState()
	{
		base.UpdateMoveState();

		// 갱신이 불가능 할 경우
		if (_MenuMgr.CurScene != ESceneType.Battle || !_isCopmpleteLoad || !this.IsSurvive || !this.StateMachine.IsEnable)
		{
			return;
		}

		this.PlayerControl();
		this.PlayerMove();
	}

	/** 물리 상태를 갱신한다 */
	public override void UpdatePhysicsState()
	{
		base.UpdatePhysicsState();

		_isBlock = Physics.Raycast(this.GetMoveRayOriginPos(),
								   transform.forward,
								   0.6f,
								   this.MoveLayerMask);

		_animator.SetBool(ComType.G_PARAMS_IS_MOVE, _vec != Vector3.zero);

		_isLand = Physics.SphereCast(this.GetGroundRayOriginPos(),
			0.25f, Vector3.down, out RaycastHit stRaycastHit, 0.65f, this.MoveLayerMask);

		_animator.SetBool(ComType.G_PARAMS_IS_AIR, !_isLand);
	}

	/** 조준 대상을 갱신한다 */
	public override void UpdateLockOnTarget(bool a_bIsForce = false)
	{
		base.UpdateLockOnTarget(a_bIsForce);
		var oTarget = this.LockOnTarget ?? this.NearestTarget;

		m_oLockOnFXHandler.SetLockOnTarget(this.LockOnTarget?.gameObject);
		m_oLockOnFXHandler.SetFollowTarget((oTarget != null && oTarget.IsSurvive) ? oTarget : null);
		m_oLockOnFXHandler.gameObject.SetActive(oTarget != null && oTarget.IsSurvive);
	}

	/** 액티브 스킬 포인트를 증가시킨다 */
	public void IncrActiveSkillPoint(int a_nPoint)
	{
		// 포인트 증가가 불가능 할 경우
		if (!this.IsEnableActiveSkill || this.IsUseSkill)
		{
			return;
		}

		var ePlayMode = GameDataManager.Singleton.PlayMode;
		float fIncrPoint = a_nPoint * m_oSkillPointRatioDict.GetValueOrDefault(ePlayMode, 0.1f);

		// 무한 모드 일 경우
		if (GameDataManager.Singleton.IsInfiniteWaveMode())
		{
			fIncrPoint = a_nPoint * m_oSkillPointRatioDict.GetValueOrDefault(EPlayMode.INFINITE, 0.1f);
		}

		this.CurActiveSkillPoint = Mathf.Clamp(this.CurActiveSkillPoint + fIncrPoint, 0.0f, this.MaxActiveSkillPoint);
		this.PageBattle.SetIsDirtyUpdateUIsState(true);
	}

	/** 발자국 이벤트를 수신했을 경우 */
	public override void Footstep(Object a_oParams)
	{
		// 달리기 상태 일 경우
		if (this.Animator.GetFloat("Speed").ExIsGreatEquals(ComType.G_SPEED_RUN))
		{
			base.Footstep(a_oParams);
			this.BattleController.PlayRunSound(this.gameObject);
		}
		else
		{
			this.BattleController.PlayWalkSound(this.gameObject);
		}
	}

	/** 조준 대상을 공격한다 */
	public override void AttackLockOnTarget()
	{
		base.AttackLockOnTarget();
		bool bIsImmediate = this.IsDoubleShot || this.AimingDelay.ExIsLessEquals(0.0f);

		this.Animator.SetTrigger(bIsImmediate ? ComType.G_PARAMS_LOCK_ON_IMMEDIATE : ComType.G_PARAMS_LOCK_ON);

		for (int i = 0; i < this.SightLineHandlers.Length; ++i)
		{
			this.SightLineHandlers[i]?.gameObject.SetActive(false);
		}
	}

	/** 부활한다 */
	public override void Revive(float a_fUntouchableTime)
	{
		for (int i = 0; i < m_oSetupLayerObjList.Count; ++i)
		{
			m_oSetupLayerObjList[i].ExSetLayer(LayerMask.NameToLayer(ComType.G_LAYER_PLAYER));
		}

		base.Revive(a_fUntouchableTime);

		this.IsRevive = true;
		this.gameObject.layer = LayerMask.NameToLayer(ComType.G_LAYER_PLAYER);

		this.SetIsSurvive(true);
		this.SetIsEnableStateMachine(true);

		this.NavMeshAgent.enabled = false;
		this.BattleController.CamDummy.SetActive(true);

		m_oLockOnFXHandler.gameObject.SetActive(true);
	}

	/** 아이템을 획득했을 경우 */
	public void OnAcquireItem(CGroundItemHandler a_oHandler)
	{
		this.OnAcquireItem(a_oHandler.Key, a_oHandler.NumItems);
		this.BattleController.GroundItemHandlerList.Remove(a_oHandler);

		_ResourceMgr.ReleaseObject(a_oHandler.gameObject, false);
	}

	/** 상호 작용 이벤트를 수신했을 경우 */
	public void OnReceiveInteractableEvent(CInteractableMapObjHandler a_oInteractableMapObjHandler)
	{
		// 상자 일 경우
		if (a_oInteractableMapObjHandler.Params.m_oTable.Prefab.Contains("FIELD_OBJECT_FIELDOBJECT_CHECT_"))
		{
			uint nRewardKey = 0;
			GameAudioManager.PlaySFX(a_oInteractableMapObjHandler.OpenAudioClip, mixerGroup: "Master/SFX/ETC");

			switch ((EFieldObjKey)a_oInteractableMapObjHandler.Params.m_oTable.PrimaryKey)
			{
				case EFieldObjKey.WEAPON_BOX: nRewardKey = this.BattleController.StageTable.WeaponChestGroup; break;
				case EFieldObjKey.MATERIAL_BOX: nRewardKey = this.BattleController.StageTable.MaterialChestGroup; break;
				case EFieldObjKey.COIN_BOX: nRewardKey = this.BattleController.StageTable.CoinChestGroup; break;
				case EFieldObjKey.CRYSTAL_BOX: nRewardKey = this.BattleController.StageTable.CrystalChestGroup; break;
			}

			// 보상이 없을 경우
			if (nRewardKey <= 0)
			{
				return;
			}

			var oGroundItemObjList = CCollectionPoolManager.Singleton.SpawnList<GameObject>();

			try
			{
				var oRewardDict = RewardTable.RandomResultInGroup((int)nRewardKey);
				this.BattleController.CreateGroundItemObjs(oRewardDict, oGroundItemObjList);

				this.BattleController.AddGroundItemObjs(oGroundItemObjList,
					a_oInteractableMapObjHandler.transform.position + Vector3.up * 1.5f);
			}
			finally
			{
				CCollectionPoolManager.Singleton.DespawnList(oGroundItemObjList);
			}
		}
		// 룰렛 일 경우
		else if (a_oInteractableMapObjHandler.Params.m_oTable.Prefab.Contains("FIELD_OBJECT_FIELDOBJECT_ROULETTE_"))
		{
			ComUtil.SetTimeScale(0.00001f, true);

			var stParams = PopupBattleBuffRoulette.MakeParams(this.BattleController.StageTable.AdditionalEffectGroupCount,
				this.BattleController.StageTable.EffectGroupRoulette, (a_oSender, a_oEffectTableList) => this.OnReceiveBattleBuffRoulettePopupCallback(a_oSender, a_oEffectTableList, a_oInteractableMapObjHandler));

			var oPopupBattleBuffRoulette = MenuManager.Singleton.OpenPopup<PopupBattleBuffRoulette>(EUIPopup.PopupBattleBuffRoulette);
			oPopupBattleBuffRoulette.Init(stParams);
		}
	}

	/** 전투 버프 룰렛 팝업 결과를 수신했을 경우 */
	private void OnReceiveBattleBuffRoulettePopupCallback(PopupBattleBuffRoulette a_oSender, List<EffectTable> a_oEffectTableList, CInteractableMapObjHandler a_oInteractableMapObjHandler)
	{
		a_oSender?.Close();
		ComUtil.SetTimeScale(1.0f, false);

		a_oInteractableMapObjHandler.Close();

		this.SetIsDirtyUpdateUIsState(true);
		this.BattleController.OpenNextStagePassage(true);

		for (int i = 0; i < a_oEffectTableList.Count; ++i)
		{
			this.PageBattle.OnReceiveBattleInstEffectPopupResult(null, a_oEffectTableList[i], a_oEffectTableList[i].Value);
		}
	}

	/** 아이템을 획득했을 경우 */
	public void OnAcquireItem(uint a_nKey, int a_nNumItems)
	{
		// 필드 오브젝트 아이템 일 경우
		if (ComUtil.IsGroundItemFieldObj(a_nKey))
		{
			this.OnAcquireGroundItemFieldObj(a_nKey);
		}
		// 획득 가능한 아이템 일 경우
		else if (ComUtil.IsEnableAcquireGroundItem(a_nKey))
		{
			string oType = a_nKey.ToString("X").Substring(0, 2);

			// 무기 일 경우
			if (oType.Equals("20"))
			{
				for (int i = 0; i < this.EquipWeaponObjs.Length; ++i)
				{
					// 빈 슬롯 일 경우
					if (this.IsEmptySlot(i))
					{
						this.TryInitializeEquipWeapon(i, a_nKey, a_bIsSetupAbility: true);
						break;
					}
				}

				GameDataManager.Singleton.AcquireWeaponList.Add(a_nKey);
			}
			else
			{
				int nNumItems = GameDataManager.Singleton.AcquireItemInfoDict.GetValueOrDefault(a_nKey);
				GameDataManager.Singleton.AcquireItemInfoDict.ExReplaceVal(a_nKey, nNumItems + a_nNumItems);
			}
		}
	}

	/** 아이템을 획득했을 경우 */
	public void OnAcquireItem(Dictionary<uint, int> a_oItemInfoDict)
	{
		foreach (var stKeyVal in a_oItemInfoDict)
		{
			this.OnAcquireItem(stKeyVal.Key, stKeyVal.Value);
		}
	}

	/** 도탄 스킬 사용을 완료했을 경우 */
	private void OnCompleteUseSkillRicochet(SkillTable a_oSkillTable)
	{
		this.RemoveEffect(EEquipEffectType.FORCE_RICOCHET_CHANCE, this.ActiveEffectStackInfoList);
	}

	/** 무적 스킬 사용을 완료했을 경우 */
	private void OnCompleteUseSkillUntouchable(SkillTable a_oSkillTable)
	{
		var oGameObjList = CCollectionPoolManager.Singleton.SpawnList<GameObject>();
		var oEffectTableList = EffectTable.GetGroup(a_oSkillTable.HitEffectGroup);

		// 넉백 효과가 존재 할 경우
		if (this.BattleController.FXModelInfo.UntouchableFXInfo.m_oKnockbackFX != null)
		{
			var oFXObj = GameResourceManager.Singleton.CreateObject(this.BattleController.FXModelInfo.UntouchableFXInfo.m_oKnockbackFX,
			this.BattleController.PathObjRoot, this.transform, ComType.G_LIFE_T_FX);

			oFXObj.transform.rotation = Quaternion.identity;
			oFXObj.transform.localScale = Vector3.one * oEffectTableList[0].Value * ComType.G_UNIT_MM_TO_M * 2.0f;

			oFXObj.GetComponentInChildren<ParticleSystem>()?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			oFXObj.GetComponentInChildren<ParticleSystem>()?.Play(true);
		}

		try
		{
			int nResult = Physics.OverlapSphereNonAlloc(this.transform.position,
				oEffectTableList[0].Value * ComType.G_UNIT_MM_TO_M, m_oOverlapColliders, this.NonPlayerLayerMask);

			for (int i = 0; i < nResult; ++i)
			{
				bool bIsValid01 = !oGameObjList.Contains(m_oOverlapColliders[i].gameObject);
				bool bIsValid02 = m_oOverlapColliders[i].TryGetComponent(out UnitController oController);

				// 넉백이 불가능 할 경우
				if (!bIsValid01 || !bIsValid02 || oController == this || !oController.IsEnableKnockBack)
				{
					continue;
				}

				var stDirection = oController.transform.position - this.transform.position;
				stDirection.y = 0.0f;

				bool bIsHit = Physics.Raycast(oController.GetMoveRayOriginPos(), stDirection.normalized, out RaycastHit stRaycastHit);
				float fKnockBackRange = GlobalTable.GetData<float>(ComType.G_VALUE_INVINCIBLE_EXPLOSION_POWER);

				// 넉백 방향에 장애물이 존재 할 경우
				if (bIsHit && this.IsStructure(stRaycastHit.collider.gameObject) && stRaycastHit.distance.ExIsLess(1.0f))
				{
					fKnockBackRange = 0.0f;
				}

				oController.SetTrackingTarget(this);
				oGameObjList.ExAddVal(m_oOverlapColliders[i].gameObject);

				oController.Rigidbody.AddForce(stDirection.normalized * fKnockBackRange, ForceMode.VelocityChange);
				oController.StartHitDirecting(default, false);
			}
		}
		finally
		{
			CCollectionPoolManager.Singleton.DespawnList(oGameObjList);
		}
	}

	/** 점프 공격 스킬 사용을 완료했을 경우 */
	private void OnCompleteUseSkillJumpAttack(SkillTable a_oSkillTable)
	{
		var oSkillState = this.StateMachine.State as CStatePlayerBattleSkill;

		// 스킬 적용 상태 일 경우
		if (oSkillState == null || oSkillState.IsApplySkill)
		{
			return;
		}

		ComUtil.SetTimeScale(1.0f, true);
		this.StateMachine.SetState(this.Animator.GetBool(ComType.G_PARAMS_IS_MOVE) ? this.CreateMoveState() : this.CreateIdleState());
	}

	/** 스킬 사용을 완료했을 경우 */
	public void OnCompleteUseSkill(SkillTable a_oSkillTable)
	{
		switch ((ESkillType)a_oSkillTable.SkillType)
		{
			case ESkillType.RICOCHET: this.OnCompleteUseSkillRicochet(a_oSkillTable); break;
			case ESkillType.UNTOUCHABLE: this.OnCompleteUseSkillUntouchable(a_oSkillTable); break;
			case ESkillType.JUMP_ATTACK: this.OnCompleteUseSkillJumpAttack(a_oSkillTable); break;
		}

		this.SetupAbilityValues(true);
	}

	/** 발사 이벤트를 수신했을 경우 */
	public override void Fire(Object a_oParams)
	{
		bool bIsLockOnState = this.StateMachine.State is CStateUnitBattleLockOn;

		// 조준 상태가 아닐 경우
		if (!bIsLockOnState)
		{
			return;
		}

		// 총알이 존재 할 경우
		if (this.MagazineInfoList[this.CurWeaponIdx].m_nNumBullets >= 1)
		{
			base.Fire(a_oParams);

			// 스나이퍼 일 경우
			if (this.WeaponAniType == EWeaponAnimationType.SR)
			{
				this.PageBattle.StartCameraShakeDirecting(this.transform.forward,
					0.05f, GlobalTable.GetData<float>(ComType.G_VALUE_SNIPER_RECOIL));
			}
		}

		this.PageBattle.SetIsDirtyUpdateUIsState(true);

		// 탄약 소비가 안되었을 경우
		if (this.IsDoubleShotFire || !this.IsEnableFire)
		{
			this.SetIsDoubleShotFire(false);
			return;
		}

		var stMagazineInfo = this.MagazineInfoList[this.CurWeaponIdx];
		stMagazineInfo.m_nNumBullets = Mathf.Max(0, stMagazineInfo.m_nNumBullets - 1);

		this.SetMagazineInfo(stMagazineInfo);
		float fAttackDelay = this.CurAbilityValDict.GetValueOrDefault(EEquipEffectType.ATK_DELAY) * ComType.G_UNIT_MS_TO_S;

		fAttackDelay = fAttackDelay * (1.0f + ComUtil.GetAbilityVal(EEquipEffectType.AttackDelayRatio, this.CurAbilityValDict));

		// 집중 사격 처리가 필요 할 경우
		if (this.CurWeaponTable.BurstCount > 0 && (this.FireTimes + 1) % this.CurWeaponTable.BurstCount == 0)
		{
			fAttackDelay = this.CurWeaponTable.BurstDelay * ComType.G_UNIT_MS_TO_S;
			fAttackDelay = fAttackDelay * (1.0f + ComUtil.GetAbilityVal(EEquipEffectType.AttackDelayRatio, this.CurAbilityValDict));
		}

		this.AttackDelayList[this.CurWeaponIdx] = fAttackDelay;
		this.MagazineInfoList[this.CurWeaponIdx] = stMagazineInfo;

		this.SetFireTimes(this.FireTimes + 1);

		// 총알이 남아있을 경우
		if (stMagazineInfo.m_nNumBullets >= 1)
		{
			return;
		}

		int nIdx = this.CurWeaponIdx;
		m_bIsEnableAutoEquipNextWeapon = true;

		StopCoroutine("ReloadWeaponDelay");
		StartCoroutine(this.ReloadWeaponDelay(nIdx, 0.5f));
	}

	/** 무기를 지연 재장전한다 */
	private IEnumerator ReloadWeaponDelay(int a_nIdx, float a_fDelay)
	{
		yield return YieldInstructionCache.WaitForSeconds(a_fDelay);
		this.ReloadWeapon(a_nIdx, m_bIsEnableAutoEquipNextWeapon);
	}

	/** 무기를 잠근다 */
	public override void LockWeapon(int a_nSlotIdx)
	{
		base.LockWeapon(a_nSlotIdx);

		var stLockInfo = this.LockInfoList[a_nSlotIdx];
		stLockInfo.m_bIsLock = true;
		stLockInfo.m_fRemainTime = stLockInfo.m_fMaxLockTime;

		this.LockInfoList[a_nSlotIdx] = stLockInfo;

		// 현재 사용 무기 일 경우
		if (this.CurWeaponIdx == a_nSlotIdx)
		{
			this.EquipNextWeapon(a_nSlotIdx + 1);
		}
	}

	/** 무기를 재장전한다 */
	public override void ReloadWeapon(int a_nSlotIdx, bool a_bIsAutoEquipNextWeapon = true)
	{
		base.ReloadWeapon(a_nSlotIdx);
		float fReloadInstPercent = UnityEngine.Random.Range(0.0f, 1.0f);

		var stReloadInfo = this.ReloadInfoList[a_nSlotIdx];
		stReloadInfo.m_bIsReload = true;
		stReloadInfo.m_fRemainTime = stReloadInfo.m_fMaxReloadTime;

		// 즉시 재장전 가능 할 경우
		if (fReloadInstPercent.ExIsLess(this.EquipWeapons[a_nSlotIdx].fReloadInstanceChance))
		{
			stReloadInfo.m_fRemainTime = 0.0f;
		}

		this.ReloadInfoList[a_nSlotIdx] = stReloadInfo;

		// 다음 무기 자동 장착 모드 일 경우
		if (a_bIsAutoEquipNextWeapon)
		{
			this.EquipNextWeapon(a_nSlotIdx + 1);
		}
	}

	/** 다음 무기를 장착한다 */
	public void EquipNextWeapon(int a_nSlotIdx)
	{
		for (int i = 0; i < this.MagazineInfoList.Count; ++i)
		{
			int nIdx = (a_nSlotIdx + i) % this.MagazineInfoList.Count;

			bool bIsValid = !this.IsEmptySlot(nIdx);
			bIsValid = bIsValid && this.MagazineInfoList[nIdx].m_nNumBullets >= 1;
			bIsValid = bIsValid && this.ReloadInfoList[nIdx].m_fRemainTime.ExIsLessEquals(0.0f);

			// 무기 장착이 가능 할 경우
			if (bIsValid)
			{
				var oState = this.StateMachine.State as CStateUnitBattle;
				oState?.SetUpdateSkipTime(0.0f);

				this.SetIsAiming(true);
				this.PageBattle.OnReceiveWeaponUIsTouchCallback(null, nIdx);

				return;
			}
		}

		m_oRangeFXHandler.SetIsOn(false);
	}

	/** UI 상태를 갱신한다 */
	protected override void UpdateUIsState(bool a_bIsIncr = false)
	{
		base.UpdateUIsState(a_bIsIncr);
		m_oHPText.text = $"{this.HP}";
	}

	/** 피격을 처리한다 */
	protected override void HandleOnHit(UnitController a_oAttacker,
		Vector3 a_stDirection, STHitInfo a_stHitInfo, bool a_bIsShowDamage)
	{
		// NPC 가 모두 제거 되었을 경우
		if (this.BattleController.NonPlayerControllerList.Count <= 0 || this.StateMachine.State is CStatePlayerBattleSkill)
		{
			a_stHitInfo.m_nDamage = 0;
		}

#if UNITY_EDITOR
		a_stHitInfo.m_nDamage = m_bIsTestUntouchable ? 0 : a_stHitInfo.m_nDamage;
#endif // #if UNITY_EDITOR

		base.HandleOnHit(a_oAttacker, a_stDirection, a_stHitInfo, a_bIsShowDamage);
		this.PageBattle.StartDamageFX();

		// 체력이 존재 할 경우
		if (this.HP > 0)
		{
			return;
		}

		this.SetIsSurvive(false);
		this.SetIsEnableStateMachine(false);

		m_oLockOnFXHandler.gameObject.SetActive(false);

		this.BattleController.CamDummy.SetActive(false);
		this.BattleController.StartClearFailDirecting();
	}

	/** 도탄을 처리한다 */
	protected override bool TryHandleRicochet(ProjectileController a_oSender, Collider a_oCollider)
	{
		base.TryHandleRicochet(a_oSender, a_oCollider);

		var oHitTarget = a_oCollider.GetComponent<UnitController>();
		UnitController oNearestTarget = null;

		float fMinDistance = float.MaxValue / 2.0f;
		float fMaxRicochetDistance = GlobalTable.GetData<int>(ComType.G_RANGE_RICOCHET) * ComType.G_UNIT_MM_TO_M;

		for (int i = 0; i < this.BattleController.NonPlayerControllerList.Count; ++i)
		{
			float fDistance = Vector3.Distance(a_oCollider.transform.position,
				this.BattleController.NonPlayerControllerList[i].transform.position);

			bool bIsValid = oHitTarget != this.BattleController.NonPlayerControllerList[i];

			// 가까운 대상 일 경우
			if (bIsValid && (fDistance.ExIsLess(fMinDistance) && fDistance.ExIsLessEquals(fMaxRicochetDistance)))
			{
				fMinDistance = fDistance;
				oNearestTarget = this.BattleController.NonPlayerControllerList[i];
			}
		}

		// 가까운 대상이 없을 경우
		if (oNearestTarget == null)
		{
			return false;
		}

		a_oSender.Ricochet(oNearestTarget);
		this.PageBattle.StartCameraShakeDirecting(Vector3.zero);

		return true;
	}

	/** 무기 재장전이 완료 되었을 경우 */
	private void OnCompleteReloadWeapon(int a_nSlotIdx)
	{
		var stMagazineInfo = this.MagazineInfoList[a_nSlotIdx];
		stMagazineInfo.m_nNumBullets = stMagazineInfo.m_nMaxNumBullets;

		this.MagazineInfoList[a_nSlotIdx] = stMagazineInfo;

		// 현재 장착 무기 일 경우
		if (a_nSlotIdx == this.CurWeaponIdx)
		{
			this.SetMagazineInfo(stMagazineInfo);
		}

		// 총알이 없을 경우
		if (this.MagazineInfo.m_nNumBullets <= 0 || this.ReloadInfoList[this.CurWeaponIdx].m_bIsReload)
		{
			this.EquipNextWeapon(this.CurWeaponIdx);
		}
	}

	/** 필드 오브젝트 아이템을 획득한다 */
	private void OnAcquireGroundItemFieldObj(uint a_nKey)
	{
		var oTable = FieldObjectTable.GetData(a_nKey);
		var oFXTableList = EffectTable.GetGroup(oTable.GainEffectGroup);

		for (int i = 0; i < oFXTableList.Count; ++i)
		{
			this.OnAcquireGroundItemFieldObj(oFXTableList[i]);
		}
	}

	/** 필드 오브젝트 아이템을 획득한다 */
	private void OnAcquireGroundItemFieldObj(EffectTable a_oTable)
	{
		// 플레이어 대상 효과 일 경우
		if ((ERangeType)a_oTable.RangeType == ERangeType.PLAYER && (EEffectCategory)a_oTable.Category == EEffectCategory.GAIN)
		{
			switch ((EEffectType)a_oTable.Type)
			{
				case EEffectType.GAIN_RECOVERY: this.HandleOnAcquireGroundItemRecovery(a_oTable); break;
			}
		}
	}

	/** 주변 아이템 획득을 시도한다 */
	private void TryAcquireGroundItemsAround()
	{
		var stPos = this.transform.position;
		stPos.y = 0.0f;

		float fRange = GlobalTable.GetData<int>(ComType.G_RANGE_GET_GROUND_ITEM) * ComType.G_UNIT_MM_TO_M;

		for (int i = 0; i < this.BattleController.GroundItemHandlerList.Count; ++i)
		{
			var stItemPos = this.BattleController.GroundItemHandlerList[i].transform.position;
			stItemPos.y = 0.0f;

			bool bIsValid01 = Vector3.Distance(stPos, stItemPos).ExIsLessEquals(fRange) || GameDataManager.Singleton.IsWaveMode();
			bool bIsValid02 = !ComUtil.IsGroundItemFieldObj(this.BattleController.GroundItemHandlerList[i].Key) || this.IsEnableAcquireGroundItemFieldObj(this.BattleController.GroundItemHandlerList[i].Key);

			// 획득 가능 할 경우
			if (bIsValid01 && bIsValid02 && !this.BattleController.GroundItemHandlerList[i].IsAcquire)
			{
				this.BattleController.GroundItemHandlerList[i].SetAcquireTarget(this.gameObject);
			}
		}
	}

	/** 회복 아이템 획득을 처리한다 */
	private void HandleOnAcquireGroundItemRecovery(EffectTable a_oTable)
	{
		int nExtraHP = (int)(this.MaxHP * a_oTable.Value * 0.01f);
		this.SetHP(this.HP + nExtraHP);

		this.UpdateUIsState(true);
		string oPrefabPath = GameResourceManager.Singleton.GetPrefabPath(EResourceType.Effect, a_oTable.PrefabFX);

		// 획득 효과가 존재 할 경우
		if (a_oTable.PrefabFX.ExIsValid() && ComUtil.TryLoadRes<GameObject>(oPrefabPath, out GameObject oFXObj))
		{
			var oParticle = GameResourceManager.Singleton.CreateObject<ParticleSystem>(oFXObj, this.transform, null, ComType.G_LIFE_T_RECOVERY_FX);
			oParticle?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			oParticle?.Play(true);
		}
	}
	#endregion // 함수

	#region 접근 함수
	/** 이동 속도를 반환한다 */
	public float GetMoveSpeed()
	{
		float fMoveSpeedRatio = ComUtil.GetAbilityVal(EEquipEffectType.MoveSpeed, this.CurAbilityValDict);
		return this.MoveSpeed * (1.0f + fMoveSpeedRatio);
	}

	/** 무기 인덱스를 반환한다 */
	public int GetWeaponIdx(ItemWeapon a_oWeapon)
	{
		for (int i = 0; i < _equipWeapon.Length; ++i)
		{
			// 장착 무기 일 경우
			if (_equipWeapon[i] != null && a_oWeapon == _equipWeapon[i])
			{
				return i;
			}
		}

		return -1;
	}

	/** 공격 광선 원점을 반환한다 */
	public override Vector3 GetAttackRayOriginPos()
	{
		return this.transform.position + m_stShootingPointOffset;
	}

	/** 스테이지 인스턴스 레벨을 변경한다 */
	public void SetStageInstLV(int a_nLV)
	{
		this.StageInstLV = Mathf.Min(a_nLV, this.MaxStageInstLV);
		uint nKey = ComUtil.GetInstLevelKey(this.StageInstLV);

		this.InstLevelTable = InstanceLevelTable.GetData(nKey);
		this.MaxAccumulateStageInstEXP = this.InstLevelTable.Exp;

		GameDataManager.Singleton.SetStageInstLV(this.StageInstLV);
	}

	/** 스테이지 누적 인스턴스 경험치를 변경한다 */
	public void SetAccumulateStageInstEXP(float a_fEXP, bool a_bIsImmediate = false)
	{
		this.AccumulateStageInstEXP = a_fEXP;
		float fPercent = this.AccumulateStageInstEXP / this.MaxAccumulateStageInstEXP;

		this.PageBattle.GaugeStageUIsHandler.SetGaugePercent(fPercent, a_bIsImmediate);
		GameDataManager.Singleton.SetAccumulateStageInstEXP(a_fEXP);

		// 레벨 업 상태 일 경우
		if (fPercent.ExIsGreatEquals(1.0f))
		{
			var oInstLevelTable = this.InstLevelTable;
			this.AccumulateStageInstEXP -= this.MaxAccumulateStageInstEXP;

			this.SetStageInstLV(this.StageInstLV + 1);
			this.PageBattle.GaugeStageUIsHandler.SetGaugePercent(0.0f, true);

			float fHPPercent = this.HP / (float)this.MaxHP;

			GameDataManager.Singleton.SetExtraInstHP(GameDataManager.Singleton.ExtraInstHP + this.InstLevelTable.AdditionHP);
			GameDataManager.Singleton.SetExtraInstDEF(GameDataManager.Singleton.ExtraInstDEF + this.InstLevelTable.AdditionDP);

			int nHP = (int)((_characterLevelTable.HP + GameManager.Singleton.user.m_nAdditionalHP) * (1.0f + GameManager.Singleton.user.m_fMaxHPRatio));
			int nDEF = (int)((_characterLevelTable.DP + GameManager.Singleton.user.m_nAdditionalDP) * (1.0f + GameManager.Singleton.user.m_fDefencePowerRatio));

			for (int i = 0; i < this.StandardAbilityValDicts.Length; ++i)
			{
				this.StandardAbilityValDicts[i].ExReplaceVal(EEquipEffectType.HP, nHP + GameDataManager.Singleton.ExtraInstHP);
				this.StandardAbilityValDicts[i].ExReplaceVal(EEquipEffectType.DEF, nDEF + GameDataManager.Singleton.ExtraInstDEF);
			}

			this.SetupAbilityValues(true);
			this.SetHP(Mathf.FloorToInt(this.MaxHP * fHPPercent + this.InstLevelTable.HPRecovery));

			this.SetAccumulateStageInstEXP(this.AccumulateStageInstEXP);
			this.PageBattle.ShowInstEffectPopup(oInstLevelTable);
		}
	}

	/** 빈 슬롯 여부를 검사한다 */
	public bool IsEmptySlot(int a_nSlotIdx)
	{
		return _goEquipWeapon[a_nSlotIdx] == null;
	}

	/** 공격 가능 여부를 검사한다 */
	public override bool IsEnableAttack()
	{
		int nIdx = this.CurWeaponIdx;
		bool bIsValid = base.IsEnableAttack() && !this.IsAttackDelay(nIdx) && !this.IsLockWeapon(nIdx);

		return bIsValid && this.ReloadInfoList[nIdx].m_fRemainTime.ExIsLessEquals(0.0f);
	}

	/** 공격 지연 여부를 검사한다 */
	public bool IsAttackDelay(int a_nSlotIdx)
	{
		bool bIsValid = !this.IsEmptySlot(a_nSlotIdx);
		return bIsValid && this.AttackDelayList[a_nSlotIdx].ExIsGreat(0.0f);
	}

	/** 잠김 슬롯 여부를 검사한다 */
	public bool IsLockWeapon(int a_nSlotIdx)
	{
		bool bIsValid = !this.IsEmptySlot(a_nSlotIdx);
		return bIsValid && this.LockInfoList[a_nSlotIdx].m_fRemainTime.ExIsGreat(0.0f);
	}

	/** 무기 장착 가능 여부를 검사한다 */
	public bool IsEnableEquipWeapon(int a_nSlotIdx)
	{
		bool bIsValid = !this.IsEmptySlot(a_nSlotIdx) && !this.IsLockWeapon(a_nSlotIdx);
		bIsValid = bIsValid && this.ReloadInfoList[a_nSlotIdx].m_fRemainTime.ExIsLessEquals(0.0f);

		return bIsValid && _curSelectWeaponIndex != a_nSlotIdx && this.MagazineInfoList[a_nSlotIdx].m_nNumBullets >= 1;
	}

	/** 무기 재장전 가능 여부를 검사한다 */
	public bool IsEnableReloadWeapon(int a_nSlotIdx)
	{
		bool bIsValid = !this.IsEmptySlot(a_nSlotIdx) && !this.IsLockWeapon(a_nSlotIdx);
		bIsValid = bIsValid && this.ReloadInfoList[a_nSlotIdx].m_fRemainTime.ExIsLessEquals(0.0f);

		return bIsValid && this.MagazineInfoList[a_nSlotIdx].m_nNumBullets < this.MagazineInfoList[a_nSlotIdx].m_nMaxNumBullets;
	}

	/** 공격 가능 여부를 검사한다 */
	public override bool IsAttackableTarget(UnitController a_oController, float a_fAttackRange)
	{
		bool bIsValid01 = a_oController.TrackingTarget != null;
		bool bIsValid02 = this.BattleController.MainCamera.ExIsVisible(a_oController.transform.position);

		return (bIsValid01 || bIsValid02) && base.IsAttackableTarget(a_oController, a_fAttackRange);
	}

	/** 필드 오브젝트 아이템 획득 가능 여부를 검사한다 */
	private bool IsEnableAcquireGroundItemFieldObj(uint a_nKey)
	{
		var oTable = FieldObjectTable.GetData(a_nKey);
		var oFXTableList = CCollectionPoolManager.Singleton.SpawnList<EffectTable>();

		try
		{
			EffectTable.GetGroup(oTable.GainEffectGroup, oFXTableList);

			for (int i = 0; i < oFXTableList.Count; ++i)
			{
				// 획득이 불가능 할 경우
				if (!this.IsEnableAcquireGroundItemFieldObj(oFXTableList[i]))
				{
					return false;
				}
			}

			return true;
		}
		finally
		{
			CCollectionPoolManager.Singleton.DespawnList(oFXTableList);
		}
	}

	/** 필드 오브젝트 아이템 획득 가능 여부를 검사한다 */
	private bool IsEnableAcquireGroundItemFieldObj(EffectTable a_oTable)
	{
		// 플레이어 대상 획득 효과가 아닐 경우
		if ((ERangeType)a_oTable.RangeType != ERangeType.PLAYER || (EEffectCategory)a_oTable.Category != EEffectCategory.GAIN)
		{
			return false;
		}

		switch ((EEffectType)a_oTable.Type)
		{
			case EEffectType.GAIN_RECOVERY: return this.IsEnableAcquireGroundItemRecovery(a_oTable);
		}

		return false;
	}

	/** 회복 아이템 획득을 처리한다 */
	private bool IsEnableAcquireGroundItemRecovery(EffectTable a_oTable)
	{
		return this.HP < this.MaxHP;
	}
	#endregion // 접근 함수
}
#endregion // 추가

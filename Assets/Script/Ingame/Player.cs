using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class Player : MonoBehaviour
{
    public static Player instance = null;

    [SerializeField]
    GameObject _objWeapon;

    [Header("temp")]
    [SerializeField]
    TextMeshProUGUI t1, t2, t3, t4;

    [SerializeField]
    FloatingJoystick floatingJoystick;

    [SerializeField]
    private GameObject _HPGauge;

    Dictionary<GameObject, float> _dicEnemy = new Dictionary<GameObject, float>();

    Rigidbody _rigidbody;
    Animator _animator;

    GameObject _objTarget;
    PageBattle _UIPage;
    Transform _tFire = null;

    Vector3 _vec;
    Vector3 _sDirection;
    Vector3 _tDirection;

    public float _speedFactor = 0.01f;

    int _iCountCol = 0;

    float _hAxis, _vAxis;
    public float _fAttackDealy = 0.5f;
    public float _fCurAttackDealy = 0.1f;
    public float _fAimDelay = 0.5f;
    public float _fCurAimDelay = 0f;

    float distance;

    bool _tWalk, _WeaponSelect0, _WeaponSelect1, _WeaponSelect2, _WeaponSelect3;

    bool _isAir = false;
    bool _isLanding = false;
    bool _isAttack = false;
    bool _isStructure = false;
    bool _isBlind = false;
    bool _isBush = false;

    public void AddEnemy(GameObject Enemy)
    {
        _dicEnemy.Add(Enemy, 0f);
    }

    public void PopEnemy(GameObject Enemy)
    {
        _dicEnemy.Remove(Enemy);
    }

    private void Awake()
    {
        instance = this;

        _fCurAttackDealy = _fAttackDealy;

        _animator = GetComponentInChildren<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
        _UIPage = GameObject.Find("PageBattle").GetComponent<PageBattle>();

        _tFire = _objWeapon.GetComponent<Weapon>().GetFirePosition();

        _HPGauge.GetComponent<HPGauge>().SetCharacterType(HPGauge.GAUGETYPE.player);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Bush")
            ChangeBushState(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Bush")
            ChangeBushState(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Structure")
        {
            _iCountCol += 1;

            if ( _iCountCol == 1 )
            {
                _isLanding = true;
                Invoke("ChangeLandingState", 0.2f);
            }
        }
    }

    void ChangeLandingState()
    {
        _isLanding = !_isLanding;
    }

    void ChangeBushState(bool state)
    {
        _isBush = state;
    }

    public void CheckParam()
    {

    }

    private void OnCollisionExit(Collision collision)
    {
        //if ( collision.contactCount == 0 )
        if ( collision.gameObject.tag == "Structure")
        {
            _iCountCol -= 1;
        }
    }

    private void FixedUpdate()
    {
        _rigidbody.angularVelocity = Vector3.zero;
        _isStructure = Physics.Raycast(transform.position + transform.up * 1f,
                                       transform.forward,
                                       1,
                                       LayerMask.GetMask("Structure"));
    }

    void AutoAttack()
    {
        if (!_isBush && !_isBlind)
        {
            transform.LookAt(_objTarget.transform);
            Invoke("Attack", 0.1f);
        }
    }

    void CheckEnemy()
    {
        if (_dicEnemy == null) return;

        _dicEnemy = _dicEnemy.OrderBy(range => range.Value).ToDictionary(x => x.Key, x => x.Value);
        

        for ( int i = 0;i < _dicEnemy.Count;i++)
        {
            _sDirection = _tFire.position;
            _tDirection = (_dicEnemy.ElementAt(i).Key.transform.position + _tFire.up) - _sDirection;

            distance = Vector3.Distance(_tDirection, _sDirection);

            _isBlind = Physics.Raycast(_sDirection, _tDirection, distance, LayerMask.GetMask("Structure"));

            //if (!_isBlind)
            {
                _objTarget = _dicEnemy.ElementAt(i).Key;
                Debug.DrawRay(_sDirection, _tDirection, Color.red);
                return;
            }
        }

        /*
        if (_dicEnemy.Count > 0)
        {
            foreach (var entry in _dicEnemy.ToList())
            {
                _dicEnemy[entry.Key] = Vector3.Distance(transform.position, entry.Key.transform.position);
            }

            _oTarget = _dicEnemy.Aggregate((x, y) => x.Value < y.Value ? x : y).Key;
        }
        else
        {
            _oTarget = null;
        }
        */

    }

    void PlayerMove()
    {
        if (_hAxis != 0 || _vAxis != 0)
        {
            ResetAttackDelay();
            ResetAimDelay();
        }

        _vec = new Vector3(_hAxis, 0, _vAxis).normalized;

        if ( !_isStructure )
            transform.position += (_vec * _speedFactor * (_tWalk == true ? 0.2f : 1f) * (_isAir == true ? 0.5f : 1f)) * Time.deltaTime;

        transform.LookAt(transform.position + _vec);
    }

    void PlayerControl()
    {
        if (_isLanding || _isAttack) return;

        _hAxis = Input.GetAxisRaw("Horizontal") + floatingJoystick.Horizontal;
        _vAxis = Input.GetAxisRaw("Vertical") + floatingJoystick.Vertical;
        _tWalk = Input.GetButton("Walk");

#if NEVER_USE_THIS
        if (Input.GetButtonDown("Select1")) _UIPage.OnButtonWeaponSeletClick(0);
        if (Input.GetButtonDown("Select2")) _UIPage.OnButtonWeaponSeletClick(1);
        if (Input.GetButtonDown("Select3")) _UIPage.OnButtonWeaponSeletClick(2);
        if (Input.GetButtonDown("Select4")) _UIPage.OnButtonWeaponSeletClick(3);
#endif // #if NEVER_USE_THIS

        //_isAttack = Input.GetButton("Fire1");
    }

    void Attack()
    {
        _fCurAimDelay += Time.deltaTime;
        _fCurAimDelay = Mathf.Min(_fCurAimDelay, _fAimDelay);

        if (_fCurAimDelay >= _fAimDelay && _fCurAttackDealy <= 0)
        {
            _animator.SetTrigger("Shot");
            _objWeapon.GetComponent<Weapon>().Fire(_objTarget);
            _isAttack = false;
            ResetAttackDelay();
        }

        _fCurAttackDealy -= Time.deltaTime;
    }

    void ResetAttackDelay()
    {
        _fCurAttackDealy = _fAttackDealy;
    }

    void ResetAimDelay()
    {
        _fCurAimDelay = 0f;
    }

    void SetAnimState()
    {
        _animator.SetBool("isRun", !_tWalk);
        _animator.SetBool("IsMove", _vec != Vector3.zero);
        _animator.SetBool("_isAir", _isAir);
    }

    private void Update()
    {
        /*
        t1.text = $"caim : {_fCurAimDelay} / aim : {_fAimDelay} / bush : {_isBush}";
        t2.text = $"catt : {_fCurAttackDealy} / att : {_fAttackDealy}";
        if ( _objTarget != null)
            t3.text = $"tar : {_objTarget.name} / blind : {_isBlind}";
        t4.text = $"ait : {_isAir} / col : {_iCountCol}";
        */

        PlayerMove();
        CheckEnemy();
        PlayerControl();
        SetAnimState();

        _isAir = _iCountCol < 1;

        if (_vec == Vector3.zero && _objTarget != null)
            AutoAttack();
    }
}

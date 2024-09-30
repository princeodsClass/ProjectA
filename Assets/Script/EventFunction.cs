using UnityEngine;

public class EventFunction : MonoBehaviour
{
    [SerializeField]
    bool _bUseUpdate, _bUseLateUpdate, _bUseFixedUpdate;

    private void Awake()
    {
        Debug.Log("Object Awake");
    }

    private void Start()
    {
        Debug.Log("Object Start");
    }

    private void OnEnable()
    {
        Debug.Log("Object Enable");
    }

    private void OnDisable()
    {
        Debug.Log("Object Disable");
    }

    private void OnDestroy()
    {
        Debug.Log("Object Destroy");
    }

    private void OnApplicationFocus(bool focus)
    {
        string state = focus ? "lose focus" : "has focus";

        Debug.Log($"Application Focus {state}");
    }

    private void OnApplicationPause(bool pause)
    {
    }

    private void OnApplicationQuit()
    {
    }

    private void Update()
    {
        if (_bUseUpdate)
            Debug.Log("Doing Update");
    }

    private void LateUpdate()
    {
        if (_bUseLateUpdate)
            Debug.Log("Doing Late Update");
    }

    private void FixedUpdate()
    {
        if (_bUseFixedUpdate)
            Debug.Log("Doing Fixed Update");
    }
}
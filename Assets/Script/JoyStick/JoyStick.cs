using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoyStick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField]
    private GameObject circle, dot;
    Touch oneTouch;
    private Vector2 touchPosition;
    private Vector2 moveDirection;
    public float maxRadius;
    [HideInInspector]
    public bool IsTouched;
    [HideInInspector]
    public float Horizontal, Vertical;
    public float speedKoef = 0.02f;
    public bool showJoystick;


    void Start()
    {
        circle.SetActive(false);
        dot.SetActive(false);
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
#if UNITY_EDITOR
        if (eventData.IsPointerMoving())
        {
            touchPosition = Input.mousePosition;
            if (!IsTouched)
            {
                IsTouched = true;
                if (showJoystick) circle.SetActive(true);
                if (showJoystick) dot.SetActive(true);
                circle.transform.position = touchPosition;
                dot.transform.position = touchPosition;
            }
            Move();
        }
#else
            if (eventData.IsPointerMoving() && eventData.pointerId == 0)
            {
                oneTouch = Input.GetTouch(0);
                touchPosition = oneTouch.position;

                if (!IsTouched)
                {
                    IsTouched = true;
                    if(showJoystick) circle.SetActive(true);
                    if(showJoystick) dot.SetActive(true);
                    circle.transform.position = touchPosition;
                    dot.transform.position = touchPosition;
                }
                Move();
            }
#endif
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        IsTouched = false;
        circle.SetActive(false);
        dot.SetActive(false);
    }

    private void Move()
    {
        dot.transform.position = touchPosition;
        dot.transform.localPosition = Vector2.ClampMagnitude(dot.transform.localPosition, maxRadius);
        moveDirection = (dot.transform.position - circle.transform.position) * speedKoef;
        Horizontal = moveDirection.x;
        Vertical = moveDirection.y;
    }
}
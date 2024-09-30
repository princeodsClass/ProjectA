using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class UIMultiScrollRect : ScrollRect, IPointerDownHandler
{
	public System.Action<UIMultiScrollRect, PointerEventData> Callback { get; set; } = null;

    bool routeToParent = false;
    bool isDrag = false;

    float delta = 0f;
    float minScrollDistance = GlobalTable.GetData<int>("valueUIMultyScroolSensitivity"); // = 200f;

    Vector2 _start = default;

    public void OnPointerDown(PointerEventData eventData)
    {
        _start = Input.mousePosition; // eventData.position;
    }

    private void DoForParents<T>(Action<T> action) where T : IEventSystemHandler
    {
        Transform parent = transform.parent;
        while (parent != null)
        {
            foreach (var component in parent.GetComponents<Component>())
            {
                if (component is T)
                    action((T)(IEventSystemHandler)component);
            }
            parent = parent.parent;
        }
    }

    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        DoForParents<IInitializePotentialDragHandler>((parent) => { parent.OnInitializePotentialDrag(eventData); });
        base.OnInitializePotentialDrag(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if ( _start == default ) _start = Input.mousePosition;

        delta = Vector2.Distance(_start, Input.mousePosition);

        if (delta >= minScrollDistance || isDrag)
        {
            isDrag = true;

            if (routeToParent)
                DoForParents<IDragHandler>((parent) => { parent.OnDrag(eventData); });
            else
                base.OnDrag(eventData);
        }
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
            if (!horizontal && Math.Abs(eventData.delta.x) > Math.Abs(eventData.delta.y))
                routeToParent = true;
            else if (!vertical && Math.Abs(eventData.delta.x) < Math.Abs(eventData.delta.y))
                routeToParent = true;
            else
                routeToParent = false;

            if (routeToParent)
                DoForParents<IBeginDragHandler>((parent) => { parent.OnBeginDrag(eventData); });
            else
                base.OnBeginDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (routeToParent)
            DoForParents<IEndDragHandler>((parent) =>
            {
                parent.OnEndDrag(eventData);

                DoParentEvent(eventData);
            });
        else
        {
            base.OnEndDrag(eventData);
        }

		this.Callback?.Invoke(this, eventData);

        routeToParent = false;
        isDrag = false;
        _start = default;
    }

    void DoParentEvent(PointerEventData eventData)
    {
        GameObject.Find("PageLobby").GetComponent<PageLobby>().PageVerticalDrage(eventData);
    }
}
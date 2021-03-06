﻿using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class FixedJoystick : Joystick
{
    Vector2 joystickPosition = Vector2.zero;
    private Camera cam = new Camera();
    

    void Start()
    {
        joystickPosition = RectTransformUtility.WorldToScreenPoint(cam, background.position);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        Vector2 direction = eventData.position - joystickPosition;        
        InputVector = (direction.magnitude > background.sizeDelta.x / 2f) ? direction.normalized : direction / (background.sizeDelta.x / 2f);
        ClampJoystick();
        handle.anchoredPosition = (InputVector * background.sizeDelta.x / 2f) * handleLimit;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        onPointerDown = true;
        OnDrag(eventData);
        StartCoroutine(OnPointerFalse());
    }

    private IEnumerator OnPointerFalse()
    {
        yield return fixedUpdate;
        onPointerDown = false;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        InputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }
}
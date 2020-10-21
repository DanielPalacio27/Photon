using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TurboButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool down { get; private set; }
    PlayerController playerController;

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        down = true;
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        down = false;
    }
}

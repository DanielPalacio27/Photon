using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;

public class Joystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler, IPointerClickHandler, IBeginDragHandler
{
    [Header("Options")]
    [Range(0f, 2f)] public float handleLimit = 1f;
    public JoystickMode joystickMode = JoystickMode.AllAxis;

    private Vector2 inputVector = Vector2.zero;

    [Header("Components")]
    public RectTransform background;
    public RectTransform handle;

    public float Horizontal { get { return InputVector.x; } }
    public float Vertical { get { return InputVector.y; } }
    public Vector2 Direction { get { return new Vector2(Horizontal, Vertical); } }
    public PhotonView pv;


    public Vector2 InputVector { get => inputVector; set => inputVector = value; }
    public bool onPointerDown { get; protected set; }
    protected WaitForFixedUpdate fixedUpdate = new WaitForFixedUpdate();

    public virtual void OnDrag(PointerEventData eventData)
    {

    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {

    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {

    }

    protected void ClampJoystick()
    {
        if (joystickMode == JoystickMode.Horizontal)
            InputVector = new Vector2(InputVector.x, 0f);
        if (joystickMode == JoystickMode.Vertical)
            InputVector = new Vector2(0f, InputVector.y);
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        
    }
}

public enum JoystickMode { AllAxis, Horizontal, Vertical}

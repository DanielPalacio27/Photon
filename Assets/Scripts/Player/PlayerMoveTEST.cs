using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerMoveTEST : MonoBehaviour, IPunObservable
{
    [SerializeField] Joystick joystick;
    float horizontalMove;
    float verticalMove;

    public delegate IEnumerator Handler();
    public static Handler movementDel;

    [Header("Movement Settings")]
    [SerializeField] float force;
    [SerializeField] float maxVelocity;
    [SerializeField] float minVelocity;
    [SerializeField] float timeToBreak;
    [SerializeField] private float rateChange;

    private float currentVelocity;

    Rigidbody mRigid;
    PhotonView pv;

    float t = 0;

    private void Start()
    {
        pv = GetComponent<PhotonView>();
        joystick = FindObjectOfType<Joystick>();
        joystick.pv = pv;
        movementDel = Movement;
        currentVelocity = minVelocity;
        mRigid = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!pv.IsMine)
        {
            mRigid.position = Vector3.MoveTowards(mRigid.position, networkPosition, Time.fixedDeltaTime);
            mRigid.rotation = Quaternion.RotateTowards(mRigid.rotation, networkRotation, Time.fixedDeltaTime * 100.0f);
        }
    }

    private void Update()
    {
        if (pv.IsMine)
        {
            if (joystick.onPointerDown)
            {
                StartCoroutine(Movement());
            }            
        }
    }

    public IEnumerator Movement()
    {
        float t = 0;
        currentVelocity = minVelocity;

        while (joystick.InputVector.magnitude > 0)
        {
            t += Time.deltaTime;
            currentVelocity += t / rateChange;
            currentVelocity = Mathf.Clamp(currentVelocity, minVelocity, maxVelocity);
            horizontalMove = joystick.Horizontal;
            verticalMove = joystick.Vertical;
            Vector3 direction = new Vector3(horizontalMove, 0, verticalMove) * Time.deltaTime * force;
            mRigid.AddForce(direction, ForceMode.VelocityChange);
            mRigid.velocity = Vector3.ClampMagnitude(mRigid.velocity, currentVelocity);
            yield return null;
        }

        //timeToBreak = Mathf.Exp(currentVelocity / rateChange) / 2f;
        //t = 0;
        //while (t <= timeToBreak)
        //{
        //    if (joystick.onPointerDown)
        //        yield break;

        //    t += Time.deltaTime;
        //    mRigid.velocity = Vector3.Lerp(mRigid.velocity, Vector3.zero, t / timeToBreak);
        //    yield return null;
        //}
    }


    Vector3 networkPosition;
    Quaternion networkRotation;

    public Vector3 RemotePlayerPosition { get; private set; }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(mRigid.position);
            stream.SendNext(mRigid.rotation);
            stream.SendNext(mRigid.velocity);
        }
        else
        {
            mRigid.position = (Vector3)stream.ReceiveNext();
            mRigid.rotation = (Quaternion)stream.ReceiveNext();
            mRigid.velocity = (Vector3)stream.ReceiveNext();

            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.timestamp));
            mRigid.position += mRigid.velocity * lag;
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, IPunObservable
{
    [SerializeField] Joystick joystick;
    [SerializeField] Button  jumpButton;

    [SerializeField] TurboButton turboButton;
    [SerializeField] RectTransform turboFill;
    [SerializeField] Vector2 turboFillInitialPos;
    [SerializeField] float fillLimit;
    bool chargingTurbo;

    float horizontalMove;
    float verticalMove;

    [Header("Movement Settings")]
    [SerializeField] private float currentVelocity;
    [SerializeField] float movementForce;
    [SerializeField] float maxVelocity, maxAirVelocity;
    [SerializeField] float minVelocity;
    [SerializeField] float timeToBreak;
    [SerializeField] private float rateChangeVel;
    [SerializeField] private float rateChangeBreak;
    private bool limitMove, isOnFloor, hasTurbo;
    float tMove = 0;
    float tBrake = 0;

    [Header("Turbo")]
    [SerializeField] float turboForce;
    [SerializeField] float jumpForce;
    [SerializeField] float timeToSlow;
    [SerializeField] float timeToMaxTurbo;
    Vector3 turbo;

    float auxMaxVelocity;

    Coroutine movementCor = null;
    Coroutine slowDownCor = null;

    Rigidbody mRigid;
    Collider mCollider;
    PhotonView pv;

    Vector3 networkPosition;
    Quaternion networkRotation;

    //Pendiente
    [SerializeField] float y;
    float m = -1f;
    float x = 0;
    float b = 30f;

    //VFX
    [SerializeField] ParticleSystem runParticles;

    //JumpCoroutine
    WaitWhile isntGrounded;
    WaitForSeconds flagWait = new WaitForSeconds(1f);

    WaitForSeconds slowDownWait;
    WaitForSeconds waitForMoveAfterColl = new WaitForSeconds(0.2f);
    WaitWhile rigidVelYzero;

    GameObject cameraObj;
    CameraFollow camFoll;

    private void Awake()
    {

        isOnFloor = true;
        auxMaxVelocity = maxVelocity;
        pv = GetComponent<PhotonView>();
        joystick = FindObjectOfType<Joystick>();
        joystick.pv = pv;

        if(pv.IsMine)
        {
            cameraObj = new GameObject("Camera of player:   " + transform.gameObject.name);
            cameraObj.AddComponent<Camera>();
            camFoll = cameraObj.AddComponent<CameraFollow>();
            camFoll.pv = pv;
            camFoll.target = transform;
        }

        //CameraFollow.pv = pv;
        //CameraFollow.cam = transform.GetChild(1);

        movementForce = 90;
        turboButton = GameObject.Find("FireButt").GetComponent<TurboButton>();
        turboFill = turboButton.transform.GetChild(0).GetComponent<RectTransform>();
        turboFillInitialPos = turboFill.localPosition;
        fillLimit = turboButton.GetComponent<RectTransform>().rect.height;
        turboFillInitialPos = new Vector2(0, -fillLimit);
        turboFill.localPosition = turboFillInitialPos;

        jumpButton = GameObject.Find("JumpButt").GetComponent<Button>();
        currentVelocity = minVelocity;
        mRigid = GetComponent<Rigidbody>();
        mCollider = GetComponent<Collider>();
        //turboButton.onClick.AddListener(() => Turbo());
        jumpButton.onClick.AddListener(() => StartCoroutine(Jump()));

        runParticles.Pause();

        isntGrounded = new WaitWhile(() => !IsGrounded());
        slowDownWait = new WaitForSeconds(timeToSlow);
        rigidVelYzero = new WaitWhile(() => Mathf.Round(mRigid.velocity.y) != 0);
        timeToMaxTurbo = 2f;
    }

    float yVel = 0, mVel = 0.33f;

    public void Turbo()
    {
        if (pv.IsMine)
        {
            hasTurbo = true;
            yVel = mVel * turboForce + 10; //Rect Equation to limit velocity when is applied a turbo Force
            maxVelocity = yVel;
            currentVelocity = yVel;

            if (Mathf.Abs(joystick.InputVector.magnitude) > 0)
            {
                turbo = new Vector3(horizontalMove * 10, 0, verticalMove * 10).normalized * turboForce;
                mRigid.AddForce(turbo, ForceMode.VelocityChange);
            }
            else if (joystick.InputVector.magnitude == 0)
            {
                //Se aplica la fuerza dependiendo de la velocidad que lleve una vez dejo de moverse
                x = Mathf.Round(mRigid.velocity.magnitude);
                y = m * x + b;
                turbo = mRigid.velocity.normalized * y;
                mRigid.AddForce(turbo, ForceMode.VelocityChange);
            }

            limitMove = true;
            StartCoroutine(SlowDown());
        }

    }

    void ClampVelocity()
    {
        if (isOnFloor)
        {
            if (runParticles.isStopped) pv.RPC("PlayParticles", RpcTarget.AllBuffered);
            mRigid.velocity = Vector3.ClampMagnitude(mRigid.velocity, currentVelocity);
        }
        else
        {
            mRigid.velocity = Vector3.ClampMagnitude(mRigid.velocity, 30);
        }
    }

    IEnumerator SlowDown()
    {
        yield return slowDownWait;
        ClampVelocity();
        limitMove = false;
        yield return slowDownWait;
        hasTurbo = false;
        if (joystick.InputVector.magnitude == 0) StartCoroutine(Brake(1));
        yield return null;
    }

    private void Update()
    {
        if (pv.IsMine)
        {
            if (joystick.onPointerDown)
                movementCor = StartCoroutine(Movement());

            if (turboButton.down && !chargingTurbo)
                StartCoroutine(Turbinho());

            //if (Input.GetKeyDown(KeyCode.C) && IsGrounded()) StartCoroutine(Jump());
        }

        //OFFLINE TEST
        /*
        if (joystick.onPointerDown)
        {
            movementCor = StartCoroutine(Movement());
        }
        */
        /*
        if (joystick.InputVector.magnitude > 0)
        {
            if (!move)
            {
                move = true;
                movementCor = StartCoroutine(Movement());
            }
        }
        */
        
    }

    float yTForce = 0, mTForce = 10, bTForce = 40;
    private IEnumerator Turbinho()
    {
        float t = 0;
        chargingTurbo = true;
        while(turboButton.down)
        {
            t += Time.deltaTime;

            yTForce = mTForce * t + bTForce;

            if (t > timeToMaxTurbo)
            {
                //break;
            }

            turboFill.localPosition = Vector2.Lerp(turboFillInitialPos, Vector2.zero, t / timeToMaxTurbo);

            yield return null;
        }

        turboForce = yTForce;
        Turbo();       
        t = 0;
        while(Mathf.Round(turboFill.localPosition.y) != Mathf.Round(-fillLimit))
        {
            t += Time.deltaTime;
            print(Mathf.Round(turboFill.localPosition.y));

            turboFill.localPosition = Vector2.Lerp(turboFill.localPosition, turboFillInitialPos, t / 1);
            yield return null;
        }

        chargingTurbo = false;
        //yield return new WaitWhile(() => turboButton.down == true);
        print("Fue true");
    }

    private IEnumerator Jump()
    {
        if (pv.IsMine)
        {
            isOnFloor = false;
            runParticles.Stop();
            yield return new WaitForFixedUpdate();
            Vector3 direction = new Vector3(horizontalMove * 20, jumpForce, verticalMove * 20) * Time.deltaTime;
            mRigid.AddForce(direction, ForceMode.VelocityChange);
            yield return flagWait;
            yield return isntGrounded;
            if (joystick.InputVector.magnitude == 0) StartCoroutine(Brake(40));
            runParticles.Play();
            isOnFloor = true;
        }
    }

    [PunRPC]
    public IEnumerator Movement()
    {
        tMove = 0;
        currentVelocity = minVelocity;
        maxVelocity = auxMaxVelocity;
        if (pv.IsMine)
        {
            if (isOnFloor) pv.RPC("PlayParticles", RpcTarget.AllBuffered); //runParticles.Play();
        }

        while (joystick.InputVector.magnitude > 0 )
        {
            if (!limitMove)
            {
                //if (isOnFloor)
                //{
                //    if (runParticles.isStopped) pv.RPC("PlayParticles", RpcTarget.AllBuffered);
                //    mRigid.velocity = Vector3.ClampMagnitude(mRigid.velocity, currentVelocity);
                //}
                //else
                //{
                //    mRigid.velocity = Vector3.ClampMagnitude(mRigid.velocity, 30);
                //}
                ClampVelocity();

                tMove += Time.fixedDeltaTime;
                currentVelocity += tMove / rateChangeVel;
                currentVelocity = Mathf.Clamp(currentVelocity, minVelocity, maxVelocity);
                horizontalMove = joystick.Horizontal;
                verticalMove = joystick.Vertical;
                Vector3 direction = new Vector3(horizontalMove, 0, verticalMove) * Time.deltaTime * movementForce;
                //pv.RPC("ForceAfterContact", RpcTarget.AllBuffered, direction);

                mRigid.AddForce(direction, ForceMode.VelocityChange);
            }
            yield return new WaitForFixedUpdate();
        }
        StartCoroutine(Brake(5));
    }

    [PunRPC]
    void PlayParticles()
    {
        runParticles.Play();
    }

    IEnumerator Brake(float factor)
    {
        timeToBreak = (Mathf.Exp(currentVelocity / rateChangeBreak)) * factor;
        tBrake = 0;
        while (tBrake <= timeToBreak)
        {
            if (joystick.onPointerDown || hasTurbo)
                yield break;

            if (Mathf.Round(mRigid.velocity.magnitude) == 0)//&& Mathf.Round(mRigid.velocity.y) != 0)
            {
                mRigid.velocity = Vector3.zero;
                yield break;
            }
            tBrake += Time.fixedDeltaTime;
            mRigid.velocity = Vector3.Lerp(mRigid.velocity, Vector3.zero, tBrake / timeToBreak);
            yield return new WaitForFixedUpdate();
            yield return rigidVelYzero;
        }
    }

    private IEnumerator OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.tag == "Player")
        {
            Rigidbody otherRigidbody = collision.gameObject.GetComponent<Rigidbody>();
            float velocityDiferrence = Mathf.Abs(mRigid.velocity.magnitude - otherRigidbody.velocity.magnitude);
            ContactPoint contactPoint = collision.contacts[0];
            tBrake = 200f;
            limitMove = true;
            Vector3 force = contactPoint.normal.normalized * (30f - currentVelocity);

            //mRigid.AddForce(force, ForceMode.VelocityChange);
            pv.RPC("ForceAfterContact", RpcTarget.AllBuffered, force);

            yield return waitForMoveAfterColl;
            currentVelocity = maxVelocity;
            StartCoroutine(Brake(5));
            limitMove = false;
            yield break;

        }
    }

    [PunRPC]
    private void ForceAfterContact(Vector3 _Force)
    {
        mRigid.AddForce(_Force, ForceMode.VelocityChange);
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 2f);
    }

    Vector3 syncEndPosition, syncStartPosition;
    float syncTime = 0f, syncDelay, lastSyncTime;
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(this.mRigid.position);
            stream.SendNext(this.mRigid.rotation);
            stream.SendNext(this.mRigid.velocity);
        }
        else
        {
            //Vector3 syncPos = (Vector3)stream.ReceiveNext();
            //Vector3 syncVelocity = (Vector3)stream.ReceiveNext();

            //syncTime = 0f;
            //syncDelay = Time.time - lastSyncTime;
            //lastSyncTime = Time.time;

            //syncEndPosition = syncPos + syncVelocity * syncDelay;
            //syncStartPosition = mRigid.position;



            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            mRigid.velocity = (Vector3)stream.ReceiveNext();

            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTimestamp));
            networkPosition += (mRigid.velocity * lag);
        }
    }

    public void FixedUpdate()
    {
        if (!pv.IsMine)
        {
            //SyncedMovement();
            mRigid.position = Vector3.MoveTowards(mRigid.position, networkPosition, Time.fixedDeltaTime);
            mRigid.rotation = Quaternion.RotateTowards(mRigid.rotation, networkRotation, Time.fixedDeltaTime * 100.0f);
        }
    }

    private void SyncedMovement()
    {
        syncTime += Time.fixedDeltaTime;
        mRigid.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
    }
}

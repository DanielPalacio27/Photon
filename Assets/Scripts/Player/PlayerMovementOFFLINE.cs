using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public enum PlayerNumber
{ uno = 1, dos, tres, cuatro}

public class PlayerMovementOFFLINE : MonoBehaviour
{

    [SerializeField] Joystick joystick;
    [SerializeField] Button turboButton;
    [SerializeField] RectTransform turboSlider;
    float horizontalMove;
    float verticalMove;

    [SerializeField] PlayerNumber playerEnum;
    int playerNumInt;
    string playerNumStr;

    public delegate IEnumerator Handler();
    public static Handler movementDel;

    [Header("Movement Settings")]
    [SerializeField] private float currentVelocity;
    [SerializeField] float force;
    [SerializeField] float maxVelocity, maxAirVelocity;
    [SerializeField] float minVelocity;
    [SerializeField] float timeToBreak;
    [SerializeField] private float rateChangeVel;
    [SerializeField] private float rateChangeBreak;
    private bool move, limitMove, isOnFloor, hasTurbo;
    private float axisInput;
    float tMove = 0;
    float tBrake = 0;

    [Header("Turbo")]
    [SerializeField] float turboForce;
    [SerializeField] float jumpForce;
    [SerializeField] float timeToSlow;
    Vector3 turbo;

    float auxMaxVelocity;
    
    Coroutine movementCor = null;
    Coroutine slowDownCor = null;

    Rigidbody mRigid;
    Collider mCollider;
    PhotonView pv;

    //Pendiente
    [SerializeField] float y;
    float m = -1f;
    float x = 0;
    float b = 30f;

    //VFX
    ParticleSystem runParticles;

    //JumpCoroutine
    WaitWhile isntGrounded;
    WaitForSeconds flagWait = new WaitForSeconds(1f);

    WaitForSeconds slowDownWait;
    WaitForSeconds waitForMoveAfterColl = new WaitForSeconds(0.2f);

    WaitWhile rigidVelYzero;

    private void Start()
    {
        playerNumInt = (int)playerEnum;
        playerNumStr = playerNumInt.ToString();

        isOnFloor = true;
        auxMaxVelocity = maxVelocity;
        pv = GetComponent<PhotonView>();
        //joystick = FindObjectOfType<Joystick>();
        //joystick.pv = pv;
        //turboButton = GameObject.Find("FireButt").GetComponent<Button>();
        movementDel = Movement;
        currentVelocity = minVelocity;
        mRigid = GetComponent<Rigidbody>();
        mCollider = GetComponent<Collider>();
        //turboButton.onClick.AddListener(() => Turbo());
        //turboButton = FindObjectOfType<TurboButton>();

        runParticles = GetComponentInChildren<ParticleSystem>();
        //runParticles.emission.enabled = false;

        isntGrounded = new WaitWhile(() => !IsGrounded());
        slowDownWait = new WaitForSeconds(timeToSlow);
        rigidVelYzero = new WaitWhile(() => Mathf.Round(mRigid.velocity.y) != 0);
    }

    float yTurbo = 0, mTurbo = 0.33f;

    public void Turbo()
    {        
        if (pv.IsMine)
        {
            maxVelocity = turboForce - 5f;
            currentVelocity = turboForce - 5f;
            rateChangeBreak = 6.5f;
            turbo = new Vector3(joystick.Horizontal * 10, 0, joystick.Vertical * 10).normalized * turboForce;
            mRigid.AddForce(turbo,ForceMode.Impulse);
            limitMove = true;
            StartCoroutine(SlowDown());
        }

        //KEYBOARD TEST
        //maxVelocity = turboForce - 30f;
        //currentVelocity = turboForce - 30f;

        yTurbo = mTurbo * turboForce + 10;
        maxVelocity = yTurbo;
        currentVelocity = yTurbo;

        hasTurbo = true;
        if (axisInput > 0)
        {
            turbo = new Vector3(horizontalMove * 10, 0, verticalMove * 10).normalized * turboForce;
            mRigid.AddForce(turbo, ForceMode.VelocityChange);

        }
        else if (axisInput == 0)
        {
            x = Mathf.Round(mRigid.velocity.magnitude);
            b = turboForce;
            y = m * x + b;
            turbo = mRigid.velocity.normalized * y;
            mRigid.AddForce(turbo, ForceMode.VelocityChange);
        }

        limitMove = true;
        StartCoroutine(SlowDown());
    }

    IEnumerator SlowDown()
    {
        yield return slowDownWait;
        ClampVelocity();
        limitMove = false;
        yield return slowDownWait;
        hasTurbo = false;
        if(axisInput == 0) StartCoroutine(Brake(1f));
        yield return null;
    }

    private void Update()
    {

        if (pv.IsMine)
        {
            if (joystick.onPointerDown )
            {
                movementCor = StartCoroutine(Movement());
            }          
        }

        //  mRigid.angularVelocity = Vector3.zero;
        //OFFLINE TEST
        //if (joystick.onPointerDown)
        //{
        //    movementCor = StartCoroutine(Movement());
        //}

        axisInput = Mathf.Abs(Input.GetAxisRaw("Horizontal" + playerNumStr)) + Mathf.Abs(Input.GetAxisRaw("Vertical" + playerNumStr));
        if (Input.GetButtonUp("Fire" + playerNumStr))
        {
            Turbo();
        }

        if (axisInput > 0)
        {             
            if (!move)
            {
                move = true;
                movementCor = StartCoroutine(Movement());
            }           
        }      

        if(Input.GetKeyDown(KeyCode.C) && IsGrounded())
        {
            StartCoroutine(Jump());            
        }
    }

    private IEnumerator Jump()
    {
        isOnFloor = false;
        runParticles.Stop();
        yield return new WaitForFixedUpdate();
        Vector3 direction = new Vector3(horizontalMove * 20, jumpForce, verticalMove * 20);
        mRigid.AddForce(direction * Time.deltaTime, ForceMode.VelocityChange);
        //mRigid.AddForce(Vector3.up * jumpForce * Time.deltaTime, ForceMode.VelocityChange);
        yield return flagWait;
        yield return isntGrounded;
        if (axisInput == 0) StartCoroutine(Brake(40));
        runParticles.Play();
        isOnFloor = true;
    }
    
    void ClampVelocity()
    {
        if (IsGrounded())
            mRigid.velocity = Vector3.ClampMagnitude(mRigid.velocity, currentVelocity);
        else
            mRigid.velocity = Vector3.ClampMagnitude(mRigid.velocity, 30);
    }

    public IEnumerator Movement()
    {
        tMove = 0;
        currentVelocity = minVelocity;
        maxVelocity = auxMaxVelocity;
        if (isOnFloor) runParticles.Play();

        while (move)//(joystick.InputVector.magnitude > 0 || move)
        {
            if (!limitMove)
            {
                //KEYBOARD TEXT
                if (axisInput == 0)
                    move = false;

                ClampVelocity();
                //if (isOnFloor)
                //    mRigid.velocity = Vector3.ClampMagnitude(mRigid.velocity, currentVelocity);
                //else
                //    mRigid.velocity = Vector3.ClampMagnitude(mRigid.velocity, 30);

                tMove += Time.deltaTime;
                currentVelocity += tMove / rateChangeVel;
                currentVelocity = Mathf.Clamp(currentVelocity, minVelocity, maxVelocity);
                //KEYBOARD TEST
                horizontalMove = Input.GetAxisRaw("Horizontal" + playerNumStr);
                verticalMove = Input.GetAxisRaw("Vertical" + playerNumStr);
                Vector3 direction = new Vector3(horizontalMove, 0, verticalMove) * Time.deltaTime * force;
                mRigid.AddForce(direction, ForceMode.VelocityChange);
                //INPUT
                //horizontalMove = joystick.Horizontal;
                //verticalMove = joystick.Vertical;               
            }
            yield return null;
        }        
        StartCoroutine(Brake(1));       
    }
    
    IEnumerator Brake(float factor)
    {
        timeToBreak = (Mathf.Exp(currentVelocity / rateChangeBreak) / 1.5f) * factor;
        tBrake = 0;
        while (tBrake <= timeToBreak)
        {
            if (axisInput > 0 || hasTurbo)//(joystick.onPointerDown || axisInput > 0 || hasTurbo)
                yield break;

            if (Mathf.Round(mRigid.velocity.magnitude) == 0 )
            {
                mRigid.angularVelocity = Vector3.zero;
                mRigid.velocity = Vector3.zero;
                yield break;
            }
            tBrake += Time.deltaTime;
            mRigid.velocity = Vector3.Lerp(mRigid.velocity, Vector3.zero, tBrake / timeToBreak);
            yield return rigidVelYzero;
        }
    }

    private IEnumerator OnCollisionEnter(Collision collision)
    {
        
        if(collision.gameObject.tag == "Player")
        {
            Rigidbody otherRigidbody = collision.gameObject.GetComponent<Rigidbody>();
            float velocityDiferrence = Mathf.Abs(mRigid.velocity.magnitude - otherRigidbody.velocity.magnitude);
            ContactPoint contactPoint = collision.contacts[0];
            tBrake = 100f;
            limitMove = true;
            Vector3 force = contactPoint.normal.normalized * (30f - currentVelocity);
            print(gameObject.name + "  force \n " + force.magnitude);

            mRigid.AddForce(force, ForceMode.VelocityChange);
            yield return waitForMoveAfterColl;
            currentVelocity = maxVelocity;
            StartCoroutine(Brake(1));
            limitMove = false;
            yield break;
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 2f);
    }
}

using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] public PhotonView pv;
    public Transform target;
    float yOffset, zOffset;
    //Transform mTransform;

    private void Awake()
    {
        yOffset = 9f;
        zOffset = -6f;
        transform.eulerAngles = new Vector3(45, 0, 0);
        StartCoroutine(WaitForAddPv());
    }

    private void LateUpdate()
    {
        transform.position = new Vector3(target.position.x, target.position.y + yOffset, target.position.z + zOffset);
    }

    IEnumerator WaitForAddPv()
    {
        yield return new WaitUntil(() => pv != null);
        if (!pv.IsMine)
            gameObject.SetActive(false);
    }
}

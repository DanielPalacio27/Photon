using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RunParticlesRot : MonoBehaviour, IPunObservable
{
    PhotonView pv;
    Vector3 networkPosition;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(transform.localPosition);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
        }
    }

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    private void LateUpdate() {

        if (pv.IsMine)
        {
            transform.localPosition = Vector3.zero;
        }
        else
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, networkPosition, Time.deltaTime);
        }
    }

}

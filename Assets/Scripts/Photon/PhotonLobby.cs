using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PhotonLobby : MonoBehaviourPunCallbacks
{
    [SerializeField] Button playButton;
    [SerializeField] Button cancelButton;
    
    private void Start()
    {
        playButton.interactable = false;
        cancelButton.gameObject.SetActive(false);
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        playButton.interactable = true;       
    }

    public void OnPlayButton()
    {
        playButton.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(true);
        PhotonNetwork.JoinRandomRoom();
        UIMenu.instance.ActivateCounterObj();
    }

    public void OnCancelButton()
    {        
        playButton.gameObject.SetActive(true);
        cancelButton.gameObject.SetActive(false);
        PhotonRoom.room.Start();
        UIMenu.instance.DesactivateCounterObj();
        PhotonNetwork.LeaveRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateRoom();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        CreateRoom();
    }

    public void CreateRoom()
    {
        int randomRoomName = Random.Range(0, 10000);
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = MultiplayerSettings.instance.maxPlayers };
        PhotonNetwork.CreateRoom("Room: " + randomRoomName, roomOps);
    }
}

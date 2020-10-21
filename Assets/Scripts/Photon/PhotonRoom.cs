using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhotonRoom : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
    public static PhotonRoom room = null;
    [SerializeField] private PhotonView pv;

    private Player[] photonPlayers;
    private int playersInRoom;
    private int indexAssignPos;
    public int myNumberInRoom;

    private int currentScene;
    public bool isGameLoaded;
    public int playerInGame;

    //delayed start
    private bool readyToCount;
    private bool readyToStart;
    public float startingTime;
    private float leesThanMaxPlayers;
    private float atMaxPlayers;
    public float timeToStart;
    private float normalizedTime2Start;

    void Awake()
    {
        if(room == null)
        {
            room = this;
        }
        else if(room != this)
        {            
            Destroy(gameObject);
            room = this;
        }

        DontDestroyOnLoad(gameObject);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
        SceneManager.sceneLoaded += OnSceneFinishedLoading;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
        SceneManager.sceneLoaded -= OnSceneFinishedLoading;
    }

    public void Start()
    {
        pv = GetComponent<PhotonView>();
        readyToCount = false;
        readyToStart = false;
        leesThanMaxPlayers = startingTime;
        atMaxPlayers = MultiplayerSettings.instance.maxPlayers;
        timeToStart = startingTime;
    }

    void Update()
    {
        if(MultiplayerSettings.instance.delayStart)
        {
            if (playersInRoom == 1)
                RestartTimer();
            if(!isGameLoaded)
            {
                if(readyToStart)
                {
                    atMaxPlayers -= Time.deltaTime;
                    leesThanMaxPlayers = atMaxPlayers;
                    timeToStart = atMaxPlayers;
                }
                else if(readyToCount)
                {
                    leesThanMaxPlayers -= Time.deltaTime;
                    timeToStart = leesThanMaxPlayers;
                }

                normalizedTime2Start = 1 - (timeToStart / startingTime);
                UIMenu.instance.countdownCircle.fillAmount = normalizedTime2Start;
                UIMenu.instance.playersNumber.text = playersInRoom + "/" + MultiplayerSettings.instance.maxPlayers;

                if (timeToStart <= 0)
                    StartGame();
            }
        }
    }

    private void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        currentScene = scene.buildIndex;
        if (currentScene == MultiplayerSettings.instance.multiplayerScene)
        {
            isGameLoaded = true;
            if (MultiplayerSettings.instance.delayStart)
            {
                pv.RPC("RPC_LoadedGameScene", RpcTarget.MasterClient);
            }
            else
            {
                RPC_InstancePlayer();
            }
        }
    }

    private void StartGame()
    {
        isGameLoaded = true;
        if (!PhotonNetwork.IsMasterClient)
            return;
        if (MultiplayerSettings.instance.delayStart)
            PhotonNetwork.CurrentRoom.IsOpen = false;


        //if (PhotonNetwork.CurrentRoom.PlayerCount == MultiplayerSettings.instance.maxPlayers)
        //  PhotonNetwork.LoadLevel(MultiplayerSettings.instance.multiplayerScene);

        PhotonNetwork.LoadLevel(MultiplayerSettings.instance.multiplayerScene);
    }

    private void RestartTimer()
    {
        leesThanMaxPlayers = startingTime;
        timeToStart = startingTime;
        atMaxPlayers = MultiplayerSettings.instance.maxPlayers;
        readyToCount = false;
        readyToStart = false;
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        photonPlayers = PhotonNetwork.PlayerList;
        playersInRoom = photonPlayers.Length;
        myNumberInRoom = playersInRoom;

        if(MultiplayerSettings.instance.delayStart)
        {
            if (playersInRoom > 1)
                readyToCount = true;
            if(playersInRoom == MultiplayerSettings.instance.maxPlayers)
            {
                readyToStart = true;
                if (!PhotonNetwork.IsMasterClient)
                    return;
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
        }
        else
        {
            StartGame();
        }

    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        photonPlayers = PhotonNetwork.PlayerList;
        playersInRoom++;
        UIMenu.instance.playersNumber.text = playersInRoom + "/" + MultiplayerSettings.instance.maxPlayers;

        if (PhotonNetwork.IsMasterClient)
            pv.RPC("RPC_SyncTimer", RpcTarget.Others, timeToStart);

        if (MultiplayerSettings.instance.delayStart)
        {
            if (playersInRoom > 1)
                readyToCount = true;

            if (playersInRoom == MultiplayerSettings.instance.maxPlayers)
            {
                readyToStart = true;
                if (!PhotonNetwork.IsMasterClient)
                    return;
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        playersInRoom--;
    }

    [PunRPC]
    private void RPC_InstancePlayer()
    {
        indexAssignPos++;
        PhotonNetwork.Instantiate(Path.Combine("Prefabs", "BallPlayer"), RandomPoints.instance.resultOfChooseSet[indexAssignPos-1].position, Quaternion.identity);
    }

    [PunRPC]
    private void RPC_LoadedGameScene()
    {
        playerInGame++;
        if (playerInGame == PhotonNetwork.PlayerList.Length)
        {
            pv.RPC("RPC_InstancePlayer", RpcTarget.All);
        }
    }
    [PunRPC]
    private void RPC_SyncTimer(float _time)
    {
        timeToStart = _time;
        leesThanMaxPlayers = _time;
        if(_time < atMaxPlayers)
        {
            atMaxPlayers = _time;
        }

    }

}

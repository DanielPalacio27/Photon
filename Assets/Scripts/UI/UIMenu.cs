using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class UIMenu : MonoBehaviour
{
    public static UIMenu instance;

    public Text playersNumber;
    public Image countdownCircle;
    [SerializeField] PhotonView pv;

    [Header("WINDOWS")]
    [SerializeField] Transform mainMenu;
    [SerializeField] Transform skinsWindow;
    [SerializeField] Transform creditsWindow;
    [SerializeField] Transform settingsWindow;     

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        playersNumber.text = string.Empty;
        playersNumber.gameObject.SetActive(false);
        countdownCircle.gameObject.SetActive(false);
        pv = GetComponent<PhotonView>();
    }

    public void ActivateCounterObj()
    {
        playersNumber.gameObject.SetActive(true);
        countdownCircle.gameObject.SetActive(true);
    }

    public void DesactivateCounterObj()
    {
        playersNumber.text = string.Empty;
        countdownCircle.fillAmount = 0;
        playersNumber.gameObject.SetActive(false);
        countdownCircle.gameObject.SetActive(false);
    }

    public void SkinsWindow()
    {
        mainMenu.gameObject.SetActive(false);
        skinsWindow.gameObject.SetActive(true);
    }

    public void MainMenu()
    {
        mainMenu.gameObject.SetActive(true);
        skinsWindow.gameObject.SetActive(false);
    }

}

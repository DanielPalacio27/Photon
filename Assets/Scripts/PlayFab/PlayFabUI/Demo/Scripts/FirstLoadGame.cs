using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab.ClientModels;
using PlayFab;

public class FirstLoadGame : MonoBehaviour
{

    public delegate void Action();
    public delegate void Actioni<in T1>(T1 arg1);
    public delegate void Action<in T1, in T2>(T1 arg1, T2 arg2);

    public delegate TResult Func<out TResult>();
    public delegate TResult Func<in T1, out TResult>(T1 arg1);
    public delegate TResult Func<in T1, in T2, out TResult>(T1 arg1, T2 arg2);

    public delegate bool Predicate();
    public delegate bool Predicate<in T>(T obj);

    [SerializeField] InputField userNameInput;
    [SerializeField] GameObject userNamePanel;
    public string username;
    [SerializeField] Button okButt;
    private string firstRunKey = "FirstRun";
    [SerializeField] bool deleteFirstRunKey;


    public void Awake()
    {

        if (deleteFirstRunKey) PlayerPrefs.DeleteKey(firstRunKey);

        userNamePanel.SetActive(PlayerPrefs.HasKey(firstRunKey) ? false : new Func<bool>(() =>
        {
            PlayerPrefs.SetInt(firstRunKey, 1);
            return true;
        })());

        userNameInput.onValueChanged.AddListener(UsernameInput);
        okButt.onClick.AddListener(SetUserName);
    }

    public void UsernameInput(string value)
    {       
        username = value;
    }

    public void SetUserName()
    {
        UpdateUserTitleDisplayNameRequest nameRequest = new UpdateUserTitleDisplayNameRequest() { DisplayName = username };        
        
        PlayFabClientAPI.UpdateUserTitleDisplayName(nameRequest, 
            (result) =>
            {                
                Debug.Log("Succesful change of username: " + result.DisplayName);

            },(error) => 
            {                
                Debug.Log("Error at change name: " + error.ErrorMessage);
            });

        userNamePanel.SetActive(false);
    }

}

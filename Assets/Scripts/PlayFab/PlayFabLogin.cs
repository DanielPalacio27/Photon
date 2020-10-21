using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayFabLogin : MonoBehaviour
{
    private string userEmail;
    private string userPassword;
    private string username;
    [SerializeField] GameObject loginPanel;


    public void Start()
    {
//        Note: Setting title Id here can be skipped if you have set the value in Editor Extensions already.
//        if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
//        {
//            PlayFabSettings.TitleId = "136B9"; // Please change this value to your own titleId from PlayFab Game Manager
//        }

//        if (PlayerPrefs.HasKey("EMAIL"))
//        {
//            userEmail = PlayerPrefs.GetString("EMAIL");
//            userPassword = PlayerPrefs.GetString("PASSWORD");
//            var request = new LoginWithEmailAddressRequest { Email = userEmail, Password = userPassword };
//            PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
//        }
//        else
//        {
//#if UNITY_ANDROID
//            var requestAndroid = new LoginWithAndroidDeviceIDRequest { AndroidDeviceId = ReturnMobileID(), CreateAccount = true};
//            PlayFabClientAPI.LoginWithAndroidDeviceID(requestAndroid, OnLoginMobileSuccess, OnLoginMobileFailure);
//#endif
//#if UNITY_IOS
//            var requestIOS = new LoginWithIOSDeviceIDRequest { DeviceId = ReturnMobileID(), CreateAccount = true };
//            PlayFabClientAPI.LoginWithIOSDeviceID(requestIOS, OnLoginMobileSuccess, OnLoginMobileFailure);
//#endif
//        }
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Congratulations, you made your first successful API call!");
        PlayerPrefs.SetString("EMAIL", userEmail);
        PlayerPrefs.SetString("PASSWORD", userPassword);
        loginPanel.SetActive(false);
    }

    private void OnRegisterSucces(RegisterPlayFabUserResult result)
    {
        print("Congratulations you are register");
        PlayerPrefs.SetString("EMAIL", userEmail);
        PlayerPrefs.SetString("PASSWORD", userPassword);
        loginPanel.SetActive(false);
    }

    private void OnLoginMobileSuccess(LoginResult result)
    {
        Debug.Log("Congratulations, you made your first successful API call!");
        loginPanel.SetActive(false);
    }

    private void OnLoginMobileFailure(PlayFabError error)
    {
        print(error.GenerateErrorReport());
    }

    private void OnLoginFailure(PlayFabError error)
    {
        var registerRequest = new RegisterPlayFabUserRequest { Email = userEmail, Password = userPassword, Username = username};
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegisterSucces, OnRegisterFailure);
    }

    private void OnRegisterFailure(PlayFabError error)
    {
        print(error.GenerateErrorReport());
    }

    public void GetUserEmail(string email)
    {
        userEmail = email;
    }

    public void GetUserPassword(string password)
    {
        userPassword = password;
    }

    public void GetUsername(string _username)
    {
        username = _username;
    }

    public void OnClickLogIn()
    {
        var request = new LoginWithEmailAddressRequest { Email = userEmail, Password = userPassword };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
    }

    public static string ReturnMobileID()
    {
        string deviceID = SystemInfo.deviceUniqueIdentifier;
        return deviceID;
    }

    public void LinkWithFacebook()
    {

        //PlayFabClientAPI.LinkFacebookAccount()
    }
}
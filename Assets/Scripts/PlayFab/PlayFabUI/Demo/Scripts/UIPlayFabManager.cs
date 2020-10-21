using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;

public class UIPlayFabManager : MonoBehaviour
{
    PlayFabAuthService authService;
    List<FriendInfo> friends;
    [SerializeField] List<Text> friendsText;
    [SerializeField] GameObject friendPanel;
    [SerializeField] Button showFriendsButt;
    [SerializeField] Button exitButt;
    [SerializeField] Text nickNameText;

    string nickname;

    IEnumerator Start()
    {
        PlayFabAuthService.OnGetFriendList += GetFriends;
        showFriendsButt.onClick.AddListener(OnUIShowFriends);
        exitButt.onClick.AddListener(ExitButton);

        yield return new WaitUntil(() => PlayFabClientAPI.IsClientLoggedIn());
        GetAccountInfoRequest request = new GetAccountInfoRequest();
        PlayFabClientAPI.GetAccountInfo(request, 
        (infoResult) =>
        {
            nickname = infoResult.AccountInfo.TitleInfo.DisplayName;
            nickNameText.text = nickname;

        }, (error) => 
        {
            Debug.Log("Error obtaining account info" + error.ErrorMessage);

        });

        //nickname = request.TitleDisplayName;
        //nickNameText.text = nickname;
    }

    
    public void OnUIShowFriends()
    {
        PlayFabAuthService.Instance.GetFriendList();
        //friendPanel.SetActive(true);
    }

    void GetFriends(GetFriendsListResult result)
    {
        friendPanel.SetActive(true);
        friends = result.Friends;
        
        for (int i = 0; i < friends.Count; i++)
        {
            friendsText[i].gameObject.SetActive(true);
            friendsText[i].text = friends[i].FacebookInfo.FullName;
            Debug.Log(friends[i].FacebookInfo.FullName);
        }
    }

    public void ExitButton()
    {
        exitButt.transform.parent.gameObject.SetActive(false);
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;

public class JoinLobbyManager : MonoBehaviour
{
    public GameObject lobbyButtonPrefab;
    public Transform contentParent; // ScrollView의 Content에 연결
    public GameObject codePanel;
    public GameObject PlayerNamePanel;
    public TMP_InputField NameInput;
    public TMP_InputField codeInput;
    string code;
    public string playerName;

    void Start()
    {
        RefreshLobbyList();
        codePanel.SetActive(false); // 처음엔 숨겨놓기
        PlayerNamePanel.SetActive(true); 

    }
    public void On_Click_PrivateBtn()
    {
        codePanel.SetActive(true);
        return;

    }
    //코드 패널 버튼 
    public async void On_Click_CodeOkBtn()
    {
        code = codeInput.text;
        
        if (string.IsNullOrWhiteSpace(code))
            {
                Debug.LogWarning("로비 코드를 입력해주세요.");
                return;
            }
        try{
            
            var lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code);
            CreateLobbyManager.LobbyHolder.CurrentLobby = lobby;

            Debug.Log("비공개 로비 참가 성공: " + lobby.Name);
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("비공개 로비 참가 실패: " + e.Message);
        }
    }
    //메인 패널의 버튼과 닉네임 버튼 공통 사용 이벤트
    public void On_Click_Cancel(){
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }

    //처음 닉네임 설정 패널 버튼 
    public void On_Name_OkBtn(){
        PlayerNamePanel.SetActive(false); 
        playerName = NameInput.text;
    }

    public async void RefreshLobbyList()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        try
        {
            var queryResponse = await LobbyService.Instance.QueryLobbiesAsync();

            foreach (var lobby in queryResponse.Results)
            {
                GameObject buttonGO = Instantiate(lobbyButtonPrefab, contentParent);
                LobbyButtonUI buttonUI = buttonGO.GetComponent<LobbyButtonUI>();
                buttonUI.SetLobbyInfo(lobby);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("로비 목록 조회 실패: " + e.Message);
        }
    }
}

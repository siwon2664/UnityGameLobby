using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;


public class CreateLobbyManager : MonoBehaviour
{
    public TMP_InputField createName, playerName;
    public Toggle publicToggle, privateToggle;
    

    void Start()
    {
        //Private 토글이 선택되었을 때
        publicToggle.onValueChanged.AddListener(OnPrivateToggleChanged);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public static class LobbyHolder
    {
        public static Lobby CurrentLobby;
    }
    void OnPrivateToggleChanged(bool isOn)
    {
        if (isOn)
        {
            Debug.Log("Private_Toggle 선택됨!");
            // 여기에 원하는 행동 추가
        }
    }
    public void onClick_Cancel(){
        SceneManager.LoadScene("MainScene");
    }
    public async void OnClick_CreateLobby()
    {
        // 본인 정보 준비 (닉네임이나 준비 상태 등)
        
        var player = new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "DisplayName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName.text) },
            }
        };
        // 로비 생성
        try
        {

            bool isPrivate = privateToggle.isOn;

            // 2. CreateLobbyOptions 생성
            var options = new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Player = player,
                Data = new Dictionary<string, DataObject>
                    {
                        
                    }
            };
            LobbyHolder.CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(createName.text, 8, options);

            Debug.Log($"로비 생성됨! 이름: {LobbyHolder.CurrentLobby.Name}, 코드: {LobbyHolder.CurrentLobby.LobbyCode}");

            SceneManager.LoadScene("Lobby");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("로비 생성 실패: " + e);
        }
    }
}


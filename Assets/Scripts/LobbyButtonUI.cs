using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class LobbyButtonUI : MonoBehaviour
{
    public TMP_Text playerNumText;
    public TMP_Text lobbyNameText;
    public TMP_Text hostNameText;
    private string lobbyId;
    private bool isPrivate;
     
    
    public void SetLobbyInfo(Lobby lobby)
    {
        isPrivate = lobby.IsPrivate;
        lobbyId = lobby.Id;
        playerNumText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
        lobbyNameText.text = lobby.Name;
        string hostName = "???";
        foreach (var player in lobby.Players)
        {
            if (player.Id == lobby.HostId)
            {
                if (player.Data != null && player.Data.ContainsKey("DisplayName"))
                {
                    hostName = player.Data["DisplayName"].Value;
                }
                break;
            }
        }

        hostNameText.text = hostName;
    }

    public async void OnClick_JoinLobby()
    {
        
        try
        {
            var joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
            CreateLobbyManager.LobbyHolder.CurrentLobby = joinedLobby;

            Debug.Log("로비 참가 완료: " + joinedLobby.Name);
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
            //인스턴스를 찾아 접근함
            var joinManager = FindFirstObjectByType<JoinLobbyManager>();
            string name = joinManager.playerName;

            //로비 참가 후 플레이어의 닉네임 설정
             await LobbyService.Instance.UpdatePlayerAsync(
            CreateLobbyManager.LobbyHolder.CurrentLobby.Id,
            AuthenticationService.Instance.PlayerId,
            new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "DisplayName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, name) }
                }
            }
        );
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("로비 참가 실패: " + e.Message);
        }
    }
}

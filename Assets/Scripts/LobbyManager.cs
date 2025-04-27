using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using System;
using UnityEngine.SceneManagement;
using Unity.Services.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;
using System.Collections;

[System.Serializable]
public class PlayerPanel
{
    public TMP_Text nicknameText;
    public TMP_Text readyText;
}

public class LobbyManager : MonoBehaviour
{
    public List<PlayerPanel> playerPanels; // 8개 고정
    public TMP_Text roomName;

    public GameObject topPanel;
    public TMP_InputField privateInput;
    public GameObject readyButton;
    public GameObject startButton;

    private bool hasJoined = false;
    Lobby previousLobbySnapshot = null;

    async void Start()
    {
        bool isHost = CreateLobbyManager.LobbyHolder.CurrentLobby.HostId == AuthenticationService.Instance.PlayerId;
        await Task.Delay(500);
        await UpdatePlayerOnce();
        if (isHost)
        {
            _ = LobbyService.Instance.UpdatePlayerAsync(
                CreateLobbyManager.LobbyHolder.CurrentLobby.Id,
                AuthenticationService.Instance.PlayerId,
                new UpdatePlayerOptions
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "true") }
                    }
                });

                StartCoroutine(HostLobbyUpdateRoutine());
        }

        topPanel.SetActive(false);
        roomName.text = CreateLobbyManager.LobbyHolder.CurrentLobby.Name;

        if (CreateLobbyManager.LobbyHolder.CurrentLobby.IsPrivate)
        {
            topPanel.SetActive(true);
            privateInput.text = CreateLobbyManager.LobbyHolder.CurrentLobby.LobbyCode;
        }

        readyButton.SetActive(!isHost);
        startButton.SetActive(false);

        
 
    if (!isHost)
    {
        StartCoroutine(CheckForJoinCodeRoutine()); // 클라이언트만 반복 체크
    }    }

    void Update()
{
    if (CreateLobbyManager.LobbyHolder.CurrentLobby == null || startButton == null)
        return;

    var lobby = CreateLobbyManager.LobbyHolder.CurrentLobby;
    bool isHost = lobby.HostId == AuthenticationService.Instance.PlayerId;

    if (!AllPlayersReady(lobby))
    {
        if (isHost)
            startButton.SetActive(false);
    }
    else
    {
        if (isHost)
            startButton.SetActive(true);
    }

    // ✅ joinCode 감지 및 연결 조건 확인
    Debug.Log("🔄 Update(): joinCode 확인 시도");

    if (!isHost && !hasJoined && lobby.Data != null &&
        lobby.Data.TryGetValue("joinCode", out var joinCodeData) &&
        !string.IsNullOrEmpty(joinCodeData.Value))
    {
        Debug.Log("🎯 joinCode 감지됨 → JoinRelayAndStartClient() 호출");
        hasJoined = true;
        JoinRelayAndStartClient();
    }
    else if (!isHost && !hasJoined)
    {
        Debug.Log("❌ 아직 joinCode 없음 또는 비어 있음");
    }
}

IEnumerator HostLobbyUpdateRoutine()
{
    while (true)
    {
        yield return new WaitForSeconds(1.5f); // 1.5초마다 확인
        _ = UpdatePlayerOnce(); // 로비 상태를 받아와서 UI 갱신
    }
}

    IEnumerator CheckForJoinCodeRoutine()
{
    while (!hasJoined)
    {
        yield return new WaitForSeconds(1f);  // 1초마다 실행
        _ = UpdatePlayerOnce();               // 비동기 함수 호출 (결과 무시)
    }
}




public async Task UpdatePlayerOnce()
{
    if (CreateLobbyManager.LobbyHolder.CurrentLobby == null) return;

    try
    {
        var after = await LobbyService.Instance.GetLobbyAsync(CreateLobbyManager.LobbyHolder.CurrentLobby.Id);
        CreateLobbyManager.LobbyHolder.CurrentLobby = after;
        previousLobbySnapshot = after;

        for (int i = 0; i < playerPanels.Count; i++)
        {
            if (i < after.Players.Count)
            {
                var player = after.Players[i];
                var panel = playerPanels[i];

                string nickname = player.Data != null && player.Data.ContainsKey("DisplayName")
                    ? player.Data["DisplayName"].Value
                    : "???";

                bool isReady = player.Data != null && player.Data.ContainsKey("Ready")
                    ? player.Data["Ready"].Value == "true"
                    : false;

                panel.nicknameText.text = (player.Id == after.HostId) ? nickname + " (Host)" : nickname;
                panel.readyText.text = isReady ? "Ready" : "Not Ready";
            }
            else
            {
                playerPanels[i].nicknameText.text = "---";
                playerPanels[i].readyText.text = "";
            }
        }

        // ✅ joinCode 확인 및 Relay 연결
        if (!hasJoined &&
            after.Data != null &&
            after.Data.TryGetValue("joinCode", out var joinCodeData) &&
            !string.IsNullOrEmpty(joinCodeData.Value) &&
            after.HostId != AuthenticationService.Instance.PlayerId)
        {
            Debug.Log("🎯 최신 로비에서 joinCode 확인됨 → 클라이언트 연결 시도!");
            hasJoined = true;
            JoinRelayAndStartClient();
        }
    }
    catch (LobbyServiceException e)
    {
        Debug.Log("로비 정보 업데이트 실패: " + e.Message);
    }
}

    bool AllPlayersReady(Lobby lobby)
    {
        foreach (var player in lobby.Players)
        {
            if (player.Data == null || !player.Data.ContainsKey("Ready") || player.Data["Ready"].Value != "true")
            {
                return false;
            }
        }
        return true;
    }

    public async void OnClick_Ready()
    {
        try
        {
            await LobbyService.Instance.UpdatePlayerAsync(
                CreateLobbyManager.LobbyHolder.CurrentLobby.Id,
                AuthenticationService.Instance.PlayerId,
                new UpdatePlayerOptions
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "true") }
                    }
                });

            Debug.Log("Ready 상태로 변경됨");

            await UpdatePlayerOnce();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Ready 상태 변경 실패: " + e.Message);
        }
    }

    public async void OnClick_StartGame()
    {
        var lobby = CreateLobbyManager.LobbyHolder.CurrentLobby;

        try
        {
            var updatedLobby = await LobbyService.Instance.GetLobbyAsync(lobby.Id);

            if (AllPlayersReady(updatedLobby))
            {
                int maxPlayers = lobby.MaxPlayers;
                var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        {"joinCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode)}
                    }
                });

                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetHostRelayData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData
                );

                bool success = NetworkManager.Singleton.StartHost();

                if (success)
                {
                    NetworkManager.Singleton.SceneManager.LoadScene("testScene", LoadSceneMode.Single);
                }
                else
                {
                    Debug.LogError("Host 시작 실패!");
                }

                Debug.Log("모든 플레이어 준비 완료 → 게임 시작!");
            }
            else
            {
                Debug.Log("아직 준비 안 된 플레이어가 있음");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("로비 조회 실패: " + e.Message);
        }
    }

    public async void JoinRelayAndStartClient()
    {
        Debug.Log("▶ JoinRelayAndStartClient 시작됨");
        var lobby = CreateLobbyManager.LobbyHolder.CurrentLobby;

        try
        {
            if (lobby.Data.TryGetValue("joinCode", out var joinCodeData))
            {
                string joinCode = joinCodeData.Value;
                Debug.Log("JoinCode 받아옴" + joinCode);

                var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetClientRelayData(
                    joinAllocation.RelayServer.IpV4,
                    (ushort)joinAllocation.RelayServer.Port,
                    joinAllocation.AllocationIdBytes,
                    joinAllocation.Key,
                    joinAllocation.ConnectionData,
                    joinAllocation.HostConnectionData
                );

                bool success = NetworkManager.Singleton.StartClient();
                if (success)
                {
                    Debug.Log("StartClient 성공 → 클라이언트 NGO 연결됨");
                }
                else
                {
                    Debug.LogError("StartClient 실패 → NGO 연결 실패");
                }
            }
            else
            {
                Debug.LogError("로비에서 JoinCode를 찾을 수 없음.");
            }
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Relay 접속 실패: " + e.Message);
        }
    }

    public async void On_Click_Cancel()
    {
        if (CreateLobbyManager.LobbyHolder.CurrentLobby == null) return;

        bool isHost = CreateLobbyManager.LobbyHolder.CurrentLobby.HostId == AuthenticationService.Instance.PlayerId;

        try
        {
            if (isHost)
            {
                await LobbyService.Instance.DeleteLobbyAsync(CreateLobbyManager.LobbyHolder.CurrentLobby.Id);
                Debug.Log("로비 삭제 완료 (호스트)");
            }
            else
            {
                await LobbyService.Instance.RemovePlayerAsync(
                    CreateLobbyManager.LobbyHolder.CurrentLobby.Id,
                    AuthenticationService.Instance.PlayerId);
                Debug.Log("로비 나가기 완료 (참가자)");
            }

            CreateLobbyManager.LobbyHolder.CurrentLobby = null;
            SceneManager.LoadScene("MainScene");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("로비 종료 실패: " + e.Message);
        }
    }
}

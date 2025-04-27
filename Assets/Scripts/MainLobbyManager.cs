    // Unity 기본
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

// UGS 초기화 및 인증
using Unity.Services.Core;
using Unity.Services.Authentication;

// Lobby 기능
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

// Relay 기능 (게임 네트워크 연결용)
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

// 씬 전환
using UnityEngine.SceneManagement;

// UI 연결용
using UnityEngine.UI;
using TMPro;
public class MainLobbyManager : MonoBehaviour
{
    
    async void Start()
    {
        //UGS초기화 
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            //UGS초기화
            await UnityServices.InitializeAsync();
        }
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            //익명 로그인
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        Debug.Log("로그인 완료");
    }

    
    void Update()
    {
        
    }

    public void CreateRoom(){
        SceneManager.LoadScene("CreateLobby");
    }
    public void JoinRoom(){
         SceneManager.LoadScene("JoinLobby");
    }
}

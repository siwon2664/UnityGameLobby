using UnityEngine;
using Unity.Netcode;
using Unity.UI;
using UnityEngine.SceneManagement;

public class testSceneManager : MonoBehaviour
{
   void Start()
{
    Debug.Log(" testScene에 진입 완료");

    Debug.Log("IsHost: " + NetworkManager.Singleton.IsHost);
    Debug.Log("IsClient: " + NetworkManager.Singleton.IsClient);
    Debug.Log("IsServer: " + NetworkManager.Singleton.IsServer);
    Debug.Log("ConnectedClients: " + NetworkManager.Singleton.ConnectedClientsList.Count);
}

}

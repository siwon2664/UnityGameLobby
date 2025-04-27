using UnityEngine;
using Unity.Netcode;

public class GameNetworkManager : MonoBehaviour
{
    private void Awake()
    {

        if (Object.FindObjectsByType<NetworkManager>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(this.gameObject);
            return;
        }

        DontDestroyOnLoad(this.gameObject);
    }
    void Start()
    {
        Debug.Log("🎮 현재 클라이언트 ID: " + NetworkManager.Singleton.LocalClientId);
        Debug.Log("🎮 현재 플레이어 수: " + NetworkManager.Singleton.ConnectedClientsList.Count);
    }
}

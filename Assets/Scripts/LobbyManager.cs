using System.ComponentModel;
using System.Security;
using System.Runtime.CompilerServices;
using System.Xml.Schema;
//using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour
{
    public GameObject playerScrollContent;
    public TMPro.TMP_Text txtPlayerNumber;
    public Button btnStart;

    public Player playerPrefab;
    public LobbyPlayerPanel playerPanelPrefab;

    public GameObject playersPanel;

    private NetworkList<PlayerInfo> allPlayers = new NetworkList<PlayerInfo>();
    private List<LobbyPlayerPanel> playerPanels = new List<LobbyPlayerPanel>();


    public void Start(){
        if(IsHost){
            AddPlayerToList(NetworkManager.LocalClientId);
            RefreshPlayerPanels();
        }
    }

    private void AddPlayerToList(ulong clientId){
        allPlayers.Add(new PlayerInfo(clientId, Color.red));
    }

    private void AddPlayerPanel(PlayerInfo info){
        LobbyPlayerPanel newPanel = Instantiate(playerPanelPrefab);
        newPanel.transform.SetParent(playerScrollContent.transform, false);
        newPanel.SetName($"Player {info.clientId.ToString()}");
        newPanel.SetColor(info.color);
        playerPanels.Add(newPanel);
    }

    private void RefreshPlayerPanels(){
        foreach(LobbyPlayerPanel panel in playerPanels){
            Destroy(panel.gameObject);
        }
        playerPanels.Clear();

        foreach(PlayerInfo pi in allPlayers){
            AddPlayerPanel(pi);
        }
    }

    private int FindPlayerIndex(ulong clientId){
        var idx = 0;
        var found = false;

        while(idx < allPlayers.Count && !found){
            if(allPlayers[idx].clientId == clientId){
                found = true;
            }
            else{
                idx += 1;
            }
        }

        if(!found){
            idx = -1;
        }

        return idx;
    }

    public override void OnNetworkSpawn() {
        if(IsHost){
            NetworkManager.Singleton.OnClientConnectedCallback += HostOnClientConnected;
            //NetworkManager.Singleton.OnClientDisconnectedCallback += HostOnClientDisconnected;
        }

        base.OnNetworkSpawn();

        if(IsClient){
            allPlayers.OnListChanged += ClientOnAllPlayersChanged;
            txtPlayerNumber.text = $"Player #{NetworkManager.LocalClientId}";
        }
    }

    private void ClientOnAllPlayersChanged(NetworkListEvent<PlayerInfo> changeEvent){
        RefreshPlayerPanels();
    }

    private void HostOnClientConnected(ulong clientId){
        AddPlayerToList(clientId);
        RefreshPlayerPanels();
    }

    private void HostOnClientDisconnected(ulong clientId){
        int index = FindPlayerIndex(clientId);
        if(index != -1){
            allPlayers.RemoveAt(index);
            RefreshPlayerPanels();
        }
    }

}

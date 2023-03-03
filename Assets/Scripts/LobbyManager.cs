using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    private readonly string gameVersion = "1.0";

    public Text connectionInfoText;
    [SerializeField] Button[] roomList;
    [SerializeField] Button previousButton;
    [SerializeField] Button nextButton;
    [SerializeField] InputField roomName;
    [SerializeField] Button createRoomButton;
    [SerializeField] Button joinRandomRoomButton;
    [SerializeField] Button goToMainButton;
    [SerializeField] Button playButton;
    [SerializeField] GameObject startScreen;
    [SerializeField] GameObject ruleScreen;

    List<RoomInfo> myList = new List<RoomInfo>();
    int currentPage = 1, maxPage, multiple;

    // Start is called before the first frame update
    void Start()
    {
        if(PhotonNetwork.IsConnected)
        {
            startScreen.SetActive(false);
        }
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();

        joinRandomRoomButton.interactable = false;
        createRoomButton.interactable = false;
        playButton.interactable = false;
        connectionInfoText.text = "Connecting to master server...";
    }

    public override void OnConnectedToMaster()
    {
        joinRandomRoomButton.interactable = true;
        createRoomButton.interactable = true;
        playButton.interactable = true;
        connectionInfoText.text = "Online : Connected to master server";
        PhotonNetwork.JoinLobby();
    }

    //public override void OnDisconnected(DisconnectCause cause)
    //{
    //    joinButton.interactable = false;
    //    connectionInfoText.text = $"Offline: Connection disabled {cause.ToString()} - Try reconnecting...";

    //    PhotonNetwork.ConnectUsingSettings();
    //}

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        connectionInfoText.text = "There is no empty room, creating new room...";
        PhotonNetwork.CreateRoom(Random.Range(1000, 9999).ToString(), new RoomOptions { MaxPlayers = 2 });
    }

    public override void OnJoinedRoom()
    {
        connectionInfoText.text = "Connected with room.";
        PhotonNetwork.LoadLevel("InGame");
    }

    public override void OnJoinedLobby()
    {
        myList.Clear();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                if (!myList.Contains(roomList[i])) myList.Add(roomList[i]);
                else myList[myList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (myList.IndexOf(roomList[i]) != -1) myList.RemoveAt(myList.IndexOf(roomList[i]));
        }
        UpdateRoomList();
    }
    // 이전 페이지 버튼 : -2 , 다음 페이지 버튼 : -1
    public void OnClickRoomPanelButton(int num)
    {
        if (num == -2) --currentPage;
        else if (num == -1) ++currentPage;
        else PhotonNetwork.JoinRoom(myList[multiple + num].Name);
        UpdateRoomList();
    }
    void UpdateRoomList()
    {
        // 최대페이지
        maxPage = (myList.Count % roomList.Length == 0) ? myList.Count / roomList.Length : myList.Count / roomList.Length + 1;

        // 이전, 다음버튼
        previousButton.interactable = (currentPage <= 1) ? false : true;
        nextButton.interactable = (currentPage >= maxPage) ? false : true;

        // 페이지에 맞는 리스트 대입
        multiple = (currentPage - 1) * roomList.Length;
        for (int i = 0; i < roomList.Length; i++)
        {
            roomList[i].interactable = (multiple + i < myList.Count) ? true : false;
            roomList[i].transform.GetChild(0).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].Name : "";
            roomList[i].transform.GetChild(1).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].PlayerCount + "/" + myList[multiple + i].MaxPlayers : "";
        }
    }
    public void OnClickJoinRandomRoomButton()
    {
        joinRandomRoomButton.interactable = false;
        if (PhotonNetwork.IsConnected)
        {
            connectionInfoText.text = "Connecting to random room...";
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            connectionInfoText.text = "Offline: Connection disabled - Try reconnecting...";
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void OnClickCreateRoomButton()
    {
        PhotonNetwork.CreateRoom(roomName.text == "" ? 
            Random.Range(1000, 9999).ToString() : roomName.text,
            new RoomOptions { MaxPlayers = 2 });
    }

    public void OnClickGoToMainButton()
    {
        startScreen.SetActive(true);
    }

    public void OnClickGameRuleButton()
    {
        startScreen.SetActive(false);
        ruleScreen.SetActive(true);
    }

    public void OnClickPlayButton()
    {
        startScreen.SetActive(false);
    }

    public void OnClickRulePlayBtn()
    {
        ruleScreen.SetActive(false);
    }
}

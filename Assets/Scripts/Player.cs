using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Player : MonoBehaviourPun
{
    private bool isObstacleSelected;

    public Tile[,] board;
    Index currentIndex;

    public List<Index> pathBuffer = new List<Index>();

    Tile target;

    MeshRenderer meshRenderer;
    public Material player1Mat;
    public Material player2Mat;

    public bool isInput; // 입력 가능 flag
    public bool isSetObstacle; // 입력 가능 flag


    // Start is called before the first frame update
    void Start()
    {
        isInput = false;
        isSetObstacle = false;
        board = GameManager.Instance.board;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "isInputDone", false } });
        meshRenderer = GetComponent<MeshRenderer>();
        if (photonView.IsMine)
        {
            meshRenderer.material = player1Mat;
        }
        else
        {
            meshRenderer.material = player2Mat;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("입력 중:" + isInput);
        if (Input.GetMouseButtonDown(0) && isInput == true) // *** 객체 hit 함수 추가
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                target = hit.collider.gameObject.GetComponent<Tile>();
            }
            if (transform.position == target.transform.position)
            { Debug.Log("다시 입력하삼"); }
            else
            { 
                isInput = false;
                isSetObstacle = true;
                target.isObstacle = true;
                int tileRow = target.tileIndex.row;
                int tileCol = target.tileIndex.col;
                PhotonView pv = gameObject.GetPhotonView();
                pv.RPC("SetObstacleFlag", RpcTarget.AllBuffered, tileRow, tileCol);
                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "isInputDone", true } });
            }
        }
    }

    public void OnObstacleButtonClick()
    {
        isObstacleSelected = true;
    }

    public void OnMoveButtonClick()
    {
        isObstacleSelected = false;
    }

    

    // *클릭으로 입력받고 클릭한 클릭한 타일의 isObstacle flag를 변경해줌
    /*public void InputObstacle()
    {
       
        
        // flag 를 이 함수에서 바꿔주고 Update() 에서 입력을 받기
        // flag는 입력을 받을 수 잇는 상태를 나타냄
        // 아래 코드가 꼭 이 함수 안에 있어야하는 거는 아님. update() 문에서도 사용 가능.

        // *클릭한 타일의 row와 col을 가져와서 tileRow, tileCol 변수에 대입
        // tile 클래스에 tileIndex 이용
       
        // *NewGameMgr에서 time이 초과되면 입력받기 중지
        // NewGameMgr의 변수 currentTime, maxTime 이용

        // network 
        // iaObastacle flag 변경해주는 코드
        // 필요에 따라 위치 이동
    
    }*/

    // *이동을 입력받고 이동 경로를 NewPlayer 클래스가 가지고 있는 pathBuffer 변수에 저장.
    public void InputMove(int diceNum)
    {
        // flag 를 이 함수에서 바꿔주고 Update() 에서 입력을 받기
        // flag는 입력을 받을 수 잇는 상태를 나타냄
        // 이동 입력받기는 InputObstacle() 함수와 마찬가지로 Update() 문에서 받기
        // 마찬가지로 flag 필요
        // 입력받으면서 player 오브젝트(체스 말) 이동 필요
        // 매개변수로 받은 diceNum 만큼 입력 받기
        // 모든 값을 다 입력받으면 원래 위치로 이동.

        int tileRow = 3;
        int tileCol = 3;
        // *NewGameMgr에서 time이 초과되면 입력받기 중지
        // NewGameMgr의 변수 currentTime, maxTime 이용


        // network
        PhotonView pv = gameObject.GetPhotonView();
        pv.RPC("SetObstacleFlag", RpcTarget.AllBuffered, tileRow, tileCol);
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "isInputDone", true } });
    }

    // RPC 함수
    [PunRPC]
    public void SetObstacleFlag(int indexRow, int indexCol)
    {
        //flag 새우기
        board[indexRow, indexCol].isObstacle = true;
    }
    [PunRPC]
    public void SetObstacle(int indexRow, int indexCol)
    {
        //
    }

}


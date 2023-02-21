using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using static Tile;
using static UnityEngine.GraphicsBuffer;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Player : MonoBehaviourPun
{
    //private bool isObstacleSelected;

    public Tile[,] board;
    public Index currentIndex;
    Index orgIndex;
    Vector3 orgPosition;
    [HideInInspector] public bool isGameLose = false;

    public List<Index> pathBuffer = new List<Index>();



    MeshRenderer meshRenderer;
    public Material player1Mat;
    public Material player2Mat;

    Tile target;
    public bool isObstacleInput; // 입력 가능 flag
    private bool isMoveInput;
    private int moveCount;
    
    // 칠하기 위한 flags
    bool isMoveRight = true;
    bool isMoveLeft = true;
    bool isMoveUp = true;
    bool isMoveDown = true;


    void Start()
    {
        isMoveInput = false;
        isObstacleInput = false;
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

        if (transform.position == GameManager.Instance.spawnPositions[0].position)
        {
            photonView.RPC("SetIndex", RpcTarget.AllBuffered, 6, 2);
        }
        else
        {
            photonView.RPC("SetIndex", RpcTarget.AllBuffered, 2, 2);
        }
        orgIndex = currentIndex;
        orgPosition = transform.position;
    }
    [PunRPC]
    void SetIndex(int row, int col)
    {
        currentIndex.row = row;
        currentIndex.col = col;
    }
    // Update is called once per frame
    void Update()
    {

        // Debug.Log("isMoveInput flag : " + isMoveInput);
        if (!photonView.IsMine) return;
        if (isObstacleInput == true && Input.GetMouseButtonDown(0)) // *** 객체 hit 함수 추가
        {
            int layerMask = -1 - (1 << LayerMask.NameToLayer("Player"));
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                target = hit.collider.gameObject.GetComponent<Tile>();
            }

            if (IsPlayerOnTile(target))
            {
                Debug.Log("플레이어가 있는 타일 선택 불가");
                // 플레이어가 존재하는 타일은 클릭 안되게
                // 조건에 있는 함수 미완성 상태
            }
            else if (target.isObstacleSet)
            {
                Debug.Log("이미 장애물 설치된 타일 선택 불가");
            }
            else
            {
                isObstacleInput = false;
                target.isObstacleInput = true;
                photonView.RPC("SetObstacleFlag", RpcTarget.AllBuffered, target.tileIndex.row, target.tileIndex.col);
                for (int i = 0; i < GameManager.Instance.diceNum; i++)
                {
                     photonView.RPC("AddPathRPC", RpcTarget.AllBuffered, -1, -1);
                }
                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "isInputDone", true } });
            }
        }
        if (isMoveInput)
        {
            if(moveCount < GameManager.Instance.diceNum)
            {
                if (Input.GetKeyDown(KeyCode.W))
                {
                    if (currentIndex.row == 0)
                    {
                        Debug.Log("Way Blocked!");
                    }
                    else
                    {
                        currentIndex.row -= 1;
                        if (board[currentIndex.row, currentIndex.col].isObstacleSet)
                        {
                            Debug.Log("Way Blocked!");
                            currentIndex.row += 1;
                            return;
                        }
                        gameObject.transform.Translate(new Vector3(0, 0, 1.25f));
                        photonView.RPC("AddPathRPC", RpcTarget.AllBuffered, currentIndex.row, currentIndex.col);
                        moveCount++;
                    }

                }
                else if (Input.GetKeyDown(KeyCode.A))
                {
                    if (currentIndex.col == 0)
                    {
                        Debug.Log("Way Blocked!");
                    }
                    else
                    {
                        currentIndex.col -= 1;
                        if (board[currentIndex.row, currentIndex.col].isObstacleSet)
                        {
                            Debug.Log("Way Blocked!");
                            currentIndex.col += 1;
                            return;
                        }
                        gameObject.transform.Translate(new Vector3(-1.25f, 0, 0));
                        photonView.RPC("AddPathRPC",RpcTarget.AllBuffered, currentIndex.row, currentIndex.col);
                        moveCount++;
                    }

                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    if (currentIndex.row == 8)
                    {
                        Debug.Log("Way Blocked!");
                    }
                    else
                    {
                        currentIndex.row += 1;
                        if (board[currentIndex.row, currentIndex.col].isObstacleSet)
                        {
                            Debug.Log("Way Blocked!");
                            currentIndex.row -= 1;
                            return;
                        }
                        gameObject.transform.Translate(new Vector3(0, 0, -1.25f));
                        photonView.RPC("AddPathRPC", RpcTarget.AllBuffered, currentIndex.row, currentIndex.col);
                        moveCount++;
                    }

                }
                else if (Input.GetKeyDown(KeyCode.D))
                {
                    if (currentIndex.col == 4)
                    {
                        Debug.Log("Way Blocked!");
                    }
                    else
                    {
                        currentIndex.col += 1;
                        if (board[currentIndex.row, currentIndex.col].isObstacleSet)
                        {
                            Debug.Log("Way Blocked!");
                            currentIndex.col -= 1;
                            return;
                        }
                        gameObject.transform.Translate(new Vector3(1.25f, 0, 0));
                        photonView.RPC("AddPathRPC", RpcTarget.AllBuffered, currentIndex.row, currentIndex.col);
                        moveCount++;
                    }

                }
            }
            else    // diceNum 만큼 입력을 다 받았을 경우
            {
                if (Input.GetKeyDown(KeyCode.Return))   // 입력 완료
                {
                    currentIndex = orgIndex;
                    transform.position = orgPosition;
                    isMoveInput = false;
                    // network
                    PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "isInputDone", true } });
                    Debug.Log("엔터 누르고 buffer 수 : " + pathBuffer.Count);
                }
            }
        }
    }

    private bool IsPlayerOnTile(Tile target)
    {
        Player[] players = FindObjectsOfType<Player>();
        foreach(Player p in players)
        {
            if(p.currentIndex.Equals(target.tileIndex)) { return true; }
        }
        return false;
    }

    // *클릭으로 입력받고 클릭한 클릭한 타일의 isObstacle flag를 변경해줌
    public void InputObstacle()
    {
        Debug.Log("InputObstacle");
        isObstacleInput = true;
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
    }

    // *이동을 입력받고 이동 경로를 NewPlayer 클래스가 가지고 있는 pathBuffer 변수에 저장.
    public void InputMove(int diceNum)
    {
        Debug.Log("InputMove");
        isMoveInput = true;
        moveCount = 0;
        pathBuffer.Clear();
        orgIndex = currentIndex;
        orgPosition = transform.position;
        // flag 를 이 함수에서 바꿔주고 Update() 에서 입력을 받기
        // flag는 입력을 받을 수 잇는 상태를 나타냄
        // 이동 입력받기는 InputObstacle() 함수와 마찬가지로 Update() 문에서 받기
        // 마찬가지로 flag 필요
        // 입력받으면서 player 오브젝트(체스 말) 이동 필요
        // 매개변수로 받은 diceNum 만큼 입력 받기
        // 모든 값을 다 입력받으면 원래 위치로 이동.

        // *NewGameMgr에서 time이 초과되면 입력받기 중지
        // NewGameMgr의 변수 currentTime, maxTime 이용
    }

    // RPC 함수
    [PunRPC]
    void SetObstacleFlag(int indexRow, int indexCol)
    {
        //flag 새우기
        board[indexRow, indexCol].isObstacleInput = true;
    }
    [PunRPC]
    void AddPathRPC(int row, int col)
    {
        Index i;
        i.row= row; i.col = col;
        pathBuffer.Add(i);
    }

    internal void SyncDiceNum()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        photonView.RPC("SyncDiceNumRPC", RpcTarget.AllBuffered, GameManager.Instance.diceNum);
    }
    [PunRPC]
    void SyncDiceNumRPC(int diceNum)
    {
        GameManager.Instance.diceNum = diceNum;
    }

    // 현재 위치에서 이동할 수 있는 위치들 flag 세팅
    public void SetPathFlag()
    {
        int maxRow = GameManager.Instance.boardRow - 1;
        int maxCol = GameManager.Instance.boardCol - 1;

        
        if(currentIndex.row == 0) { isMoveUp = false; } // 위로 이동 불가
        else { isMoveUp = true; }

        if(currentIndex.row == maxRow) { isMoveDown = false; } // 아래로 이동 불가
        else { isMoveDown = true; }

        if(currentIndex.col == 0){ isMoveLeft = false; } // 왼쪽으로 이동 불가
        else { isMoveLeft = true;}

        if(currentIndex.col == maxCol){ isMoveRight = false;} // 오른쪽으로 이동 불가
        else { isMoveRight = true; }
    }
    // 이동 함수
    public void Move(Index moveIndex)
    {

        currentIndex = moveIndex;

        Tile moveTile = board[currentIndex.row, currentIndex.col];
        float moveX = moveTile.transform.position.x;
        float moveZ = moveTile.transform.position.z;
        transform.position = new Vector3(moveX, transform.position.y, moveZ);
        orgIndex = currentIndex;
        // 움직이면서 타일 뒤집기 
        // flag를 통해서 tile 뒤집기 tag를 통해서 색깔 지정
        // tag로 player 지정
        int mode;
        if(photonView.IsMine) { mode = 1; } // Blue 색 
        else { mode = 2; } //Red 색
        // check flag
        SetPathFlag();
        // change color
        board[currentIndex.row , currentIndex.col].changeColor(mode);
        if(isMoveUp) { board[currentIndex.row - 1 , currentIndex.col].changeColor(mode); }
        if(isMoveDown) { board[currentIndex.row + 1 , currentIndex.col].changeColor(mode); }
        if(isMoveRight) { board[currentIndex.row , currentIndex.col + 1].changeColor(mode); }
        if(isMoveLeft) { board[currentIndex.row , currentIndex.col - 1].changeColor(mode); }
    }


    internal void TimeOver()
    {
        StartCoroutine(TimeOverCoroutine());
    }

    IEnumerator TimeOverCoroutine()
    {
        yield return new WaitForSeconds(2f);
        int dN = GameManager.Instance.diceNum;
        if (pathBuffer.Count < dN)
        {
            int bufferCount = dN - pathBuffer.Count;
            for (int i = 0; i < bufferCount; i++)
            {
                // photonView.RPC("AddPathRPC", RpcTarget.AllBuffered, -1, -1);
                // buffer에 값 넣어주기
                pathBuffer.Add(new Index(-1, -1));
            }
        }
        isObstacleInput = false;

        // network
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "isInputDone", true } });
    }

    internal void UpdateIndex()
    {
        photonView.RPC("SetIndex", RpcTarget.AllBuffered,currentIndex.row, currentIndex.col);
    }
}


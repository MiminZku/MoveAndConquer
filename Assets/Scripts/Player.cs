using Photon.Pun;
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
    [SerializeField] Index currentIndex;
    Index orgIndex;
    Vector3 orgPosition;

    public List<Index> pathBuffer = new List<Index>();



    MeshRenderer meshRenderer;
    public Material player1Mat;
    public Material player2Mat;

    Tile target;
    public bool isObstacleInput; // �Է� ���� flag
    private bool isMoveInput;
    private int moveCount;
    
    // ĥ�ϱ� ���� flags
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
        if (!photonView.IsMine) return;
        if (isObstacleInput == true && Input.GetMouseButtonDown(0)) // *** ��ü hit �Լ� �߰�
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
                // �÷��̾ �����ϴ� Ÿ���� Ŭ�� �ȵǰ�
                // ���ǿ� �ִ� �Լ� �̿ϼ� ����
            }
            else
            {
                isObstacleInput = false;
                target.isObstacle = true;
                photonView.RPC("SetObstacleFlag", RpcTarget.AllBuffered, target.tileIndex.row, target.tileIndex.col);
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
                        gameObject.transform.Translate(new Vector3(0, 0, 1.25f));
                        currentIndex.row -= 1;
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
                        gameObject.transform.Translate(new Vector3(-1.25f, 0, 0));
                        currentIndex.col -= 1;
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
                        gameObject.transform.Translate(new Vector3(0, 0, -1.25f));
                        currentIndex.row += 1;
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
                        gameObject.transform.Translate(new Vector3(1.25f, 0, 0));
                        currentIndex.col += 1;
                        photonView.RPC("AddPathRPC", RpcTarget.AllBuffered, currentIndex.row, currentIndex.col);
                        moveCount++;
                    }

                }
            }
            else    // diceNum ��ŭ �Է��� �� �޾��� ���
            {
                if (Input.GetKeyDown(KeyCode.Return))   // �Է� �Ϸ�
                {
                    currentIndex = orgIndex;
                    transform.position = orgPosition;
                    isMoveInput = false;
                    // network
                    PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "isInputDone", true } });
                }
            }
        }
    }

    private bool IsPlayerOnTile(Tile target)
    {
        return false;
    }

    // *Ŭ������ �Է¹ް� Ŭ���� Ŭ���� Ÿ���� isObstacle flag�� ��������
    public void InputObstacle()
    {
        isObstacleInput = true;
        // flag �� �� �Լ����� �ٲ��ְ� Update() ���� �Է��� �ޱ�
        // flag�� �Է��� ���� �� �մ� ���¸� ��Ÿ��
        // �Ʒ� �ڵ尡 �� �� �Լ� �ȿ� �־���ϴ� �Ŵ� �ƴ�. update() �������� ��� ����.

        // *Ŭ���� Ÿ���� row�� col�� �����ͼ� tileRow, tileCol ������ ����
        // tile Ŭ������ tileIndex �̿�
        // *NewGameMgr���� time�� �ʰ��Ǹ� �Է¹ޱ� ����
        // NewGameMgr�� ���� currentTime, maxTime �̿�

        // network 
        // iaObastacle flag �������ִ� �ڵ�
        // �ʿ信 ���� ��ġ �̵�
    }

    // *�̵��� �Է¹ް� �̵� ��θ� NewPlayer Ŭ������ ������ �ִ� pathBuffer ������ ����.
    public void InputMove(int diceNum)
    {
        isMoveInput = true;
        moveCount = 0;
        pathBuffer.Clear();
        orgIndex = currentIndex;
        orgPosition = transform.position;
        // flag �� �� �Լ����� �ٲ��ְ� Update() ���� �Է��� �ޱ�
        // flag�� �Է��� ���� �� �մ� ���¸� ��Ÿ��
        // �̵� �Է¹ޱ�� InputObstacle() �Լ��� ���������� Update() ������ �ޱ�
        // ���������� flag �ʿ�
        // �Է¹����鼭 player ������Ʈ(ü�� ��) �̵� �ʿ�
        // �Ű������� ���� diceNum ��ŭ �Է� �ޱ�
        // ��� ���� �� �Է¹����� ���� ��ġ�� �̵�.

        // *NewGameMgr���� time�� �ʰ��Ǹ� �Է¹ޱ� ����
        // NewGameMgr�� ���� currentTime, maxTime �̿�
    }

    // RPC �Լ�
    [PunRPC]
    void SetObstacleFlag(int indexRow, int indexCol)
    {
        //flag �����
        board[indexRow, indexCol].isObstacle = true;
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

    // ���� ��ġ���� �̵��� �� �ִ� ��ġ�� flag ����
    public void setPathFlag()
    {
        int maxRow = GameManager.Instance.boardRow - 1;
        int maxCol = GameManager.Instance.boardCol - 1;

        
        if(currentIndex.row == 0) { isMoveUp = false; } // ���� �̵� �Ұ�
        else { isMoveUp = true; }

        if(currentIndex.row == maxRow) { isMoveDown = false; } // �Ʒ��� �̵� �Ұ�
        else { isMoveDown = true; }

        if(currentIndex.col == 0){ isMoveLeft = false; } // �������� �̵� �Ұ�
        else { isMoveLeft = true;}

        if(currentIndex.col == maxCol){ isMoveRight = false;} // ���������� �̵� �Ұ�
        else { isMoveRight = true; }
    }
    // �̵� �Լ�
    public void Move(Index moveIndex)
    {

        currentIndex = moveIndex;

        Tile moveTile = board[currentIndex.row, currentIndex.col];
        float moveX = moveTile.transform.position.x;
        float moveZ = moveTile.transform.position.z;
        transform.position = new Vector3(moveX, transform.position.y, moveZ);
        orgIndex = currentIndex;
        // �����̸鼭 Ÿ�� ������ 
        // flag�� ���ؼ� tile ������ tag�� ���ؼ� ���� ����
        // tag�� player ����
        int mode;
        if(photonView.IsMine) { mode = 1; } // Blue �� 
        else { mode = 2; } //Red ��
        // check flag
        setPathFlag();
        // change color
        board[currentIndex.row , currentIndex.col].changeColor(mode);
        if(isMoveUp) { board[currentIndex.row - 1 , currentIndex.col].changeColor(mode); }
        if(isMoveDown) { board[currentIndex.row + 1 , currentIndex.col].changeColor(mode); }
        if(isMoveRight) { board[currentIndex.row , currentIndex.col + 1].changeColor(mode); }
        if(isMoveLeft) { board[currentIndex.row , currentIndex.col - 1].changeColor(mode); }
    }
}


using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Player : MonoBehaviourPun
{
    private bool isObstacleSelected;
    private bool isMoveInput = false;

    public Tile[,] board;
    Index currentIndex;

    public List<Index> pathBuffer = new List<Index>();
    int inputMoveCount;


    MeshRenderer meshRenderer;
    public Material player1Mat;
    public Material player2Mat;


    // Start is called before the first frame update
    void Start()
    {
        inputMoveCount = 0;
        board = GameManager.Instance.board;
        if (transform.position == GameManager.Instance.spawnPositions[0].position) {
            currentIndex.row = 6;
            currentIndex.column = 2;
        }
        if (transform.position == GameManager.Instance.spawnPositions[1].position) {
            currentIndex.row = 2;
            currentIndex.column = 2;
        }


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

        if (isMoveInput && inputMoveCount < GameManager.Instance.diceNum) // *** ��ü hit �Լ� �߰�
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                /*Column 1 ����, ������ �̵����Ѿ���. ��� 0�϶� ������ �ϸ� ����.*/
                if (currentIndex.column == 0)
                {
                    Debug.Log("Way Blocked!");
                }
                else
                { // �̵� ����. column 1 ���� �� 
                    currentIndex.column -= 1;
                    pathBuffer.add(currentIndex);
                    inputMoveCount++;
                }

            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                if (currentIndex.row == 0)
                {
                    Debug.Log("Way Blocked!");
                }
                else
                { // �̵� ����. row 1 ���� �� 
                    currentIndex.row -= 1;
                    pathBuffer.add(currentIndex);
                    inputMoveCount++;
                }

            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                if (currentIndex.column == 8)
                {
                    Debug.Log("Way Blocked!");
                }
                else
                { // �̵� ����. column 1 ���� �� 
                    currentIndex.column += 1;
                    pathBuffer.add(currentIndex);
                    inputMoveCount++;
                }

            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                if (currentIndex.row == 4)
                {
                    Debug.Log("Way Blocked!");
                }
                else
                { // �̵� ����. row 1 ���� �� 
                    currentIndex.row += 1;
                    pathBuffer.add(currentIndex);
                    inputMoveCount++;
                }

            }

        }
        else { 
            /*network stuff*/
        }
    }

    public void OnObstacleButtonClick()
    {
        isObstacleSelected = true;
    }

    public void OnMoveButtonClick()
    {
        isObstacleSelected = false;
        pathBuffer.Clear();
    }


    // *Ŭ������ �Է¹ް� Ŭ���� Ŭ���� Ÿ���� isObstacle flag�� ��������
    public void InputObstacle()
    {
        // flag �� �� �Լ����� �ٲ��ְ� Update() ���� �Է��� �ޱ�
        // flag�� �Է��� ���� �� �մ� ���¸� ��Ÿ��
        // �Ʒ� �ڵ尡 �� �� �Լ� �ȿ� �־���ϴ� �Ŵ� �ƴ�. update() �������� ��� ����.

        // *Ŭ���� Ÿ���� row�� col�� �����ͼ� tileRow, tileCol ������ ����
        // tile Ŭ������ tileIndex �̿�
        int tileRow = 3;
        int tileCol = 3;
        // *NewGameMgr���� time�� �ʰ��Ǹ� �Է¹ޱ� ����
        // NewGameMgr�� ���� currentTime, maxTime �̿�

        // network 
        // iaObastacle flag �������ִ� �ڵ�
        // �ʿ信 ���� ��ġ �̵�
        PhotonView pv = gameObject.GetPhotonView();
        pv.RPC("SetObstacleFlag", RpcTarget.AllBuffered, tileRow, tileCol);
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "isInputDone", true } });
    }

    // *�̵��� �Է¹ް� �̵� ��θ� NewPlayer Ŭ������ ������ �ִ� pathBuffer ������ ����.
    public void InputMove(int diceNum)
    {

        inputMoveCount = 0;

        // flag �� �� �Լ����� �ٲ��ְ� Update() ���� �Է��� �ޱ�
        // flag�� �Է��� ���� �� �մ� ���¸� ��Ÿ��
        // �̵� �Է¹ޱ�� InputObstacle() �Լ��� ���������� Update() ������ �ޱ�
        // ���������� flag �ʿ�
        // �Է¹����鼭 player ������Ʈ(ü�� ��) �̵� �ʿ�
        // �Ű������� ���� diceNum ��ŭ �Է� �ޱ�
        // ��� ���� �� �Է¹����� ���� ��ġ�� �̵�.
        
       

        // *NewGameMgr���� time�� �ʰ��Ǹ� �Է¹ޱ� ����
        // NewGameMgr�� ���� currentTime, maxTime �̿�


        // network
        PhotonView pv = gameObject.GetPhotonView();
        pv.RPC("SetObstacleFlag", RpcTarget.AllBuffered, tileRow, tileCol);
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "isInputDone", true } });
    }

    // RPC �Լ�
    [PunRPC]
    public void SetObstacleFlag(int indexRow, int indexCol)
    {
        //flag �����
        board[indexRow, indexCol].isObstacle = true;
    }
    [PunRPC]
    public void SetObstacle(int indexRow, int indexCol)
    {
        //
    }

}


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

    public bool isInput; // �Է� ���� flag
    public bool isSetObstacle; // �Է� ���� flag


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
        Debug.Log("�Է� ��:" + isInput);
        if (Input.GetMouseButtonDown(0) && isInput == true) // *** ��ü hit �Լ� �߰�
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                target = hit.collider.gameObject.GetComponent<Tile>();
            }
            if (transform.position == target.transform.position)
            { Debug.Log("�ٽ� �Է��ϻ�"); }
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

    

    // *Ŭ������ �Է¹ް� Ŭ���� Ŭ���� Ÿ���� isObstacle flag�� ��������
    /*public void InputObstacle()
    {
       
        
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
    
    }*/

    // *�̵��� �Է¹ް� �̵� ��θ� NewPlayer Ŭ������ ������ �ִ� pathBuffer ������ ����.
    public void InputMove(int diceNum)
    {
        // flag �� �� �Լ����� �ٲ��ְ� Update() ���� �Է��� �ޱ�
        // flag�� �Է��� ���� �� �մ� ���¸� ��Ÿ��
        // �̵� �Է¹ޱ�� InputObstacle() �Լ��� ���������� Update() ������ �ޱ�
        // ���������� flag �ʿ�
        // �Է¹����鼭 player ������Ʈ(ü�� ��) �̵� �ʿ�
        // �Ű������� ���� diceNum ��ŭ �Է� �ޱ�
        // ��� ���� �� �Է¹����� ���� ��ġ�� �̵�.

        int tileRow = 3;
        int tileCol = 3;
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


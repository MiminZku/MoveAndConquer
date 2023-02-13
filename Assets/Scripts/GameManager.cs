using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;
using UnityEngine.UI; // **** �߰�


public enum BattleState
{
    Start,
    Input,
    SetObstacle,
    Move,
    Finish
}


public class GameManager : MonoBehaviourPunCallbacks
{
    // �̱���
    public static GameManager Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<GameManager>();

            return instance;
        }
    }
    private static GameManager instance;
    // board ����
    [SerializeField] GameObject tiles;
    public Tile[,] board;
    const int boardRow = 9;
    const int boardCol = 5;
    // Turn üũ ����
    int mainTurnNum = 6;
    int currentTurn = 1;
    // ���� �ð� ����
    int maxTime = 30;
    int currentTime = 0;
    // �ֻ��� �� ���� ����
    int diceNum;
    // ���� ���� ���μ��� ����
    BattleState state;
    // Player ��ü�� ����.
    GameObject myPlayerObject;
    Player myPlayer;
    // player ����
    [SerializeField] Transform[] spawnPositions;
    [SerializeField] GameObject playerPrefab;
    // processing flag
    bool isProcessing = false;
    // obstacle ���� flag
    bool isObstacleSelected;
    // obstacle ��ư�� �����ϵ� move ��ư�� �����ϵ� �����ϸ� true
    bool isBtnSelected;
    // Dice ��ü -> �ϴ� �̻��
    //Dice dice;

    // ��ֹ� ��ü ���� **** �߰�
    [SerializeField] GameObject obstaclePrefab;

    // ��ư UI ��ü  **** �߰�
    [SerializeField] GameObject obstacleButton;

    // ��ư UI ��ü  **** �߰�
    [SerializeField] Text diceUi;


    void Start()
    {
        obstacleButton.SetActive(false);
        // board �ʱ�ȭ
        board = new Tile[boardRow, boardCol];
        for (int i = 0; i < boardRow; i++)
        {
            for (int j = 0; j < boardCol; j++)
            {
                Transform child = tiles.transform.GetChild(i * boardCol + j);
                board[i, j] = child.GetComponent<Tile>();

                board[i, j].tileIndex.row = i;
                board[i, j].tileIndex.col = j;
                Debug.Log("tile index : " + i + ", " + j);
            }
        }
        // player ����
        SpanwPlayer();
        // ���� �ʱ�ȭ
        state = BattleState.Start;
    }

    private void SpanwPlayer()
    {
        var localPlayerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        var spawnPosition = spawnPositions[localPlayerIndex % spawnPositions.Length];
        myPlayerObject = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition.position, spawnPosition.rotation);
        myPlayer = myPlayerObject.GetComponent<Player>();
    }

    void Update()
    {
        GameProcess();
    }

    private void GameProcess()
    {
        switch (state)
        {
            case BattleState.Start:
                state = BattleState.Input;
                break;
            case BattleState.Input:
                if (isProcessing) return;
                else isProcessing = true;
                StartCoroutine(InputProcess());
                break;
            case BattleState.SetObstacle:
                if (isProcessing) return;
                else isProcessing = true;
                // ���ӸŴ����� Ÿ�ϵ��� Ž���ؼ� ��ֹ� �÷��װ� �ִ� Ÿ�Ͽ� ��ֹ� ������ ��ġ
                StartCoroutine(SetObstacle());
                break;
            case BattleState.Move:
                // �̵� �Է¿� ���� �÷��̾�� �̵�
                // ������ ���̸� ���� Finish�� ����
                if (isProcessing) return;
                else isProcessing = true;
                StartCoroutine(AllMove());
                break;
            case BattleState.Finish:
                // ���� ����
                if (isProcessing) return;
                else isProcessing = true;
                StartCoroutine(Finish());
                break;
        }
    }
    // ��� player�� �Է��� �޾Ҵ��� üũ
    private bool EveryPlayerReady()
    {
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            Hashtable cp = player.CustomProperties;
            Debug.Log(cp["isMoveReady"]);
            if (!(bool)cp["isMoveReady"]) return false;
        }
        return true;
    }

    // *��ֹ� UI ǥ��
    private void ShowSelectUI()
    {
        obstacleButton.SetActive(true);
        // ��ֹ�, �̵� ���� UI �����
        // *��ֹ�, �̵� ���� UI ǥ�� -> SetActive() �Լ� �̿�
        // ��ֹ� ��ư ���ý� OnClick Listener -> OnClickObstacleBtn()
        // �̵� ��ư ���ý� OnClick Listener -> OnClickMoveBtn()

    }
    // *�ֻ��� UI ������Ʈ
    private void ChangeDiceUI()
    {
        diceUi.text = diceNum.ToString();
        // dice ���� ǥ���ϴ� UI �����
        // *dice ���� ǥ���ϴ� UI�� �޾ƿͼ� ������Ʈ
        // NewGameMgr �� ���� diceNum �̿�
    }
    // Time �پ��� �Լ�
    IEnumerator TimeCount()
    {
        // *Time UI ����
        ShowTimeUI();
        // *�ð� ���̸鼭 ���� �� UI�� ������Ʈ
        // yield ����ؼ� ����
        // currentTime, maxTime ���� �̿�
        // �ð��� �� ������ Time UI ��Ȱ��ȭ
        // battle state �� setObstacle�� ����
        if (currentTime >= maxTime)
        {

            // network
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "isInputDone", true } });
        }

        // �Ʒ� �ڵ�� ���� ������ ���� �ڵ� 
        yield return null;
    }
    // *Time UI ����
    void ShowTimeUI()
    {

    }
    // *Time UI �����
    void HideTimeUI()
    {

    }


    // dice Ŭ���� �����ҽ�
    void RollingDice()
    {
        // ������ Ŭ���̾�Ʈ������ ȣ��
        if (PhotonNetwork.IsMasterClient)
        {
            diceNum = Random.Range(1, 7);
            // dice �ִϸ��̼� �ʿ�
            Debug.Log("�ֻ��� ��������~");
            Debug.Log(diceNum + "��");
        }
    }

    // RPC �Լ��� ���⿡�� �����ص� �ǳ�
    // �ȵȴ�..
    //[PunRPC]
    //void SetDiceNum(int diceNum)
    //{
    //    this.diceNum = diceNum;
    //}

    void SyscDiceNum()
    {
        //PhotonView pv = myPlayerObject.GetPhotonView();
        //pv.RPC("SetDiceNum", RpcTarget.AllBuffered, diceNum);
    }


    // *OnClick Listener
    public void OnClickObstacleBtn()
    {
        isObstacleSelected = true;
        isBtnSelected = true;
        obstacleButton.SetActive(false);
        myPlayer.isInput = true;
        // *��ư Ŭ�� flag (isBtnSelected) ������ ����.
        // *Btn UI ��Ȱ��ȭ
    }
    public void OnClickMoveBtn()
    {
        isObstacleSelected = false;
        // *��ư Ŭ�� flag (isBtnSelected) ������ ����.
        // *Btn UI ��Ȱ��ȭ
    }

    // coroutine
    IEnumerator InputProcess()
    {
        // �ֻ��� ������ -> master client�� ȣ��
        RollingDice();  // delay �ֱ�
        yield return new WaitForSeconds(2f);

        // �ֻ��� ����ȭ
        SyscDiceNum();

        // *�ֻ��� �� �� UI�� ǥ��
        ChangeDiceUI();

        // UI�� ��ֹ� �������� �̵��Ұ��� �Է� ����
        // *��ֹ�, �̵� ���� UI ǥ��
        ShowSelectUI();

      /*  // *�ð� ���� �Լ�(�ð� count)
        StartCoroutine(TimeCount());

        // ��ư�� ���õ��� �ʰų� �ð��� �ʰ����� ������ ���
        while (!isBtnSelected || (currentTime >= maxTime))
        {
            yield return null;
        }
        // �Է¿� ���� �̵�, ��ֹ� ��ġ �Է� �Լ� ���� */
        if (isBtnSelected)
        {
            if (isObstacleSelected)
            {
                // *�̵� �������� ���, �÷��̾�� �̵� �Է� ����
                if (myPlayer.isSetObstacle == true)
                {
                    Debug.Log("�Է� �Ϸ�");
                }
            }
            else
            {
                // *��ֹ� ��ġ �������� ���, �÷��̾�� ��ֹ� ��ġ �Է� ����
                myPlayer.InputMove(diceNum);
            }
           
        }
        yield return new WaitForSeconds(10f);

 /*       // ��Ʈ��ũ�� ��� �÷��̾ �Է��� �Ϸ� �ƴ��� Ȯ��
        // if (EveryPlayerReady())
        // {
        //     state = BattleState.SetObstacle;
        //     isProcessing = false;
        // }
        // �Ѵ� �ϼ����� ������ ���
        while (!EveryPlayerReady())
        {
            yield return null;
        }*/
        state = BattleState.SetObstacle;
        isProcessing = false;
    }

    // *board�� ������ obstacle ��ġ
    IEnumerator SetObstacle()
    {
        
        for (int i = 0; i < boardRow; i++)
        {
            for (int j = 0; j < boardCol; j++)
            {
                if (board[i, j].isObstacle == true) Instantiate(obstaclePrefab, board[i, j].transform.position, board[i, j].transform.rotation);
            }
        }
        Debug.Log("��ġ �Ϸ�");
        // *isObstacle flag�� ������ �ִ� tile���� ã�� �� tile���� tileIndex ��������
        // tile�� index�� ���� ��ֹ� ��ġ
        // ��ֹ��� prefab���� �޾Ƽ� tile�� tileIndex�� �̿��ؼ� �ش� ��ġ�� ��ֹ� spawn


        // 2�� ���
        yield return new WaitForSeconds(2f);
        state = BattleState.Move;
        isProcessing = false;
    }

    // **��� �÷��̾� �̵��ϸ鼭 Ÿ�� ��ĥ 
    IEnumerator AllMove()
    {
        // *myPlayer ��� ������ �̿��ؼ� �̵� + ��ĥ
        // �̵� + ��ĥ�� RPC �Լ� �ȿ��� �����ؾ��ҵ�
        // ��ĥ�� ���� ���� �ʿ�
        // AllMove()�� �����ϱ� ���ؼ� Player�� �ʿ��� �Լ��� ������ ���� �����
        // �̵��� player�� buffer�� ���ؼ� �ش� ��η� �̵�
        // ���� buffer�� ��� ������ (�Է°��� ������) return
        // �̵��� delay �ʿ��ҵ� -> yield�� Ȱ��
        // *�̵��� board�� tile�� ��ֹ��� ��ġ�Ǿ� ������ �ش� �̵� �Ұ�
        // ��� �ý��� �ʿ� -> �ϴ� ������ ���� ���ϴ� �ɷ� (��Ʈ��ũ �κ��� ���� ����غ�����)


        // ���� ���� Ȯ��
        currentTurn++;

        if (currentTurn > mainTurnNum) state = BattleState.Finish;
        else state = BattleState.Input;
        isProcessing = false; 
        yield return null;
    }

    // **���� Ȯ��
    IEnumerator Finish()
    {
        
        // * board�� Ž���ؼ� player1�� player2�� ���� ã�� 
        // * ���� UI ǥ��
        yield return null;
    }



}
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
    public int diceNum;
    // ���� ���� ���μ��� ����
    BattleState state;
    // Player ��ü�� ����.
    GameObject myPlayerObject;
    Player myPlayer;
    // player ����
    public Transform[] spawnPositions;
    [SerializeField] GameObject playerPrefab;
    // processing flag
    bool isProgressing = false;
    // obstacle ���� flag
    bool isObstacleSelected;
    // obstacle ��ư�� �����ϵ� move ��ư�� �����ϵ� �����ϸ� true
    bool isBtnSelected;

    // ��ֹ� ��ü ���� **** �߰�
    [SerializeField] GameObject obstaclePrefab;
    [SerializeField] GameObject inputObstacleButton;
    [SerializeField] GameObject inputMoveButton;
    [SerializeField] Text diceText;
    [SerializeField] Text timeText;

    void Start()
    {
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
        if (PhotonNetwork.PlayerList.Length < 2) return;
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
                if (isProgressing) return;
                else isProgressing = true;
                StartCoroutine(InputProcess());
                break;
            case BattleState.SetObstacle:
                // ���ӸŴ����� Ÿ�ϵ��� Ž���ؼ� ��ֹ� �÷��װ� �ִ� Ÿ�Ͽ� ��ֹ� ������ ��ġ
                if (isProgressing) return;
                else isProgressing = true;
                StartCoroutine(SetObstacle());
                break;
            case BattleState.Move:
                // �̵� �Է¿� ���� �÷��̾�� �̵�
                // ������ ���̸� ���� Finish�� ����
                if (isProgressing) return;
                else isProgressing = true;
                StartCoroutine(AllMove());
                break;
            case BattleState.Finish:
                // ���� ����
                if (isProgressing) return;
                else isProgressing = true;
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
            Debug.Log(cp["isInputDone"]);
            if (!(bool)cp["isInputDone"]) return false;
        }
        return true;
    }

    // *��ֹ� UI ǥ��
    private void ShowSelectUI()
    {
        isBtnSelected = false;
        inputObstacleButton.SetActive(true);
        inputMoveButton.SetActive(true);
        // ��ֹ�, �̵� ���� UI �����
        // *��ֹ�, �̵� ���� UI ǥ�� -> SetActive() �Լ� �̿�
        // ��ֹ� ��ư ���ý� OnClick Listener -> OnClickObstacleBtn()
        // �̵� ��ư ���ý� OnClick Listener -> OnClickMoveBtn()
    }
    // *�ֻ��� UI ������Ʈ
    private void ChangeDiceUI()
    {
        diceText.text = "Dice : " + diceNum.ToString();
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
        }
    }

    void SyncDiceNum()
    {
        myPlayer.SyncDiceNum();
    }


    // *OnClick Listener
    public void OnClickObstacleBtn()
    {
        isBtnSelected = true;
        isObstacleSelected = true;
        inputObstacleButton.SetActive(false);
        inputMoveButton.SetActive(false);
        // *��ư Ŭ�� flag (isBtnSelected) ������ ����.
        // *Btn UI ��Ȱ��ȭ
    }
    public void OnClickMoveBtn()
    { 
        isBtnSelected = true;
        isObstacleSelected = false;
        inputObstacleButton.SetActive(false);
        inputMoveButton.SetActive(false);
        // *��ư Ŭ�� flag (isBtnSelected) ������ ����.
        // *Btn UI ��Ȱ��ȭ
    }

    // coroutine
    IEnumerator InputProcess()
    {
        // �ֻ��� ������ -> master client�� ȣ��
        RollingDice();  // delay �ֱ�
        yield return new WaitForSeconds(1f);

        // �ֻ��� ����ȭ
        SyncDiceNum();
        yield return new WaitForSeconds(1f);

        // *�ֻ��� �� �� UI�� ǥ��
        ChangeDiceUI();

        // UI�� ��ֹ� �������� �̵��Ұ��� �Է� ����
        // *��ֹ�, �̵� ���� UI ǥ��
        ShowSelectUI();

        // *�ð� ���� �Լ�(�ð� count)
        //StartCoroutine(TimeCount());

        // ��ư�� ���õ��� �ʰų� �ð��� �ʰ����� ������ ���
        while (!isBtnSelected || (currentTime >= maxTime))
        {
            yield return null;
        }
        // �Է¿� ���� �̵�, ��ֹ� ��ġ �Է� �Լ� ���� 
        if (isBtnSelected)
        {
            if (isObstacleSelected)
            {
                // *�̵� �������� ���, �÷��̾�� �̵� �Է� ����
                myPlayer.InputObstacle();
            }
            else
            {
                // *��ֹ� ��ġ �������� ���, �÷��̾�� ��ֹ� ��ġ �Է� ����
                myPlayer.InputMove(diceNum);
            }
        }
        // �Ѵ� �Է��� �Ϸ�� ������ ���
        while (!EveryPlayerReady())
        {
            yield return null;
        }
        state = BattleState.SetObstacle;
        isProgressing = false;
    }

    // *board�� ������ obstacle ��ġ
    IEnumerator SetObstacle()
    {
        // *isObstacle flag�� ������ �ִ� tile���� ã�� �� tile���� tileIndex ��������
        // tile�� index�� ���� ��ֹ� ��ġ
        // ��ֹ��� prefab���� �޾Ƽ� tile�� tileIndex�� �̿��ؼ� �ش� ��ġ�� ��ֹ� spawn
        for (int i = 0; i < boardRow; i++)
        {
            for (int j = 0; j < boardCol; j++)
            {
                if (board[i, j].isObstacle == true)
                    Instantiate(obstaclePrefab, board[i, j].transform.position, board[i, j].transform.rotation);
            }
        }

        // 2�� ���
        yield return new WaitForSeconds(2f);
        state = BattleState.Move;
        isProgressing = false;
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
        if (++currentTurn > mainTurnNum)
            state = BattleState.Finish;
        else
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "isInputDone", false } });
            state = BattleState.Input;
        }
        isProgressing = false;
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
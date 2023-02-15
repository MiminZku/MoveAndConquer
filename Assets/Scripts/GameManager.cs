using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;
using UnityEngine.UI;


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
    // 싱글톤
    public static GameManager Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<GameManager>();

            return instance;
        }
    }
    private static GameManager instance;
    // board 변수
    [SerializeField] GameObject tiles;
    public Tile[,] board;
    [HideInInspector] public int boardRow = 9;
    [HideInInspector] public int boardCol = 5;
    // Turn 체크 변수
    int mainTurnNum = 6;
    int currentTurn = 1;
    // 선택 시간 변수
    // int maxTime = 30;
    // int currentTime = 0;

    // 주사위 수 저장 변수
    public int diceNum { get; set;}
    // 현재 게임 프로세스 상태
    BattleState state;
    // Player 객체들 저장.
    GameObject myPlayerObject;
    Player myPlayer;
    // player 생성
    public Transform[] spawnPositions;
    [SerializeField] GameObject playerPrefab;
    // processing flag
    bool isProcessing = false;
    // obstacle 선택 flag
    bool isObstacleSelected;
    // obstacle 버튼을 선택하든 move 버튼을 선택하든 선택하면 true
    bool isBtnSelected;
    // Dice 객체 -> 일단 미사용
    //Dice dice;

    // Time 체크 변수 
    [HideInInspector] public float currentTime = 0;
    float startTime = 0;
    [HideInInspector] public float maxTime = 30f;
    bool isTimeCheck = false;
    [SerializeField] GameObject timeText;

    // 여러 필요한 레퍼런스 변수들
    [SerializeField] GameObject obstaclePrefab;
    [SerializeField] GameObject inputObstacleButton;
    [SerializeField] GameObject inputMoveButton;
    [SerializeField] Text diceText;

    void Start()
    {
        // board 초기화
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
        // player 생성
        SpanwPlayer();
        // 상태 초기화
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
        if(isTimeCheck) CheckTime();
        GameProcess();
    }

    // 시간 체크
    void CheckTime()
    {
        // 장애물과 이동 선택 UI 뜨는 순간부터 시간 재기 시작
        currentTime = Time.time - startTime;

        // 시간 줄이면서 줄인 값 UI에 업데이트
        Text txt = timeText.GetComponent<Text>();
        txt.text = "Time : " + (int)(maxTime - currentTime);

        // 시간이 다 지나면 Time UI 비활성화
        if (currentTime >= maxTime)
        {
            timeText.SetActive(false);
            myPlayer.TimeOver();
            // network
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "isInputDone", true } });
            // 시간 체크 flag false로 설정
            isTimeCheck = false;
        }

        // ShowTimeUI, HideTimeUI() 굳이 필요 없을듯
     
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
                // 게임매니저의 타일들을 탐색해서 장애물 플래그가 있는 타일에 장애물 실제로 설치
                if (isProcessing) return;
                else isProcessing = true;
                StartCoroutine(SetObstacle());
                break;
            case BattleState.Move:
                // 이동 입력에 따라서 플레이어에서 이동
                // 마지막 턴이면 상태 Finish로 변경
                if (isProcessing) return;
                else isProcessing = true;
                StartCoroutine(AllMove());
                break;
            case BattleState.Finish:
                // 승패 판정
                if (isProcessing) return;
                else isProcessing = true;
                StartCoroutine(Finish());
                break;
        }
    }
    // 모든 player가 입력을 받았는지 체크
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

    // *장애물 UI 표시
    private void ShowSelectUI()
    {
        // 장애물, 이동 선택 UI 만들기
        // *장애물, 이동 선택 UI 표시 -> SetActive() 함수 이용
        // 장애물 버튼 선택시 OnClick Listener -> OnClickObstacleBtn()
        // 이동 버튼 선택시 OnClick Listener -> OnClickMoveBtn()
        isBtnSelected = false;
        inputObstacleButton.SetActive(true);
        inputMoveButton.SetActive(true);

    }
    // *주사위 UI 업데이트
    private void ChangeDiceUI()
    {
        // dice 수를 표시하는 UI 만들기
        // *dice 수를 표시하는 UI를 받아와서 업데이트
        // NewGameMgr 의 변수 diceNum 이용
        diceText.text = "Dice : " + diceNum.ToString();
    }

    // Time 줄어드는 함수 -> 사용 안함
    // IEnumerator TimeCount()
    // {
    //     // *Time UI 띄우기
    //     ShowTimeUI();
    //     // *시간 줄이면서 줄인 값 UI에 업데이트
    //     // yield 사용해서 구현
    //     // currentTime, maxTime 변수 이용
    //     // 시간이 다 지나면 Time UI 비활성화
    //     // battle state 를 setObstacle로 변경
    //     if (currentTime >= maxTime)
    //     {

    //         // network
    //         PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "isInputDone", true } });
    //     }

    //     // 아래 코드는 오류 방지를 위한 코드 
    //     yield return null;
    // }

    // *Time UI 띄우기 -> 별로 필요없을듯
    void ShowTimeUI()
    {
        timeText.SetActive(true);
    }
    // *Time UI 숨기기
    void HideTimeUI()
    {
        timeText.SetActive(false);
    }


    // dice 클래스 사용안할시
    void RollingDice()
    {
        // 마스터 클라이언트에서만 호출
        if (PhotonNetwork.IsMasterClient)
        {
            diceNum = Random.Range(1, 7);
            // dice 애니메이션 필요
        }
    }

    void SyscDiceNum()
    {
        //PhotonView pv = myPlayerObject.GetPhotonView();
        //pv.RPC("SetDiceNum", RpcTarget.AllBuffered, diceNum);
        myPlayer.SyncDiceNum();
    }


    // *OnClick Listener
    public void OnClickObstacleBtn()
    {

        // *버튼 클릭 flag (isBtnSelected) 참으로 설정.
        // *Btn UI 비활성화
        isBtnSelected = true;
        isObstacleSelected = true;
        inputObstacleButton.SetActive(false);
        inputMoveButton.SetActive(false);
    }

    public void OnClickMoveBtn()
    {
        // *버튼 클릭 flag (isBtnSelected) 참으로 설정.
        // *Btn UI 비활성화
        isBtnSelected = true;
        isObstacleSelected = false;
        inputObstacleButton.SetActive(false);
        inputMoveButton.SetActive(false);
    }

    // coroutine
    IEnumerator InputProcess()
    {
        // 주사위 굴리기 -> master client만 호출
        RollingDice();  // delay 주기
        yield return new WaitForSeconds(2f);

        // 주사위 동기화
        SyscDiceNum();
        yield return new WaitForSeconds(2f);

        // *주사위 눈 수 UI에 표시
        ChangeDiceUI();

        // UI로 장애물 놓을건지 이동할건지 입력 받음
        // *장애물, 이동 선택 UI 표시
        ShowSelectUI();

        // *시간 제한 함수(시간 count)
        // StartCoroutine(TimeCount());
        // 시간 재는 flag true 설정
        isTimeCheck = true;
        startTime = Time.time;
        ShowTimeUI();

        // 버튼이 선택되지 않으면 대기
        // 하지만 버튼이 선택되지 않더라도 시간이 초과되면 빠져나오기 
        while (!isBtnSelected && currentTime <= maxTime)
        {
            yield return null;
        }
        // 입력에 따라 이동, 장애물 설치 입력 함수 실행 
        if (isBtnSelected)
        {
            if (isObstacleSelected)
            {
                // *이동 선택했을 경우, 플레이어에서 이동 입력 받음
                myPlayer.InputObstacle();
            }
            else
            {
                // *장애물 설치 선택했을 경우, 플레이어에서 장애물 설치 입력 받음
                myPlayer.InputMove(diceNum);
            }
        }

        // 둘다 완성되지 않으면 대기
        while (!EveryPlayerReady())
        {
            yield return null;
        }
        //
        isTimeCheck = false; 
        //
        state = BattleState.SetObstacle;
        isProcessing = false;
    }

    // *board를 뒤져서 obstacle 설치
    IEnumerator SetObstacle()
    {
        // *isObstacle flag가 새워져 있는 tile들을 찾고 그 tile들의 tileIndex 가져오기
        // tile의 index를 통해 장애물 설치
        // 장애물을 prefab으로 받아서 tile의 tileIndex을 이용해서 해당 위치에 장애물 spawn
        for (int i = 0; i < boardRow; i++)
        {
            for (int j = 0; j < boardCol; j++)
            {
                if (board[i, j].isObstacle == true)
                    Instantiate(obstaclePrefab, board[i, j].transform.position, board[i, j].transform.rotation);
            }
        }

        // 2초 대기
        yield return new WaitForSeconds(2f);
        state = BattleState.Move;
        isProcessing = false;
    }

    // **모든 플레이어 이동하면서 타일 색칠 
    IEnumerator AllMove()
    {
        // *myPlayer 멤버 변수를 이용해서 이동 + 색칠
        // 이동 + 색칠은 RPC 함수 안에서 구현해야할듯
        // 색칠시 예외 조건 필요
        // AllMove()를 구현하기 위해서 Player에 필요한 함수가 있을시 따로 만들기
        // 이동은 player의 buffer를 통해서 해당 경로로 이동
        // 만약 buffer가 비어 있으면 (입력값이 없으면) return
        // 이동시 delay 필요할듯 -> yield문 활용
        // *이동시 board의 tile에 장애물이 설치되어 있으면 해당 이동 불가
        // 잡기 시스템 필요 -> 일단 지금은 구현 안하는 걸로 (네트워크 부분이 들어가서 얘기해봐야함)
        // 모든 player move buffer를 바탕으로 move 

        Player[] players = FindObjectsOfType<Player>();
        int bigger = players[0].pathBuffer.Count > players[1].pathBuffer.Count ?
                        players[0].pathBuffer.Count : players[1].pathBuffer.Count;
        for (int j = 0; j < bigger; j++)
        {
            Index moveIndex;
            Index moveIndex1, moveIndex2;
            if (players[0].pathBuffer.Count != 0 && players[1].pathBuffer.Count != 0)
            {
                moveIndex1 = players[0].pathBuffer[j];
                moveIndex2 = players[1].pathBuffer[j];
                if(moveIndex1.row >= 0 && moveIndex2.row >= 0)
                {
                    // 충돌 타일은 기존 색 그대로 
                    SetCrashTile(moveIndex1, moveIndex2);
                }
            }
            foreach (Player p in players)
            {
                // buffer가 비어있으면 pass
                if (p.pathBuffer.Count == 0)    continue;
                
                moveIndex = p.pathBuffer[j];
                if (moveIndex.row < 0) continue;
                // 이동할 index의 타일이 장애물 타일이면 그 뒤에 경로도 없애고 pass
                if (board[moveIndex.row, moveIndex.col].isObstacle)
                {
                    p.pathBuffer.Clear();
                    continue;
                }
                // 이동하면서 타일 색칠
                p.Move(moveIndex);
            }
            // 충돌타일 설정 초기화
            for (int r = 0; r < boardRow; r++)
            {
                for (int c = 0; c < boardCol; c++)
                {
                    board[r, c].isCrash = false;
                }
            }
            // 자기 타일 색은 우선권 갖기
            foreach (Player p in players)
            {
                // buffer가 비어있으면 pass
                if (p.pathBuffer.Count == 0) continue;
                moveIndex = p.pathBuffer[j];
                if (moveIndex.row < 0) continue;
                if (p.photonView.IsMine)
                {
                    board[moveIndex.row, moveIndex.col].changeColor(1);
                }
                else
                {
                    board[moveIndex.row, moveIndex.col].changeColor(2);
                }
            }
            yield return new WaitForSeconds(1f);
        }
        // 전부 이동하면 buffer 초기화
        foreach(Player p in players)
        {
            p.pathBuffer.Clear();
        }


        // 종료 조건 확인
        if (++currentTurn > mainTurnNum)
            state = BattleState.Finish;
        else
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "isInputDone", false } });
            state = BattleState.Input;
        }
        isProcessing = false;
        yield return null;
    }

    private void SetCrashTile(Index moveIndex1, Index moveIndex2)
    {
        Index[] tileIndex1 = {moveIndex1,
            new Index(moveIndex1.row-1, moveIndex1.col),
            new Index(moveIndex1.row+1, moveIndex1.col),
            new Index(moveIndex1.row, moveIndex1.col-1),
            new Index(moveIndex1.row, moveIndex1.col+1)};
        Index[] tileIndex2 = {moveIndex2,
            new Index(moveIndex2.row-1, moveIndex2.col),
            new Index(moveIndex2.row+1, moveIndex2.col),
            new Index(moveIndex2.row, moveIndex2.col-1),
            new Index(moveIndex2.row, moveIndex2.col+1)};

        foreach(Index i in tileIndex1)
        {
            foreach(Index j in tileIndex2)
            {
                if (i.Equals(j)) board[i.row, i.col].isCrash = true;
            }
        }
    }

    // **승패 확인
    IEnumerator Finish()
    {

        // * board를 탐색해서 player1과 player2의 영역 찾기 
        // * 승패 UI 표시
        yield return null;
    }



}
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    UIManager uiMgr;
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
    public int diceNum { get; set; }
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
    // 말이 잡혀서 게임이 종료되는 경우 사용
    bool isGameOver = false;

    // Time 체크 변수 
    [HideInInspector] public float currentTime = 0;
    float startTime = 0;
    [SerializeField] float maxTime;
    bool isTimeCheck = false;
    [SerializeField] GameObject timeText;

    // 여러 필요한 레퍼런스 변수들
    [SerializeField] GameObject obstaclePrefab;
    [SerializeField] GameObject inputObstacleButton;
    [SerializeField] GameObject inputMoveButton;
    public GameObject inputButtons;
    [SerializeField] Text diceText;
    [SerializeField] Text turnText;
    [SerializeField] Text roomNameText;
    private bool isGameStart;

    void Start()
    {
        roomNameText.text = "Room : " + PhotonNetwork.CurrentRoom.Name;
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
        timeText.GetComponent<Text>().text = "" + maxTime;
        // UI Manager 가져오기
        uiMgr = UIManager.Instance;
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
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            if(isGameStart)
            {
                PhotonNetwork.LeaveRoom();
                SceneManager.LoadScene(0);
            }
            else
            {
                return;
            }
        }
        if (isTimeCheck) CheckTime();
        GameProcess();
    }

    // 시간 초기화
    void InitTime()
    {
        Debug.Log("InitTime");
        isTimeCheck = true;
        startTime = Time.time;
        currentTime = Time.time - startTime;
        ShowTimeUI();
    }
    // 시간 체크
    void CheckTime()
    {
        
        // 장애물과 이동 선택 UI 뜨는 순간부터 시간 재기 시작
        currentTime = Time.time - startTime;

        // 시간 줄이면서 줄인 값 UI에 업데이트
        Text txt = timeText.GetComponent<Text>();
        txt.text = ""+(int)(maxTime - currentTime  + 1);

        // 시간이 다 지나면 Time UI 비활성화
        if (currentTime >= maxTime)
        {
            Debug.Log("시간초과");
            // 선택 UI도 비활성화
            HideSelectUI();
            timeText.SetActive(false);
            // myPlayer.TimeOver();
            Player[] players = FindObjectsOfType<Player>();
            foreach(Player p in players)
            {
                p.TimeOver();
                Debug.Log("Time check에서 pathbuffer : " + p.pathBuffer.Count);
            }
            // 시간 체크 flag false로 설정
            isTimeCheck = false;
        }

    // ShowTimeUI, HideTimeUI() 굳이 필요 없을듯

}

    private void GameProcess()
    {
        if (!isGameStart) isGameStart = true;
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
            // Debug.Log(cp["isInputDone"]);
            if (!(bool)cp["isInputDone"]) return false;
        }
        return true;
    }

    // *장애물 UI 표시
    private void ShowSelectUI()
    {
        Debug.Log("ShowSelectUI");
        // 장애물, 이동 선택 UI 만들기
        // *장애물, 이동 선택 UI 표시 -> SetActive() 함수 이용
        // 장애물 버튼 선택시 OnClick Listener -> OnClickObstacleBtn()
        // 이동 버튼 선택시 OnClick Listener -> OnClickMoveBtn()
        isBtnSelected = false;
        inputObstacleButton.SetActive(true);
        inputMoveButton.SetActive(true);

    }
    // 장애물 UI 없애기
    private void HideSelectUI()
    {
        inputObstacleButton.SetActive(false);
        inputMoveButton.SetActive(false);
    }

    // *주사위 UI 업데이트
    private void ChangeDiceUI()
    {
        Debug.Log("ChangeDiceUI");
        // dice 수를 표시하는 UI 만들기
        // *dice 수를 표시하는 UI를 받아와서 업데이트
        // NewGameMgr 의 변수 diceNum 이용
        diceText.text = " : " + diceNum.ToString();
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
        Debug.Log("ShowTimeUI");
        timeText.SetActive(true);
    }
    // *Time UI 숨기기
    void HideTimeUI()
    {
        Debug.Log("HideTimeUI");
        timeText.SetActive(false);
    }


    // dice 클래스 사용안할시
    void RollingDice()
    {
        Debug.Log("RollingDice");
        // 마스터 클라이언트에서만 호출
        if (PhotonNetwork.IsMasterClient)
        {
            diceNum = Random.Range(1, 7);
            // dice 애니메이션 필요
        }
    }

    void SyscDiceNum()
    {
        Debug.Log("SyscDiceNum");
        //PhotonView pv = myPlayerObject.GetPhotonView();
        //pv.RPC("SetDiceNum", RpcTarget.AllBuffered, diceNum);
        myPlayer.SyncDiceNum();
    }


    // *OnClick Listener
    public void OnClickObstacleBtn()
    {
        Debug.Log("OnClickObstacleBtn");
        StartCoroutine(uiMgr.BlinkObstacleHelperToast());
        // *버튼 클릭 flag (isBtnSelected) 참으로 설정.
        // *Btn UI 비활성화
        isBtnSelected = true;
        isObstacleSelected = true;
        inputObstacleButton.SetActive(false);
        inputMoveButton.SetActive(false);
        inputButtons.SetActive(true);
        Debug.Log("isBtnSelected : " + isBtnSelected);
        Debug.Log("isObstacleSelected : " + isObstacleSelected);
    }

    public void OnClickMoveBtn()
    {
        Debug.Log("OnClickMoveBtn");
        StartCoroutine(uiMgr.BlinkMoveHelperToast());
        // *버튼 클릭 flag (isBtnSelected) 참으로 설정.
        // *Btn UI 비활성화
        isBtnSelected = true;
        isObstacleSelected = false;
        inputObstacleButton.SetActive(false);
        inputMoveButton.SetActive(false);
        inputButtons.SetActive(true);
        myPlayer.transform.GetChild(0).gameObject.SetActive(true);
        Debug.Log("isBtnSelected : " + isBtnSelected);
        Debug.Log("isObstacleSelected : " + isObstacleSelected);
    }

    // coroutine
    IEnumerator InputProcess()
    {
        // 현재 턴 잠깐 표시
        yield return new WaitForSeconds(2f);
        uiMgr.UpdateTurnToastText(currentTurn);
        uiMgr.ShowTurnToast();
        yield return new WaitForSeconds(1.5f);
        uiMgr.HideTurnToast();
        

        ChangeTurnUI();
        // 주사위 굴리기 -> master client만 호출
        RollingDice();  // delay 주기
        yield return new WaitForSeconds(1f);

        // 주사위 동기화
        SyscDiceNum();
        yield return new WaitForSeconds(1f);

        // 주사위 눈 수 UI에 표시
        ChangeDiceUI();
        // 주사위 toast 띄우기
        uiMgr.UpdateDiceToastText(diceNum);
        uiMgr.ShowDiceToast();
        yield return new WaitForSeconds(1.5f);
        uiMgr.HideDiceToast();

        // UI로 장애물 놓을건지 이동할건지 입력 받음
        // *장애물, 이동 선택 UI 표시
        yield return new WaitForSeconds(1f);
        ShowSelectUI();
        StartCoroutine(uiMgr.BlinkChoiceHelperToast());

        // *시간 제한 함수(시간 count)
        // 시간 재거나 필요한 변수들 초기화
        InitTime();

        // 버튼이 선택되지 않으면 대기
        // 하지만 버튼이 선택되지 않더라도 시간이 초과되면 빠져나오기 
        Debug.Log("currentTime 전: " + currentTime);
        while (!isBtnSelected && currentTime <= maxTime)
        {
            yield return null;
        }
        Debug.Log("currentTime 후: " + currentTime);
        // 입력에 따라 이동, 장애물 설치 입력 함수 실행 
        if (isBtnSelected)
        {
            if (isObstacleSelected)
            {
                // *이동 선택했을 경우, 플레이어에서 이동 입력 받음
                Debug.Log("장애물 선택");
                myPlayer.InputObstacle();
            }
            else
            {
                Debug.Log("이동 선택");
                // *장애물 설치 선택했을 경우, 플레이어에서 장애물 설치 입력 받음
                myPlayer.InputMove(diceNum);
            }
        }
        else // 버튼을 누르지 않았는데 시간이 초과되는 경우
        {
            //// 선택 UI도 비활성화
            //HideSelectUI();
            //timeText.SetActive(false);
            //myPlayer.TimeOver();
            //yield return new WaitForSeconds(2f);
            //// network
            //PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "isInputDone", true } });
        }
 
        // 둘다 완성되지 않으면 대기
        while (!EveryPlayerReady())
        {
            // 대기중인데 내 cp가 참이면 UI 띄우기
            Hashtable cp = PhotonNetwork.LocalPlayer.CustomProperties;
            if ((bool)cp["isInputDone"]) uiMgr.ShowWaitBar();
            yield return null;
        }
        uiMgr.HideWaitBar();
  

        // 시간이 초과되지 않아도 다 입력이 완료되면 time check 비활성화 
        yield return new WaitForSeconds(1.5f);
        if(EveryPlayerReady()) isTimeCheck = false;

        // yield return new WaitForSeconds(1.5f);

        state = BattleState.SetObstacle;
        isProcessing = false;
        inputButtons.SetActive(false);
        myPlayer.transform.GetChild(0).gameObject.SetActive(false);
    }

    private void ChangeTurnUI()
    {
        turnText.text = "Turn : " + currentTurn;
    }

    // *board를 뒤져서 obstacle 설치
    IEnumerator SetObstacle()
    {
        Debug.Log("SetObstacle");
        // *isObstacle flag가 새워져 있는 tile들을 찾고 그 tile들의 tileIndex 가져오기
        // tile의 index를 통해 장애물 설치
        // 장애물을 prefab으로 받아서 tile의 tileIndex을 이용해서 해당 위치에 장애물 spawn
        for (int i = 0; i < boardRow; i++)
        {
            for (int j = 0; j < boardCol; j++)
            {
                if (board[i, j].isObstacleInput == true && board[i,j].isObstacleSet == false)
                {
                    Vector3 spawnPos = board[i, j].transform.position;
                    spawnPos.y = 15;
                    Instantiate(obstaclePrefab, spawnPos, board[i, j].transform.rotation);
                    board[i, j].GetComponent<Tile>().isObstacleSet = true;
                }
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
        Debug.Log("AllMove");
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
        
        myPlayer.RestoreTileColor();
        Player[] players = FindObjectsOfType<Player>();
        Debug.Log("Player 0 : " + players[0].pathBuffer.Count);
        Debug.Log("Player 1 : " + players[1].pathBuffer.Count);
        Debug.Log("MyPlayer : " + myPlayer.pathBuffer.Count);
        //int bigger = players[0].pathBuffer.Count > players[1].pathBuffer.Count ?
        //                players[0].pathBuffer.Count : players[1].pathBuffer.Count;
        for (int j = 0; j < diceNum; j++)
        {
            Index moveIndex;
            Index moveIndex1, moveIndex2;

            if (players[0].pathBuffer[j].row >= 0 && players[1].pathBuffer[j].row >= 0)
            {
                moveIndex1 = players[0].pathBuffer[j];
                moveIndex2 = players[1].pathBuffer[j];

                // 서로 이동할 경로가 겹치는지 확인
                if(moveIndex1.Equals(moveIndex2))
                {
                    // 충돌 애니매이션 재생
                    Debug.Log("충돌 모든 이동 중지");
                    break;
                }

                //// 충돌 확인
                if (moveIndex1.row >= 0 && moveIndex2.row >= 0)
                {
                    // 충돌 타일은 기존 색 그대로 
                    SetCrashTile(moveIndex1, moveIndex2);
                }
            }


            // 움직이기
            foreach (Player p in players)
            {
                // buffer가 비어있으면 pass
                //if (p.pathBuffer.Count == 0) continue;

                moveIndex = p.pathBuffer[j];
                if (moveIndex.row < 0) continue;
                // 이동할 index의 타일이 장애물 타일이면 그 뒤에 경로도 없애고 pass
                if (board[moveIndex.row, moveIndex.col].isObstacleInput)
                {
                    //p.pathBuffer.Clear();
                    for(int k = j; k < diceNum; k++)
                    {
                        p.pathBuffer[k] = new Index(-1, -1);
                    }
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
                    board[moveIndex.row, moveIndex.col].Flip(1);
                }
                else
                {
                    board[moveIndex.row, moveIndex.col].Flip(2);
                }
            }
            yield return new WaitForSeconds(1f); //한칸 이동
            
            // ---서로 겹치는지 확인 && 겹치면 장애물 설치한 player가 짐
            //if(players[0].currentIndex.Equals(players[1].currentIndex))
            //{
            //    Debug.Log("Equals 함수 테스트: " + (players[0].currentIndex.Equals(players[1].currentIndex)));
            //    Debug.Log("Player 0 : " + players[0].pathBuffer.Count);
            //    Debug.Log("Player 1 : " + players[1].pathBuffer.Count);
            //    foreach( Player p in players)
            //    {
            //        // buffer가 비어있으면 장애물 선택
            //        if(p.pathBuffer.Count == 0)
            //        {
            //            isGameOver = true;
            //            p.isGameLose = true;
            //            Destroy(p.gameObject);
            //            // 이중 for 문 빠져나오기
            //            j = bigger;
            //            break;
            //        }                   
            //    } 
            //}
            // ---

            // -- Index의 값의 부호가 서로 다르고 나의 현재 위치와 상대방의 현재 위치가 같으면 잡힘
            if((players[0].pathBuffer[j].row*players[1].pathBuffer[j].row) < 0 &&
                players[0].currentIndex.Equals(players[1].currentIndex))
            {
                Debug.Log("새로 만듬");
                for(int i = 0; i < players.Length; i++)
                {
                    if(players[i].pathBuffer[j].row < 0)
                    {
                        if(players[i].photonView.IsMine) // local이 잡아 먹힘
                        {
                            Debug.Log("내가 잡아 먹힘");
                        }
                        else
                        {
                            Debug.Log("내가 잡아 먹음");
                        }
                        // players[i].gameObject.SetActive(false);
                        isGameOver = true;
                        players[i].isGameLose = true;
                        state = BattleState.Finish;
                        isProcessing = false;
                        yield break;
                        // 이중 for 문 빠져나오기
                        //j = bigger;
                        //break;
                    }
                }
            }
            // --
        }
        
        // 전부 이동하면 buffer 초기화
        foreach (Player p in players)
        {
            p.pathBuffer.Clear();
            p.previousColors.Clear();
        }
        myPlayer.UpdateIndex();

        // 종료 조건 확인
        if (++currentTurn > mainTurnNum || isGameOver)
            state = BattleState.Finish;
        else
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "isInputDone", false } });
            state = BattleState.Input;
        }
        isProcessing = false;
    }

    private void SetCrashTile(Index moveIndex1, Index moveIndex2)
    {
        Debug.Log("SetCrashTile");
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

        foreach (Index i in tileIndex1)
        {
            foreach (Index j in tileIndex2)
            {
                if (i.Equals(j)) board[i.row, i.col].isCrash = true;
            }
        }
    }

    // **승패 확인
    IEnumerator Finish()
    {
        // 잡혀서 죽는 경우 -> 여기 UI 아직 안 만듬
        if(isGameOver)
        {
            Player[] players = FindObjectsOfType<Player>();
            foreach(Player p in players)
            {
                if(p.isGameLose)
                {
                    if(p.photonView.IsMine)
                    {
                        Debug.Log("내가 짐");
                        uiMgr.ShowCatchEndWindow();
                        uiMgr.ShowCatchLoseText();
                    }
                        p.gameObject.SetActive(false);
                }
                else
                {
                    if(p.photonView.IsMine)
                    {
                        Debug.Log("내가 이김");
                        uiMgr.ShowCatchEndWindow();
                        uiMgr.ShowCatchWinText();
                    }
                }
            
            }
            yield break;
        }
        

        // * board를 탐색해서 player1과 player2의 영역 찾기
        int myTiles = 0; int opponentTiles = 0;
        for (int i = 0; i < boardRow; i++)
        {
            for(int j = 0; j < boardCol; j++)
            {
                if( board[i,j].color == Tile.TileColor.BLUE )
                {
                    myTiles += 1;
                }
                else if(board[i,j].color == Tile.TileColor.RED)
                {
                    opponentTiles += 1;
                }
            }
        }
        Debug.Log(string.Format("내가 칠한 타일 수 : {0}", myTiles));
        Debug.Log(string.Format("상대가 칠한 타일 수 : {0}", opponentTiles));

        // Game over 창 띄우기
        uiMgr.showGameoverWindow();
        // 포인트 update
        uiMgr.BluePointUpdate(myTiles);
        uiMgr.RedPointUpdate(opponentTiles);
        // 승패 UI 표시
        if (myTiles > opponentTiles)
        {
            uiMgr.ShowWinText();
            Debug.Log("승리!!");
        }
        else if(myTiles < opponentTiles)
        {
            uiMgr.ShowLoseText();
            Debug.Log("패배...");
        }
        else
        {
            uiMgr.ShowDrawText();
            Debug.Log("무승부!");
        }
        yield break;
    }

    public void OnClickLobbyButton()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }

    public void OnClickResetButton()
    {
        myPlayer.ResetInput();
    }

    public void OnClickConfirmButton()
    {
        myPlayer.ConfirmInput();
        inputButtons.SetActive(false);
    }

}
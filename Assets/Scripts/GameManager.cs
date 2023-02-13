using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;
using UnityEngine.UI; // **** 추가


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
    const int boardRow = 9;
    const int boardCol = 5;
    // Turn 체크 변수
    int mainTurnNum = 6;
    int currentTurn = 1;
    // 선택 시간 변수
    int maxTime = 30;
    int currentTime = 0;
    // 주사위 수 저장 변수
    int diceNum;
    // 현재 게임 프로세스 상태
    BattleState state;
    // Player 객체들 저장.
    GameObject myPlayerObject;
    Player myPlayer;
    // player 생성
    [SerializeField] Transform[] spawnPositions;
    [SerializeField] GameObject playerPrefab;
    // processing flag
    bool isProcessing = false;
    // obstacle 선택 flag
    bool isObstacleSelected;
    // obstacle 버튼을 선택하든 move 버튼을 선택하든 선택하면 true
    bool isBtnSelected;
    // Dice 객체 -> 일단 미사용
    //Dice dice;

    // 장애물 객체 저장 **** 추가
    [SerializeField] GameObject obstaclePrefab;

    // 버튼 UI 객체  **** 추가
    [SerializeField] GameObject obstacleButton;

    // 버튼 UI 객체  **** 추가
    [SerializeField] Text diceUi;


    void Start()
    {
        obstacleButton.SetActive(false);
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
                // 게임매니저의 타일들을 탐색해서 장애물 플래그가 있는 타일에 장애물 실제로 설치
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
            Debug.Log(cp["isMoveReady"]);
            if (!(bool)cp["isMoveReady"]) return false;
        }
        return true;
    }

    // *장애물 UI 표시
    private void ShowSelectUI()
    {
        obstacleButton.SetActive(true);
        // 장애물, 이동 선택 UI 만들기
        // *장애물, 이동 선택 UI 표시 -> SetActive() 함수 이용
        // 장애물 버튼 선택시 OnClick Listener -> OnClickObstacleBtn()
        // 이동 버튼 선택시 OnClick Listener -> OnClickMoveBtn()

    }
    // *주사위 UI 업데이트
    private void ChangeDiceUI()
    {
        diceUi.text = diceNum.ToString();
        // dice 수를 표시하는 UI 만들기
        // *dice 수를 표시하는 UI를 받아와서 업데이트
        // NewGameMgr 의 변수 diceNum 이용
    }
    // Time 줄어드는 함수
    IEnumerator TimeCount()
    {
        // *Time UI 띄우기
        ShowTimeUI();
        // *시간 줄이면서 줄인 값 UI에 업데이트
        // yield 사용해서 구현
        // currentTime, maxTime 변수 이용
        // 시간이 다 지나면 Time UI 비활성화
        // battle state 를 setObstacle로 변경
        if (currentTime >= maxTime)
        {

            // network
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "isInputDone", true } });
        }

        // 아래 코드는 오류 방지를 위한 코드 
        yield return null;
    }
    // *Time UI 띄우기
    void ShowTimeUI()
    {

    }
    // *Time UI 숨기기
    void HideTimeUI()
    {

    }


    // dice 클래스 사용안할시
    void RollingDice()
    {
        // 마스터 클라이언트에서만 호출
        if (PhotonNetwork.IsMasterClient)
        {
            diceNum = Random.Range(1, 7);
            // dice 애니메이션 필요
            Debug.Log("주사위 굴러가요~");
            Debug.Log(diceNum + "번");
        }
    }

    // RPC 함수를 여기에서 정의해도 되나
    // 안된다..
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
        // *버튼 클릭 flag (isBtnSelected) 참으로 설정.
        // *Btn UI 비활성화
    }
    public void OnClickMoveBtn()
    {
        isObstacleSelected = false;
        // *버튼 클릭 flag (isBtnSelected) 참으로 설정.
        // *Btn UI 비활성화
    }

    // coroutine
    IEnumerator InputProcess()
    {
        // 주사위 굴리기 -> master client만 호출
        RollingDice();  // delay 주기
        yield return new WaitForSeconds(2f);

        // 주사위 동기화
        SyscDiceNum();

        // *주사위 눈 수 UI에 표시
        ChangeDiceUI();

        // UI로 장애물 놓을건지 이동할건지 입력 받음
        // *장애물, 이동 선택 UI 표시
        ShowSelectUI();

      /*  // *시간 제한 함수(시간 count)
        StartCoroutine(TimeCount());

        // 버튼이 선택되지 않거나 시간이 초과되지 않으면 대기
        while (!isBtnSelected || (currentTime >= maxTime))
        {
            yield return null;
        }
        // 입력에 따라 이동, 장애물 설치 입력 함수 실행 */
        if (isBtnSelected)
        {
            if (isObstacleSelected)
            {
                // *이동 선택했을 경우, 플레이어에서 이동 입력 받음
                if (myPlayer.isSetObstacle == true)
                {
                    Debug.Log("입력 완료");
                }
            }
            else
            {
                // *장애물 설치 선택했을 경우, 플레이어에서 장애물 설치 입력 받음
                myPlayer.InputMove(diceNum);
            }
           
        }
        yield return new WaitForSeconds(10f);

 /*       // 네트워크에 모든 플레이어가 입력이 완료 됐는지 확인
        // if (EveryPlayerReady())
        // {
        //     state = BattleState.SetObstacle;
        //     isProcessing = false;
        // }
        // 둘다 완성되지 않으면 대기
        while (!EveryPlayerReady())
        {
            yield return null;
        }*/
        state = BattleState.SetObstacle;
        isProcessing = false;
    }

    // *board를 뒤져서 obstacle 설치
    IEnumerator SetObstacle()
    {
        
        for (int i = 0; i < boardRow; i++)
        {
            for (int j = 0; j < boardCol; j++)
            {
                if (board[i, j].isObstacle == true) Instantiate(obstaclePrefab, board[i, j].transform.position, board[i, j].transform.rotation);
            }
        }
        Debug.Log("설치 완료");
        // *isObstacle flag가 새워져 있는 tile들을 찾고 그 tile들의 tileIndex 가져오기
        // tile의 index를 통해 장애물 설치
        // 장애물을 prefab으로 받아서 tile의 tileIndex을 이용해서 해당 위치에 장애물 spawn


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


        // 종료 조건 확인
        currentTurn++;

        if (currentTurn > mainTurnNum) state = BattleState.Finish;
        else state = BattleState.Input;
        isProcessing = false; 
        yield return null;
    }

    // **승패 확인
    IEnumerator Finish()
    {
        
        // * board를 탐색해서 player1과 player2의 영역 찾기 
        // * 승패 UI 표시
        yield return null;
    }



}
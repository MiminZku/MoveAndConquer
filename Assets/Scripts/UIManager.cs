using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 여러 UI들 관리 
// Manager이므로 싱글톤

public class UIManager : MonoBehaviour
{
    public static UIManager Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<UIManager>();

            return instance;
        }
    }
    private static UIManager instance;


    // UI 들
    [SerializeField] GameObject waitBar;
    [SerializeField] GameObject gameoverWindow;
    [SerializeField] GameObject loseText;
    [SerializeField] GameObject drawText;
    [SerializeField] GameObject winText;
    [SerializeField] Text bluePointText;
    [SerializeField] Text redPointText;
    [SerializeField] GameObject catchEndWindow;
    [SerializeField] GameObject catchWinText;
    [SerializeField] GameObject catchLoseText;

    [SerializeField] GameObject startToast;
    [SerializeField] GameObject diceToast;
    [SerializeField] Text diceToastText;
    [SerializeField] GameObject turnToast;
    [SerializeField] Text turnToastText;
    [SerializeField] GameObject moveHelperToast;
    [SerializeField] GameObject obstacleHelperToast;
    [SerializeField] GameObject choiceHelperToast;
    [SerializeField] GameObject setObstacleHelperToast;
    [SerializeField] GameObject playerTagBottom_I;
    [SerializeField] GameObject playerTagBottom_Other; 
    [SerializeField] GameObject playerTagUp_I; 
    [SerializeField] GameObject playerTagUp_Other; 




    public void ShowWaitBar()
    {
        waitBar.SetActive(true);
    }

    public void HideWaitBar()
    {
        waitBar.SetActive(false);
    }

    public void showGameoverWindow()
    {
        gameoverWindow.SetActive(true);
    }

    public void ShowLoseText()
    {
        loseText.SetActive(true);
    }

    public void ShowWinText()
    {
        winText.SetActive(true);
    }
    public void ShowDrawText()
    {
        drawText.SetActive(true);
    }

    public void BluePointUpdate(int n)
    {
        bluePointText.text = "" + n;
    }

    public void RedPointUpdate(int n)
    {
        redPointText.text = "" + n;
    }
    public void ShowCatchEndWindow()
    {
        catchEndWindow.SetActive(true);
    }
    public void ShowCatchWinText()
    {
        catchWinText.SetActive(true);
    }

    public void ShowCatchLoseText()
    {
        catchLoseText.SetActive(true);
    }
    public void ShowDiceToast()
    {
        diceToast.SetActive(true);
    }
    public void HideDiceToast()
    {
        diceToast.SetActive(false);
    }
    public void UpdateDiceToastText(int n)
    {
        diceToastText.text = ": " + n;
    }

    public void ShowTurnToast()
    {
        turnToast.SetActive(true);
    }
    public void HideTurnToast()
    {
        turnToast.SetActive(false);
    }
    public void UpdateTurnToastText(int n)
    {
        turnToastText.text = "Turn " + n;
    }
    public void ShowStartToast()
    {
        startToast.SetActive(true);
    }
    public void HideStartToast()
    {
        startToast.SetActive(false);
    }
    public IEnumerator BlinkMoveHelperToast()
    {
        moveHelperToast.SetActive(true);
        yield return new WaitForSeconds(3f);
        moveHelperToast.SetActive(false);
    }
    public IEnumerator BlinkObstacleHelperToast()
    {
        obstacleHelperToast.SetActive(true);
        yield return new WaitForSeconds(3f);
        obstacleHelperToast.SetActive(false);
    }
    public IEnumerator BlinkChoiceHelperToast()
    {
        choiceHelperToast.SetActive(true);
        yield return new WaitForSeconds(3f);
        choiceHelperToast.SetActive(false);
    }
    public IEnumerator BlinkSetObstacleHelperToast()
    {
        Debug.Log("Set Obstacle");
        setObstacleHelperToast.SetActive(true);
        yield return new WaitForSeconds(2f);
        setObstacleHelperToast.SetActive(false);
    }

    public IEnumerator BlinkMyPlayerIsUp()
    {
        Debug.Log("My Player is Up");
        playerTagUp_I.SetActive(true);
        playerTagBottom_Other.SetActive(true);
        yield return new WaitForSeconds(5f);
        playerTagUp_I.SetActive(false);
        playerTagBottom_Other.SetActive(false);
    }
    public IEnumerator BlinkMyPlayerIsBottm()
    {
        Debug.Log("My Player is bottom");
        playerTagBottom_I.SetActive(true);
        playerTagUp_Other.SetActive(true);
        yield return new WaitForSeconds(5f);
        playerTagBottom_I.SetActive(false);
        playerTagUp_Other.SetActive(false);
    }

    // --일단 사용 안함
    public void ShowMoveHelperToast()
    {
        moveHelperToast.SetActive(true);
    }
    public void HideMoveHelperToast()
    {
        moveHelperToast.SetActive(false);
    }
    public void ShowObstacleHelperToast()
    {
        obstacleHelperToast.SetActive(true);
    }
    public void HideObstacleHelperToast()
    {
        obstacleHelperToast.SetActive(false);
    }
    public void ShowChoiceHelperToast()
    {
        choiceHelperToast.SetActive(true);
    }
    public void HideChoiceHelperToast()
    {
        choiceHelperToast.SetActive(false);
    }
    // --


    // 버튼 onclicklistener 만들기
    public void OnClickCheckBtn()
    {
        gameoverWindow.SetActive(false);
    }
    public void OnClickCatchCkBtn()
    {
        catchEndWindow.SetActive(false);
    }
    
}

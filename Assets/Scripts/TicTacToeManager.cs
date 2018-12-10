using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum PlayerType { PlayerOne, PlayerTwo }
public enum GameState
{
    None,
    PlayerTurn,
    OpponentTurn,
    GameOver,
    OpponentDisconnect,
}
public enum Player { Player, Opponent }

public class TicTacToeManager : MonoBehaviour
{
    public Sprite circleSprite;     // O 스프라이트
    public Sprite crossSprite;      // X 스프라이트
    public GameObject[] cells;      // Cell 정보를 담을 배열
    public RectTransform gameOverPanel; 

    enum Mark { Circle, Cross }
    PlayerType playerType;
    int[] cellStates = new int[9]; // o,x 값 정보
    TicTacToeNetwork networkManager;

    int rowNum = 3;
  
    // 게임 승패
    enum Winner { None, Player, Opponent, Tie }
    // 게임 상태
    GameState gameState;

    private void Awake()
    {
        gameState = GameState.None;
    }

    private void Start()
    {
        networkManager = GetComponent<TicTacToeNetwork>();

        // cell 정보 배열 초기화
        for (int i = 0; i < cellStates.Length; i++)
        {
            cellStates[i] = -1;
        }
    }

    private void Update()
    {
        if (gameState == GameState.PlayerTurn)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 pos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                RaycastHit2D hitInfo = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(pos), Vector2.zero);


                if (hitInfo)
                {
                    GameObject cell = hitInfo.transform.gameObject;
                    int cellIndex = int.Parse(cell.name);
                    // 서버에 Cell Index 전달
                    networkManager.DoPlayer(cellIndex);
                    // TODO: 셀에 O 혹은 X 표시하기
                    DrawMark(cellIndex, Player.Player);
                }
            }
        }
    }

    public void DrawMark(int cellIndex, Player player)
    {
        Sprite markSprite;
        if (player == Player.Player)
        {
            markSprite =
                playerType == PlayerType.PlayerOne ? circleSprite : crossSprite;

            cellStates[cellIndex] =
                playerType == PlayerType.PlayerOne ? 0 : 1;
        }
        else
        {
            markSprite =
                playerType == PlayerType.PlayerOne ? crossSprite : circleSprite;

            cellStates[cellIndex] =
                playerType == PlayerType.PlayerOne ? 1 : 0;
        }

        //// 해당 Cell에 Sprite 할당하기
        //cells[cellIndex].GetComponent<SpriteRenderer>().sprite = markSprite;
        //// 할당된 Cell을 터치가 불가능하게 비활성
        //cells[cellIndex].GetComponent<BoxCollider2D>().enabled = false;

        GameObject go = cells[cellIndex];
        go.GetComponent<SpriteRenderer>().sprite = markSprite;
        go.GetComponent<BoxCollider2D>().enabled = false;
        //크기 두배로 키운 후 다시 1배로 돌려놓기
        go.transform.DOScale(2, 0).OnComplete(() =>
        {
            go.transform.DOScale(1, 1).SetEase(Ease.OutBounce);
            go.GetComponent<SpriteRenderer>().DOFade(1, 1);

            Camera.main.DOShakePosition(0.5f, 1, 15);
 
        });

        Winner result = CheckWinner();

        //턴변경
        //1. 게임이 진행중인 상태이어야 한다.(승패 결정 전)
        if (result == Winner.None)
        {
            if (gameState == GameState.PlayerTurn)
            {
                gameState = GameState.OpponentTurn;
            }
            else if (gameState == GameState.OpponentTurn)
            {
                gameState = GameState.PlayerTurn;
            }           
        }
        else
        {
            gameState = GameState.GameOver;
            //게임오버 패널 보이기
            ShowGameOverPanel();

            //게임의 결과 화면에 표시
            if (result == Winner.Player)
            {
                Debug.Log("Player Win!!");
            }
            else if (result == Winner.Opponent)
            {
                Debug.Log("Opponent Win!!");

            }
            else if (result == Winner.Tie)
            {
                Debug.Log("Tie!!");

            }
        }
    }

    Winner CheckWinner() //승패계산 함수 구현
    {
        int playerTypeValu = (int)playerType;

        //가로방향 체크
        for (int y = 0; y < rowNum; ++y)//현재 rowNum이 3이니 총 3번 반복
        {
            int mark = cellStates[y * rowNum];
            int num = 0; // 같은 것이 몇개 인지
            for (int x = 0; x < rowNum; ++x)//현재 rowNum이 3이니 총 3번 반복
            {
                int index = y * rowNum + x; //0,1,2 / 3,4,5 / 6,7,8
                if (mark == cellStates[index])
                {
                    ++num;
                }
            }
            if (mark != -1 && num == rowNum)//이상태는 3개연속 mark값이 동일할 때
            {
                int markValue = (int)mark;
                return (playerTypeValu == markValue) ? Winner.Player : Winner.Opponent;
            }
        }
        //세로방향 체크
        for (int x = 0; x < rowNum; x++) 
        {
            int mark = cellStates[x]; 
            int num = 0; // 같은 것이 몇개 인지
            for (int y = 0; y < rowNum; y++)
            {
                int index = y * rowNum + x;//0,3,6 /1,4,7/ 2,5,8
                if (mark == cellStates[index])
                {
                    ++num;
                }
            }
            if (mark != -1 && num == rowNum)
            {
                int markValue = (int)mark;
                return (playerTypeValu == markValue) ? Winner.Player : Winner.Opponent;
            }
        }

        { //코드블럭 시작 : 그래서 하단에서 mark를 중복해서 선언할 수 있음
            int mark = cellStates[0];
            int num = 0;
            for (int xy = 0; xy < rowNum; xy++)
            {
                int index = xy * rowNum + xy; //0,4,8
                if (mark == cellStates[index])
                {
                    ++num;
                }
            }
            if (mark != -1 && num == rowNum)
            {
                int markValue = (int)mark;
                return (playerTypeValu == markValue) ? Winner.Player : Winner.Opponent;
            }
        }
        { //코드블럭 시작
            int mark = cellStates[rowNum - 1];
            int num = 0;
            for (int xy = 0; xy < rowNum; ++xy)
            {
                int index = xy * rowNum + rowNum - xy - 1; //2,4,6
                if (mark == cellStates[index])
                {
                    ++num;
                }
            }
            if (mark != -1 && num == rowNum)
            {
                int markValue = (int)mark;
                return (playerTypeValu == markValue) ? Winner.Player : Winner.Opponent;
            }
        }
        { //코드블럭 시작
            int num = 0;
            foreach (int cellState in cellStates)
            {
                if (cellState == -1)
                {
                    ++num;
                }
            }
            if (num == 0)
            {
                return Winner.Tie;
            }
        }
        return Winner.None;
    }

    public void StartGame(PlayerType type)
    {
        playerType = type;

        if (type == PlayerType.PlayerOne)
        {
            gameState = GameState.PlayerTurn;
        }
        else
        {
            gameState = GameState.OpponentTurn;
        }
    }
    
    public void ShowGameOverPanel()
    {
        // gameOverPanel.anchoredPosition = new Vector2(0, 0);
        gameOverPanel.DOLocalMoveY(0, 1).SetEase(Ease.InOutBack);
        gameOverPanel.GetComponent<CanvasGroup>().DOFade(1, 1);
    }

}
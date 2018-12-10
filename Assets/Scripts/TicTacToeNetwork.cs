using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SocketIO;

public class TicTacToeNetwork : MonoBehaviour {

    //Start Panel
    public RectTransform startPanel;
    public Text startMessageText;
    public Button connectButton;
    public Button closeButton;

    private Vector2 startPanelPos;
    private PlayerType playerType;

    string myRoomId; //내가 조인한 방이름

    //TicTacToeManager 를 오출할 변수 선언
    TicTacToeManager gameManager;

    //Socket.IO
    SocketIOComponent socket;

    public void Connect() // 수동으로 서버에 접속
    {
        socket.Connect();
        startMessageText.text = "상대를 기다리는 중" ;
        connectButton.gameObject.SetActive(false); //버튼이 사라짐
    }

    public void Close() // 수동으로 서버에 접
    {
        socket.Close();
    }

    private void Start()
    {
        GameObject so = GameObject.Find("SocketIO");
        socket = so.GetComponent<SocketIOComponent>();
        //TicTacToeManager 호출
        gameManager = GetComponent<TicTacToeManager>();

        socket.On("joinRoom", JoinRoom);
        socket.On("createRoom", CreateRoom);       
        socket.On("startGame", StartGame);

        socket.On("doOpponent", DoOpponent);
        socket.On("exitRoom", ExitRoom);
        startPanel.gameObject.SetActive(true);
        closeButton.interactable = false;
    }

    void StartGame(SocketIOEvent e)
    {
        startPanel.gameObject.SetActive(false);
        closeButton.interactable = true;
        //TicTacToeManager에게 게임시작 알림
        gameManager.StartGame(playerType);
    }

    // 방이 있을 경우
    void JoinRoom(SocketIOEvent e)
    {
        string roomId = e.data.GetField("room").str;

        if (!string.IsNullOrEmpty(roomId))
        {
            myRoomId = roomId;
        }
        playerType = PlayerType.PlayerTwo;
    }
    // 방이 없을 경우
    void CreateRoom(SocketIOEvent e)
    {
        string roomId = e.data.GetField("room").str;
        if (!string.IsNullOrEmpty(roomId))
        {
            myRoomId = roomId;
        }
        playerType = PlayerType.PlayerOne;
    }
    //방에서 나갔을 경우
    void ExitRoom(SocketIOEvent e)
    {
        socket.Close();
    }

    //플레이어 게임 정보 서버로 전송
    public void DoPlayer(int index)
    {
        JSONObject playInfo = new JSONObject();
        playInfo.AddField("position", index);
        playInfo.AddField("room", myRoomId);

        socket.Emit("doPlayer", playInfo);
    }

    //
    void DoOpponent(SocketIOEvent e) // e에는 {position: cellIndex})값이 들어있음
    {
        int cellIndex = -1;
        e.data.GetField(ref cellIndex, "position");

        gameManager.DrawMark(cellIndex, Player.Opponent);
    }
}

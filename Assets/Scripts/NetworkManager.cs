using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using UnityEngine.UI;
using System.Text;

// 접속을 할 수 있는 코드 작성
public class NetworkManager : MonoBehaviour
{

    public InputField messageInputField;
    public Text messageText;
    string nickname;

    SocketIOComponent socket;

    // Use this for initialization
    void Start()
    {
        GameObject io = GameObject.Find("SocketIO");
        socket = io.GetComponent<SocketIOComponent>();

        nickname = "Dongseop"; // ToDo; 서버 닉네임 가져오기

        socket.On("chat", UpdateMessage);
    }

    void UpdateMessage(SocketIOEvent e)
    {
        string nick = e.data.GetField("nick").str;
        string msg = e.data.GetField("msg").str;

        messageText.text += string.Format("{0}:{1}\n", nick, msg);

        //var data = e.data;
        //Debug.Log("Received Mesage... ");
    }

    public void Send()
    {
        //자신의 메시지 화면에 표시
        string message = messageInputField.text;
        messageText.text += string.Format("{0}:{1}\n", nickname, message);

        //자신이 입력한 메시지 서버에 전송
        JSONObject obj = new JSONObject();
        obj.AddField("nick", nickname);
        obj.AddField("msg", message);

        socket.Emit("message", obj);
    }
}

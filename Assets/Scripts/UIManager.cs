using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text;

public class UIManager : MonoBehaviour
{
    public Text ServerMessage;

    public InputField scoreInputField;
    public Text scoreText;
       
    public void OnClickedAddScore()
    {
        string scoreStr = scoreInputField.text;
        if (!string.IsNullOrEmpty(scoreStr))
        {
            StartCoroutine(AddScore(scoreStr));
        }
    }
    public void OnClickedGetScore()
    {
        StartCoroutine(GetScore());
    }

    IEnumerator AddScore(string score)
    {
        using (UnityWebRequest www = UnityWebRequest.Get("http://localhost:3000/users/addscore/" + score))
        {
            string sid = PlayerPrefs.GetString("sid");
            if (!string.IsNullOrEmpty(sid))
            {
                www.SetRequestHeader("Cookie", sid);
            }
            yield return www.Send();
            string resultStr = www.downloadHandler.text;
        }
    }

    IEnumerator GetScore()
    {
        using (UnityWebRequest www = UnityWebRequest.Get("http://localhost:3000/users/score/"))
        {
            string sid = PlayerPrefs.GetString("sid");
            if (!string.IsNullOrEmpty(sid))
            {
                www.SetRequestHeader("Cookie", sid);
            }
            yield return www.Send();
            string resultStr = www.downloadHandler.text;

            if (!string.IsNullOrEmpty(resultStr))
            {
                scoreText.text = resultStr;
            }
        }
    }


    public void OnButtonClicked()
    {
        StartCoroutine(GetUserInfo());
    }

    IEnumerator GetUserInfo()
    {
        using (UnityWebRequest www = 
            UnityWebRequest.Get("http://localhost:3000/users/info"))
        {
            string username = PlayerPrefs.GetString("username");

            if (!string.IsNullOrEmpty(username))
            {
                www.SetRequestHeader("Cookie", "username=" + username);
            }

            yield return www.Send();

            ServerMessage.text = www.downloadHandler.text;
        }
    }
}

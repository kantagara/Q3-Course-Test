using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.Networking;
using System.Collections;

public class URLReader : MonoBehaviour
{
    #if !UNITY_EDITOR && UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern string GetURLFromPage();

    [DllImport("__Internal")]
    private static extern string GetQueryParam(string paramId);

    private string jwtToken;
    private string secret = "jkjwiFfjf92rwelr932t6"; // The secret provided

    void Start()
    {
        jwtToken = ReadQueryParam("token");
        Debug.Log("JWT Token: " + jwtToken);
    }

    public string ReadQueryParam(string paramId)
    {
        return GetQueryParam(paramId);
    }

    public string ReadURL()
    {
        return GetURLFromPage();
    }

    public void SendNotification(string message, string type)
    {
        StartCoroutine(SendNotificationCoroutine(message, type));
    }

    private IEnumerator SendNotificationCoroutine(string message, string type)
    {
        string url = "https://b3-main-development.up.railway.app/launcher";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.SetRequestHeader("X-Service-Method", "sendNotification");
        request.SetRequestHeader("Authorization", "Bearer " + secret);

        string jsonBody = JsonUtility.ToJson(new NotificationData
        {
            message = message,
            type = type,
            launcherJwt = jwtToken
        });

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Notification sent successfully!");
        }
        else
        {
            Debug.LogError("Error sending notification: " + request.error);
        }
    }

    [System.Serializable]
    private class NotificationData
    {
        public string message;
        public string type;
        public string launcherJwt;
    }

#endif
}

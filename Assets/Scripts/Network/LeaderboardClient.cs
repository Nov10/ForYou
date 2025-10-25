using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

[System.Serializable]
public class ScorePayload
{
    public string playerId;
    public string name;
    public int score;
    public string signature; // 선택
}

public class LeaderboardClient : MonoBehaviour
{
    [SerializeField] string webAppUrl = "웹앱_URL_여기에"; // 예: https://script.google.com/macros/s/AKfycb.../exec
    [SerializeField] string sharedSecret = "임의의_긴_비밀키_여기에"; // 서버와 동일(선택)

    public void SubmitScore(string id, string name, int score, System.Action<bool> onDone)
    {
        StartCoroutine(PostScore(id, name, score, (success, response) =>
        {
            Debug.Log("SubmitScore response: " + response);
            onDone?.Invoke(success);
        }));
    }
    IEnumerator PostScore(string playerId, string name, int score, System.Action<bool, string> onDone)
    {
        var payload = new ScorePayload { playerId = playerId, name = name, score = score };
        var json = JsonUtility.ToJson(payload);

        // 선택: HMAC 서명 추가
        if (!string.IsNullOrEmpty(sharedSecret))
        {
            payload.signature = SimpleHmac.Sign(json, sharedSecret);
            json = JsonUtility.ToJson(payload);
        }

        var req = new UnityWebRequest(webAppUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            onDone?.Invoke(true, req.downloadHandler.text);
        else
            onDone?.Invoke(false, req.error);
    }

    [System.Serializable]
    public class RankRow
    {
        public string playerId;
        public string name;
        public int score;
        public string updatedAt;
    }
    [System.Serializable]
    public class TopResponse
    {
        public bool ok;
        public RankRow[] data;
    }

    public void GetTop(int limit, System.Action<RankRow[]> onDone)
    {
        StartCoroutine(GetTop(limit, (success, response) =>
        {
            if (success && response != null && response.ok)
                onDone?.Invoke(response.data);
            else
                onDone?.Invoke(null);
        }));
    }
    IEnumerator GetTop(int limit, System.Action<bool, TopResponse> onDone)
    {
        string url = $"{webAppUrl}?action=top&limit={limit}";
        using (var req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
            {
                var res = JsonUtility.FromJson<TopResponse>(req.downloadHandler.text);
                onDone?.Invoke(true, res);
            }
            else onDone?.Invoke(false, null);
        }
    }
}

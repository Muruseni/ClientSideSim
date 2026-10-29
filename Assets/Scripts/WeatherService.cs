using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

namespace ClientSideSim
{
    public class WeatherService : MonoBehaviour
    {
        [Serializable] class Response { public Current current; }
        [Serializable] class Current { public float temperature_2m; public float wind_speed_10m; }
        public IEnumerator Fetch(double latitude, double longitude, Action<string, bool> finished)
        {
            string url = "https://api.open-meteo.com/v1/forecast?latitude=" + latitude.ToString(CultureInfo.InvariantCulture)
                + "&longitude=" + longitude.ToString(CultureInfo.InvariantCulture)
                + "&current=temperature_2m,wind_speed_10m&wind_speed_unit=ms";
            using (var request = UnityWebRequest.Get(url))
            {
                request.timeout = 15;
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                { finished("Weather unavailable. Check your connection and retry.", false); yield break; }
                string message;
                bool windy;
                try
                {
                    var result = JsonUtility.FromJson<Response>(request.downloadHandler.text);
                    if (result == null || result.current == null) throw new FormatException();
                    windy = result.current.wind_speed_10m > 5f;
                    message = string.Format(CultureInfo.InvariantCulture, "{0:0.0} °C  |  Wind: {1:0.0} m/s",
                        result.current.temperature_2m, result.current.wind_speed_10m);
                }
                catch (Exception) { message = "Weather service returned unexpected data."; windy = false; }
                finished(message, windy);
            }
        }
    }
}

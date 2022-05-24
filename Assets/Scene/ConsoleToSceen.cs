using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleToSceen : MonoBehaviour
{
    const int maxLines = 50;
    const int maxLineLength = 120;
    private string _logStr = "";

    GUIStyle titleStyle2 = new GUIStyle();

    private readonly List<string> _lines = new List<string>();
    void OnEnable() { Application.logMessageReceived += Log; }
    void OnDisable() { Application.logMessageReceived -= Log; }
    void Update() { }

    public void Log(string logString, string stackTrace, LogType type)
    {
        foreach (var line in logString.Split('\n'))
        {
            if (line.Length <= maxLineLength)
            {
                _lines.Add(line);
                continue;
            }
            var lineCount = line.Length / maxLineLength + 1;
            for (int i = 0; i < lineCount; i++)
            {
                if ((i + 1) * maxLineLength <= line.Length)
                {
                    _lines.Add(line.Substring(i * maxLineLength, maxLineLength));
                }
                else
                {
                    _lines.Add(line.Substring(i * maxLineLength, line.Length - i * maxLineLength));
                }
            }
        }
        if (_lines.Count > maxLines)
        {
            _lines.RemoveRange(0, _lines.Count - maxLines);
        }
        _logStr = string.Join("\n", _lines);
    }

    void OnGUI()
    {
        titleStyle2.fontSize = 20;
        titleStyle2.normal.textColor = Color.white;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
            new Vector3(Screen.width / 1920f, Screen.height / 800.0f, 1.0f));
        GUI.Label(new Rect(10, 10, 800, 370), _logStr, titleStyle2);
    }
}
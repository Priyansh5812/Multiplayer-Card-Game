using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct MenuData
{
    public CanvasGroup cgMain;
    public Button startAsClient;
    public Button startAsHost;
    public TextMeshProUGUI statusText;

    public string ConnectingTxt;
    public string ConnectionSuccess;
}

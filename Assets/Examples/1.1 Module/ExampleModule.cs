using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

public class ExampleModule : MonoBehaviour
{
    public KMSelectable[] pins;
    public GameObject[] wires;
    public MeshRenderer[] bands;
    public Material materialBlack;
    public Material materialBrown;
    public Material materialRed;
    public Material materialOrange;
    public Material materialYellow;
    public Material materialGreen;
    public Material materialBlue;
    public Material materialViolet;
    public Material materialGray;
    public Material materialWhite;
    public Material materialGold;
    public Material materialSilver;

    int correctIndex;
    bool isActivated = false;
    bool[,] connections;
    int startedConnecting = -1;

    void Start()
    {
        Init();

        GetComponent<KMBombModule>().OnActivate += ActivateModule;
        connections = new bool[8, 8]; // all false
    }

    void Init()
    {
        for(int i = 0; i < pins.Length; i++) {
            int j = i;
            pins[i].OnInteract += delegate () { OnPress(j); return false; };
        }
    }

    void ActivateModule()
    {
        isActivated = true;

        KMBombInfo info = GetComponent<KMBombInfo> ();

        string serial = JsonConvert.DeserializeObject<Dictionary<string, string>>(info.QueryWidgets (KMBombInfo.QUERYKEY_GET_SERIAL_NUMBER, null)[0])["serial"];
        List<int> notDigitIndexes = new List<int> ();
        for (int i = 0; i < serial.Length; i++) {
            if (serial [i] < '0' || '9' < serial [i]) {
                notDigitIndexes.Insert (0, i);
            }
        }
        foreach (int i in notDigitIndexes) {
            serial = serial.Remove (i, 1);
        }
        Debug.Log ("[Resistors] Serial number digits are " + serial);

        int batteries = 0;
        bool dcell = false;
        foreach (string response in info.QueryWidgets (KMBombInfo.QUERYKEY_GET_BATTERIES, null)) {
            int bats = JsonConvert.DeserializeObject<Dictionary<string, int>> (response)["numbatteries"];
            batteries += bats;
            if (bats == 1)
                dcell = true;
        }
        Debug.Log ("[Resistors] " + batteries + " batteries, D cell = " + dcell);

        bool litFRK = false;
        foreach (string response in info.QueryWidgets(KMBombInfo.QUERYKEY_GET_INDICATOR, null)) {
            Dictionary<string, string> obj = JsonConvert.DeserializeObject<Dictionary<string, string>> (response);
            if (obj ["label"] == "FRK" && obj ["on"] == "True") {
                litFRK = true;
            }
        }
        Debug.Log ("[Resistors] Lit FRK = " + litFRK);

        for (int i = 0; i < 10; i++) {
            if (serial.Length <= i) {
                bands [i].enabled = false;
            } else {
                switch (serial[i]) {
                case '0':
                    bands [i].material = materialBlack;
                    break;
                case '1':
                    bands [i].material = materialBrown;
                    break;
                case '2':
                    bands [i].material = materialRed;
                    break;
                case '3':
                    bands [i].material = materialOrange;
                    break;
                case '4':
                    bands [i].material = materialYellow;
                    break;
                case '5':
                    bands [i].material = materialGreen;
                    break;
                case '6':
                    bands [i].material = materialBlue;
                    break;
                case '7':
                    bands [i].material = materialViolet;
                    break;
                case '8':
                    bands [i].material = materialGray;
                    break;
                case '9':
                    bands [i].material = materialWhite;
                    break;
                default:
                    bands [i].enabled = false;
                    break;
                }
            }
        }
    }

    void OnPress(int buttonNumber)
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);

        if (!isActivated) {
            Debug.Log("[Resistors] Pressed button before module has been activated!");
            GetComponent<KMBombModule>().HandleStrike();
            return;
        }

        if (startedConnecting == -1) {
            startedConnecting = buttonNumber;
            pins [buttonNumber].GetComponentInChildren<MeshRenderer> ().material = materialWhite;
            return;
        }

        SetWire (startedConnecting, buttonNumber, true);
        startedConnecting = -1;
    }

    void SetWire(int point1, int point2, bool isPresent)
    {
        pins [point1].GetComponentInChildren<MeshRenderer> ().material = materialGold;
        pins [point2].GetComponentInChildren<MeshRenderer> ().material = materialGold;

        if (point1 == point2) return;
        connections [point1, point2] = isPresent;
        connections [point2, point1] = isPresent;

        // 0..7 are ABCDEFGH

        int wireIndex = 0;
        if (point1 > point2) {
            int temp = point1;
            point1 = point2;
            point2 = temp;
        }

        if      (point1 == 0 && point2 == 1) wireIndex = 0;
        else if (point1 == 0 && point2 == 2) wireIndex = 1;
        else if (point1 == 0 && point2 == 3) wireIndex = 2;
        else if (point1 == 0 && point2 == 4) wireIndex = 3;
        else if (point1 == 0 && point2 == 5) wireIndex = 4;
        else if (point1 == 0 && point2 == 6) wireIndex = 5;
        else if (point1 == 0 && point2 == 7) wireIndex = 6;

        else if (point1 == 1 && point2 == 2) wireIndex = 7;
        else if (point1 == 1 && point2 == 3) wireIndex = 8;
        else if (point1 == 1 && point2 == 4) wireIndex = 9;
        else if (point1 == 1 && point2 == 5) wireIndex = 10;
        else if (point1 == 1 && point2 == 6) wireIndex = 11;
        else if (point1 == 1 && point2 == 7) wireIndex = 12;

        else if (point1 == 2 && point2 == 3) wireIndex = 13;

        else if (point1 == 2 && point2 == 4) wireIndex = 14;
        else if (point1 == 3 && point2 == 4) wireIndex = 15;
        else if (point1 == 4 && point2 == 5) wireIndex = 16;
        else if (point1 == 4 && point2 == 6) wireIndex = 17;
        else if (point1 == 4 && point2 == 7) wireIndex = 18;

        else if (point1 == 2 && point2 == 5) wireIndex = 19;
        else if (point1 == 3 && point2 == 5) wireIndex = 20;
        else if (point1 == 5 && point2 == 6) wireIndex = 21;
        else if (point1 == 5 && point2 == 7) wireIndex = 22;

        else if (point1 == 2 && point2 == 6) wireIndex = 23;
        else if (point1 == 3 && point2 == 6) wireIndex = 24;
        else if (point1 == 6 && point2 == 7) wireIndex = 25;

        else if (point1 == 2 && point2 == 7) wireIndex = 26;
        else if (point1 == 3 && point2 == 7) wireIndex = 27;
        
        wires [wireIndex].SetActive (isPresent);
    }
}

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
	double resistor1 = 0;
	double resistor2 = 0;
	double goalResistAB = double.PositiveInfinity;
	double goalResistAC = double.PositiveInfinity;
	double goalResistAD = double.PositiveInfinity;
	double goalResistBC = double.PositiveInfinity;
	double goalResistBD = double.PositiveInfinity;
	// resistance C-D not checked

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

	double DifferentFrom(double value)
	{
		return value * 2.0; // TODO randomize this
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

        char firstDigit = serial.Length == 0 ? '0' : serial[0];
		char lastDigit = serial.Length == 0 ? '0' : serial[serial.Length - 1];
		bool primaryInputA = firstDigit == '0' || firstDigit == '2' || firstDigit == '4' || firstDigit == '6' || firstDigit == '8';
		bool primaryOutputC = lastDigit == '0' || lastDigit == '2' || lastDigit == '4' || lastDigit == '6' || lastDigit == '8';
        int targetResistance;
        if (serial.Length == 0) {
            targetResistance = 0;
        } else if (serial.Length == 1) {
            targetResistance = int.Parse(serial);
        } else {
            targetResistance = int.Parse(serial.Substring(0, 2));
        }
		for (int i = 0; i < batteries; i++) {
			targetResistance *= 10;
		}

		if (primaryInputA && primaryOutputC) {
			goalResistAC = targetResistance;
			if (litFRK) {
				goalResistAD = targetResistance;
			} else if (dcell) {
				goalResistBD = 0;
			}
		} else if (primaryInputA && !primaryOutputC) {
			goalResistAD = targetResistance;
			if (litFRK) {
				goalResistAC = targetResistance;
			} else if (dcell) {
				goalResistBC = 0;
			}
		} else if (!primaryInputA && primaryOutputC) {
			goalResistBC = targetResistance;
			if (litFRK) {
				goalResistBD = targetResistance;
			} else if (dcell) {
				goalResistAD = 0;
			}
		} else if (!primaryInputA && !primaryOutputC) {
			goalResistBD = targetResistance;
			if (litFRK) {
				goalResistBC = targetResistance;
			} else if (dcell) {
				goalResistAC = 0;
			}
		}
		Debug.Log ("[Resistors] A to B resistance should be " + goalResistAB);
		Debug.Log ("[Resistors] A to C resistance should be " + goalResistAC);
		Debug.Log ("[Resistors] A to D resistance should be " + goalResistAD);
		Debug.Log ("[Resistors] B to C resistance should be " + goalResistBC);
		Debug.Log ("[Resistors] B to D resistance should be " + goalResistBD);

		float puzzle = Random.value;
		if (puzzle < 0.3f) {
			// use resistor 1
			resistor1 = targetResistance;
			resistor2 = DifferentFrom (targetResistance);
		} else if (puzzle < 0.6f) {
			// use resistor 2
			resistor2 = targetResistance;
			resistor1 = DifferentFrom (targetResistance);
		} else if (puzzle < 0.9f) {
			// 2 resistors in serial
			// TODO randomize this
			resistor1 = targetResistance * 0.4;
			resistor2 = targetResistance - resistor1;
		} else {
			// 2 resistors in parallel
			// TODO randomize this
			resistor1 = targetResistance * 1.5;
			resistor2 = 1.0 / (1.0 / targetResistance - 1.0 / resistor1);
		}
		Debug.Log ("[Resistors] Top resistor is " + resistor1);
		Debug.Log ("[Resistors] Bottom resistor is " + resistor2);

		DisplayResistor (resistor1, 0, 1, 2, 3, 4);
		DisplayResistor (resistor2, 5, 6, 7, 8, 9);
    }

	Material GetBandMaterial (int index) {
		switch (index) {
		case -2:
			return materialSilver;
		case -1:
			return materialGold;
		case 0:
			return materialBlack;
		case 1:
			return materialBrown;
		case 2:
			return materialRed;
		case 3:
			return materialOrange;
		case 4:
			return materialYellow;
		case 5:
			return materialGreen;
		case 6:
			return materialBlue;
		case 7:
			return materialViolet;
		case 8:
			return materialGray;
		case 9:
			return materialWhite;
		default:
			return null;
		}
	}

	void DisplayResistor (double resistanceValue, int i0, int i1, int i2, int i3, int i4) {
		int multiplier, digit1, digit2;
		if (resistanceValue < 10) {
			multiplier = -1;
		} else {
			multiplier = 0;
			while (true) {
				if (resistanceValue < System.Math.Pow(10.0, multiplier + 2))
					break;
				multiplier++;
			}
		}
		int display = (int) (resistanceValue / System.Math.Pow (10.0, multiplier));
		digit1 = display / 10;
		digit2 = display % 10;

		bool displayFlipped = Random.Range (0, 5) == 0;
		if (displayFlipped) {
			bands [i0].material = materialGray;
			bands [i1].enabled = false;
			bands [i2].material = GetBandMaterial (multiplier);
			bands [i3].material = GetBandMaterial (digit2);
			bands [i4].material = GetBandMaterial (digit1);
		} else {
			bands [i0].material = GetBandMaterial (digit1);
			bands [i1].material = GetBandMaterial (digit2);
			bands [i2].material = GetBandMaterial (multiplier);
			bands [i3].enabled = false;
			bands [i4].material = materialGray;
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

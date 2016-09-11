using UnityEngine;

public class ExampleModule : MonoBehaviour
{
    public KMSelectable[] buttons;
    public GameObject[] wires;

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
        for(int i = 0; i < buttons.Length; i++)
        {
            int j = i;
            buttons[i].OnInteract += delegate () { OnPress(j); return false; };
        }
    }

    void ActivateModule()
    {
        isActivated = true;
    }

	void OnPress(int buttonNumber)
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);

        if (!isActivated) {
            Debug.Log("Pressed button before module has been activated!");
            GetComponent<KMBombModule>().HandleStrike();
			return;
        }

		if (startedConnecting == -1) {
			startedConnecting = buttonNumber;
			return;
		}

		SetWire (startedConnecting, buttonNumber, true);
        startedConnecting = -1;
    }

	void SetWire(int point1, int point2, bool isPresent)
	{
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

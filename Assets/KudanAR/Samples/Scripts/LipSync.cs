using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;

public class MLipSync : MonoBehaviour
{

    Hashtable phonemesTable;
    bool isStart = false;
    public InputField textBox;
    public Button startButton;
    public AudioSource audio;

    // Use this for initialization
    void Start()
    {
        startButton.onClick.AddListener(() => sendTextToJava());
        audio = this.gameObject.GetComponent<AudioSource>();

        phonemesTable = new Hashtable();
        phonemesTable.Add("AA", 1);
        phonemesTable.Add("AE", 2);
        phonemesTable.Add("AH", 2);
        phonemesTable.Add("AO", 3);
        phonemesTable.Add("AW", 4);
        phonemesTable.Add("AY", 5);
        phonemesTable.Add("B", 6);
        phonemesTable.Add("M", 6);
        phonemesTable.Add("P", 6);
        phonemesTable.Add("CH", 7);
        phonemesTable.Add("SH", 7);
        phonemesTable.Add("JH", 7);
        phonemesTable.Add("ZH", 7);
        phonemesTable.Add("D", 8);
        phonemesTable.Add("N", 8);
        phonemesTable.Add("T", 8);
        phonemesTable.Add("DH", 9);
        phonemesTable.Add("TH", 9);
        phonemesTable.Add("UH", 10);
        phonemesTable.Add("EH", 10);
        phonemesTable.Add("ER", 11);
        phonemesTable.Add("EY", 11);
        phonemesTable.Add("F", 12);
        phonemesTable.Add("V", 12);
        phonemesTable.Add("G", 13);
        phonemesTable.Add("K", 13);
        phonemesTable.Add("NG", 13);
        phonemesTable.Add("HH", 14);
        phonemesTable.Add("IH", 15);
        phonemesTable.Add("IY", 15);
        phonemesTable.Add("Y", 15);
        phonemesTable.Add("L", 16);
        phonemesTable.Add("OW", 17);
        phonemesTable.Add("OY", 18);
        phonemesTable.Add("R", 19);
        phonemesTable.Add("S", 20);
        phonemesTable.Add("Z", 20);
        phonemesTable.Add("UW", 21);
        phonemesTable.Add("W", 21);
        phonemesTable.Add("SIL", 22);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void sendTextToJava()
    {
        string text = textBox.text;

        isStart = !isStart;
        if (isStart)
        {
            startButton.GetComponentInChildren<Text>().text = "Stop";



            readFile();
        }
        else
        {
            startButton.GetComponentInChildren<Text>().text = "Start";
        }

    }

    public void readFile()
    {
        string filePath = "file://" + Application.persistentDataPath + "/phonemes.txt"; // <storage>/Android/data/eu.kudan.ar/files/phonemes.txt

        WWW loader = new WWW(filePath);
        while (!loader.isDone)
        {
            Debug.Log("uploading");
        }

        string text = loader.text;
        //textBox.text = text;

        string[] phonemes = text.Split('\n');
        receivePhonemes(phonemes);

    }

    public void receivePhonemes(string[] phonemes)
    {
        StartCoroutine(startLipSync(phonemes));
    }



    IEnumerator startLipSync(string[] phonemes)
    {
        string filePath = "file://" + Application.persistentDataPath + "/speech.wav"; // <storage>/Android/data/eu.kudan.ar/files/speech.wav

        WWW audioLoader = new WWW(filePath);
        while (!audioLoader.isDone)
        {
            Debug.Log("uploading");
        }

        audio.clip = audioLoader.GetAudioClip(true, false);

        float audioDuration = audio.clip.length;       //secs
        float secPerPho = audioDuration / phonemes.Length;
        textBox.text = secPerPho + "\n";
        audio.Play();

        foreach (string p in phonemes)
        {
            string phoneme=p.Trim();
            textBox.text = textBox.text+" " + phoneme;
           
            yield return new WaitForSeconds(secPerPho);
            if (!isStart)
            {
                break;
            }

            if (phonemesTable.ContainsKey(phoneme))
            {
                this.gameObject.GetComponent<Renderer>().material.mainTexture = Resources.Load(phonemesTable[phoneme].ToString()) as Texture2D;
                 }

        }

        audio.Stop();
        Destroy(audio.clip);
        
        isStart = false;
        startButton.GetComponentInChildren<Text>().text = "Start";
    }
}

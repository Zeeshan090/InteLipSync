using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using RogoDigital.Lipsync;


public class MyLipSync : MonoBehaviour
{

    AndroidJavaClass FaceState;
    AudioClip audioClip;
    Hashtable phonemesMap;
    Hashtable phonemesDuration;

    public Text textbox;
    public Button start;

    public AudioSource audioS;
    LipSync lipSync;

    void Awake()
    {
        QualitySettings.vSyncCount = 0;  // VSync must be disabled

        Application.targetFrameRate = 30;
    }

    void Start()
    {
        start.onClick.AddListener(() => startClicked());
        lipSync = gameObject.GetComponent<LipSync>();

        //FaceState = new AndroidJavaClass("eu.kudan.ar.FaceState");


        phonemesMap = new Hashtable();
        phonemesMap.Add("AA", Phoneme.AI);
        phonemesMap.Add("AE", Phoneme.E);
        phonemesMap.Add("AH", Phoneme.E);
        phonemesMap.Add("AO", Phoneme.U);
        phonemesMap.Add("AW", Phoneme.O);
        phonemesMap.Add("AY", Phoneme.AI);
        phonemesMap.Add("B", Phoneme.MBP);
        phonemesMap.Add("M", Phoneme.MBP);
        phonemesMap.Add("P", Phoneme.MBP);
        phonemesMap.Add("CH", Phoneme.CDGKNRSThYZ);
        phonemesMap.Add("SH", Phoneme.CDGKNRSThYZ);
        phonemesMap.Add("JH", Phoneme.CDGKNRSThYZ);
        phonemesMap.Add("ZH", Phoneme.CDGKNRSThYZ);
        phonemesMap.Add("D", Phoneme.CDGKNRSThYZ);
        phonemesMap.Add("N", Phoneme.CDGKNRSThYZ);
        phonemesMap.Add("T", Phoneme.CDGKNRSThYZ);
        phonemesMap.Add("DH", Phoneme.CDGKNRSThYZ);
        phonemesMap.Add("TH", Phoneme.CDGKNRSThYZ);
        phonemesMap.Add("UH", Phoneme.U);
        phonemesMap.Add("EH", Phoneme.E);
        phonemesMap.Add("ER", Phoneme.CDGKNRSThYZ);
        phonemesMap.Add("EY", Phoneme.E);
        phonemesMap.Add("F", Phoneme.FV);
        phonemesMap.Add("V", Phoneme.FV);
        phonemesMap.Add("G", Phoneme.CDGKNRSThYZ);
        phonemesMap.Add("K", Phoneme.CDGKNRSThYZ);
        phonemesMap.Add("NG", Phoneme.CDGKNRSThYZ);
        phonemesMap.Add("HH", Phoneme.AI);
        phonemesMap.Add("IH", Phoneme.AI);
        phonemesMap.Add("IY", Phoneme.AI);
        phonemesMap.Add("Y", Phoneme.AI);
        phonemesMap.Add("L", Phoneme.L);
        phonemesMap.Add("OW", Phoneme.O);
        phonemesMap.Add("OY", Phoneme.O);
        phonemesMap.Add("R", Phoneme.CDGKNRSThYZ);
        phonemesMap.Add("S", Phoneme.CDGKNRSThYZ);
        phonemesMap.Add("Z", Phoneme.CDGKNRSThYZ);
        phonemesMap.Add("UW", Phoneme.WQ);
        phonemesMap.Add("W", Phoneme.WQ);

        phonemesDuration = new Hashtable();
        phonemesDuration.Add("AA", 4);
        phonemesDuration.Add("AE", 4);
        phonemesDuration.Add("AH", 3);
        phonemesDuration.Add("AO", 3);
        phonemesDuration.Add("AW", 5);
        phonemesDuration.Add("AY", 6);
        phonemesDuration.Add("B", 2);
        phonemesDuration.Add("M", 3);
        phonemesDuration.Add("P", 5);
        phonemesDuration.Add("CH", 4);
        phonemesDuration.Add("SH", 4);
        phonemesDuration.Add("JH", 4);
        phonemesDuration.Add("ZH", 4);
        phonemesDuration.Add("D", 3);
        phonemesDuration.Add("N", 3);
        phonemesDuration.Add("T", 3);
        phonemesDuration.Add("DH", 2);
        phonemesDuration.Add("TH", 3);
        phonemesDuration.Add("EH", 4);
        phonemesDuration.Add("UH", 3);
        phonemesDuration.Add("ER", 4);
        phonemesDuration.Add("EY", 5);
        phonemesDuration.Add("F", 3);
        phonemesDuration.Add("V", 3);
        phonemesDuration.Add("G", 4);
        phonemesDuration.Add("K", 2);
        phonemesDuration.Add("NG", 3);
        phonemesDuration.Add("HH", 3);
        phonemesDuration.Add("IH", 6);
        phonemesDuration.Add("IY", 6);
        phonemesDuration.Add("Y", 6);
        phonemesDuration.Add("L", 3);
        phonemesDuration.Add("OW", 8);
        phonemesDuration.Add("OY", 11);
        phonemesDuration.Add("R", 4);
        phonemesDuration.Add("S", 4);
        phonemesDuration.Add("Z", 4);
        phonemesDuration.Add("UW", 10);
        phonemesDuration.Add("W", 4);

        //FaceState.CallStatic("setFaceAlive", true);

    }

    // Update is called once per frame
    void Update()
    {


    }
    public void speakLiza(string parameterStr = "Menu\nmenuPhonemes")
    {
        //string parameterStr = "Menu\nmenuPhonemes";
        Debug.Log("startClicked() got called!");
        string[] parameters = parameterStr.Split('\n');
        string audioFilename = parameters[0], phonemeFilename = parameters[1];

        string path = "file://" + Application.persistentDataPath; // <storage>/Android/data/eu.kudan.ar/files/
        path = path.Replace("Android/data/eu.kudan.ar/files", "InteLipSync");

        string phonemesFilePath = Path.Combine(path, phonemeFilename + ".txt");
        string audioFilePath = Path.Combine(path, audioFilename + ".wav");

        WWW loader = new WWW(phonemesFilePath);
        while (!loader.isDone)
        {
            Debug.Log("reading phonemes file");
        }
        string text = loader.text;
        string[] phonemes = text.Split('\n');

        loader = new WWW(audioFilePath);
        while (!loader.isDone)
        {
            Debug.Log("reading audio file");
        }
        audioClip = loader.GetAudioClip(false, false, AudioType.WAV);

        List<PhonemeMarker> phonemeMarkers = new List<PhonemeMarker>();
        float nextStart = 0;
        float timePerFrame = (audioClip.length - (float)(audioClip.length*0.11)) / getFramesCount(phonemes);
        
        foreach (string p in phonemes)
        {
            if (p == null || p.CompareTo("") == 0)
                continue;

            string phoneme = p.Trim();
            if (p.Length > 2)
                continue;
            PhonemeMarker marker = new PhonemeMarker((Phoneme)phonemesMap[phoneme],nextStart );
            phonemeMarkers.Add(marker);
            nextStart+=((int)phonemesDuration[phoneme] * timePerFrame);
            
        }
        PhonemeMarker markerRest = new PhonemeMarker(Phoneme.Rest, nextStart);
        phonemeMarkers.Add(markerRest);

        textbox.text += " " + nextStart + "\n";
        LipSyncData lipSyncData = new LipSyncData(audioClip, phonemeMarkers.ToArray());
        lipSync.Play(lipSyncData);
    }

    public void startClicked()
    {

        speakLiza();

    }

    public int getFramesCount(string[] phonemes)
    {
        int count = 0;
        foreach (string p in phonemes)
        {
            if (p == null || p.CompareTo("") == 0)
                continue;

            string phoneme = p.Trim();
            if (phoneme.Length > 2)
                continue;

            count += (int)phonemesDuration[phoneme];
        }

        return count;
    }

 
    void OnDestroy()
    {
        //FaceState.CallStatic("setFaceAlive", false);
    }
}

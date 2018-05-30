using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class TableManager : MonoBehaviour {

    public RectTransform table;
    public GridLayoutGroup grid;
    Hashtable tableElements = new Hashtable();
	// Use this for initialization
	void Start () {
        grid = table.GetComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;

        tableElements = new Hashtable();

        listNewsTopics("topics_list.txt");
    }
	
	// Update is called once per frame
	void Update () {
	
	}


    void fillTable(string title, List<string> names, List<List<string>> data)
    {
        Debug.Log(data.Count);
        grid.constraintCount = names.Count;
        for(int i=0;i<names.Count;i++)
        {
            GameObject go = new GameObject("col_name"+i, typeof(RectTransform));
            go.transform.SetParent(table.transform);
            Text txt = go.AddComponent<Text>();
            txt.text = names[i];
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        }

        for (int i = 0; i < data.Count; i++)
        {
            add2Table(data[i][0], data[i], names);
        }
    }

    void add2Table(string id, List<string> row, List<string> names)
    {
        List<GameObject> elementsRow = new List<GameObject>();
        for (int j = 0; j < row.Count; j++)
        {
            GameObject go = new GameObject(names[j] + id, typeof(RectTransform));
            go.transform.SetParent(table.transform);
            Text txt = go.AddComponent<Text>();
            txt.text = row[j];
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            elementsRow.Add(go);
        }

        tableElements.Add(id, elementsRow);
    }

    void updateTable(string id, List<string> row)
    {
        List<GameObject> elementsRow = (List<GameObject>)tableElements[id];
        if (elementsRow != null)
        {
            for (int j = 0; j < row.Count; j++)
            {
                elementsRow[j].GetComponent<Text>().text = row[j];
            }

        }

    }

    List<List<string>> getData(string filename)
    {
        string path = "file://" + Application.persistentDataPath; // <storage>/Android/data/eu.kudan.ar/files/
        path = path.Replace("Android/data/eu.kudan.ar/files", "InteLipSync");
        string dataFile = Path.Combine(path, filename + ".txt");

        WWW loader = new WWW(dataFile);
        while (!loader.isDone)
        {
            Debug.Log("reading table data file");
        }
        string text = loader.text;
        string[] rows = text.Split('\n');

        List<List<string>> data = new List<List<string>>();
        for (int i = 0; i < rows.Length; i++)
        {
            string[] cols = rows[i].Split(',');
            List<string> rowList = new List<string>();

            for (int j = 0; j < cols.Length; j++)
            {
                rowList.Add(cols[j]);
            }

            data.Add(rowList);
        }

        return data;
    }

    void listEvents(string filename)
    {
        emptyTable();

        List<string> names=new List<string>(){"ID","Title","Day","Date","Time","ReminderDate", "ReminderTime","Venue","RelatedPersonnel"};

        fillTable("Events", names, getData(filename));
    }

    void listNewsTopics(string filename)
    {
        emptyTable();

        List<string> names = new List<string>() { "ID", "Topic" };
        fillTable("News Topics", names, getData(filename));
    }

    void updateEvent(string attrs)
    {
        List<string> row = new List<string>();
        row.AddRange(attrs.Split(','));
       
        updateTable(row[0],row);
    }

    void updateNewsTopic(string attrs)
    {
        List<string> row = new List<string>();
        row.AddRange(attrs.Split(','));

        updateTable(row[0], row);
    }

    void addNewsTopic(string attrs)
    {
        List<string> names = new List<string>() {"ID", "Topic" };

        if (attrs.Length == 0)
        {
            emptyTable();
            List<List<string>> data = new List<List<string>>() { new List<string>(){"1", "" }};
            fillTable("Add News Topic", names, data);
        }
        else
        {
            List<string> row = new List<string>();
            row.AddRange(attrs.Split(','));
            updateTable("1", row);
        }

    }

    void addEvent(string attrs)
    {
        List<string> names = new List<string>() { "ID", "Title", "Day", "Date", "Time", "ReminderDate", "ReminderTime", "Venue", "RelatedPersonnel" };

        if (attrs.Length == 0)
        {
            emptyTable();

            List<List<string>> data = new List<List<string>>() { new List<string>() { "1", "", "", "", "", "", "", "", "" } };
            fillTable("Add Event", names, data);
        }
        else
        {
            List<string> row = new List<string>();
            row.AddRange(attrs.Split(','));
            updateTable("1", row);
        }

    }

    void emptyTable()
    {
        for (var i = table.transform.childCount - 1; i >= 0; i--)
        {
            var objectA = table.transform.GetChild(i);
            objectA.transform.SetParent(null);
        }

        tableElements.Clear();
    }
}

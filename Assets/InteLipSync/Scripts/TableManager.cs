using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class TableManager : MonoBehaviour
{

    public Text log;
    public RectTransform table;
    public GameObject scrollView;
    GridLayoutGroup grid;
    Hashtable tableElements = new Hashtable();
    // Use this for initialization
    void Start()
    {
        grid = table.GetComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        scrollView.SetActive(false);

        tableElements = new Hashtable();

        //    listNewsTopics("topics_list");
    }

    // Update is called once per frame
    void Update()
    {

    }


    void fillTable(string title, List<string> names, List<List<string>> data)
    {
        scrollView.SetActive(true);

        //log.text = data.Count+"";
        grid.constraintCount = names.Count;
        for (int i = 0; i < names.Count; i++)
        {
            GameObject go = new GameObject("col_name" + i, typeof(RectTransform));
            go.transform.SetParent(table.transform);
            Text txt = go.AddComponent<Text>();
            txt.text = names[i];
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.fontSize = 20;
            txt.color = Color.black;
            txt.fontStyle = FontStyle.Bold;

        }
        for (int i = 0; i < data.Count; i++)
        {
            //log.text += " hre ";
            add2Table(data[i][0], data[i], names);
            //log.text += " "+data[i][0] + " " + data[i][1];
        }
    }

    void add2Table(string id, List<string> row, List<string> names)
    {
        //log.text += "\n" + id + " " + row[1];
        List<GameObject> elementsRow = new List<GameObject>();
        for (int j = 0; j < row.Count; j++)
        {
            GameObject go = new GameObject(names[j] + id, typeof(RectTransform));
            go.transform.SetParent(table.transform);
            Text txt = go.AddComponent<Text>();
            txt.text = row[j];
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.fontSize = 20;
            txt.color = Color.black;

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

        //log.text = text+" end";

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

        List<string> names = new List<string>() { "ID", "Title", "Day", "Date", "Time", "RemDate", "RemTime", "Venue", "RelPerson" };

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

        updateTable(row[0], row);
    }

    void updateNewsTopic(string attrs)
    {
        List<string> row = new List<string>();
        row.AddRange(attrs.Split(','));

        updateTable(row[0], row);
    }

    void addNewsTopic(string attrs)
    {
        List<string> names = new List<string>() { "ID", "Topic" };

        if (attrs.Length == 0)
        {
            emptyTable();
            List<List<string>> data = new List<List<string>>() { new List<string>() { "1", "" } };
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
        List<string> names = new List<string>() { "ID", "Title", "Day", "Date", "Time", "Rem Date", "Rem Time", "Venue", "Related Person" };

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
        scrollView.SetActive(false);
    }
    int f = 0;
    public void onStartClicked()
    {
        if (f == 0)
        {
            addEvent("");
            f = 1;
        }
        else
        {
            addEvent("1,match,monday,20 may 2018,12:00 pm,20 may 2018,12:00 pm,ground,omair");
        }
    }
}

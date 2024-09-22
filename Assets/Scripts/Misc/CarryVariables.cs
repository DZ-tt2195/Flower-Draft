using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System.Reflection;
using UnityEngine.UI;
using System.IO;

public class CarryVariables : MonoBehaviour
{
    public static CarryVariables instance;
    [Foldout("Prefabs", true)]
    public CardLayout cardPrefab;
    public Player playerPrefab;
    public Popup textPopup;
    public Popup cardPopup;
    public Button playerButtonPrefab;

    [Foldout("Right click", true)]
    [SerializeField] Transform rightClickBackground;
    [SerializeField] CardLayout rightClickCard;

    [Foldout("Misc", true)]
    [SerializeField] Transform permanentCanvas;
    [ReadOnly] public Dictionary<string, MethodInfo> dictionary = new();
    [ReadOnly] public List<string> cardNames = new();
    public Sprite faceDownSprite;
    public int undecided { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Application.targetFrameRate = 60;
            undecided = -100000;
            DontDestroyOnLoad(this.gameObject);
            GetScripts();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void GetScripts()
    {
        if (Application.isEditor)
        {
            string filePath = $"Assets/Resources/AvailableScripts.txt";
            List<string[]> allStrings = new() { ScriptsInRange("Cards") };
            File.WriteAllText(filePath, Format(allStrings));
        }

        var data = ReadFile("", "AvailableScripts");
        for (int i = 0; i < data[1].Length; i++)
        {
            string nextLine = data[1][i].Trim().Replace("\"", "");
        }

        for (int i = 1; i < data.Length; i++)
        {
            for (int j = 0; j < data[i].Length; j++)
            {
                string nextObject = data[i][j].Replace("\"", "").Replace("\\", "").Replace("]", "").Trim();
                cardNames.Add(nextObject);
            }
        }
    }

    string[][] ReadFile(string range, string file)
    {
        TextAsset data = Resources.Load($"{range}{file}") as TextAsset;

        string editData = data.text;
        editData = editData.Replace("],", "").Replace("{", "").Replace("}", "");

        string[] numLines = editData.Split("[");
        string[][] list = new string[numLines.Length][];

        for (int i = 0; i < numLines.Length; i++)
        {
            list[i] = numLines[i].Split("\",");
        }
        return list;
    }

    string Format(List<string[]> allStrings)
    {
        string content = "{\n";
        for (int i = 0; i < allStrings.Count; i++)
        {
            content += "  [\n";
            for (int j = 0; j < allStrings[i].Length; j++)
            {
                content += $"    \"{allStrings[i][j]}\"";
                if (j < allStrings[i].Length - 1)
                    content += ",";
                content += "\n";
            }
            content += "  ]";
            if (i < allStrings.Count - 1)
                content += ",";
            content += "\n";
        }
        content += "}\n";
        return content;
    }

    string[] ScriptsInRange(string range)
    {
        string[] list = Directory.GetFiles($"Assets/Scripts/{range}", "*.cs", SearchOption.TopDirectoryOnly);
        string[] answer = new string[list.Length];
        for (int i = 0; i < list.Length; i++)
            answer[i] = Path.GetFileNameWithoutExtension(list[i]);

        return answer;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            rightClickBackground.gameObject.SetActive(false);
    }

    public void RightClickDisplay(Card card, float alpha)
    {
        rightClickBackground.gameObject.SetActive(true);
        rightClickCard.FillInCards(card, alpha);
    }
}

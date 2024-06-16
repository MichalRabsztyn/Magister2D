using System;
using System.IO;
using UnityEngine;
using TMPro;

public class ScoreDatabase : MonoBehaviour
{
    private const string _fileName = "levelScore.dat";
    private int _numOfLevels = 6;

    [SerializeField]
    private TextMeshProUGUI[] _scoreTextArray;

    public void SaveScore(int level, int score)
    {
        string filePath = Path.Combine(Application.dataPath, _fileName);

        if (!File.Exists(filePath))
        {
            CreateFile();
        }

        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                for (int i = 0; i < _numOfLevels; i++)
                {
                    string[] data = lines[i].Split(',');
                    int existingLevel = int.Parse(data[0]);
                    int existingAttempts = int.Parse(data[1]);
                    int existingScore = int.Parse(data[2]);

                    if (existingLevel == level)
                    {
                        if (score < existingScore)
                        {
                            writer.WriteLine(level + "," + (existingAttempts + 1) + "," + score);
                        }
                        else
                        {
                            writer.WriteLine(existingLevel + "," + (existingAttempts + 1) + "," + existingScore);
                        }
                    }
                    else
                    {
                        writer.WriteLine(existingLevel + "," + existingAttempts + "," + existingScore);
                    }
                }
            }
        }
    }

    public void ReadScore()
    {
        string filePath = Path.Combine(Application.dataPath, _fileName);

        if (!File.Exists(filePath))
        {
            CreateFile();
        }

        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                string[] data = line.Split(',');
                if (data.Length == 3)
                {
                    int level = Convert.ToInt32(data[0]);
                    int attempts = Convert.ToInt32(data[1]);
                    int score = Convert.ToInt32(data[2]);

                    string scoreString = "Poziom " + level + "\nPróby: " + attempts + "\nWynik:  " + score;

                    _scoreTextArray[level].text = scoreString;
                }
            }
        }
    }

    public void CreateFile()
    {
        string filePath = Path.Combine(Application.dataPath, _fileName);
        using (StreamWriter writer = File.CreateText(filePath))
        {
            for (int i = 0; i < _numOfLevels; i++)
            {
                writer.WriteLine(i + "," + 0 + "," + int.MaxValue);
            }
        }
    }

    public void DEBUG_GenerateRandomScore()
    {
        int randomLevel = UnityEngine.Random.Range(0, 6);
        int randomScore = UnityEngine.Random.Range(0, 10000);
        SaveScore(randomLevel, randomScore);
        ReadScore();
    }
}

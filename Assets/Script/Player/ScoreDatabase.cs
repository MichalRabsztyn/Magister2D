using System;
using System.IO;
using UnityEngine;
using TMPro;

public class ScoreDatabase : MonoBehaviour
{
	private const string _fileName = "levelScore.dat";
	private int _numOfLevels = 6;

	[SerializeField] private TextMeshProUGUI[] _scoreTextArray;

	public void SaveScore(int levelIndex, int score)
	{
		if(levelIndex < 0 || levelIndex >= _numOfLevels)
		{
			Debug.LogError("Invalid level number");
			return;
		}

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
					int existingLevelIndex = int.Parse(data[0]);
					int existingAttempts = int.Parse(data[1]);
					int existingScore = int.Parse(data[2]);

					if (existingLevelIndex == levelIndex)
					{
						if (score > 0 && (score < existingScore || existingScore <= 0))
						{
							writer.WriteLine(existingLevelIndex + "," + (existingAttempts + 1) + "," + score);
						}
						else
						{
							writer.WriteLine(existingLevelIndex + "," + (existingAttempts + 1) + "," + existingScore);
						}
					}
					else
					{
						writer.WriteLine(existingLevelIndex + "," + existingAttempts + "," + existingScore);
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

			if (lines.Length != _numOfLevels)
			{
				CreateFile();
			}
			else
			{
				foreach (string line in lines)
				{
					string[] data = line.Split(',');
					if (data.Length == 3)
					{
						int level = Convert.ToInt32(data[0]);
						int attempts = Convert.ToInt32(data[1]);
						int score = Convert.ToInt32(data[2]);

						string scoreString = "Poziom " + level + "\nPróby: " + attempts + "\nWynik: " + score;

						_scoreTextArray[level].text = scoreString;
					}
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
				writer.WriteLine(i + "," + 0 + "," + 0);
			}
		}
	}

	public void ResetScore()
	{
		string filePath = Path.Combine(Application.dataPath, _fileName);
		if (File.Exists(filePath))
		{
			File.Delete(filePath);
		}
		CreateFile();
		ReadScore();
	}

	public void DEBUG_GenerateRandomScore()
	{
		int randomLevel = UnityEngine.Random.Range(0, 6);
		int randomScore = UnityEngine.Random.Range(0, 10000);
		SaveScore(randomLevel, randomScore);
		ReadScore();
	}
}

using UnityEngine;
using UnityEngine.SceneManagement;

namespace GlucoseGames.Sheep
{
	public class GameController : MonoBehaviour
	{
		public int CurrentLevel;

		public int NextLevel;

		string LevelString => "Game" + CurrentLevel.ToString();

		string NextLevelString => "Game" + NextLevel.ToString();

		private void Awake()
		{
			Application.targetFrameRate = 60;
		}

		public void Restart()
		{
			SceneManager.LoadScene(LevelString);
		}

		public void GoNextLevel()
		{
			SceneManager.LoadScene(NextLevelString);
		}
	}
}

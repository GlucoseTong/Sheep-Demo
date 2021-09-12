using UnityEngine;

namespace GlucoseGames.Sheep
{
	[RequireComponent(typeof(Walkable))]
	public class WalkableTarget : MonoBehaviour
	{
		int m_TotalNumbersOfSheep;

		Walkable m_ThisWalkable;
		GameController m_GameController;
		SheepSolver m_SheepSolver;

		void Start()
		{
			m_ThisWalkable = this.gameObject.GetComponent<Walkable>();

			m_SheepSolver = GameObject.FindWithTag("SheepSolver").GetComponent<SheepSolver>();
			if (m_SheepSolver == null)
				Debug.LogError("NavController is missing in scene");

			m_GameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
			if (m_GameController == null)
				Debug.LogError("GameController is missing in scene");

			m_TotalNumbersOfSheep = m_SheepSolver.Sheeps.Count;
		}

		void FixedUpdate()
		{
			CheckComplete();
		}

		void CheckComplete()
		{
			int i = 0;
			foreach (var s in m_SheepSolver.Sheeps)
				if (s.CurrentWalkable == m_ThisWalkable)
					i++;

			if (i == m_TotalNumbersOfSheep)
				LevelComplete();
		}

		void LevelComplete()
		{
			m_GameController.GoNextLevel();
		}
	}
}


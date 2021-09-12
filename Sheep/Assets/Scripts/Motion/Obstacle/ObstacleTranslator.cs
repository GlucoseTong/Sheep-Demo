using UnityEngine;
using GlucoseGames.Sheep.Motion;

namespace GlucoseGames.Sheep
{
	[RequireComponent(typeof(Obstacle))]
	public class ObstacleTranslator : Translator
	{
		Obstacle m_Obstacle;

		protected override void Start()
		{
			base.Start();
			m_Obstacle = GetComponent<Obstacle>();
		}

		void FixedUpdate()
		{
			m_TranslateRatio += Time.fixedDeltaTime * m_Velocity / m_Distance;

			if (m_TranslateRatio > 1)
				m_TranslateRatio = 0;

			this.transform.position = m_Path.Position(m_TranslateRatio);
		}
	}
}

using UnityEngine;
using GlucoseGames.Sheep.Motion;

namespace GlucoseGames.Sheep
{
	[RequireComponent(typeof(WalkableCarry))]
	public class WalkableTranslator : Translator
	{
		WalkableCarry m_WalkableCarry;

		protected override void Start()
		{
			base.Start();
			m_WalkableCarry = GetComponent<WalkableCarry>();
		}

		void FixedUpdate()
		{
			m_TranslateRatio += Time.fixedDeltaTime * m_Velocity / m_Distance;

			if (m_TranslateRatio > 1)
				m_TranslateRatio = 0;

			m_WalkableCarry.ModifiedDeltaPos(m_Path.Position(m_TranslateRatio) - this.transform.position);
		}
	}
}

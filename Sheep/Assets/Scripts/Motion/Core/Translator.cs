using UnityEngine;

namespace GlucoseGames.Sheep.Motion
{
	[RequireComponent(typeof(Path))]
	public abstract class Translator : MonoBehaviour
	{
		public float InitialTranslateRatio;
		public float TotalTimeSingleTrip;

		protected Path m_Path;
		protected float m_Distance => m_Path.TotalLenth;
		protected float m_Velocity => m_Distance / TotalTimeSingleTrip;
		protected float m_TranslateRatio;

		protected virtual void Start()
		{
			m_Path = GetComponent<Path>();

			m_TranslateRatio = InitialTranslateRatio;
		}
	}
}

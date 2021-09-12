using System.Collections.Generic;
using UnityEngine;

namespace GlucoseGames.Sheep.Motion
{
	public class Path : MonoBehaviour
	{
		public float TotalLenth => m_TotalLenth;
		public List<Vector2> ThisPath = new List<Vector2>();

		List<Vector3> m_Path3D = new List<Vector3>();
		float m_WalkableMovingPlane;
		float m_TotalLenth;

		private void OnEnable()
		{
			m_TotalLenth = 0;
			m_Path3D.Clear();
			m_WalkableMovingPlane = this.transform.position.y;

			for (int i = 0; i < ThisPath.Count; i++)
			{
				m_Path3D.Add(new Vector3(ThisPath[i].x, m_WalkableMovingPlane, ThisPath[i].y));
				if (ThisPath.Count > i + 1)
					m_TotalLenth += (ThisPath[i] - ThisPath[i + 1]).magnitude;
			}
		}

		public Vector3 Position(float PathRatio)
		{
			float PathLength = m_TotalLenth * PathRatio;
			float TestLength = 0;

			for (int i = 0; i < m_Path3D.Count; i++)
			{
				if (m_Path3D.Count > i + 1)
				{
					float distance = (m_Path3D[i] - m_Path3D[i + 1]).magnitude;
					TestLength += distance;

					if (TestLength >= PathLength)
						return Vector3.Lerp(m_Path3D[i + 1], m_Path3D[i], (TestLength - PathLength) / distance);
				}
			}

			return ThisPath[ThisPath.Count - 1];
		}

		//Scene drawer, not using 
		//private void OnDrawGizmosSelected()
		//{
		//	foreach (var p in Path3D)
		//	{
		//		Gizmos.color = Color.red;
		//	}
		//}

		//public void AddPoint()
		//{
		//	ThisPath.Add(new Vector2(this.transform.position.x, this.transform.position.z));
		//}

		//public void RemovePath()
		//{
		//	ThisPath.Clear();
		//}
	}
}


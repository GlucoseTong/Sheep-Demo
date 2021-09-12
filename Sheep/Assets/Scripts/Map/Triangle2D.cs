using UnityEngine;

namespace GlucoseGames.Sheep
{
	[System.Serializable]
	public struct Triangle2D
	{
		public Vector2 VerA;
		public Vector2 VerB;
		public Vector2 VerC;

		public Triangle2D(Vector2 VerA_, Vector2 VerB_, Vector2 VerC_)
		{
			VerA = VerA_; VerB = VerB_; VerC = VerC_;
		}

		float sign(Vector2 p1, Vector2 p2, Vector2 p3)
		{
			return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
		}

		public bool PointInTriangle(Vector2 pt)
		{
			float d1, d2, d3;
			bool has_neg, has_pos;

			d1 = sign(pt, VerA, VerB);
			d2 = sign(pt, VerB, VerC);
			d3 = sign(pt, VerC, VerA);

			has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
			has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

			return !(has_neg && has_pos);
		}
	}
}

using UnityEngine;

namespace GlucoseGames.Sheep
{
	[System.Serializable]
	public struct PolygonEdge
	{
		public Vector2 VertexA;
		public Vector2 VertexB;

		public PolygonEdge(Vector2 VertexA_, Vector2 VertexB_)
		{
			VertexA = VertexA_;
			VertexB = VertexB_;
		}

		public bool Equal(Vector2 posA, Vector2 posB)
		{
			if ((posA == VertexA && posB == VertexB) || (posB == VertexA && posA == VertexB))
			{
				return true;
			}
			return false;
		}

		public void Flip()
		{
			Vector2 tempv2;
			tempv2 = VertexA;
			VertexA = VertexB;
			VertexB = tempv2;
		}

		public bool Collinear(PolygonEdge E)
		{
			if (Collinear(E.VertexA) && Collinear(E.VertexB))
			{
				return true;
			}
			return false;
		}

		bool Collinear(Vector2 pos)
		{
			Vector2 a = VertexA - pos;
			Vector2 b = VertexB - pos;
			float f = Vector3.Cross(a, b).magnitude;
			return f <= 0.000026;
		}

		//find point on edge from test position
		public Vector2 Projection(Vector2 Position_)
		{
			Vector2 ap = Position_ - VertexA;
			Vector2 ab = VertexB - VertexA;
			return VertexA + Vector2.Dot(ap, ab) / Vector2.Dot(ab, ab) * ab;
		}

		// point line test collision
		public bool Collided(Vector2 Position_, float TestWidth)
		{
			Vector2 d = VertexB - VertexA;
			Vector2 f = VertexA - Position_;

			float a = Vector2.Dot(d, d);
			float b = 2 * Vector2.Dot(f, d);
			float c = Vector2.Dot(f, f) - TestWidth * TestWidth;

			float discriminant = b * b - 4 * a * c;
			if (discriminant < 0)
			{
				return false;
				// no intersection
			}
			else
			{
				// ray didn't totally miss sphere,
				// so there is a solution to
				// the equation.

				discriminant = Mathf.Sqrt(discriminant);

				// either solution may be on or off the ray so need to test both
				// t1 is always the smaller value, because BOTH discriminant and
				// a are nonnegative.
				float t1 = (-b - discriminant) / (2 * a);
				float t2 = (-b + discriminant) / (2 * a);

				// 3x HIT cases:
				//          -o->             --|-->  |            |  --|->
				// Impale(t1 hit,t2 hit), Poke(t1 hit,t2>1), ExitWound(t1<0, t2 hit), 

				// 3x MISS cases:
				//       ->  o                     o ->              | -> |
				// FallShort (t1>1,t2>1), Past (t1<0,t2<0), CompletelyInside(t1<0, t2>1)

				if (t1 >= 0 && t1 <= 1)
				{
					// t1 is the intersection, and it's closer than t2
					// (since t1 uses -b - discriminant)
					// Impale, Poke
					return true;
				}

				// here t1 didn't intersect so we are either started
				// inside the sphere or completely past it
				if (t2 >= 0 && t2 <= 1)
				{
					// ExitWound
					return true;
				}

				// no intn: FallShort, Past, CompletelyInside
				return false;
			}

		}

		public void FindIntersection(Vector2 p3, Vector2 p4, out bool segments_intersect, out Vector2 intersection)
		{
			bool lines_intersect;
			Vector2 p1 = VertexA;
			Vector2 p2 = VertexB;

			// Get the segments' parameters.
			float dx12 = p2.x - p1.x;
			float dy12 = p2.y - p1.y;
			float dx34 = p4.x - p3.x;
			float dy34 = p4.y - p3.y;

			// Solve for t1 and t2
			float denominator = (dy12 * dx34 - dx12 * dy34);

			float t1 = ((p1.x - p3.x) * dy34 + (p3.y - p1.y) * dx34) / denominator;
			if (float.IsInfinity(t1))
			{
				// The lines are parallel (or close enough to it).
				lines_intersect = false;
				segments_intersect = false;
				intersection = new Vector2(float.NaN, float.NaN);
				return;
			}
			lines_intersect = true;

			float t2 = ((p3.x - p1.x) * dy12 + (p1.y - p3.y) * dx12) / -denominator;

			// Find the point of intersection.
			intersection = new Vector2(p1.x + dx12 * t1, p1.y + dy12 * t1);

			// The segments intersect if t1 and t2 are between 0 and 1.
			segments_intersect = ((t1 >= 0) && (t1 <= 1) && (t2 >= 0) && (t2 <= 1));

			// Find the closest points on the segments.
			if (t1 < 0)
			{
				t1 = 0;
			}
			else if (t1 > 1)
			{
				t1 = 1;
			}

			if (t2 < 0)
			{
				t2 = 0;
			}
			else if (t2 > 1)
			{
				t2 = 1;
			}
		}

		public bool Collided(Vector2 c, Vector2 d)
		{
			float denominator = ((VertexB.x - VertexA.x) * (d.y - c.y)) - ((VertexB.y - VertexA.y) * (d.x - c.x));
			float numerator1 = ((VertexA.y - c.y) * (d.x - c.x)) - ((VertexA.x - c.x) * (d.y - c.y));
			float numerator2 = ((VertexA.y - c.y) * (VertexB.x - VertexA.x)) - ((VertexA.x - c.x) * (VertexB.y - VertexA.y));

			// Detect coincident lines (has a problem, read below)
			if (denominator == 0) return numerator1 == 0 && numerator2 == 0;

			float r = numerator1 / denominator;
			float s = numerator2 / denominator;

			return (r >= 0 && r <= 1) && (s >= 0 && s <= 1);
		}

		public bool IsNeighbour(Vector2 a_, Vector2 b_)
		{
			if ((VertexA == a_ && VertexB != b_) || (VertexA != a_ && VertexB == b_) || (VertexB == a_ && VertexA != b_) || (VertexB != a_ && VertexA == b_))
			{
				return true;
			}
			return false;
		}
	}
}

using System.Collections.Generic;
using UnityEngine;

namespace GlucoseGames.Sheep
{
	public class Walkable : MonoBehaviour
	{
		public float WalkableHeight => this.transform.position.y;

		[SerializeField]
		List<Walkable> m_SubWalkable = new List<Walkable>();

		[SerializeField]
		List<Triangle2D> TrianglesLocalSpace = new List<Triangle2D>();

		[SerializeField]
		List<PolygonEdge> m_PolygonEdges;

		List<Vector2> m_RayIntersection = new List<Vector2>();
		bool m_IsSubWalkable = false;

		// construct edge in local space and init triangles
		// init by scene baker, not using
		/*public void Init(List<PolygonEdge> Edge, List<Triangle2D> Triangles_)
		{
			List<PolygonEdge> temp = new List<PolygonEdge>();
			foreach (var e in Edge)
			{
				Vector3 Va = new Vector3(e.VertexA.x, 0, e.VertexA.y);
				Vector3 Vb = new Vector3(e.VertexB.x, 0, e.VertexB.y);
				Vector2 WorldA2D = new Vector2(this.transform.InverseTransformPoint(Va).x, this.transform.InverseTransformPoint(Va).z);
				Vector2 WorldB2D = new Vector2(this.transform.InverseTransformPoint(Vb).x, this.transform.InverseTransformPoint(Vb).z);
				temp.Add(new PolygonEdge(WorldA2D, WorldB2D));
			}
			m_PolygonEdges = temp;

			List<Triangle2D> Tris = new List<Triangle2D>();
			foreach (var t in Triangles_)
			{
				Vector3 Va = new Vector3(t.VerA.x, 0, t.VerA.y);
				Vector3 Vb = new Vector3(t.VerB.x, 0, t.VerB.y);
				Vector3 Vc = new Vector3(t.VerC.x, 0, t.VerC.y);
				Vector2 WorldA2D = new Vector2(this.transform.InverseTransformPoint(Va).x, this.transform.InverseTransformPoint(Va).z);
				Vector2 WorldB2D = new Vector2(this.transform.InverseTransformPoint(Vb).x, this.transform.InverseTransformPoint(Vb).z);
				Vector2 WorldC2D = new Vector2(this.transform.InverseTransformPoint(Vc).x, this.transform.InverseTransformPoint(Vc).z);
				Tris.Add(new Triangle2D(WorldA2D, WorldB2D, WorldC2D));
			}
			TrianglesLocalSpace = Tris;
		}*/

		//not ensure point inside bounday, use with is point in polygon
		public bool IsInteractWithBoundary(Vector2 Testposition, float DistanceToBoundary)
		{
			foreach (PolygonEdge e in m_PolygonEdges)
				if (e.Collided(EffectiveLocalPosition(Testposition), DistanceToBoundary))
					return true;

			return false;
		}

		//convert sheep position to walkable local space
		//Pushing force with edge normal direction
		//Turning force by double cross product
		public Vector2 VelocityInduce(Sheep sheep, float EdgeRadius)
		{
			if (m_IsSubWalkable == true)
				return Vector2.zero;

			m_RayIntersection.Clear();

			//walkable space
			Vector2 EffectivePosition2D = EffectiveLocalPosition(sheep.Position2D);

			//Pushing force
			Vector2 DeltaPos = Vector2.zero;

			//Turning force
			Vector3 DeltaPosByRay3D = Vector3.zero;

			//sheep space
			Vector3 LocalizedForward3D = Quaternion.Euler(0, sheep.transform.eulerAngles.y, 0) * Vector3.back;

			float ReplusiveConstant = 0;
			foreach (PolygonEdge edge in m_PolygonEdges)
			{
				if (edge.Collided(EffectivePosition2D, EdgeRadius))
				{
					Vector2 Proj = edge.Projection(EffectivePosition2D);
					DeltaPos += (EffectivePosition2D - Proj).normalized;

					float Magnitude = (EdgeRadius - (EffectivePosition2D - Proj).magnitude) / EdgeRadius;

					if (Magnitude > ReplusiveConstant)
						ReplusiveConstant = Magnitude;
				}

				List<Vector2> EffectiveRays = EffectiveLocalPositions(sheep.Rays);

				for (int i = 0; i < EffectiveRays.Count; i++)
				{
					edge.FindIntersection(EffectivePosition2D, EffectiveRays[i], out bool IsIntersect, out Vector2 Interscetion);

					if (IsIntersect)
					{
						Vector3 Ray3D = new Vector3(EffectiveRays[i].x, 0.5f, EffectiveRays[i].y) - new Vector3(EffectivePosition2D.x, 0.5f, EffectivePosition2D.y);
						Vector3 v = Vector3.Cross(LocalizedForward3D, Ray3D).normalized;
						v = Vector3.Cross(LocalizedForward3D, v);
						DeltaPosByRay3D += v;
						m_RayIntersection.Add(Interscetion);

						//Debug draw
						GizmoRay.Add(Interscetion);
					}
				}
			}

			//transform deltapos to world space
			DeltaPos = DeltaPos.normalized * ReplusiveConstant;
			DeltaPos = EffectiveWorldVector(DeltaPos);

			//transform DeltaPosByRay3D to world space
			DeltaPosByRay3D = DeltaPosByRay3D.normalized;
			DeltaPosByRay3D = this.transform.TransformVector(DeltaPosByRay3D);

			Vector2 DeltaPosByRay2D = new Vector2(DeltaPosByRay3D.x, DeltaPosByRay3D.y);
			DeltaPosByRay2D *= ReplusiveConstant;

			return DeltaPos + DeltaPosByRay2D;
		}

		public bool IsPointInPolygon(Vector2 WorldTestPoint)
		{
			foreach (var sub in m_SubWalkable)
			{
				//Prevent recursion
				if (sub.IsSubWalkble(this))
					Debug.Break();

				if (sub.IsPointInPolygon(WorldTestPoint) == true)
					return false;
			}

			foreach (var tri in TrianglesLocalSpace)
				if (tri.PointInTriangle(EffectiveLocalPosition(WorldTestPoint)))
					return true;

			return false;
		}

		public bool IsSubWalkble(Walkable w)
		{
			foreach (var ww in m_SubWalkable)
				if (ww == w) return true;

			return false;
		}

		void OnEnable()
		{
			foreach (var walkable in m_SubWalkable)
				walkable.m_IsSubWalkable = true;
		}

		Vector2 EffectiveLocalPosition(Vector2 testPoint)
		{
			Vector3 p = this.transform.InverseTransformPoint(new Vector3(testPoint.x, 0, testPoint.y));
			return new Vector2(p.x, p.z);
		}

		Vector2 EffectiveWorldVector(Vector2 testPoint)
		{
			Vector3 p = this.transform.TransformVector(new Vector3(testPoint.x, 0, testPoint.y));
			return new Vector2(p.x, p.z);
		}

		List<Vector2> EffectiveLocalPositions(List<Vector2> testPoint)
		{
			List<Vector2> temp = new List<Vector2>();
			foreach (var t in testPoint)
			{
				Vector3 p = this.transform.InverseTransformPoint(new Vector3(t.x, 0, t.y));
				temp.Add(new Vector2(p.x, p.z));
			}
			return temp;
		}

#if UNITY_EDITOR

		List<Vector2> GizmoRay = new List<Vector2>();

		void OnDrawGizmos()
		{
			foreach (var i in GizmoRay)
			{
				Gizmos.color = Color.red;
				Vector3 p = this.transform.TransformPoint(new Vector3(i.x, 0.5f, i.y));
				Gizmos.DrawSphere(p, 0.5f);
			}
			GizmoRay.Clear();

			for (int i = 0; i < m_PolygonEdges.Count; i++)
			{
				Gizmos.color = Color.red;
				var e = m_PolygonEdges[i];
				Vector3 Va = new Vector3(e.VertexA.x, 0.5f, e.VertexA.y);
				Vector3 Vb = new Vector3(e.VertexB.x, 0.5f, e.VertexB.y);
				Vector3 WorldA2D = new Vector3(this.transform.TransformPoint(Va).x, this.transform.TransformPoint(Va).z);
				Vector3 WorldB2D = new Vector3(this.transform.TransformPoint(Vb).x, this.transform.TransformPoint(Vb).z);
				Vector3 a = new Vector3(WorldA2D.x, 0.5f, WorldA2D.y);
				Vector3 b = new Vector3(WorldB2D.x, 0.5f, WorldB2D.y);
				Gizmos.DrawLine(a, b);
			}
		}
#endif
	}
}

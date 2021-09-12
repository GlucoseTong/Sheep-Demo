using System.Collections.Generic;
using UnityEngine;

namespace GlucoseGames.Sheep
{
	public class SheepSolver : MonoBehaviour
	{
		//Maximum sheep to sheep interaction range
		const float SheepAlliedRadius = 5;

		//Maximum sheep to edge interaction range
		const float EdgeRadius = 1;

		//Sheep to sheep turning interaction constant
		const float SheepRadAllience = 0.5f;

		//Sheep to edge replusive constant
		const float EdgeReplusive = 25;

		//Sheep to Sheep replusive constant
		const float SheepReplusive = 20;

		//Sheep to dog replusive constant
		const float DogReplusive = 50;

		//Raycast layer
		const int ObstacleLayer = 11;

		public bool IsApplyForce;

		public List<Sheep> Sheeps => m_Sheeps;
		public List<Walkable> Walkables => m_Walkables;

		List<Sheep> m_Sheeps = new List<Sheep>();
		List<Walkable> m_Walkables = new List<Walkable>();
		Dog m_Dog;

		void FixedUpdate()
		{
			if (IsApplyForce == true) UpdateSheep();
		}

		//handle priority 1 & 2 only, check for priority 0
		//priority-1 sheep on moving walkable
		//priority0 sheep block by physics collider
		//priority1 compute velocity induce by all inducer
		//priority2 compute sheep to sheep allience 
		//apply result
		void UpdateSheep()
		{
			Dictionary<Sheep, Vector2> SheepDeltaVelDict = new Dictionary<Sheep, Vector2>();

			//compute velocity induce by all inducer
			foreach (Sheep sheep in Sheeps)
				SheepDeltaVelDict.Add(sheep, SheepDeltaVel(sheep));

			//compute sheep allience and modify dictionary
			//apply result
			foreach (Sheep sheep in Sheeps)
				sheep.UpdatePosition(SheepComputedVel(sheep, SheepDeltaVelDict) * Time.fixedDeltaTime,
									 DogReplusive * m_Dog.VelocityInduce(sheep) / m_Dog.Radius);
		}

		//Sheep to other interaction
		//Pushing sheep away
		Vector2 SheepDeltaVel(Sheep sheep)
		{
			Vector2 DeltaVel = Vector2.zero;

			DeltaVel += DogReplusive * m_Dog.VelocityInduce(sheep);

			foreach (var ss in Sheeps)
			{
				if (sheep == ss)
					continue;

				DeltaVel += SheepReplusive * ss.VelocityInduce(sheep);
			}

			if (sheep.CurrentWalkable == null)
				return DeltaVel;

			DeltaVel += EdgeReplusive * sheep.CurrentWalkable.VelocityInduce(sheep, EdgeRadius);

			//Test RayCast
			//Prevent sheep penetrate any physic collider
			if (Physics.Raycast(sheep.transform.position, PositionTo3D(DeltaVel).normalized, out RaycastHit hit, 2, ObstacleLayer))
			{
				float Distance = (sheep.Collider.ClosestPoint(hit.point) - hit.point).magnitude;
				Distance /= Time.fixedDeltaTime;

				if (DeltaVel.magnitude > Distance)
					DeltaVel = DeltaVel.normalized * Distance;
			}
			return DeltaVel;
		}

		//Add sheep to sheep allied interaction
		//Pulling sheep together
		Vector2 SheepComputedVel(Sheep sheep, Dictionary<Sheep, Vector2> SheepDeltaVelDict)
		{
			int neighbour = 0;
			Vector2 Center = Vector2.zero;

			foreach (var item in SheepDeltaVelDict)
			{
				if (sheep == item.Key)
					continue;

				if (Vector2.Distance(item.Key.Position2D, sheep.Position2D) <= SheepAlliedRadius)
				{
					Center += item.Key.Position2D;
					neighbour++;
				}
			}

			//sheep ioslated
			if (neighbour == 0)
				return SheepDeltaVelDict[sheep];

			Center /= (float)(neighbour);

			Vector3 GoDir = SheepDeltaVelDict[sheep].normalized;
			Vector3 VecToCenterDir = (Center - sheep.Position2D).normalized;

			//angle between sheep (delta pos dir) and (this to center of sheeps)
			float SinAngle = Vector3.Dot(Vector3.forward, Vector3.Cross(GoDir, VecToCenterDir));

			return Rotate(SheepDeltaVelDict[sheep], Mathf.Lerp(0, Mathf.Asin(SinAngle), SheepRadAllience));
		}

		Vector2 Rotate(Vector2 v, float Rad)
		{
			float sin = Mathf.Sin(Rad);
			float cos = Mathf.Cos(Rad);

			float tx = v.x;
			float ty = v.y;
			v.x = (cos * tx) - (sin * ty);
			v.y = (sin * tx) + (cos * ty);
			return v;
		}

		Vector3 PositionTo3D(Vector2 value) => new Vector3(value.x, 0, value.y);

		void OnEnable()
		{
			m_Walkables.Clear();
			m_Sheeps.Clear();

			foreach (GameObject wGO in GameObject.FindGameObjectsWithTag("Walkable"))
				m_Walkables.Add(wGO.GetComponent<Walkable>());

			if (m_Walkables.Count == 0)
				Debug.Log("Walkable is missing in scene");


			foreach (GameObject s in GameObject.FindGameObjectsWithTag("Sheep"))
				m_Sheeps.Add(s.GetComponent<Sheep>());

			if (m_Sheeps.Count == 0)
				Debug.Log("Sheep is missing in scene");

			m_Dog = GameObject.FindGameObjectWithTag("Dog").GetComponent<Dog>();
			if (m_Dog == null)
				Debug.Log("Dog is missing in scene");
		}
	}
}

using System.Collections.Generic;
using UnityEngine;

namespace GlucoseGames.Sheep
{
	[RequireComponent(typeof(Collider))]
	[RequireComponent(typeof(AnimalMovement))]
	public abstract class Animal : MonoBehaviour
	{
		//Maximum Jump Search distance
		const float JumpDistance = 3f;

		//Jump Search angle
		const float CheckJumpViewAngle = 90;

		//Distance within 0.8f to edge skip jump search
		const float JumpDistinationWalkEdgeBuffer = 0.8f;

		//Numbers of jump search rays
		const int RaysCheckJumpNumberOfCast = 4;

		//Angle between 2 rays
		const float RaysAngleIncrease = CheckJumpViewAngle / ((float)RaysCheckJumpNumberOfCast - 1f);

		//layer mask
		const int ObstacleLayer = 11;

		//Interaction Radius
		public float Radius => m_Radius;

		public List<Vector2> Rays => m_Rays;
		public Vector2 Position2D => new Vector2(this.transform.position.x, this.transform.position.z);
		public Walkable CurrentWalkable => m_CurrentWalkable;
		public Collider Collider => m_Collider;

		protected System.Action m_EventStopUpdate;
		protected AnimalMovement m_AnimalMovement;

		[SerializeField]
		float m_Radius;
		List<Vector2> m_Rays = new List<Vector2>();
		SheepSolver m_SheepSolver;
		GameController m_GameController;
		Walkable m_CurrentWalkable;
		Collider m_Collider;

		float StartAngle => this.transform.eulerAngles.y - CheckJumpViewAngle / 2;

		// Direction = (TestPosition - Position2D).normalized)
		// Velocity = (Radius - (TestPosition - Position2D).magnitude) / Radius
		public Vector2 VelocityInduce(Animal animal)
		{
			if (Vector2.Distance(animal.Position2D, Position2D) >= Radius)
				return Vector2.zero;

			return ((animal.Position2D - Position2D).normalized) * (Radius - (animal.Position2D - Position2D).magnitude) / Radius;
		}

		//update form obstacle
		public void ForceUpdatePosition(Vector2 DeltaPos_)
		{
			SearchWalkable();
			m_AnimalMovement.UpdateDeltaTransform(DeltaPos_);
		}

		//update from walkable carry
		public void ForceUpdatePosition(Vector3 DeltaPos, float DeltaEularY)
		{
			m_AnimalMovement.UpdateDeltaTransform(DeltaPos, DeltaEularY);
		}

		//stop animal velocity input 
		//stop sheep solver update
		public void StopUpdate()
		{
			m_EventStopUpdate += () => { };
		}

		public virtual void UpdateTrait(Vector2 deltaPos)
		{

		}

		//search walkable and test for jump
		protected void SearchWalkable()
		{
			if (m_EventStopUpdate != null)
				return;

			Walkable walkable = null;
			Walkable RayWalkable = null;
			int NumberOfHit = 0;
			float k = 0;

			//brutal force search
			foreach (var w in m_SheepSolver.Walkables)
			{
				if (w.IsPointInPolygon(Position2D))
				{
					walkable = w;
				}
				for (int i = 0; i < RaysCheckJumpNumberOfCast; i++)
				{
					if (w.IsPointInPolygon(Rays[i]))
					{
						if (w.IsInteractWithBoundary(Rays[i], JumpDistinationWalkEdgeBuffer))
							continue;

						RayWalkable = w;

						if (w == m_CurrentWalkable)
							continue;

						NumberOfHit++;
						k += i;
					}
				}
			}

			//case on same walkable
			//case ray not on any walkable
			if ((RayWalkable == walkable || RayWalkable == null)
				 && walkable != null)
			{
				m_CurrentWalkable = walkable;
				return;
			}

			// Jump
			// case jump search other walkable while still on this walkable
			// except sub-walkable / parent walkable
			if (RayWalkable != walkable && walkable != null && RayWalkable != null)
			{
				if (walkable.IsSubWalkble(RayWalkable) || RayWalkable.IsSubWalkble(walkable))
				{
					m_CurrentWalkable = walkable;
					return;
				}
				k /= (float)NumberOfHit;
				Vector3 Destination = Quaternion.Euler(0, StartAngle + RaysAngleIncrease * k, 0) * Vector3.back;
				m_AnimalMovement.UpdateJump((JumpDistance), Destination, RayWalkable);
				m_CurrentWalkable = null;
				return;
			}

			//otherwise fall
			Fall();
			m_CurrentWalkable = null;
		}

		protected void ComputeRays()
		{
			m_Rays.Clear();
			for (int i = 0; i < RaysCheckJumpNumberOfCast; i++)
			{
				Vector3 Ray = Quaternion.Euler(0, StartAngle + RaysAngleIncrease * i, 0) * Vector3.back * JumpDistance;

				if (Physics.Raycast(transform.position, Ray.normalized, out RaycastHit hit, JumpDistance))
				{
					Ray = Ray.normalized * hit.distance;
				}

				m_Rays.Add(PositionTo2D(Ray) + PositionTo2D(this.transform.position));
			}
		}

		protected virtual void Start()
		{
			m_SheepSolver = GameObject.FindWithTag("SheepSolver").GetComponent<SheepSolver>();
			if (m_SheepSolver == null)
				Debug.LogError("NavController is missing in scene");

			m_GameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
			if (m_GameController == null)
				Debug.LogError("GameController is missing in scene");
			
			m_Collider = GetComponent<Collider>();
			m_AnimalMovement = GetComponent<AnimalMovement>();

			this.transform.eulerAngles = new Vector3(0, Random.Range(0, 360f), 0);

			ComputeRays();

			InitialCurrentWalkable();
		}

		protected virtual void FixedUpdate()
		{
			ComputeRays();
		}
	
		void InitialCurrentWalkable()
		{
			foreach (var w in m_SheepSolver.Walkables)
				if (w.IsPointInPolygon(Position2D))
					m_CurrentWalkable = w;
		}

		//Add animation later
		void Fall()
		{
			m_GameController.Restart();
			Debug.Log("Fall");
		}
	
		Vector2 PositionTo2D(Vector3 value) => new Vector2(value.x, value.z);

	#if UNITY_EDITOR

		protected virtual void OnDrawGizmos()
		{
			Gizmos.color = Color.blue;
			foreach (var r in Rays)
			{
				Vector3 Start = new Vector3(this.transform.position.x, 0.5f, this.transform.position.z);
				Vector3 End = new Vector3(r.x, 0.5f, r.y);
				Gizmos.DrawLine(Start, End);
			}

			Gizmos.color = Color.red;
			Vector3 Center = new Vector3(this.transform.position.x, 0.5f, this.transform.position.z);
			Gizmos.DrawWireSphere(Center, m_Radius);
		}

	#endif

	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GlucoseGames.Sheep
{
	[RequireComponent(typeof(Animal))]
	public class AnimalMovement : MonoBehaviour
	{
		// Jump perform by transfering to new walkable 
		// Walk perfrom by movement within same walkable

		// Jump(vertical) Magnitude
		const float JumpMagnitude = 3f;

		// Jump(horizontal) speed
		const float JumpSpeed = 10f;

		// Walk(vertical) Magnitude
		const float WalkMagnitude = 0.3f;

		// Total Walk(horizontal) distance
		const float WalkCycleDistance = 0.7f;

		// Walk(horizontal) speed
		const float WalkSpeed = 7f;

		event System.Action StopReset;
		List<Vector2> m_DirectionTrait = new List<Vector2>();
		float m_CurrentWalkDistance;
		Vector3 m_ThisPosition;
		Vector2 m_DeltaPos2D;
		Animal m_Animal;

		private void OnEnable()
		{
			m_DirectionTrait.Clear();

			m_Animal = GetComponent<Animal>();
		}

		//Reset animal y position while not moving
		private void FixedUpdate()
		{
			m_ThisPosition = this.transform.position;
			StartCoroutine(DeltaPos());

			//return if have delta pos input
			if (StopReset != null)
			{
				StopReset = null;
				return;
			}

			if (m_CurrentWalkDistance != 0)
			{
				m_CurrentWalkDistance += WalkSpeed * Time.deltaTime;

				if (m_CurrentWalkDistance > WalkCycleDistance)
					m_CurrentWalkDistance = 0;

				this.transform.position =
					new Vector3(this.transform.position.x,
					GroundHeight + WalkMagnitude * HeightRatio(m_CurrentWalkDistance / WalkCycleDistance),
					this.transform.position.z);
			}
		}

		// Update walk(height) after getting delta pos 2d this frame
		IEnumerator DeltaPos()
		{
			yield return new WaitForFixedUpdate();

			if (m_DeltaPos2D.magnitude <= 0.05f)
				yield break;

			m_CurrentWalkDistance += m_DeltaPos2D.magnitude;
			StopReset += () => { };

			if (m_CurrentWalkDistance > WalkCycleDistance)
				m_CurrentWalkDistance -= WalkCycleDistance;

			this.transform.position =
				new Vector3(this.transform.position.x,
				GroundHeight + WalkMagnitude * HeightRatio(m_CurrentWalkDistance / WalkCycleDistance),
				this.transform.position.z);

			m_DeltaPos2D = Vector2.zero;
		}

		// update from animal walk
		public void UpdateDeltaTransform(Vector2 DeltaPos, Vector2 LookDir)
		{
			m_DeltaPos2D += DeltaPos;
			this.transform.position += PositionTo3D(DeltaPos);
			this.transform.eulerAngles = new Vector3(0, Vector2ToRadian(LookDir) + 180, 0);
		}

		// update from Obstacle
		public void UpdateDeltaTransform(Vector2 DeltaPos)
		{
			StopReset += () => { };
			m_DeltaPos2D += DeltaPos;
			this.transform.position += PositionTo3D(DeltaPos);
		}

		// update from walkable carry
		public void UpdateDeltaTransform(Vector3 DeltaPos, float DeltaEularY)
		{
			this.transform.position += DeltaPos;
			this.transform.eulerAngles += new Vector3(0, DeltaEularY, 0);
		}

		public void UpdateJump(float JumpDistance, Vector3 LookDir, Walkable TargetWalkable)
		{
			StopAllCoroutines();
			StartCoroutine(IeUpdateJump(JumpDistance, LookDir, TargetWalkable));
		}

		// use cos(t) curve as jump(vertical) curve
		// jump(horizontal) update in linear speed
		// force animal stop interact with other force
		IEnumerator IeUpdateJump(float JumpDistance, Vector3 LookDir, Walkable TargetWalkable)
		{
			Vector3 WorldDestination = this.transform.position + LookDir.normalized * JumpDistance;
			Vector3 TargetLocalSpace = TargetWalkable.transform.InverseTransformPoint(WorldDestination);

			Vector2 InitialPos2D = new Vector2(this.transform.position.x, this.transform.position.z);

			float totaltime = JumpDistance / JumpSpeed;
			float t = 0;

			float eularY = Vector2ToRadian(new Vector2(LookDir.normalized.x, LookDir.normalized.z));

			while (t <= totaltime)
			{
				yield return new WaitForFixedUpdate();
				m_CurrentWalkDistance += JumpSpeed * Time.deltaTime;
				t += Time.fixedDeltaTime;

				// x-z plane
				Vector2 ThisFramePosition = new Vector2(this.transform.position.x, this.transform.position.z);

				Vector3 TargetWorldSpace3D = TargetWalkable.transform.TransformPoint(TargetLocalSpace);
				Vector2 TargetWorldSpace2D = new Vector2(TargetWorldSpace3D.x, TargetWorldSpace3D.z);
				Vector2 Pos2D = Vector2.Lerp(InitialPos2D, TargetWorldSpace2D, t / totaltime);
				Vector3 Pos3D = new Vector3(Pos2D.x, GroundHeight, Pos2D.y);

				this.transform.position = Pos3D + JumpMagnitude * HeightRatio(t / totaltime) * Vector3.up;
				this.transform.eulerAngles = new Vector3(0, eularY + 180, 0);
				m_Animal.UpdateTrait(Pos2D - ThisFramePosition);
				m_Animal.StopUpdate();
			}
		}

		// Return normalized height ratio from cycle Ratio 
		float HeightRatio(float cycleRatio)
		{
			if (float.NaN == (0.5f - Mathf.Cos(cycleRatio) / 2))
				return 0;

			return 0.5f - Mathf.Cos(cycleRatio * Mathf.PI * 2) / 2f;
		}

		float Vector2ToRadian(Vector2 p_vector2)
		{
			if (p_vector2.x < 0)
				return 360 - (Mathf.Atan2(p_vector2.x, p_vector2.y) * Mathf.Rad2Deg * -1);

			else
				return Mathf.Atan2(p_vector2.x, p_vector2.y) * Mathf.Rad2Deg;
		}

		Vector3 PositionTo3D(Vector2 value) => new Vector3(value.x, 0, value.y);

		//Ground level set as 0
		float GroundHeight => (m_Animal.CurrentWalkable == null) ? 0 : m_Animal.CurrentWalkable.WalkableHeight;
	}
}

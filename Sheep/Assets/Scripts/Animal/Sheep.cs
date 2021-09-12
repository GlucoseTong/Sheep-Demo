using System.Collections.Generic;
using UnityEngine;

namespace GlucoseGames.Sheep
{
	public class Sheep : Animal
	{
		// Smallest Total displacement of trait
		const float DirectionTraitAbsLenght = 1;

		// Largest Total distance of trait
		const float DirectionTraitLenght = 3;

		// If delta position is smaller than this value stop update
		const float DeltaPositionThreshold = 0.05f;

		List<Vector2> DirectionTrait = new List<Vector2>();

		//Updated from navcontroller / fixed update
		public void UpdatePosition(Vector2 DeltaPos_, Vector2 DogVelocityInduce)
		{
			if (m_EventStopUpdate != null)
			{
				m_EventStopUpdate = null;
				return;
			}

			SearchWalkable();

			if (DeltaPos_.magnitude <= DeltaPositionThreshold || float.IsNaN(DeltaPos_.x) || float.IsNaN(DeltaPos_.y))
				return;

			UpdateTrait(DeltaPos_);

			m_AnimalMovement.UpdateDeltaTransform(DeltaPos_, LookDir(DogVelocityInduce));
		}

		// compute look direction from trait and dog position
		Vector2 LookDir(Vector2 DogVelocityInduce)
		{
			Vector2 temp = Vector2.zero;

			foreach (Vector2 v in DirectionTrait)
				temp += v;

			temp += DogVelocityInduce * 2;

			return temp.normalized;
		}

		public override void UpdateTrait(Vector2 deltaPos)
		{
			if (deltaPos.magnitude >= DeltaPositionThreshold)
				DirectionTrait.Add(deltaPos);

			float lenght = 0;
			int RemoveCount = 0;
			Vector2 Path = Vector2.zero;

			for (int i = DirectionTrait.Count - 1; 0 <= i; i--)
			{
				Path += DirectionTrait[i];
				lenght += DirectionTrait[i].magnitude;
				if (lenght > DirectionTraitLenght && Path.magnitude > DirectionTraitAbsLenght)
				{
					RemoveCount = i;
					break;
				}
			}
			DirectionTrait.RemoveRange(0, RemoveCount);
		}

#if UNITY_EDITOR

		protected override void OnDrawGizmos()
		{
			base.OnDrawGizmos();

			Gizmos.color = Color.green;
			Vector3 Origin = new Vector3(this.transform.position.x, 0.5f, this.transform.position.z);
			Vector3 vi = Origin;

			for (int i = DirectionTrait.Count - 1; 0 <= i; i--)
			{
				Vector3 vi1 = Origin - new Vector3(DirectionTrait[i].x, 0, DirectionTrait[i].y);
				Gizmos.DrawLine(Origin, vi1);
				Origin -= new Vector3(DirectionTrait[i].x, 0, DirectionTrait[i].y);
			}
		}

#endif
	}
}

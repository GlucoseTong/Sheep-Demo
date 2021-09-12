using UnityEngine;

namespace GlucoseGames.Sheep
{
	public class Dog : Animal
	{
		const float DogMoveVelocity = 7;
		FloatingJoystick FloatingJoystick;

		protected override void Start()
		{
			base.Start();

			FloatingJoystick = GameObject.FindWithTag("FloatingJoystick").GetComponent<FloatingJoystick>();
			if (FloatingJoystick == null)
				Debug.Log("FloatingJoystick is missing");
		}

		protected override void FixedUpdate()
		{
			base.FixedUpdate();

			Vector3 direction = Vector3.forward * FloatingJoystick.Vertical + Vector3.right * FloatingJoystick.Horizontal;
			Vector3 DeltaPos = direction.normalized * DogMoveVelocity * Time.fixedDeltaTime;

			UpdatePosition(DeltaPos, direction);
		}

		//When jumping, stop update from this
		void UpdatePosition(Vector3 DeltaPos_, Vector3 LookDir)
		{
			if (m_EventStopUpdate != null)
			{
				m_EventStopUpdate = null;
				return;
			}

			SearchWalkable();

			if (DeltaPos_.magnitude <= 0.05f || float.IsNaN(DeltaPos_.x) || float.IsNaN(DeltaPos_.y))
				return;

			m_AnimalMovement.UpdateDeltaTransform(new Vector2(DeltaPos_.x, DeltaPos_.z), new Vector2(LookDir.x, LookDir.z));
		}
	}
}
using UnityEngine;
using GlucoseGames.Sheep.Motion;

namespace GlucoseGames.Sheep
{
	public class ObstacleRotor : Rotor
	{
		void FixedUpdate()
		{
			this.transform.eulerAngles = new Vector3(0, CurrentAngle + AngularVelocity * Time.fixedDeltaTime, 0);
		}

#if UNITY_EDITOR
		private void Reset()
		{
			Collider[] Colliders = GetComponentsInChildren<Collider>();
			foreach (var c in Colliders)
				if (c.gameObject.GetComponent<Obstacle>() == null)
					c.gameObject.AddComponent<Obstacle>();
		}
#endif
	}
}

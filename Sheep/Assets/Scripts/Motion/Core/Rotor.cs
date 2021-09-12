using UnityEngine;

namespace GlucoseGames.Sheep.Motion
{
	public abstract class Rotor : MonoBehaviour
	{
		public float AngularVelocity;
		public float CurrentAngle => this.transform.eulerAngles.y;
	}
}

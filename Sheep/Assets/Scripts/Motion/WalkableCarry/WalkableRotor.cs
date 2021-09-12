using UnityEngine;
using GlucoseGames.Sheep.Motion;

namespace GlucoseGames.Sheep
{
	[RequireComponent(typeof(WalkableCarry))]
	public class WalkableRotor : Rotor
	{
		WalkableCarry m_WalkableCarry;

		void Start()
		{
			m_WalkableCarry = GetComponent<WalkableCarry>();
		}

		void FixedUpdate()
		{
			m_WalkableCarry.ModifiedDeltaEularY(AngularVelocity * Time.fixedDeltaTime);
		}
	}
}

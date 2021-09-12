using System.Collections.Generic;
using UnityEngine;

namespace GlucoseGames.Sheep
{
	[RequireComponent(typeof(Walkable))]
	public class WalkableCarry : MonoBehaviour
	{
		Walkable m_Walkable;
		List<Animal> m_Animals = new List<Animal>();
		float m_DeltaEularY;
		Vector3 m_DeltaPos;

		public void ModifiedDeltaEularY(float value)
		{
			m_DeltaEularY += value;
		}

		public void ModifiedDeltaPos(Vector3 value)
		{
			m_DeltaPos += value;
		}

		void Start()
		{
			SheepSolver SheepSolver = GameObject.FindGameObjectWithTag("SheepSolver").GetComponent<SheepSolver>();
			if (SheepSolver == null)
				Debug.Log("SheepSolver is missing in scene");

			m_Walkable = this.gameObject.GetComponent<Walkable>();

			m_Animals.Add(GameObject.FindGameObjectWithTag("Dog").GetComponent<Dog>());
			m_Animals.AddRange(SheepSolver.Sheeps);
		}

		//Apply delta transform
		void FixedUpdate()
		{
			List<Animal> AnimalOnThis = m_Animals.FindAll(x => x.CurrentWalkable == m_Walkable);

			List<Vector3> V3 = new List<Vector3>();

			for (int i = 0; i < AnimalOnThis.Count; i++)
				V3.Add(this.transform.worldToLocalMatrix.MultiplyPoint3x4(AnimalOnThis[i].transform.position));

			this.transform.eulerAngles = new Vector3(0, this.transform.eulerAngles.y + m_DeltaEularY, 0);
			this.transform.position += m_DeltaPos;

			for (int i = 0; i < AnimalOnThis.Count; i++)
			{
				Vector3 DeltaPos = transform.TransformPoint(V3[i]) - AnimalOnThis[i].transform.position;
				AnimalOnThis[i].ForceUpdatePosition(DeltaPos, m_DeltaEularY);
			}

			m_DeltaEularY = 0;
			m_DeltaPos = Vector3.zero;
		}
	}
}

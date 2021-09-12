using UnityEngine;

namespace GlucoseGames.Sheep
{
	[RequireComponent(typeof(Rigidbody))]
	public class Obstacle : MonoBehaviour
	{
		Collider m_Collider;

		void Start()
		{
			m_Collider = GetComponent<Collider>();
		}

		void OnTriggerStay(Collider other)
		{
			if (other.gameObject.CompareTag("Sheep") || other.gameObject.CompareTag("Dog"))
			{
				Physics.ComputePenetration(m_Collider, this.transform.position, this.transform.rotation, other,
					other.transform.position, other.transform.rotation, out Vector3 direction, out float distance);

				Vector3 TotalDeltaD = -direction * distance;

				other.GetComponent<Animal>().ForceUpdatePosition(new Vector2(TotalDeltaD.x, TotalDeltaD.z));
			}
		}

#if UNITY_EDITOR
		private void Reset()
		{
			if (this.gameObject.GetComponent<Rigidbody>() == null)
				this.gameObject.AddComponent<Rigidbody>();

			Rigidbody R = this.gameObject.GetComponent<Rigidbody>();
			R.isKinematic = true;
			R.useGravity = false;
		}
#endif
	}
}

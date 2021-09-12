using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawMeshTriangle : MonoBehaviour
{
	public MeshFilter mf;

	private void OnDrawGizmos()
	{	
		Mesh m = mf.sharedMesh;
		m.RecalculateNormals(60);
		
		Vector3[] vertices = m.vertices;
		int[] triangles = m.triangles;
		
		for (int i = 0; i < triangles.Length / 3; i++)
		{			
			int k = i * 3;
			Vector3 VA = vertices[triangles[k]];
			Vector3 VB = vertices[triangles[k + 1]];
			Vector3 VC = vertices[triangles[k + 2]];
			Gizmos.DrawLine(VA, VB);
			Gizmos.DrawLine(VC, VB);
			Gizmos.DrawLine(VA, VC);
		}

		for (int i = 0; i < vertices.Length; i++)
		{
			Gizmos.DrawSphere(vertices[i],0.05f);
			Gizmos.DrawLine(vertices[i], vertices[i] + m.normals[i].normalized);
		}
	}
}

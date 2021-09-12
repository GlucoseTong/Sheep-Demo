using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothNormal : MonoBehaviour
{
	public Vector3[] vertices;
	public Vector3[] normals;
	public int[] triangles;
	public float NormalPushValue;
	public float SmoothAngle;

	void test()
	{
		var mesh = GetComponent<MeshFilter>().sharedMesh;
		Mesh clonedMesh = new Mesh();

		clonedMesh.name = mesh.name + "PushNormal" + NormalPushValue.ToString();
		clonedMesh.vertices = mesh.vertices;
		clonedMesh.triangles = mesh.triangles;
		clonedMesh.normals = mesh.normals;
		clonedMesh.uv = mesh.uv;
		clonedMesh.RecalculateNormals(SmoothAngle);

		vertices = clonedMesh.vertices;
		triangles = clonedMesh.triangles;
		normals = clonedMesh.normals;

		for (int i = 0; i < vertices.Length; i++)
		{
			Debug.Log(vertices[i]);
			vertices[i] += normals[i] * NormalPushValue;
			Debug.Log(vertices[i]);			
		}
		clonedMesh.vertices = vertices;
		GetComponent<MeshFilter>().mesh = clonedMesh;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		for (int i = 0; i < vertices.Length; i++)
		{
			Gizmos.DrawSphere(vertices[i], 0.05f);
			Gizmos.DrawLine(vertices[i], vertices[i] + normals[i].normalized);
		}		
	}
}

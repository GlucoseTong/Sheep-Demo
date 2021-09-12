using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MeshSkirtGeneration : MonoBehaviour
{
	[System.Serializable]
	public struct Edge
	{
		public Vector3 VerA;
		public Vector3 VerB;
		public int VerAInt;
		public int VerBInt;

		public Edge(Vector3 VerA_, Vector3 VerB_, int VerAInt_, int VerBInt_)
		{
			VerA = VerA_; VerB = VerB_; VerAInt = VerAInt_; VerBInt = VerBInt_;
		}

		public bool OneEndMatch(Edge E)
		{
			bool IsVaEqual = (V3Equal(VerA, E.VerA) || V3Equal(VerA, E.VerB));
			bool IsVbEqual = (V3Equal(VerB, E.VerA) || V3Equal(VerB, E.VerB));
			return (IsVaEqual == true && IsVbEqual == false) || (IsVaEqual == false && IsVbEqual == true);
		}

		public bool Equal(Edge E)
		{
			bool IsVaEqual = (V3Equal(VerA, E.VerA) || V3Equal(VerA, E.VerB));
			bool IsVbEqual = (V3Equal(VerB, E.VerA) || V3Equal(VerB, E.VerB));
			return (IsVaEqual == true && IsVbEqual == true);
		}

		public float Flatness
		{
			get
			{
				return Mathf.Abs(Vector3.Dot(Vector3.up,(VerA - VerB)));
			}
		}

		public bool V3Equal(Vector3 a, Vector3 b)
		{
			return Vector3.SqrMagnitude(a - b) < 0.00000001;
		}

		public bool IsConsist(Vector3 TestPoint)
		{
			return V3Equal(TestPoint, VerA) || V3Equal(TestPoint, VerB);
		}

		public void Flip()
		{
			Vector3 tempv3;
			int tempint;
			tempv3 = VerA;
			VerA = VerB;
			VerB = tempv3;
			tempint = VerAInt;
			VerAInt = VerBInt;
			VerBInt = tempint;
		}
	}


	public float WaterLevel;
	public float SkirtRadius;
	public Material WaveMaterial;

	[SerializeField]
	LayerMask m_LayerMask = ~0;
	public LayerMask layerMask { get { return m_LayerMask; } set { m_LayerMask = value; } }

	Vector3 Origin;

	public void Bake()
	{			
		List<GameObject> GOs = new List<GameObject>();
		GameObject[] goArray = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

		GOs.AddRange(goArray);
		
		for (var i = 0; i < goArray.Length; i++)
		{
			Transform[] allChildren = goArray[i].GetComponentsInChildren<Transform>();
			foreach (Transform child in allChildren)
			{
				GOs.Add(child.gameObject);
			}
		}

		for (int i = 0; i < GOs.Count; i++)
		{
			if (m_LayerMask == (m_LayerMask | (1 << GOs[i].layer)))
			{
				MeshFilter mf = GOs[i].GetComponent<MeshFilter>();
				if (mf == null) continue;
				Bake(GOs[i]);
			}
		}
	}

	public void Bake(GameObject OriginGo)
	{
		Origin = Vector3.positiveInfinity;
		List<int> tris = new List<int>();
		List<Edge> Edges = new List<Edge>();
		List<Vector3> SkirtNormal = new List<Vector3>();
		List<List<Edge>> SplitPaths = new List<List<Edge>>();
		List<Edge> MeshLoop = new List<Edge>();
		List<Vector3> MeshSmallLoopV3 = new List<Vector3>();
		List<Vector3> MeshlLargeLoopV3 = new List<Vector3>();
		List<Vector3> MeshMergeLoopV3 = new List<Vector3>();
		List<Vector2> SmallLoopUV = new List<Vector2>();
		List<Vector2> LargeLoopUV = new List<Vector2>();
		List<Vector2> MergeLoopUV = new List<Vector2>();
		List<Vector3> MeshNormal = new List<Vector3>();
		List<Vector3> ProjectedNormal = new List<Vector3>();
		List<Edge> temp = new List<Edge>();

		var mesh = OriginGo.GetComponent<MeshFilter>().sharedMesh;

		Mesh clonedMesh = new Mesh();
		clonedMesh.name = mesh.name + "Clone";
		clonedMesh.vertices = mesh.vertices;
		clonedMesh.triangles = mesh.triangles;
		clonedMesh.normals = mesh.normals;
		clonedMesh.uv = mesh.uv;
		mesh = clonedMesh;	

		//Create Edge list
		for (int i = 0; i < mesh.triangles.Length / 3; i++)
		{
			int k = i * 3;
			if (Mathf.Abs(Vector3.Dot(mesh.normals[mesh.triangles[k]], Vector3.up)) >= 0.99f) continue;

			Vector3 VA = mesh.vertices[mesh.triangles[k]];
			Vector3 VB = mesh.vertices[mesh.triangles[k + 1]];
			Vector3 VC = mesh.vertices[mesh.triangles[k + 2]];

			int VerA = mesh.triangles[k];
			int VerB = mesh.triangles[k + 1];
			int VerC = mesh.triangles[k + 2];

			temp.Add(new Edge(VA, VB, VerA, VerB));
			temp.Add(new Edge(VC, VB, VerC, VerB));
			temp.Add(new Edge(VA, VC, VerA, VerC));
		}

		//Add only unique flat Edge
		for (int i = 0; i < temp.Count; i++)
		{
			bool IsEqual = false;
			for (int j = 0; j < i; j++)
			{
				if (temp[j].Equal(temp[i])) IsEqual = true;
			}

			if (temp[i].Flatness < 0.1f && IsEqual == false)
			{
				Edges.Add(temp[i]);
			}
		}

		//Split Path
		List<Edge> Path = new List<Edge>();
		Path.AddRange(Edges);
		int safe = 0;
		do
		{
			List<Edge> temp2 = new List<Edge>();
			temp2.Add(Path[0]);
			Path.RemoveAt(0);
			safe++;
			bool PathEnd;
			do
			{
				safe++;
				PathEnd = true;
				for (int i = 0; i < Path.Count; i++)
				{
					Edge e = Path[i];
					if (temp2[temp2.Count - 1].OneEndMatch(e))
					{
						temp2.Add(e);
						Path.RemoveAt(i);
						PathEnd = false;
					}
				}				
			} while (PathEnd == false && safe < 100000);
			SplitPaths.Add(temp2);
		} while (Path.Count != 0 && safe < 100000);

		mesh.RecalculateNormals(180);
		MeshNormal.AddRange(mesh.normals);

		//Search Origin	
		foreach (var E in Edges)
		{
			if (E.VerA.y <= Origin.y)
			{
				Origin = E.VerA;
			}
			if (E.VerB.y <= Origin.y)
			{
				Origin = E.VerB;
			}
		}

		// Search for Mesh Base Loop Path		
		foreach (var p in SplitPaths)
		{
			foreach (var edge in p)
			{
				if (edge.IsConsist(Origin)) MeshLoop = p;
			}
		}

		// construct Small & Large & Merge Loop vertices from edge
		for (int i = 0; i < MeshLoop.Count; i++)
		{
			if (i == 0) continue;
			if (V3Equal(MeshLoop[i-1].VerB, MeshLoop[i].VerA) == false)
			{
				MeshLoop[i].Flip();
			}
		}

		//get Projected Normal
		for (int i = 0; i < MeshNormal.Count; i++)
		{
			ProjectedNormal.Add(new Vector3(MeshNormal[i].x, 0, MeshNormal[i].z).normalized);
		}

		//small Loop
		for (int i = 0; i < MeshLoop.Count; i++)
		{
			MeshSmallLoopV3.Add(MeshLoop[i].VerA);					
		}
		
		float f = SignedPolygonArea(MeshSmallLoopV3);
		if (f < 0 )	MeshSmallLoopV3.Reverse();
		MeshSmallLoopV3.Add(MeshSmallLoopV3[0]);

		//Large Loop
		for (int i = 0; i < MeshLoop.Count; i++)
		{			
			MeshlLargeLoopV3.Add(MeshLoop[i].VerA + ProjectedNormal[MeshLoop[i].VerAInt] * SkirtRadius);					
		}

		float f1 = SignedPolygonArea(MeshlLargeLoopV3);
		if (f1 < 0) MeshlLargeLoopV3.Reverse();
		MeshlLargeLoopV3.Add(MeshlLargeLoopV3[0]);

		///Merge Loop
		for (int i = 0; i < MeshSmallLoopV3.Count; i++)
		{
			MeshMergeLoopV3.Add(MeshSmallLoopV3[i]);
			MeshMergeLoopV3.Add(MeshlLargeLoopV3[i]);
			SkirtNormal.Add(Vector3.up);
			SkirtNormal.Add(Vector3.up);
		}

		//Set Mesh plane to zero
		for (int i = 0; i < MeshMergeLoopV3.Count; i++)
		{
			MeshMergeLoopV3[i] = new Vector3(MeshMergeLoopV3[i].x,0, MeshMergeLoopV3[i].z);
		}

		// Construct Mesh triangle
		for (int i = 0; i < MeshLoop.Count; i++)
		{
			int k = i * 2;
			int K0, K1, K2, K3;
			K0 = k;
			K1 = (k + 1);
			K2 = (k + 2);
			K3 = (k + 3);
			tris.Add(K0);
			tris.Add(K1);
			tris.Add(K2);
			tris.Add(K1);
			tris.Add(K3);
			tris.Add(K2);
		}

		// Construct Mesh uv
		float TotalSmallLoop = 0;
		for (int i = 0; i < MeshSmallLoopV3.Count; i++)
		{			
			if (i == 0) continue;
			TotalSmallLoop += (MeshSmallLoopV3[i] - MeshSmallLoopV3[i - 1]).magnitude;
		}

		float CurrentLoopD = 0;
		SmallLoopUV.Add(new Vector2(1, 0));
		for (int i = 0; i < MeshSmallLoopV3.Count; i++)
		{			
			if (i == 0) continue;
			CurrentLoopD += (MeshSmallLoopV3[i] - MeshSmallLoopV3[i - 1]).magnitude;			
			SmallLoopUV.Add(new Vector2(Mathf.Abs(1 - CurrentLoopD/ TotalSmallLoop), 0));
		}

		float TotalLargeLoop = 0;
		for (int i = 0; i < MeshlLargeLoopV3.Count; i++)
		{
			if (i == 0) continue;
			TotalLargeLoop += (MeshlLargeLoopV3[i] - MeshlLargeLoopV3[i - 1]).magnitude;
		}

		CurrentLoopD = 0;
		LargeLoopUV.Add(new Vector2(1, 1));
		for (int i = 0; i < MeshlLargeLoopV3.Count; i++)
		{
			if (i == 0) continue;
			CurrentLoopD += (MeshlLargeLoopV3[i] - MeshlLargeLoopV3[i - 1]).magnitude;
			LargeLoopUV.Add(new Vector2(Mathf.Abs(1 - CurrentLoopD / TotalLargeLoop), 1));
		}

		for (int i = 0; i < MeshSmallLoopV3.Count; i++)
		{
			MergeLoopUV.Add(SmallLoopUV[i]);
			MergeLoopUV.Add(LargeLoopUV[i]);
		}

		//creata mesh
		Mesh SkirtMesh = new Mesh();
		SkirtMesh.name = OriginGo.gameObject.name + "Skirt";
		SkirtMesh.vertices = MeshMergeLoopV3.ToArray();
		SkirtMesh.triangles = tris.ToArray();
		SkirtMesh.normals = SkirtNormal.ToArray();
		SkirtMesh.uv = MergeLoopUV.ToArray();
		SkirtMesh.RecalculateBounds();

		GameObject Go = new GameObject();
		Go.name = OriginGo.gameObject.name + "Skirt";
		Go.transform.SetParent(OriginGo.transform);
		MeshFilter mF = Go.AddComponent<MeshFilter>();
		mF.sharedMesh = SkirtMesh;
		MeshRenderer mR = Go.AddComponent<MeshRenderer>();
		mR.material = WaveMaterial;
		Go.transform.localPosition = Vector3.zero;
		Go.transform.localEulerAngles = Vector3.zero;
		Go.transform.localScale = Vector3.one;
		Go.transform.position = new Vector3(Go.transform.position.x, WaterLevel, Go.transform.position.z);
	}

	public bool V3Equal(Vector3 a, Vector3 b)
	{
		return Vector3.SqrMagnitude(a - b) < 0.00000001;
	}

	// vector3 with polygon draw on x-z plane
	float SignedPolygonArea(List<Vector3> Points)
	{
		// Add the first point to the end.
		int num_points = Points.Count;
		Vector3[] pts = new Vector3[num_points + 1];
		Points.CopyTo(pts, 0);
		pts[num_points] = Points[0];

		// Get the areas.
		float area = 0;
		for (int i = 0; i < Points.Count; i++)
		{
			area +=
				(pts[i + 1].x - pts[i].x) *
				(pts[i + 1].z + pts[i].z) / 2;
		}

		// Return the result.
		return area;
	}
}


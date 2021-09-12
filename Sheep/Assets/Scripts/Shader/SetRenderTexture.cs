using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRenderTexture : MonoBehaviour
{
	public Camera Cam;
	public RenderTexture rt;
	public Material WaterDistortion;
	public MeshRenderer WaterDistortionMR;

    void Start()
    {
		Cam = GetComponent<Camera>();
		Cam.targetTexture = new RenderTexture(Screen.width / 4, Screen.height / 4, 16, RenderTextureFormat.ARGB32);
		rt = Cam.targetTexture;
		WaterDistortion.SetTexture("_MainTex", Cam.targetTexture);
		WaterDistortionMR.material = WaterDistortion;
	} 
}

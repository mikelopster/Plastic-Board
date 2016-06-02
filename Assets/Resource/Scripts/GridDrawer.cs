using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;

public class GridDrawer : MonoBehaviour {

	// need to compile a material shader for each color
	// for colored lines to work properly on android.
	private IDictionary<Color, Material> materialsByColor = new Dictionary<Color, Material>();
	Material temp;
	CameraController m_camController;
	Controller m_leapController;
	public Vector3 m_center;
	bool m_show = true;
	Color m_gridGray;
	Color m_gridBlue;
	Color m_currentGridColor;

	void Start() {
		temp = new Material( "Shader \"Lines/Colored Blended\" {" +
		                            "SubShader { Pass { " + 
		                            "    BindChannels { Bind \"Color\",color } " +
		                            "    Blend SrcAlpha OneMinusSrcAlpha " + 
		                            "    ZWrite Off Cull Off Fog { Mode Off } " + 
		                            "} } }" ); 
		m_camController = Camera.main.GetComponent<CameraController>();
		m_center = Vector3.zero;
		m_leapController = new Controller();

		m_gridGray = new Color(0.549f, 0.549f, 0.549f);
		m_gridBlue = new Color(0.1f, 0.1f, 0.85f);
		m_currentGridColor = m_gridGray;
	}

	private Material GetLineMaterial(Color color)
	{
		Material lineMaterial;
		if( !materialsByColor.TryGetValue(color, out lineMaterial) ) 
		{
			lineMaterial = new Material( "Shader \"Lines/Colored Blended\" {" +
			                            " Properties { _Color (\"Main Color\", Color) = ("+color.r+","+color.g+","+color.b+","+color.a+") } " +
			                            " SubShader { Pass { " +
			                            " Blend SrcAlpha OneMinusSrcAlpha " +
			                            " ZWrite Off Cull Off Fog { Mode Off } " +
			                            " Color[_Color] " +
			                            " BindChannels { Bind \"Vertex\", vertex Bind \"Color\", color }" +
			                            "} } }" );
			
			lineMaterial.hideFlags = HideFlags.HideAndDontSave;
			lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
			
			materialsByColor.Add(color, lineMaterial);
		}
		return lineMaterial;
	}

	void DrawLine(Vector3 p1, Vector3 p2, Color c, float lineWidth = 0.5f) {

		if (lineWidth == 0.5f) {
			GL.Begin( GL.LINES );
			#if UNITY_ANDROID 
			GetLineMaterial(c).SetPass(0);
			#endif
			GL.Color(c);
			GL.Vertex(p1);
			GL.Vertex(p2);
			GL.End();
		} else {
			GL.Begin(GL.QUADS);
			#if UNITY_ANDROID 
			GetLineMaterial(c).SetPass(0);
			#endif
			GL.Color(c);
			float thisWidth = 1.0f/UnityEngine.Screen.width * lineWidth * 0.5f;
			Vector3 edge1 = Camera.main.transform.position - (p2+p1)/2.0f;
			Vector3 edge2 = p2-p1;
			Vector3 perpendicular = Vector3.Cross(edge1,edge2).normalized * thisWidth;
			
			GL.Vertex(p1 - perpendicular);
			GL.Vertex(p1 + perpendicular);
			GL.Vertex(p2 + perpendicular);
			GL.Vertex(p2 - perpendicular);
			GL.End();		}


	}
	
/*	void DrawLine(Vector3 start, Vector3 end, Color c, bool wide = false) {
		GL.Begin( GL.LINES );
#if UNITY_ANDROID 
			GetLineMaterial(c).SetPass(0);
#endif
			GL.Color(c);
			GL.Vertex(start);
			GL.Vertex(end);
		GL.End();
	}
*/

	void Update() {
		if (Input.GetKeyDown(KeyCode.G)) {
			m_show = !m_show;
		}
		if (Input.GetKeyDown(KeyCode.C)) {
			Frame f = m_leapController.Frame();
			if (f.Hands.Count == 0) {
				// recenter at zero.
				m_center = Vector3.zero;
			} else {
				// find average hand pos and center around that.
				m_center = Vector3.zero;
				for (int i = 0; i < f.Hands.Count; ++i) {
					m_center += f.Hands[i].PalmPosition.ToUnityScaled();
				}
				m_center /= f.Hands.Count;
				m_center.y -= 0.15f;
				Vector3 newCamPos = Camera.main.transform.position;
				newCamPos.x = m_center.x;
				Camera.main.transform.position = newCamPos;
			}
		}
		if (Input.GetKeyDown(KeyCode.J)) {
			if (m_currentGridColor == m_gridGray) {
				m_currentGridColor = m_gridBlue;
			} else {
				m_currentGridColor = m_gridGray;
			}
		}
	}
	
	void OnPostRender() 
	{   
		if (m_show == false) return;
		GL.PushMatrix();
#if !UNITY_ANDROID
		temp.SetPass(0);
#endif
		// X-Y Plane.

		Color c = m_currentGridColor;
		float thickWidth = 1.5f;

		float zVal = 0.15f;
		if (m_camController.IsMirrored()) {
			zVal *= -1.0f;
		}

		// draw horizontal lines
		{
			float xMin = -0.25f; float xMax = 0.25f;
			float yMin = 0.0f; float yMax = 0.3f;
			float increment = 0.01f;
			int count = 0;
			while (yMin <= yMax) {
				Vector3 start = new Vector3(xMin, yMin, zVal) + m_center;
				Vector3 end = new Vector3(xMax, yMin, zVal) + m_center;
				if (count % 5 == 0) {
					DrawLine(start, end, c, thickWidth);
				} else {
					DrawLine(start, end, c);
				}
				count++;
				yMin += increment;
			}
		}

		// vertical lines
		{
			float xMin = -0.25f; float xMax = 0.251f;
			float yMin = 0.0f; float yMax = 0.3f;
			float increment = 0.01f;
			int count = 0;
			while (xMin <= xMax) {
				Vector3 start = new Vector3(xMin, yMin, zVal) + m_center;
				Vector3 end = new Vector3(xMin, yMax, zVal) + m_center;
				if (count % 5 == 0) {
					DrawLine(start, end, c, thickWidth);
				} else {
					DrawLine(start, end, c);
				}
				count++;
				xMin += increment;
			}
		}

		// X-Z plane
		{
			float xMin = -0.25f; float xMax = 0.25f;
			float zMin = -0.15f; float zMax = 0.15f;
			float increment = 0.01f;
			int count = 0;
			while (zMin <= zMax) {
				Vector3 start = new Vector3(xMin, 0, zMin) + m_center;
				Vector3 end = new Vector3(xMax, 0, zMin) + m_center;
				if (count % 5 == 0) {
					DrawLine(start, end, c, thickWidth);
				} else {
					DrawLine(start, end, c);
				}
				count++;
				zMin += increment;
			}
		}

		{
			float xMin = -0.25f; float xMax = 0.251f;
			float zMin = -0.15f; float zMax = 0.15f;
			float increment = 0.01f;
			int count = 0;
			while (xMin <= xMax) {
				Vector3 start = new Vector3(xMin, 0, zMin) + m_center;
				Vector3 end = new Vector3(xMin, 0, zMax) + m_center;
				if (count % 5 == 0) {
					DrawLine(start, end, c, thickWidth);
				} else {
					DrawLine(start, end, c);
				}
				count++;
				xMin += increment;
			}
		}
		GL.PopMatrix();
	}
}
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PlanetChunk : MonoBehaviour {

	
	public int resolution = 10;
	
	public Vector3 offset;
	public Vector3 rotation;
	public float strength = 1f;
	public bool damping;
	public float frequency = 1f;
	public int octaves = 1;
	public float lacunarity = 2f;
	public float persistence = 0.5f;
	public int dimensions = 3;
	public NoiseMethodType type;
	public Gradient coloring;
	public bool coloringForStrength;
	public bool analyticalDerivatives;
	public bool showNormals;
	
	private Mesh mesh;
	private Vector3[] vertices;
	private Vector3[] normals;
	private Color[] colors;
	
	private int currentResolution;
	
	protected int lodLevel;
	PlanetChunk parent; 
	protected Vector3 rootPosition;
	
	private void OnEnable () {
		if (mesh == null) {
			mesh = new Mesh();
			mesh.name = "Surface Mesh";
			GetComponent<MeshFilter>().mesh = mesh;
		}
		//refresh();
	}
	public void Initialize (PlanetChunk parent,ChunckPosition position,gridVertexMethod method)
	{
		if(parent != null){
			this.parent = parent;
			lodLevel = parent.lodLevel + 1;
			rootPosition = Vector3.right*10;
		}
		else {
			lodLevel =0;
			rootPosition = Vector3.zero;
		}
		//TODO on a encore des problems ac les uv ya de la distortion dans les coins... 
		//set one of the main plane to test subdivision here
					
		CreateGrid(method);
	}
	
	public void yell()
	{
		Debug.Log("YELL");
		foreach(Vector3 vert in mesh.vertices)
		{
			Debug.Log(vert);
		}
	}

	private Vector3 cubeToSphere(Vector3 v)
	{
		float xC = v.x*v.x; 
		float yC = v.y*v.y;
		float zC = v.z*v.z;
		
		float x = v.x * Mathf.Sqrt( 1.0f - (yC /2.0f) - (zC/2.0f) + (zC*yC/3.0f));
   		float y = v.y * Mathf.Sqrt( 1.0f - (xC /2.0f) - (zC/2.0f) + (zC*xC/3.0f));
   		float z = v.z * Mathf.Sqrt( 1.0f - (xC /2.0f) - (yC/2.0f) + (yC*xC/3.0f));
   		
   		return new Vector3(x,y,z);
	}
	
	private void cubeToSphere(ref Vector3 v)
	{
		float xC = v.x*v.x; 
		float yC = v.y*v.y;
		float zC = v.z*v.z;
		
		float x = v.x * Mathf.Sqrt( 1.0f - (yC /2.0f) - (zC/2.0f) + (zC*yC/3.0f));
		float y = v.y * Mathf.Sqrt( 1.0f - (xC /2.0f) - (zC/2.0f) + (zC*xC/3.0f));
		float z = v.z * Mathf.Sqrt( 1.0f - (xC /2.0f) - (yC/2.0f) + (yC*xC/3.0f));
		
		v.x = x;
		v.y = y;
		v.z = z;
	}
	
	private void squareToCircle(ref Vector2 v)
	{
		float xC = v.x*v.x; 
		float yC = v.y*v.y;
		v.x = v.x*Mathf.Sqrt(1-yC/2.0f);
		v.y = v.y*Mathf.Sqrt(1-xC/2.0f);
		
	}
	
	private Vector2 squareToCircle(Vector2 v)//TODO bug, in na 4 me semble que c'est pas normale v soit dans un range -1 a 1 
	{
		float xC = v.x*v.x; 
		float yC = v.y*v.y;
		float x = v.x * Mathf.Sqrt(1-yC/2);
		float y = v.y * Mathf.Sqrt(1-xC/2);
		return new Vector2(x,y);
	}
	
	private Vector2 toZeroToOne(Vector2 v)
	{
		return new Vector2 ((v.x+1.0f)/2.0f , ((v.y+1)/2.0f));
	}
	private Vector2 toMinusOneToOne(Vector2 v) 
	{
		return( new Vector2(v.x*2.0f - 1, v.y*2.0f - 1));
	}
	
	private void CreateGrid (gridVertexMethod method) {
		currentResolution = resolution;
		mesh.Clear();
		vertices = new Vector3[(resolution + 1) * (resolution + 1)];
		colors = new Color[vertices.Length];
		normals = new Vector3[vertices.Length];
		Vector2[] uv = new Vector2[vertices.Length];
		float stepSize = 1f / resolution;
		
		for (int v = 0, z = 0; z <= resolution; z++) {
			for (int x = 0; x <= resolution; x++, v++) {
				Vector3 cubeCoords = (method(x,z,stepSize) + rootPosition)*2; //new Vector3(x * stepSize - 0.5f, 0f, z * stepSize - 0.5f);
				cubeToSphere(ref cubeCoords);
				vertices[v] = cubeCoords;
				
				colors[v] = Color.black;
				normals[v] = Vector3.up;
				uv[v] = new Vector2(x * stepSize, z * stepSize);
				float fx = x / (float)resolution;
				float fz = z / (float)resolution;
				
				Vector2 uvCoords = toMinusOneToOne(new Vector2(fx,fz));
                uvCoords = squareToCircle(uvCoords);
				uvCoords = toZeroToOne(uvCoords);
				uv[v] = uvCoords;
			}
		}
	
		mesh.vertices = vertices;
		mesh.colors = colors;
		mesh.normals = normals;
		mesh.uv = uv;
		
		int[] triangles = new int[resolution * resolution * 6];
		for (int t = 0, v = 0, y = 0; y < resolution; y++, v++) {
			for (int x = 0; x < resolution; x++, v++, t += 6) {
				triangles[t] = v;
				triangles[t + 1] = v + resolution + 1;
				triangles[t + 2] = v + 1;
				triangles[t + 3] = v + 1;
				triangles[t + 4] = v + resolution + 1;
				triangles[t + 5] = v + resolution + 2;
			}
		}
		mesh.triangles = triangles;
	}
}

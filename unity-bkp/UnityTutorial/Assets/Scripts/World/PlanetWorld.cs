using UnityEngine;
using System.Collections;

public enum ChunckPosition{
	topLeft,
	topRight,
	bottomLeft,
	bottomRight,
	
};

public delegate Vector3 gridVertexMethod(int i, int j, float stepSize);

public enum gridMethodsNames{
	topGridVertex,
	bottomGridVertex,
	leftGridVertex,
	rightGridVertex,
	forwardGridVertex,
	backGridVertex,
};

public class PlanetWorld : MonoBehaviour {
	
	public static gridVertexMethod[] gridMethods = {
		topGridVertex,
		bottomGridVertex,
		leftGridVertex,
		rightGridVertex,
		forwardGridVertex,
		backGridVertex,
	};
	
	private static Vector3 topGridVertex(int i, int j,float stepSize){
		return new Vector3(i * stepSize - 0.5f, 0.5f, j * stepSize - 0.5f);
	}
	private static Vector3 bottomGridVertex(int i, int j,float stepSize){
		return new Vector3(j * stepSize - 0.5f, -0.5f, i * stepSize - 0.5f);
	}
	private static Vector3 leftGridVertex(int i, int j,float stepSize){
		return new Vector3(-0.5f, i * stepSize - 0.5f, j * stepSize - 0.5f);
	}
	private static Vector3 rightGridVertex(int i, int j,float stepSize){
		return new Vector3(0.5f, j * stepSize - 0.5f, i * stepSize - 0.5f);
	}
	private static Vector3 forwardGridVertex(int i, int j,float stepSize){
		return new Vector3(j * stepSize - 0.5f, i * stepSize - 0.5f, 0.5f);
	}
	private static Vector3 backGridVertex(int i, int j,float stepSize){
		return new Vector3(i * stepSize - 0.5f,  j * stepSize - 0.5f,-0.5f);
	}
	
	[Range(1, 200)]
	public int resolution = 10;
	
	[Range(0f, 1f)]
	public float strength = 1f;
	public bool damping;
	[Range(1, 12)]
	public float frequency = 1f;
	[Range(1, 8)]
	public int octaves = 1;
	[Range(1f, 4f)]
	public float lacunarity = 2f;
	[Range(0f, 1f)]
	public float persistence = 0.5f;
	[Range(1, 3)]
	public int dimensions = 3;
	
	public NoiseMethodType type;
	public int radius;
	public Vector3 offset;
	public Vector3 rotation;
	
	public PlanetChunk PlanetChunkPrefab; 
	
	private static Vector3[] directions = {
		Vector3.left,
		Vector3.back,
		Vector3.right,
		Vector3.forward
	};
	private int currentResolution;
	
	private Mesh mesh;
	private Vector3[] vertices;
	private Vector3[] normals;
	private Color[] colors;
	private float hRatio;
	
	
	private PlanetChunk[] chunks;
	private void OnEnable () {
		/*if (mesh == null) {
			mesh = new Mesh();
			mesh.name = "Surface Mesh";
			GetComponent<MeshFilter>().mesh = mesh;
		}
		Refresh();
		*/
		
		// TODO : rendre sa plus propre un jour
		chunks = new PlanetChunk[6];
		int i = 0;
		PlanetChunk p = Instantiate(PlanetChunkPrefab) as PlanetChunk;
		p.Initialize(null,ChunckPosition.topLeft,gridMethods[(int)gridMethodsNames.topGridVertex]);
		chunks[i++] = p;
		
		p = Instantiate(PlanetChunkPrefab) as PlanetChunk;
		p.Initialize(null,ChunckPosition.topLeft,gridMethods[(int)gridMethodsNames.bottomGridVertex]);
		chunks[i++] = p;
		
		p = Instantiate(PlanetChunkPrefab) as PlanetChunk;
		p.Initialize(null,ChunckPosition.topLeft,gridMethods[(int)gridMethodsNames.leftGridVertex]);
		chunks[i++] = p;
		
		p = Instantiate(PlanetChunkPrefab) as PlanetChunk;
		p.Initialize(null,ChunckPosition.topLeft,gridMethods[(int)gridMethodsNames.rightGridVertex]);
		chunks[i++] = p;
		
		p = Instantiate(PlanetChunkPrefab) as PlanetChunk;
		p.Initialize(null,ChunckPosition.topLeft,gridMethods[(int)gridMethodsNames.forwardGridVertex]);
		chunks[i++] = p;
		
		p = Instantiate(PlanetChunkPrefab) as PlanetChunk;
		p.Initialize(null,ChunckPosition.topLeft,gridMethods[(int)gridMethodsNames.backGridVertex]);
		chunks[i++] = p;
	}
	
	
	
	public void yellPosition()
	{
		chunks[0].yell();
	}
}

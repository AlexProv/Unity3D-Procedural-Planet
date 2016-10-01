using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SurfaceCreator : MonoBehaviour {

	[Range(1, 200)]
	public int resolution = 10;

	public Vector3 offset;
	public Vector3 rotation;

	[Range(0f, 1f)]
	public float strength = 1f;

	public bool damping;

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
	
	public Gradient coloring;

	public bool coloringForStrength;

	public bool analyticalDerivatives;
	public bool showNormals;

	private Mesh mesh;
	private Vector3[] vertices;
	private Vector3[] normals;
	private Color[] colors;

	private int currentResolution;

	private void OnEnable () {
		if (mesh == null) {
			mesh = new Mesh();
			mesh.name = "Surface Mesh";
			GetComponent<MeshFilter>().mesh = mesh;
		}
		Refresh();
	}

	public void Refresh () {
		if (resolution != currentResolution) {
			CreateGrid();
		}
		Quaternion q = Quaternion.Euler(rotation);
		Quaternion qInv = Quaternion.Inverse(q);
		Vector3 point00 = q * new Vector3(-0.5f, -0.5f) + offset;
		Vector3 point10 = q * new Vector3( 0.5f, -0.5f) + offset;
		Vector3 point01 = q * new Vector3(-0.5f, 0.5f) + offset;
		Vector3 point11 = q * new Vector3( 0.5f, 0.5f) + offset;

		NoiseMethod method = Noise.methods[(int)type][dimensions - 1];
		float stepSize = 1f / resolution;
		float amplitude = damping ? strength / frequency : strength;
		for (int v = 0, y = 0; y <= resolution; y++) {
			Vector3 point0 = Vector3.Lerp(point00, point01, y * stepSize);
			Vector3 point1 = Vector3.Lerp(point10, point11, y * stepSize);
			for (int x = 0; x <= resolution; x++, v++) {
				Vector3 point = Vector3.Lerp(point0, point1, x * stepSize);
				NoiseSample sample = Noise.Sum(method, point, frequency, octaves, lacunarity, persistence);
				sample = type == NoiseMethodType.Value ? (sample - 0.5f) : (sample * 0.5f);
				if (coloringForStrength) {
					colors[v] = coloring.Evaluate(sample.value + 0.5f);
					sample *= amplitude;
				}
				else {
					sample *= amplitude;
					colors[v] = coloring.Evaluate(sample.value + 0.5f);
				}
				vertices[v].y = sample.value;
				sample.derivative = qInv * sample.derivative;
				if (analyticalDerivatives) {
					normals[v] = new Vector3(-sample.derivative.x, 1f, -sample.derivative.y).normalized;
				}
			}
		}
		mesh.vertices = vertices;
		mesh.colors = colors;
		if (!analyticalDerivatives) {
			CalculateNormals();
		}
		mesh.normals = normals;
	}


	public void RefreshFromMesh(){
		Quaternion q = Quaternion.Euler(rotation);
		Quaternion qInv = Quaternion.Inverse(q);
		Vector3 point00 = q * new Vector3(-0.5f, -0.5f) + offset;
		Vector3 point10 = q * new Vector3( 0.5f, -0.5f) + offset;
		Vector3 point01 = q * new Vector3(-0.5f, 0.5f) + offset;
		Vector3 point11 = q * new Vector3( 0.5f, 0.5f) + offset;
		
		NoiseMethod method = Noise.methods[(int)type][dimensions - 1];
		float stepSize = 1f / resolution;
		float amplitude = damping ? strength / frequency : strength;
		
	}
	
	
	private void CreateGrid () {
		currentResolution = resolution;
		mesh.Clear();
		vertices = new Vector3[(resolution + 1) * (resolution + 1)];
		colors = new Color[vertices.Length];
		normals = new Vector3[vertices.Length];
		Vector2[] uv = new Vector2[vertices.Length];
		float stepSize = 1f / resolution;
		for (int v = 0, z = 0; z <= resolution; z++) {
			for (int x = 0; x <= resolution; x++, v++) {
				vertices[v] = new Vector3(x * stepSize - 0.5f, 0f, z * stepSize - 0.5f);
				colors[v] = Color.black;
				normals[v] = Vector3.up;
				uv[v] = new Vector2(x * stepSize, z * stepSize);
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

	private float GetXDerivative (int x, int z) {
		int rowOffset = z * (resolution + 1);
		float left, right, scale;
		if (x > 0) {
			left = vertices[rowOffset + x - 1].y;
			if (x < resolution) {
				right = vertices[rowOffset + x + 1].y;
				scale = 0.5f * resolution;
			}
			else {
				right = vertices[rowOffset + x].y;
				scale = resolution;
			}
		}
		else {
			left = vertices[rowOffset + x].y;
			right = vertices[rowOffset + x + 1].y;
			scale = resolution;
		}
		return (right - left) * scale;
	}

	private float GetZDerivative (int x, int z) {
		int rowLength = resolution + 1;
		float back, forward, scale;
		if (z > 0) {
			back = vertices[(z - 1) * rowLength + x].y;
			if (z < resolution) {
				forward = vertices[(z + 1) * rowLength + x].y;
				scale = 0.5f * resolution;
			}
			else {
				forward = vertices[z * rowLength + x].y;
				scale = resolution;
			}
		}
		else {
			back = vertices[z * rowLength + x].y;
			forward = vertices[(z + 1) * rowLength + x].y;
			scale = resolution;
		}
		return (forward - back) * scale;
	}

	private void CalculateNormals () {
		for (int v = 0, z = 0; z <= resolution; z++) {
			for (int x = 0; x <= resolution; x++, v++) {
				normals[v] = new Vector3(-GetXDerivative(x, z), 1f, -GetZDerivative(x, z)).normalized;
			}
		}
	}

	private void OnDrawGizmosSelected () {
		float scale = 1f / resolution;
		if (showNormals && vertices != null) {
			Gizmos.color = Color.yellow;
			for (int v = 0; v < vertices.Length; v++) {
				Gizmos.DrawRay(vertices[v], normals[v] * scale);
			}
		}
	}
}
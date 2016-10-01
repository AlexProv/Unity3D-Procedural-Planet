using UnityEngine;
using System.Collections;

public class Fractal : MonoBehaviour {
	
	public Mesh mesh;
	public Material material;
	public int maxDepth;
	public float childScale; 
	
	private int depth; 
	private Vector3 direction;
	
	
	private static Vector3[] childDirections = {
		Vector3.up,
		//Vector3.down,
		Vector3.right,
		Vector3.left,
		Vector3.forward,
		Vector3.back,
	};
	
	private static Quaternion[] childOrientations = {
		Quaternion.identity,
		//Quaternion.Inverse(Quaternion.identity),
		Quaternion.Euler(0f, 0f, -90f),
		Quaternion.Euler(0f, 0f, 90f),
		Quaternion.Euler(90f, 0f, 0f),
		Quaternion.Euler(-90f, 0f, 0f),
	};
	
	private Material[,] materials;
	
	private void InitializeMaterials () {
		materials = new Material[maxDepth + 1, 3];
		for (int i = 0; i <= maxDepth; i++) {
			float t = i / (maxDepth - 1f);
			t *= t;
			
			materials[i, 0] = new Material(material);
			materials[i, 0].color = Color.Lerp(Color.white, Color.yellow, t);
			materials[i, 1] = new Material(material);
			materials[i, 1].color = Color.Lerp(Color.white, Color.cyan, t);
			materials[i, 2] = new Material(material);
			materials[i, 2].color = Color.Lerp(Color.white, Color.green, t);
		}
		materials[maxDepth, 0].color = Color.magenta;
		materials[maxDepth, 1].color = Color.red;
		materials[maxDepth, 2].color = Color.blue;
	}
	
	void Awake()
	{
		if(maxDepth == null || maxDepth == 0)
		{
			maxDepth = 1;
		}
		if(childScale == null || childScale == 0.0)
		{
			childScale = 0.5f;
		}
	}
	
	private IEnumerator createChildren()
	{
	
		for (int i = 0; i < childDirections.Length; i++) {
			yield return new WaitForSeconds(Random.Range(0.1f,0.6f));
			new GameObject("Fractal Child").AddComponent<Fractal>().Initialize(this,i);
		}
	}
	
	private void Initialize (Fractal parent, int childIndex) {
		mesh = parent.mesh;
		materials = parent.materials;
		maxDepth = parent.maxDepth;
		depth = parent.depth + 1;
		childScale = parent.childScale;
		transform.parent = parent.transform;
		this.direction = childDirections[childIndex];
		this.transform.localRotation = childOrientations[childIndex];
	}

	// Use this for initialization
	void Start () {
		if (materials == null) 
		{
			InitializeMaterials();
		}
	
		gameObject.AddComponent<MeshFilter>().mesh = mesh;
		gameObject.AddComponent<MeshRenderer>().material = materials[depth, Random.Range(0, 3)];
		//renderer.material.color = Color.Lerp(Color.white, Color.yellow, (float)depth / maxDepth);
		if(depth < maxDepth)
		{
			StartCoroutine(createChildren());
		}
		
		transform.localScale = Vector3.one * childScale;
		transform.localPosition = direction * (0.5f + 0.5f * childScale);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

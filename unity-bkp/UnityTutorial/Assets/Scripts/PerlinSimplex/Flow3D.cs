using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class Flow3D : MonoBehaviour {
	
	public Vector3 offset;
	public Vector3 rotation;
	
	public float morphSpeed;
	
	private float morphOffset;
	
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
	
	private ParticleSystem system;
	private ParticleSystem.Particle[] particles;
	
	private void LateUpdate () {
		if (system == null) {
			system = GetComponent<ParticleSystem>();
		}
		if (particles == null || particles.Length < system.maxParticles) {
			particles = new ParticleSystem.Particle[system.maxParticles];
		}
		int particleCount = system.GetParticles(particles);
		PositionParticles();
		system.SetParticles(particles, particleCount);
	}
	
	private void PositionParticles () {
		Quaternion q = Quaternion.Euler(rotation);
		Quaternion qInv = Quaternion.Inverse(q);
		NoiseMethod method = Noise.methods[(int)type][dimensions - 1];
		float amplitude = damping ? strength / frequency : strength;
		
		morphOffset += Time.deltaTime * morphSpeed;
		if (morphOffset > 256f) {
			morphOffset -= 256f;
		}
		
		for (int i = 0; i < particles.Length; i++) {
			Vector3 position = particles[i].position;
			Vector3 point = q * new Vector3(position.z, position.y, position.x + morphOffset) + offset;
			
			NoiseSample sampleX = Noise.Sum(method, point, frequency, octaves, lacunarity, persistence);
			sampleX *= amplitude;
			sampleX.derivative = qInv * sampleX.derivative;
			point = q * new Vector3(position.x + 100f, position.z, position.y + morphOffset) + offset;
			
			NoiseSample sampleY = Noise.Sum(method, point, frequency, octaves, lacunarity, persistence);
			sampleY *= amplitude;
			sampleY.derivative = qInv * sampleY.derivative;
			point = q * new Vector3(position.y, position.x + 100f, position.z + morphOffset) + offset;
			
			NoiseSample sampleZ = Noise.Sum(method, point, frequency, octaves, lacunarity, persistence);
			sampleZ *= amplitude;
			sampleZ.derivative = qInv * sampleZ.derivative;
			
			Vector3 curl;
			curl.x = sampleZ.derivative.x - sampleY.derivative.y;
			curl.y = sampleX.derivative.x - sampleZ.derivative.y + (1f / (1f + position.y));
			curl.z = sampleY.derivative.x - sampleX.derivative.y;
			particles[i].velocity = curl;
		}
	}
	
	
}
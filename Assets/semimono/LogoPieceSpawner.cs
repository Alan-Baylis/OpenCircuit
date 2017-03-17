using UnityEngine;

public class LogoPieceSpawner : MonoBehaviour {

	public Vector3 volume;
	public float spawnRate;
	public int spawnCount;
	public float maxSpinSpeed;
	public GameObject[] piecePrefabs;

	private float lastSpawn;

	public void Start() {
		Physics.IgnoreLayerCollision(2, 2);
		lastSpawn = 0;
	}
	
	public void Update () {
		float spawnTime = 1f / spawnRate;
		while (spawnCount > 0 && lastSpawn <= Time.time -spawnTime) {
			lastSpawn += spawnTime;
			--spawnCount;
			spawn();
		}
	}

	private void spawn() {
		GameObject prefab = piecePrefabs[Random.Range(0, piecePrefabs.Length)];
		Vector3 position = transform.position + new Vector3(
			                   Random.Range(-0.5f, 0.5f) *volume.x,
			                   Random.Range(-0.5f, 0.5f) *volume.y,
			                   Random.Range(-0.5f, 0.5f) *volume.z);
		GameObject piece = Instantiate(prefab);
		piece.transform.position = position;
		Rigidbody rb = piece.GetComponent<Rigidbody>();
		rb.angularVelocity = new Vector3(0, 0, Random.Range(-maxSpinSpeed, maxSpinSpeed));
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Broccoli.Factory;
using Broccoli.Pipe;

namespace Broccoli.Examples 
{
	using Pipeline = Broccoli.Pipe.Pipeline;
	public class RuntimeSceneController : MonoBehaviour {
		#region Vars
		/// <summary>
		/// If true then the tree is spawned on the clicking site on
		/// the sphere surface.
		/// </summary>
		public bool selectSpawnPoint = true;
		/// <summary>
		/// The max number of trees.
		/// </summary>
		public int maxNumberOfTrees = 10;
		/// <summary>
		/// Path to the pipeline we will use to generate the trees.
		/// The pipeline asset file should be on a Resources folder.
		/// </summary>
		private static string runtimePipelineResourcePath = "BroccoliRuntimeExamplePipeline";
		/// <summary>
		/// The tree factory.
		/// </summary>
		private TreeFactory treeFactory = null;
		/// <summary>
		/// The pipeline.
		/// </summary>
		private Pipeline pipeline = null;
		/// <summary>
		/// The positioner element.
		/// </summary>
		private PositionerElement positionerElement = null;
		/// <summary>
		/// The positions on sphere.
		/// </summary>
		List<Vector3> positionsOnSphere = new List<Vector3> ();
		/// <summary>
		/// The points on the surface of the sphere to spawn trees from.
		/// </summary>
		List<Broccoli.Pipe.Position> directionsOnSphere = new List<Broccoli.Pipe.Position> ();
		/// <summary>
		/// The spawned trees.
		/// </summary>
		Queue<GameObject> spawnedTrees = new Queue<GameObject> ();
		/// <summary>
		/// The maximum number of points on the sphere.
		/// </summary>
		int maxPoints = 35;
		/// <summary>
		/// The sphere radius.
		/// </summary>
		float sphereRadius = 4f;
		#endregion

		#region Events
		/// <summary>
		/// Start this instance.
		/// </summary>
		void Start () {
			// Get a factory.
			treeFactory = TreeFactory.GetFactory ();
			// Load a pipeline, null if it couldn't be found.
			pipeline = treeFactory.LoadPipeline (runtimePipelineResourcePath);
			// Get the positioner element on the pipeline (to dynamically assign the position).
			if (pipeline != null && pipeline.Validate ()) {
				positionerElement = (PositionerElement) pipeline.root.GetDownstreamElement (PipelineElement.ClassType.Positioner);
				positionerElement.positions.Clear ();
			}
			// Generate all the possible positions.
			GeneratePointsOnSphere (maxPoints);
		}
		/// <summary>
		/// Update this instance.
		/// </summary>
		void Update () {
			if (Input.GetMouseButtonDown (0) && positionerElement != null) {
				Vector3 positionToSpawn = Vector3.zero;
				if (selectSpawnPoint) {
					// The tree is spawned raycasting to the sphere surface.
					RaycastHit hit;
					Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
					if (Physics.Raycast (ray, out hit)) {
						Transform objectHit = hit.transform;
						positionerElement.positions.Add (new Position (Vector3.zero, hit.point.normalized, true));
						positionToSpawn = hit.point;
					}
				} else {
					// The tree is spawned selecting from a collection of surface points.
					int index = Random.Range (0, maxPoints);
					positionerElement.positions.Add (directionsOnSphere [index]);
					positionToSpawn = positionsOnSphere [index];
				}
				if (positionerElement.positions.Count > 0) {
					GameObject tree = treeFactory.Spawn ();
					float scale = Random.Range (0.7f, 1f);
					tree.transform.localScale = Vector3.zero;
					tree.transform.position = positionToSpawn;
					if (spawnedTrees.Count >= maxNumberOfTrees) {
						GameObject treeToDestroy = spawnedTrees.Dequeue ();
						StartCoroutine (ScaleTo (treeToDestroy, 0f, 0.7f, true));
					}
					StartCoroutine (ScaleTo (tree, scale, 1f));
					spawnedTrees.Enqueue (tree);
					positionerElement.positions.Clear ();
				}
			}
		}
		#endregion

		#region Sphere positions
		/// <summary>
		/// Generates the points on the sphere where trees could be spawned.
		/// </summary>
		/// <param name="numberOfPoints">Number of points.</param>
		void GeneratePointsOnSphere (int numberOfPoints = 10) {
			positionsOnSphere.Clear ();
			directionsOnSphere.Clear ();

			float latitude, longitude, u, v, x, y, z = 0;
			Vector3 pointOnSphere;

			// Create all the points and their directions.
			for (int i = 0; i < numberOfPoints; i++) {
				u = Random.Range (0f, 1f);
				v = Random.Range (0f, 1f);
				latitude = Mathf.Acos (2 * v - 1);
				longitude = 2 * Mathf.PI * u;
				x = sphereRadius * Mathf.Sin (latitude) * Mathf.Cos (longitude);
				y = sphereRadius * Mathf.Sin (latitude) * Mathf.Sin (longitude);
				z = sphereRadius * Mathf.Cos (latitude);
				pointOnSphere = new Vector3 (x, Mathf.Abs (y), z);
				positionsOnSphere.Add (pointOnSphere);
				directionsOnSphere.Add (new Position (Vector3.zero, pointOnSphere.normalized, true));
			}
		}
		#endregion

		#region Animations
		/// <summary>
		/// Scales a game object to a target scale.
		/// </summary>
		/// <returns>The to.</returns>
		/// <param name="gameObject">Game object.</param>
		/// <param name="targetScale">Target scale.</param>
		/// <param name="seconds">Seconds.</param>
		/// <param name="destroyAtEnd">If set to <c>true</c> destroy at end.</param>
		IEnumerator ScaleTo (GameObject gameObject, float targetScale, float seconds, bool destroyAtEnd = false){
			float progress = 0;
			Vector3 initialScale = gameObject.transform.localScale;
			Vector3 finalScale = new Vector3 (targetScale, targetScale, targetScale);
			while (progress <= 1) {
				gameObject.transform.localScale = Vector3.Lerp (initialScale, finalScale, Easing.EaseInOutCirc (0f, 1f, progress));
				progress += Time.deltaTime * (1f / seconds);
				yield return null;
			}
			gameObject.transform.localScale = finalScale;
			if (destroyAtEnd) {
				#if UNITY_EDITOR
				DestroyImmediate (gameObject);
				#else
				Destroy (gameObject);
				#endif
			}
		}
		#endregion
	}
}
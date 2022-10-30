using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Broccoli.Examples
{
	public class CameraRotateAround : MonoBehaviour
	{
		public bool autoStartRotation = true;
		public float targetCamSpeed = 1f;
		public Transform target;
		private bool cameraRotate = true;
		private float cameraSpeed = 0f;
		private float cameraDir = 1f;
		private bool isRotationEnabled = false;
		/// <summary>
		/// Start this instance.
		/// </summary>
		void Start () {
			if (autoStartRotation) {
				isRotationEnabled = true;
			}
		}
		/// <summary>
		/// Update this instance.
		/// </summary>
		void Update() {
			if (Input.GetKeyDown (KeyCode.S)) {
				isRotationEnabled = !isRotationEnabled;
			}
			Vector3 targetPosition = Vector3.zero;
			if (target != null) {
				targetPosition = target.position;
			}
			if (isRotationEnabled) {
				transform.RotateAround (targetPosition, Vector3.up, 20 * Time.deltaTime * cameraDir * cameraSpeed);
				//transform.LookAt (targetPosition);

				if (cameraRotate)
					cameraSpeed = Mathf.Lerp (cameraSpeed, targetCamSpeed, 4 * Time.deltaTime);
				else {
					if (cameraSpeed > 0.01f)
						cameraSpeed = Mathf.Lerp (cameraSpeed, 0f, 4 * Time.deltaTime);
					else
						cameraSpeed = 0f;
				}
				/*
				if( Input.GetMouseButton (0))
					targetCamSpeed = 5f;
				else
					targetCamSpeed = 1f;
				*/
			}
		}
	}
}
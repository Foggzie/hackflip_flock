using System.Collections.Generic;

using UnityEngine;

namespace Flocking
{
	public class Stage : MonoBehaviour
	{
		[SerializeField] private Material lineMaterial;
		[SerializeField] private GameObject driverPrefab;

		[SerializeField] private float simulationScale;
		private float flockRadiusSq;
		private float friendRadiusSq;
		private float boundsRadiusSq;

		private HashSet<Driver> drivers = new HashSet<Driver>();
		private HashSet<GameObject> avoidables = new HashSet<GameObject>();

		private void Awake()
		{
			var flockRadius = simulationScale * 30.0f;
			var friendRadius = flockRadius / 1.3f;
			var boundsRadius = simulationScale * 150.0f;

			flockRadiusSq = flockRadius * flockRadius;
			friendRadiusSq = friendRadius * friendRadius;
			boundsRadiusSq = boundsRadius * boundsRadius;

			// Make Drivers
			for (int i = 0; i < 150; ++i)
			{
				InstantiateDriver(new Vector2(
					Random.Range(-boundsRadius, boundsRadius),
					Random.Range(-boundsRadius, boundsRadius)));
			}

			// Make a circle
			CreateBoundsRenderer(boundsRadius, 100);
		}

		private void Update()
		{
			StepAllDrivers();
		}

		private void StepAllDrivers()
		{
			foreach (var driver in drivers)
			{
				driver.Step();
			}	
		}

		private void InstantiateDriver(Vector2 position)
		{
			var newObject = GameObject.Instantiate<GameObject>(
				driverPrefab,
				new Vector3(position.x, 0.0f, position.y),
				Quaternion.identity,
				this.transform);
			
			var driver = newObject.GetComponent<Driver>();
			driver.Initialize(simulationScale, boundsRadiusSq, RefreshNeighbors);
			drivers.Add(driver);
		}

		private void RefreshNeighbors(Driver driver, ref HashSet<Driver> friends, ref HashSet<Driver> flock)
		{
			flock.Clear();
			friends.Clear();
			foreach (var neighbor in drivers)
			{
				if (driver != neighbor)
				{
					var distSq = (driver.transform.position - neighbor.transform.position).sqrMagnitude; 
					if (distSq <= friendRadiusSq)
					{
						friends.Add(neighbor);
					}
					else if (distSq <= flockRadiusSq)
					{
						flock.Add(neighbor);
					}
				}
			}
		}

		private void CreateBoundsRenderer(float radius, int segments)
		{
			var line = gameObject.AddComponent<LineRenderer>();
			line.numPositions = segments + 1;
			line.useWorldSpace = true;
			line.startColor = Color.white;
			line.endColor = Color.white;
			line.startWidth = 0.25f;
			line.endWidth = 0.25f;
			line.material = lineMaterial;

			float angle = 0.0f;
			float stepSize = 360.0f / segments;
			for (int i = 0; i < segments + 1; ++i, angle += stepSize)
			{
				float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
				float y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
				line.SetPosition(i, new Vector3(x, 0.0f, y));
			}
		}
	}
}
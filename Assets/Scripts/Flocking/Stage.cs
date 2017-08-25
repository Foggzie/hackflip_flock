using System.Collections.Generic;

using UnityEngine;

namespace Flocking
{
	public class Stage : MonoBehaviour
	{
		[SerializeField] private GameObject driverPrefab;

		[SerializeField] private float simulationScale;
		private float flockRadiusSq;
		private float friendRadiusSq;
		private float boundsRadiusSq;

		private HashSet<Driver> drivers = new HashSet<Driver>();
		private HashSet<GameObject> avoidables = new HashSet<GameObject>();

		private void Awake()
		{
			var flockRadius = simulationScale * 60.0f;
			var friendRadius = flockRadius / 1.3f;
			var boundsRadius = simulationScale * 10.0f;

			flockRadiusSq = flockRadius * flockRadius;
			friendRadiusSq = friendRadius * friendRadius;
			boundsRadiusSq = boundsRadius * boundsRadius;

			InstantiateDriver(new Vector2(0.0f, 0.0f));
			InstantiateDriver(new Vector2(1.0f, 1.0f));
			InstantiateDriver(new Vector2(2.0f, 2.0f));
			InstantiateDriver(new Vector2(3.0f, 3.0f));
			InstantiateDriver(new Vector2(4.0f, 4.0f));
			InstantiateDriver(new Vector2(5.0f, 5.0f));
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
	}
}
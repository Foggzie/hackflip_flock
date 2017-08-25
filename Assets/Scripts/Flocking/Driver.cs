using System;
using System.Collections.Generic;

using UnityEngine;
using System.Linq;

namespace Flocking
{
	using Rand = UnityEngine.Random;

	public delegate void Socializer(Driver driver, ref HashSet<Driver> friends, ref HashSet<Driver> flock);

	public class Driver : MonoBehaviour
	{
		public Vector2 Position
		{
			get
			{
				return new Vector2(transform.position.x, transform.position.z);
			}
			set
			{
				transform.position = new Vector3(value.x, transform.position.y, value.y);
			}
		}

		public Vector2 Velocity { get; private set; }

		private void Awake()
		{
			socialFrame = Rand.Range(0, 5); // [0,5)
		}

		public void Initialize(float simluationScale, float boundsRadiusSq, Socializer socializer)
		{
			maxSpeed = simluationScale * 1.0f;
			wanderWeight = simluationScale * 0.25f;
			accordanceWeight = simluationScale * 0.05f;

			this.boundsRadiusSq = boundsRadiusSq;
			this.socializer = socializer;
		}

		public void Step()
		{
			StepSocializing();
			StepFlocking();
		}

		private void StepSocializing()
		{
			socialFrame = (socialFrame + 1) % 5;
			if (socialFrame == 0)
			{
				socializer(this, ref friends, ref flock);
			}
		}

		private void StepFlocking()
		{
			Vector2 flockDirection = GetFlockDirection();
			Vector2 avoidDirection = GetAvoidDirection();
			Vector2 accordance = GetAccordance();
			Vector2 bounds = GetBounds();
			Vector2 wander = GetWander();

			Velocity = Velocity + flockDirection + avoidDirection + accordance + bounds + wander;
			Velocity = Vector2.ClampMagnitude(Velocity, maxSpeed);
			Position += Velocity;

			transform.rotation = Quaternion.LookRotation(Velocity);
		}

		/// <summary>Helps align with the flock.</summary>
		private Vector2 GetFlockDirection()
		{
			int count = 0;
			Vector2 average = Vector2.zero;
			foreach (var friend in flock.Concat(friends))
			{
				var dist = (friend.Position - Position).magnitude;
				average += (friend.Velocity.normalized / dist);
				++count;
			}

			average = (count > 1) ? (average / count) : average;
			return average;
		}

		/// <summary>Helps prevent a flock from clumping.</summary>
		private Vector2 GetAvoidDirection()
		{
			int count = 0;
			Vector2 average = Vector2.zero;
			foreach (var friend in friends)
			{
				Vector2 difference = Position - friend.Position;
				average += difference.normalized / difference.magnitude;
				++count;
			}

			average = (count > 1) ? (average / count) : average;
			return average;
		}

		/// <summary>Helps a flock stay together.</summary>
		private Vector2 GetAccordance()
		{
			int count = 0;
			Vector2 average = Vector2.zero;
			foreach (var friend in flock.Concat(friends))
			{
				average += friend.Position;
				++count;
			}

			if (count == 0)
			{
				return average;
			}
			else
			{
				average = average / count;
				Vector2 direction = average - Position;
				direction.Normalize();
				return direction * accordanceWeight;
			}
		}
			
		/// <summary>Ensures we dont leave the system.</summary>
		private Vector2 GetBounds()
		{
			var differenceSq = Position.sqrMagnitude - boundsRadiusSq;
			if (differenceSq > 0)
			{
				return -(Position).normalized * (float)Math.Sqrt(differenceSq) * 0.05f;
			}
			else
			{
				return Vector2.zero;
			}
		}

		/// <summary>I am an individual!</summary>
		private Vector2 GetWander()
		{
			return new Vector2(
				Rand.Range(-wanderWeight, wanderWeight),
				Rand.Range(-wanderWeight, wanderWeight));
		}

		private int socialFrame;

		private float maxSpeed;
		private float wanderWeight;
		private float accordanceWeight;
		private float boundsRadiusSq;

		private Socializer socializer;

		private HashSet<Driver> friends = new HashSet<Driver>();
		private HashSet<Driver> flock = new HashSet<Driver>();
	}
}

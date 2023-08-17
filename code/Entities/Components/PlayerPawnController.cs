using Sandbox;
using System;
using System.Collections.Generic;

namespace BoxBusters.Entities.Components
{
	public class PlayerPawnController : EntityComponent<PlayerPawn>
	{
		public int StepSize { get; set; } = 24;
		public int GroundAngle { get; set; } = 45;
		public int JumpSpeed { get; set; } = 300;
		public float Gravity { get; set; } = 800f;

		public bool Grounded => Entity.GroundEntity != null && Entity.GroundEntity.IsValid();

		public void Simulate( IClient cl )
		{

			Vector3 movement = Entity.InputDirection.Normal;
			Angles angles = Entity.ViewAngles.WithPitch( 0 );
			Vector3 moveVector = Rotation.From( angles ) * movement * 320f;
			Entity groundEntity = CheckForGround();

			if ( groundEntity != null && groundEntity.IsValid() )
			{
				if ( !Grounded )
				{
					Entity.Velocity = Entity.Velocity.WithZ( 0 );
				}

				Accelerate( moveVector.Normal, moveVector.Length,
					200f * (Input.Down( "run" ) ? 2.5f : 1f), 7.5f );
				ApplyFriction( 4f );
			}
			else
			{
				Accelerate( moveVector.Normal, moveVector.Length, 100, 20f );
				Entity.Velocity += Vector3.Down * Gravity * Time.Delta;
			}

			if ( Input.Pressed( "jump" ) && Grounded )
			{
				Entity.Velocity += Vector3.Up * JumpSpeed;
			}

			MoveHelper mh = new MoveHelper( Entity.Position, Entity.Velocity );
			mh.Trace = mh.Trace.Size( Entity.Hull ).Ignore( Entity );

			if ( mh.TryMoveWithStep( Time.Delta, StepSize ) > 0 )
			{
				if ( Grounded )
				{
					mh.Position = TryStepDown( mh.Position );
				}

				Entity.Position = mh.Position;
				Entity.Velocity = mh.Velocity;
			}

			Entity.GroundEntity = groundEntity;
		}

		private Entity CheckForGround()
		{
			if ( Entity.Velocity.z > 100f )
			{
				return null;
			}

			TraceResult trace = Entity.TraceBoundingBox( Entity.Position, Entity.Position + Vector3.Down, 2f );

			if ( !trace.Hit || trace.Normal.Angle( Vector3.Up ) > GroundAngle )
			{
				return null;
			}

			return trace.Entity;
		}

		private void ApplyFriction( float frictionAmount )
		{
			float speed = Entity.Velocity.Length;
			if ( speed < 0.1f )
			{
				return;
			}

			float control = (speed < 100f) ? 100f : speed;

			float drop = control * Time.Delta * frictionAmount;

			float newSpeed = Math.Max(0, speed - drop);
			if ( newSpeed == speed )
			{
				return;
			}

			Entity.Velocity *= newSpeed / speed;
		}

		private void Accelerate( Vector3 wishDirection, float wishSpeed, float speedLimit, float acceleration )
		{
			if ( speedLimit > 0 && wishSpeed > speedLimit )
			{
				wishSpeed = speedLimit;
			}

			float currentSpeed = Entity.Velocity.Dot( wishDirection );
			float speedDifference = wishSpeed - currentSpeed;

			if ( speedDifference <= 0 )
			{
				return;
			}

			float accelSpeed = acceleration * Time.Delta * wishSpeed;
			accelSpeed = Math.Min( accelSpeed, speedDifference );
			
			Entity.Velocity += wishDirection * accelSpeed;
		}

		private Vector3 TryStepDown( Vector3 position )
		{
			Vector3 start = position + Vector3.Up * 2;
			Vector3 end = position + Vector3.Down * StepSize;

			// Start from the highest accessible point
			TraceResult trace = Entity.TraceBoundingBox( position, start );
			start = trace.EndPosition;
			
			trace = Entity.TraceBoundingBox( start, end );

			if (
				trace.Fraction <= 0 ||
				trace.Fraction >= 1 ||
				trace.StartedSolid ||
				Vector3.GetAngle( Vector3.Up, trace.Normal ) > GroundAngle
			)
			{
				return position;
			}

			return trace.EndPosition;
		}
	}
}

using Sandbox;
using System;

namespace BoxBusters.Entities.Components
{
	/// <summary>
	/// The controller for the player pawn, responsible for movement and physics.
	/// </summary>
	public class PlayerPawnController : EntityComponent<PlayerPawn>
	{
		/// <summary>
		/// Gets or sets a value indicating how many units the pawn can step up.
		/// </summary>
		/// <value>
		/// The amount of units the pawn can step up.
		/// </value>
		public int StepSize { get; set; } = 24;
		
		/// <summary>
		/// Gets or sets a value indicating the angle at which surfaces will be considered ground rather than walls.
		/// </summary>
		/// <value>
		/// The degree angle ground should be in.
		/// </value>
		public int GroundAngle { get; set; } = 45;
		
		/// <summary>
		/// Gets or sets a value indicating how much speed the pawn gains when it jumps.
		/// </summary>
		/// <value>
		/// How much speed the pawn will gain when it jumps.
		/// </value>
		public int JumpSpeed { get; set; } = 300;
		
		/// <summary>
		/// Gets or sets a value representing the strength of gravity when it is applied to the pawn.
		/// </summary>
		/// <value>
		/// The strength of gravity.
		/// </value>
		public float Gravity { get; set; } = 800f;

		/// <summary>
		/// Gets a value indicating whether the pawn is on the ground.
		/// </summary>
		/// <value>
		/// true if the pawn is on the ground, false otherwise.
		/// </value>
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

		/// <summary>
		/// Recalculates the ground entity for the pawn.
		/// </summary>
		/// <returns>The new ground entity, or null if the pawn isn't grounded.</returns>
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

		/// <summary>
		/// Applies friction to the velocity of the pawn.
		/// </summary>
		/// <param name="frictionAmount">How much friction to apply.</param>
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

		/// <summary>
		/// Accelerates the pawn in a direction.
		/// </summary>
		/// <param name="wishDirection">The direction that is desired to accelerate in.</param>
		/// <param name="wishSpeed">The desired speed to accelerate at.</param>
		/// <param name="speedLimit">The value to limit <paramref name="wishSpeed"/> at.</param>
		/// <param name="acceleration">How much to try to accelerate by.</param>
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

		/// <summary>
		/// Tries to step down from a position.
		/// </summary>
		/// <param name="position">The position to try to step down from.</param>
		/// <returns>The end position, whether or not stepping down was successful.</returns>
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

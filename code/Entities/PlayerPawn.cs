using System.ComponentModel;
using BoxBusters.Entities.Components;
using BoxBusters.Entities.Items;
using Sandbox;

namespace BoxBusters.Entities
{
	/// <summary>
	/// The pawn which represents players in the game world.
	/// </summary>
	public partial class PlayerPawn : AnimatedEntity
	{
		/// <summary>
		/// Gets or sets a value representing where the pawn's eyes are in world space. In other words, where the pawn's
		/// view originates from.
		/// </summary>
		/// <value>
		/// The position of the pawn's eyes in world space.
		/// </value>
		/// <seealso cref="EyeLocalPosition"/>
		[Browsable( false )]
		public Vector3 EyePosition
		{
			get => Transform.PointToWorld( EyeLocalPosition );
			set => EyeLocalPosition = Transform.PointToLocal( value );
		}
		
		/// <summary>
		/// Gets or sets a value representing where the pawn's eyes are in local space.
		/// </summary>
		/// <value>
		/// The position of the pawn's eyes relative to the pawn's origin.
		/// </value>
		/// <seealso cref="EyePosition"/>
		[Net, Predicted, Browsable( false )]
		public Vector3 EyeLocalPosition { get; set; }
		
		/// <summary>
		/// Gets or sets a value representing the "rotation" of the pawn's eyes in world space. In other words,
		/// the direction the pawn is looking.
		/// </summary>
		/// <value>
		/// The direction the pawn's eyes are looking in world space.
		/// </value>
		/// <seealso cref="EyeLocalRotation"/>
		[Browsable( false )]
		public Rotation EyeRotation
		{
			get => Transform.RotationToWorld( EyeLocalRotation );
			set => EyeLocalRotation = Transform.RotationToLocal( value );
		}
		
		/// <summary>
		/// Gets or sets a value representing where the pawn's eyes are looking in local space.
		/// </summary>
		/// <value>
		/// The direction the pawn's eyes are looking relative to the pawn's origin.
		/// </value>
		/// <seealso cref="EyeRotation"/>
		[Net, Predicted, Browsable( false )]
		public Rotation EyeLocalRotation { get; set; }
		
		/// <summary>
		/// Gets a value representing the directions the player is trying to move in.
		/// </summary>
		/// <value>
		/// The directions represented by the player's movement inputs.
		/// </value>
		[ClientInput]
		public Vector3 InputDirection { get; private set; }
		
		/// <summary>
		/// Gets a value representing the angles the player is looking in. Determines the <see cref="EyeRotation"/>.
		/// </summary>
		/// <value>
		/// The angles represented by the player's look inputs.
		/// </value>
		[ClientInput]
		public Angles ViewAngles { get; private set; }
		
		/// <summary>
		/// Gets a value representing the collision bounding box of the pawn.
		/// </summary>
		/// <value>
		/// The bounding box of the pawn.
		/// </value>
		public BBox Hull { get; set; } = new BBox(new Vector3(-16, -16, 0), new Vector3(16, 16, 72));
		
		public override void Spawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );

			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			Components.Create<PlayerPawnController>();
			Components.Create<InventoryComponent>();

			Components.Get<InventoryComponent>().AddItem( new LaserGun(), true );
		}

		public override void BuildInput()
		{
			if ( Input.StopProcessing )
			{
				return;
			}
			
			InputDirection = Input.AnalogMove;

			Angles look = Input.AnalogLook;

			if ( ViewAngles.pitch is > 90f or < -90f )
			{
				look = look.WithYaw( -look.yaw );
			}

			Angles newViewAngles = ViewAngles;
			newViewAngles += look;
			newViewAngles.pitch = newViewAngles.pitch.Clamp( -89f, 89f );
			newViewAngles.roll = 0f;
			
			ViewAngles = newViewAngles;
		}

		public override void Simulate( IClient cl )
		{
			EyeRotation = ViewAngles.ToRotation();
			Rotation = ViewAngles.WithPitch( 0 ).ToRotation();
			EyeLocalPosition = Vector3.Up * (64f * Scale);
			
			// TODO: Automatic component simulation
			Components.Get<PlayerPawnController>().Simulate( cl );
			Components.Get<InventoryComponent>().Simulate( cl );
		}

		public override void FrameSimulate( IClient cl )
		{
			EyeRotation = ViewAngles.ToRotation();
			Rotation = ViewAngles.WithPitch( 0 ).ToRotation();
			
			Camera.Rotation = EyeRotation;
			Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );
			Camera.FirstPersonViewer = this;
			Camera.Position = EyePosition;
		}
		
		/// <summary>
		/// Traces a ray from a start point to an end point, with the size of the pawn's <see cref="Hull"/>.
		/// </summary>
		/// <param name="start">The start point to trace from.</param>
		/// <param name="end">The end point where the trace should stop.</param>
		/// <param name="liftFeet">IDK</param>
		/// <returns>The result of the trace.</returns>
		/// <!-- TODO: Document liftFeet -->
		public TraceResult TraceBoundingBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
		{
			return TraceBoundingBox( start, end, Hull.Mins, Hull.Maxs, liftFeet );
		}

		/// <summary>
		/// Traces a ray from a start point to an end point with the specified size of bounding box.
		/// </summary>
		/// <param name="start">The start point to trace from.</param>
		/// <param name="end">The end point where the trace should stop.</param>
		/// <param name="boundMin">The minimum corner extents of the bounding box.</param>
		/// <param name="boundMax">The maximum corner extents of the bounding box.</param>
		/// <param name="liftFeet">IDK</param>
		/// <returns>The result of the trace.</returns>
		/// <!-- TODO: Document liftFeet -->
		public TraceResult TraceBoundingBox( Vector3 start, Vector3 end, Vector3 boundMin, Vector3 boundMax, float liftFeet = 0.0f )
		{
			if ( liftFeet > 0 )
			{
				start += Vector3.Up * liftFeet;
				boundMax = boundMax.WithZ( boundMax.z - liftFeet );
			}

			TraceResult tr = Trace.Ray( start, end )
				.Size( boundMin, boundMax )
				.WithAnyTags( "solid", "playerclip", "passbullets" )
				.Ignore( this )
				.Run();

			return tr;
		}
	}
}

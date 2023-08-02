using System.ComponentModel;
using Sandbox;

namespace BoxBusters.Entities
{
	public partial class PlayerPawn : AnimatedEntity
	{
		[Browsable( false )]
		public Vector3 EyePosition
		{
			get => Transform.PointToWorld( EyeLocalPosition );
			set => EyeLocalPosition = Transform.PointToLocal( value );
		}
		
		[Net, Predicted, Browsable( false )]
		public Vector3 EyeLocalPosition { get; set; }
		
		[Browsable( false )]
		public Rotation EyeRotation
		{
			get => Transform.RotationToWorld( EyeLocalRotation );
			set => EyeLocalRotation = Transform.RotationToLocal( value );
		}
		
		[Net, Predicted, Browsable( false )]
		public Rotation EyeLocalRotation { get; set; }
		
		[ClientInput]
		public Vector3 InputDirection { get; private set; }
		
		[ClientInput]
		public Angles ViewAngles { get; private set; }
		
		public override void Spawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );

			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
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
	}
}

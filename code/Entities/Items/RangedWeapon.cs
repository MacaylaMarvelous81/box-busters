using Sandbox;

namespace BoxBusters.Entities.Items
{
	public partial class RangedWeapon : Item
	{
		public virtual float PrimaryRate => 5f;
		
		public virtual float SecondaryRate => 5f;
		
		[Net, Predicted]
		public TimeSince TimeSincePrimaryAttack { get; private set; }
		
		[Net, Predicted]
		public TimeSince TimeSinceSecondaryAttack { get; private set; }

		public bool CanPrimaryAttack
		{
			get
			{
				if ( PrimaryRate <= 0 )
				{
					return true;
				}
				
				return TimeSincePrimaryAttack >= 1 / PrimaryRate;
			}
		}
		
		public bool CanSecondaryAttack
		{
			get
			{
				if ( SecondaryRate <= 0 )
				{
					return true;
				}
				
				return TimeSinceSecondaryAttack >= 1 / SecondaryRate;
			}
		}
		
		public virtual void Reload() {}

		public virtual void AttackPrimary()
		{
			TimeSincePrimaryAttack = 0;
		}
		
		public virtual void AttackSecondary()
		{
			TimeSinceSecondaryAttack = 0;
		}

		public override void Simulate( IClient cl )
		{
			if ( !Owner.IsValid() )
			{
				return;
			}

			if ( Input.Pressed( "reload" ) )
			{
				Reload();
			}
			
			if ( CanPrimaryAttack && Input.Down( "attack1" ) )
			{
				AttackPrimary();
			}
			
			if (CanSecondaryAttack && Input.Down( "attack2" ) )
			{
				AttackSecondary();
			}
		}
		
		public virtual TraceResult TraceBullet( Vector3 start, Vector3 end, float radius = 2f )
		{
			bool underwater = Trace.TestPoint( start, "water" );

			Trace trace = Trace.Ray( start, end )
				.UseHitboxes()
				.WithAnyTags( "solid", "player", "npc" )
				.Ignore( this )
				.Size( radius );

			if ( !underwater )
			{
				trace = trace.WithAnyTags( "water" );
			}

			return trace.Run();
		}
	}
}

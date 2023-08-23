using Sandbox;

namespace BoxBusters.Entities.Items
{
	/// <summary>
	/// A ranged weapon, which is an item that can fire bullets.
	/// </summary>
	public partial class RangedWeapon : Item
	{
		/// <summary>
		/// Gets a value indicating the rate at which the weapon can fire its primary attack.
		/// </summary>
		/// <remarks>
		/// This value is in units of shots per second. The weapon will be able to fire its primary attack without
		/// restriction if this value is zero or lower.
		/// </remarks>
		/// <value>
		/// How many shots per second the weapon can fire.
		/// </value>
		public virtual float PrimaryRate => 5f;
		
		/// <summary>
		/// The rate at which the weapon can fire its secondary attack.
		/// </summary>
		/// <remarks>
		/// This value is in units of shots per tick. The weapon will be able to fire its secondary attack without
		/// restriction if this value is zero or lower.
		/// </remarks>
		public virtual float SecondaryRate => 5f;
		
		/// <summary>
		/// Gets a timer representing how much time has passed since the weapon's primary attack was last fired.
		/// </summary>
		/// <value>
		/// A timer.
		/// </value>
		[Net, Predicted]
		public TimeSince TimeSincePrimaryAttack { get; private set; }
		
		/// <summary>
		/// Gets a timer representing how much time has passed since the weapon's secondary attack was last fired.
		/// </summary>
		/// <value>
		/// A timer.
		/// </value>
		[Net, Predicted]
		public TimeSince TimeSinceSecondaryAttack { get; private set; }

		/// <summary>
		/// Gets a value representing whether the weapon may fire its primary attack at this time, based on by the
		/// weapon's <see cref="PrimaryRate"/> and <see cref="TimeSincePrimaryAttack"/>.
		/// </summary>
		/// <value>
		/// true if the weapon can fire its primary attack, false otherwise.
		/// </value>
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
		
		/// <summary>
		/// Gets a value representing whetherweapon may fire its secondary attack at this time, based on the weapon's
		/// <see cref="SecondaryRate"/> and <see cref="TimeSinceSecondaryAttack"/>.
		/// </summary>
		/// <value>
		/// true if the weapon can fire its secondary attack, false otherwise.
		/// </value>
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
		
		/// <summary>
		/// Reloads the weapon's ammunition.
		/// </summary>
		/// <remarks>
		/// This method has no default behavior. Override it to implement reloading.
		/// </remarks>
		public virtual void Reload() {}

		/// <summary>
		/// Fires the weapon's primary attack.
		/// </summary>
		/// <remarks>
		/// By default, this method resets the <see cref="TimeSincePrimaryAttack"/> timer.
		/// </remarks>
		public virtual void AttackPrimary()
		{
			TimeSincePrimaryAttack = 0;
		}
		
		/// <summary>
		/// Fires the weapon's secondary attack.
		/// </summary>
		/// <remarks>
		/// By default, this method resets the <see cref="TimeSinceSecondaryAttack"/> timer.
		/// </remarks>
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
		
		/// <summary>
		/// Traces a bullet that hits solids, players, and NPCs. It also hits water, unless the bullet starts
		/// submerged in water.
		/// </summary>
		/// <param name="start">The start position of the bullet.</param>
		/// <param name="end">The end position of the bullet.</param>
		/// <param name="radius">The radius to trace with.</param>
		/// <returns>The result of the trace.</returns>
		/// <!-- TODO: return IEnumerable of TraceResults, and hit both water and through water with yield return -->
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

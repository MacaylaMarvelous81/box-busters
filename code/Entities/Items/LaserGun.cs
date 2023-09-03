using Sandbox;

namespace BoxBusters.Entities.Items
{
	public class LaserGun : RangedWeapon
	{
		protected override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

		public override float PrimaryRate => 1f;
		public override float SecondaryRate => 1f;

		public override void Spawn()
		{
			base.Spawn();
			
			SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
		}

		public override void AttackPrimary()
		{
			base.AttackPrimary();

			if ( Owner is AnimatedEntity animatedOwner )
			{
				animatedOwner.SetAnimParameter( "b_attack", true );
			}
			
			Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

			if ( IsLocalPawn )
			{
				ViewModelEntity.SetAnimParameter( "fire", true );
			}

			PlaySound( "rust_pistol.shoot" );

			ApplyAbsoluteImpulse( Vector3.Backward * 200f );
		}
	}
}

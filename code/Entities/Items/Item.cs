using Sandbox;

namespace BoxBusters.Entities.Items
{
	public class Item : AnimatedEntity
	{
		protected BaseViewModel ViewModelEntity { get; private set; }
		
		protected virtual string ViewModelPath => null;

		public virtual bool CanCarry => true;

		public Entity EffectEntity
		{
			get
			{
				if ( ViewModelEntity.IsValid() && IsFirstPersonMode )
				{
					return ViewModelEntity;
				}

				return this;
			}
		}

		public override void Spawn()
		{
			base.Spawn();

			PhysicsEnabled = true;
			UsePhysicsCollision = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
			
			Tags.Add( "item" );
		}

		public virtual void OnCarryStart( Entity carrier )
		{
			// Don't do anything on the client, let s&box network.
			if ( Game.IsClient )
			{
				return;
			}

			SetParent( carrier, true );
			Owner = carrier;
			EnableAllCollisions = false;
			EnableDrawing = false;
		}

		public virtual void OnCarryDrop( Entity dropper )
		{
			EnableDrawing = true;

			if ( IsLocalPawn )
			{
				DestroyViewModel();
				DestroyHudElements();

				CreateViewModel();
				CreateHudElements();
			}
		}

		public void CreateViewModel()
		{
			Game.AssertClient();

			if ( string.IsNullOrEmpty( ViewModelPath ) )
			{
				return;
			}
			
			ViewModelEntity = new BaseViewModel();
			ViewModelEntity.Position = Position;
			ViewModelEntity.Owner = Owner;
			ViewModelEntity.EnableViewmodelRendering = true;
			ViewModelEntity.SetModel( ViewModelPath );
		}
		
		public void DestroyViewModel()
		{}
		
		public virtual void CreateHudElements()
		{}
		
		public virtual void DestroyHudElements()
		{}
	}
}

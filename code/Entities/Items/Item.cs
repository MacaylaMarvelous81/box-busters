using Sandbox;

namespace BoxBusters.Entities.Items
{
	/// <summary>
	/// An item, which can be carried by entities which opt into the system.
	/// </summary>
	public class Item : AnimatedEntity
	{
		/// <summary>
		/// Gets a value representing the model path for the item's view model. This should be overridden by subclasses.
		/// </summary>
		/// <value>
		/// The model path for the item's view model.
		/// </value>
		protected virtual string ViewModelPath => null;
		
		/// <summary>
		/// Gets a reference to the item's view model as an entity.
		/// </summary>
		/// <remarks>
		/// The view model won't exist if <see cref="ViewModelPath"/> isn't specified.
		/// </remarks>
		/// <value>
		/// A reference to the item's view model.
		/// </value>
		protected BaseViewModel ViewModelEntity { get; private set; }

		/// <summary>
		/// Gets a value indicating whether item can be carried.
		/// </summary>
		/// <remarks>
		/// By default, items can always be carried.
		/// </remarks>
		/// <value>
		/// true if the item can be carried, false otherwise.
		/// </value>
		public virtual bool CanCarry => true;

		/// <summary>
		/// Gets a reference to the entity at which effects should be played on.
		/// </summary>
		/// <remarks>
		/// This will be the view model entity if it's valid and the item is being rendered in first
		/// person mode. Otherwise, it will be the item entity.
		/// </remarks>
		/// <value>
		/// The entity where effects should originate from.
		/// </value>
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

		/// <summary>
		/// Behavior for when the item is picked up by an entity.
		/// </summary>
		/// <remarks>
		/// By default, it will set the item's parent and owner to the <paramref name="carrier"/>, and disable
		/// collisions and drawing.
		/// </remarks>
		/// <param name="carrier">The new owner of the item.</param>
		protected virtual void OnCarryStart( Entity carrier )
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

		/// <summary>
		/// The behavior for when the item is dropped by an entity.
		/// </summary>
		/// <remarks>
		/// By default, it will enable drawing for the item, and for the local client, recreate the view model and
		/// HUD elements.
		/// </remarks>
		/// <param name="dropper">The former owner of the item.</param>
		protected virtual void OnCarryDrop( Entity dropper )
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

		/// <summary>
		/// Creates the <see cref="ViewModelEntity"/> from the path in <see cref="ViewModelPath"/>, if there is one.
		/// </summary>
		/// <remarks>
		/// If <see cref="ViewModelPath"/> is unspecified, no view model will be created.
		/// </remarks>
		/// <exception cref="System.Exception">Thrown when called anywhere other than the client.</exception>
		private void CreateViewModel()
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

		/// <summary>
		/// Destroys the <see cref="ViewModelEntity"/>.
		/// </summary>
		private void DestroyViewModel()
		{
			ViewModelEntity.Delete();
			ViewModelEntity = null;
		}
		
		/// <summary>
		/// This is where HUD items related to the item should be created.
		/// </summary>
		protected virtual void CreateHudElements() {}
		
		/// <summary>
		/// This is where HUD items from CreateHudElements should be cleaned up.
		/// </summary>
		protected virtual void DestroyHudElements() {}
	}
}

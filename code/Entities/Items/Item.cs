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
		/// Determines whether the item can be carried by an entity.
		/// </summary>
		/// <param name="carrier">The entity in question.</param>
		/// <returns>true if the item can be carried, false otherwise.</returns>
		public virtual bool CanCarry( Entity carrier ) => true;

		/// <summary>
		/// Behavior for when the item is picked up by an entity.
		/// </summary>
		/// <remarks>
		/// By default, it will set the item's parent and owner to the <paramref name="carrier"/>, and become invisible
		/// and intangible.
		/// </remarks>
		/// <param name="carrier">The new owner of the item.</param>
		protected internal virtual void OnCarryStart( Entity carrier )
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
		/// The behavior for when the item is dropped back into the world by an entity.
		/// </summary>
		/// <remarks>
		/// By default, it will become unowned, and become visible and tangible.
		/// </remarks>
		/// <param name="dropper">The former owner of the item.</param>
		protected internal virtual void OnCarryDrop( Entity dropper )
		{
			// Don't do anything on the client, let s&box network.
			if ( Game.IsClient )
			{
				return;
			}
			
			SetParent( null );
			Owner = null;
			EnableAllCollisions = true;
			EnableDrawing = true;
		}

		/// <summary>
		/// The behavior for when an entity starts to actively use the item.
		/// </summary>
		/// <remarks>
		/// By default, it enables drawing the item, and (re)creates the view model and HUD elements for the local
		/// player.
		/// </remarks>
		/// <param name="entity">The entity using the item.</param>
		protected internal virtual void ActiveStart( Entity entity )
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
		/// The behavior for when an entity stops actively using the item.
		/// </summary>
		/// <remarks>
		/// By default, it disables drawing the item if it wasn't dropped, and destroys the view model and HUD elements.
		/// </remarks>
		/// <param name="entity">The entity that used the item.</param>
		/// <param name="dropped">Whether the entity stopped using the item because they dropped it.</param>
		protected internal virtual void ActiveEnd( Entity entity, bool dropped )
		{
			if ( !dropped )
			{
				EnableDrawing = false;
			}

			if ( IsLocalPawn )
			{
				DestroyViewModel();
				DestroyHudElements();
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

			ItemViewModel viewModel = new ItemViewModel();
			viewModel.SetModel( ViewModelPath );
			ViewModelEntity = viewModel;
		}

		/// <summary>
		/// Destroys the <see cref="ViewModelEntity"/>.
		/// </summary>
		private void DestroyViewModel()
		{
			ViewModelEntity?.Delete();
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

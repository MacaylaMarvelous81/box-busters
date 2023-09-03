using System.Collections.Generic;
using BoxBusters.Entities.Items;
using Sandbox;

namespace BoxBusters.Entities.Components
{
	/// <summary>
	/// The inventory component, responsible for managing the items so that an entity can carry them.
	/// </summary>
	public partial class InventoryComponent : EntityComponent
	{
		private List<Entity> _items = new List<Entity>();

		/// <summary>
		/// Gets a reference to the current active entity. (item being held)
		/// </summary>
		/// <value>
		/// A reference to the current active entity.
		/// </value>
		[Net, Predicted]
		public Entity ActiveEntity { get; private set; }
		
		/// <summary>
		/// Gets a reference to the active entity from the last simulated tick.
		/// </summary>
		/// <remarks>
		/// This will be different from <see cref="ActiveEntity"/> if the active entity changed since the last
		/// simulated tick.
		/// </remarks>
		/// <value>
		/// A reference to the active entity from the last simulated tick.
		/// </value>
		[Predicted]
		public Entity LastActiveEntity { get; private set; }
		
		/// <summary>
		/// Gets a value indicating the number of items in the inventory.
		/// </summary>
		/// <value>
		/// How many items are in the inventory.
		/// </value>
		public int Count => _items.Count;

		public void Simulate( IClient cl )
		{
			if ( LastActiveEntity != ActiveEntity )
			{
				if ( LastActiveEntity is Item lastItem )
				{
					lastItem.ActiveEnd( Entity, lastItem.Owner != Entity );
				}

				if ( ActiveEntity is Item activeItem )
				{
					activeItem.ActiveStart( Entity );
				}
				
				LastActiveEntity = ActiveEntity;
			}

			if ( !LastActiveEntity.IsValid )
			{
				return;
			}

			if ( LastActiveEntity.IsAuthority )
			{
				LastActiveEntity.Simulate( cl );
			}
		}

		/// <summary>
		/// Determines whether an item can be added to the inventory.
		/// </summary>
		/// <remarks>
		/// An item can be added to the inventory if it is an <see cref="Item"/>, and the item can be carried by the
		/// component's "owner" entity.
		/// </remarks>
		/// <param name="entity">The item to check for.</param>
		/// <returns>true if the item can be carried, and false if not.</returns>
		/// <seealso cref="Item.CanCarry"/>
		public bool CanAddItem( Entity entity )
		{
			return entity is Item item && item.CanCarry( Entity );
		}

		/// <summary>
		/// Safely sets the active entity to the entity passed to this method.
		/// </summary>
		/// <remarks>
		/// This method fails if the entity passed to this method is already the active entity, or if the entity is not
		/// in the inventory.
		/// </remarks>
		/// <param name="entity">The entity to set to the active entity</param>
		/// <returns>true if the entity became the active entity, false otherwise.</returns>
		/// TODO: Replace with public setter?
		public bool SetActiveEntity( Entity entity )
		{
			if ( ActiveEntity == entity || !_items.Contains( entity ))
			{
				return false;
			}

			ActiveEntity = entity;
			return true;
		}

		/// <summary>
		/// Safely adds an item to the inventory.
		/// </summary>
		/// <remarks>
		/// This method will fail if the item is already in the inventory, or <see cref="CanAddItem"/> returns false.
		/// </remarks>
		/// <param name="entity">The new item.</param>
		/// <param name="makeActive">Whether the item should immediately become active.</param>
		/// <returns>true if the item was added, false if not.</returns>
		/// <exception cref="System.Exception">Thrown when called anywhere but the server.</exception>
		public bool AddItem( Entity entity, bool makeActive )
		{
			// Only tolerate adding items on the server.
			Game.AssertServer();
			
			// If the item is already in the inventory, stop.
			if ( _items.Contains( entity ) )
			{
				return false;
			}
			
			// If the item cannot be added to the inventory, stop.
			if ( !CanAddItem( entity ) )
			{
				return false;
			}

			entity.Parent = Entity;
			_items.Add( entity );

			if ( entity is Item item )
			{
				item.OnCarryStart( Entity );
			}

			if ( makeActive )
			{
				SetActiveEntity( entity );
			}

			return true;
		}

		/// <summary>
		/// Gets the item located at the specified slot.
		/// </summary>
		/// <param name="slot">The slot to check.</param>
		/// <returns>The requested item, or null if the slot is out of range.</returns>
		public Entity AtSlot( int slot )
		{
			return slot < 0 || slot >= _items.Count ? null : _items[slot];
		}

		/// <summary>
		/// Finds the slot of the specified item.
		/// </summary>
		/// <param name="entity">The item to search for.</param>
		/// <returns>The item's index, or -1 if it wasn't found.</returns>
		public int SlotOf( Entity entity )
		{
			return _items.IndexOf( entity );
		}

		/// <summary>
		/// Safely sets the active entity by slot number.
		/// </summary>
		/// <param name="slot">The slot to use.</param>
		/// <seealso cref="SetActiveEntity"/>
		public void SetActiveSlot( int slot )
		{
			SetActiveEntity( AtSlot( slot ) );
		}

		/// <summary>
		/// Drops an item from the inventory by slot.
		/// </summary>
		/// <param name="slot">The slot of the item to drop.</param>
		public void DropSlot( int slot )
		{
			// Only drop items on the server since they are networked
			if ( !Game.IsServer )
			{
				return;
			}

			Entity entity = AtSlot( slot );

			if ( entity == null )
			{
				return;
			}

			entity.Parent = null;

			if ( Entity is Item item )
			{
				item.OnCarryDrop( Entity );
			}
		}
	}
}

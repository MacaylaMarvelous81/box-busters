using System.Collections.Generic;
using BoxBusters.Entities.Items;
using Sandbox;

namespace BoxBusters.Entities.Components
{
	public partial class InventoryComponent : EntityComponent
	{
		private List<Entity> _items = new List<Entity>();

		[Net, Predicted]
		public Entity ActiveEntity { get; set; }
		
		public int Count => _items.Count;

		public void Simulate( IClient cl )
		{
			ActiveEntity?.Simulate( cl );
		}

		public bool CanAddItem( Entity entity )
		{
			return entity is Item item && item.CanCarry( Entity );
		}

		public bool AddItem( Entity entity, bool makeActive )
		{
			// Only tolerate adding items on the server.
			Game.AssertServer();
			
			// If the item is already in the inventory, stop.
			if ( _items.Contains( entity ) )
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
				ActiveEntity = entity;
			}

			return true;
		}

		public Entity AtSlot( int slot )
		{
			return slot < 0 || slot >= _items.Count ? null : _items[slot];
		}

		public int SlotOf( Entity entity )
		{
			return _items.IndexOf( entity );
		}

		public void SetActiveSlot( int slot )
		{
			ActiveEntity = AtSlot( slot );
		}

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

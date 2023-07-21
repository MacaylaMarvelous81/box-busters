using System;
using System.Linq;
using Sandbox;

namespace BoxBusters.Entities.Factory
{
	public class PawnFactory
	{
		public static IEntity CreatePlayerPawn(IPawnFactory factory)
		{
			IEntity pawn = factory.CreatePawn();

			IEntity spawnPoint =
				Entity.All.OfType<SpawnPoint>()
				.MinBy(spawn => Guid.NewGuid());

			if ( spawnPoint != null )
			{
				Transform spawnTransform = spawnPoint.Transform;
				
				// Raise spawn position by 50 units to avoid spawning inside the ground
				spawnTransform.Position += Vector3.Up * 50f;

				pawn.Position = spawnTransform.Position;
				pawn.Rotation = spawnTransform.Rotation;
			}

			return pawn;
		}
	}
}

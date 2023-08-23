using System;
using System.Linq;
using Sandbox;

namespace BoxBusters.Entities.Factory
{
	/// <summary>
	/// A factory which produces and configures pawns.
	/// </summary>
	public class PawnFactory
	{
		/// <summary>
		/// Creates a pawn and moves it to a spawn point.
		/// </summary>
		/// <param name="factory">The <see cref="IPawnFactory"/> to use.</param>
		/// <returns>The new pawn.</returns>
		public static IEntity SpawnPawn(IPawnFactory factory)
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

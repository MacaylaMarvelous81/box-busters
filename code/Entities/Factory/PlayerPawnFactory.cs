using Sandbox;

namespace BoxBusters.Entities.Factory
{
	/// <summary>
	/// Produces a <see cref="PlayerPawn"/>.
	/// </summary>
	public class PlayerPawnFactory : IPawnFactory
	{
		public IEntity CreatePawn()
		{
			PlayerPawn pawn = new PlayerPawn();
			
			return pawn;
		}
	}
}

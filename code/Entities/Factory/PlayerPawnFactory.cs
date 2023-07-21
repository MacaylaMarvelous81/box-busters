using Sandbox;

namespace BoxBusters.Entities.Factory
{
	public class PlayerPawnFactory : IPawnFactory
	{
		public IEntity CreatePawn()
		{
			PlayerPawn pawn = new PlayerPawn();
			
			return pawn;
		}
	}
}

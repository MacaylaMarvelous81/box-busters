using BoxBusters.Entities;
using BoxBusters.Entities.Factory;
using Sandbox;

namespace BoxBusters
{
	public class BoxBustersManager : GameManager
	{
		public override void ClientJoined( IClient cl )
		{
			base.ClientJoined( cl );

			IEntity pawn = PawnFactory.SpawnPawn( new PlayerPawnFactory() );
			cl.Pawn = pawn;
		}
	}
}

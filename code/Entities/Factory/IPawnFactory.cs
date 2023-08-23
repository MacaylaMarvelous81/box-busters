using Sandbox;

namespace BoxBusters.Entities.Factory
{
	/// <summary>
	/// A factory which produces pawns.
	/// </summary>
	public interface IPawnFactory
	{
		/// <summary>
		/// Creates a pawn.
		/// </summary>
		/// <returns>The new pawn.</returns>
		public IEntity CreatePawn();
	}
}

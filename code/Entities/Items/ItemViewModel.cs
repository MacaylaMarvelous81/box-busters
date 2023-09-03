using Sandbox;

namespace BoxBusters.Entities.Items
{
	/// <summary>
	/// The viewmodel for items without any special viewmodel behaviour.
	/// </summary>
	public class ItemViewModel : BaseViewModel
	{
		public ItemViewModel()
		{
			EnableShadowCasting = false;
			EnableViewmodelRendering = true;
		}
		
		public override void PlaceViewmodel()
		{
			base.PlaceViewmodel();
			
			Camera.Main.SetViewModelCamera( 80f, 1f, 500f );
		}
	}
}

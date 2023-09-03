using Sandbox;

namespace BoxBusters.Entities.Items
{
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

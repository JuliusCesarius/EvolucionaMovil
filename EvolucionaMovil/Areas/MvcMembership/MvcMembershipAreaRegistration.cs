using System.Web.Mvc;

namespace EvolucionaMovil.Areas.MvcMembership
{
	public class MvcMembershipAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get
			{
				return "MvcMembership";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapRoute(
				"MvcMembership_default",
				"MvcMembership/{controller}/{action}/{id}",
				new { action = "Index", id = UrlParameter.Optional }
			);
		}
	}
}

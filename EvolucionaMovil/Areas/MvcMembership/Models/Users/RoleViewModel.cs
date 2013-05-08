using System.Collections.Generic;
using System.Web.Security;

namespace EvolucionaMovil.Areas.MvcMembership.Models.Users
{
	public class RoleViewModel
	{
		public string Role { get; set; }
		public IDictionary<string, MembershipUser> Users { get; set; }
	}
}

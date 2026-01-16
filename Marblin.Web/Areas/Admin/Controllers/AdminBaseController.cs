using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marblin.Web.Areas.Admin.Controllers
{
    /// <summary>
    /// Base controller for all Admin area controllers.
    /// Requires authenticated admin users.
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public abstract class AdminBaseController : Controller
    {
    }
}

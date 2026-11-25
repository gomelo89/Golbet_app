using Microsoft.AspNetCore.Mvc;

namespace GolBet.Web.Controllers;

public class ErrorController : Controller
{
    [Route("Error/{statusCode:int}")]
    public IActionResult Handle(int statusCode)
    {
        if (statusCode == 404)
            return View("NotFound");

        return View("General");
    }

    [Route("Error/General")]
    public IActionResult General()
        => View("General");
}

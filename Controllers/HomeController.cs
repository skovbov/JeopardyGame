using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Host()
    {
        return View();
    }

    public IActionResult Play()
    {
        return View();
    }
}
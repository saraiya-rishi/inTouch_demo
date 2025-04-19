using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using inTouch_demo.Models;
using System.DirectoryServices.AccountManagement;
using Microsoft.AspNetCore.Authentication;

namespace inTouch_demo.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var model = new ResetRequest();
        ViewBag.Users = GetLocalUsers();
        return View(model);
    }

    [HttpPost]
    public IActionResult Index(ResetRequest model)
    {
        ViewBag.Users = GetLocalUsers();

        if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.NewPassword))
        {
            model.Result = "Username and Password are required.";
            return View(model);
        }

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c net user {model.Username} {model.NewPassword}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(psi))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error))
                {
                    model.Result = "Error resetting password: " + error;
                }
                else
                {
                    model.Result = "Password reset successful.";
                }
            }
        }
        catch (Exception ex)
        {
            model.Result = $"Exception: {ex.Message}";
        }

        return View(model);
    }

    private List<string> GetLocalUsers()
    {
        var users = new List<string>();
        using (var ctx = new PrincipalContext(ContextType.Machine))
        {
            using (var searcher = new PrincipalSearcher(new UserPrincipal(ctx)))
            {
                foreach (var result in searcher.FindAll())
                {
                    if (result is UserPrincipal user)
                    {
                        users.Add(user.SamAccountName);
                    }
                }
            }
        }
        return users;
    }



    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

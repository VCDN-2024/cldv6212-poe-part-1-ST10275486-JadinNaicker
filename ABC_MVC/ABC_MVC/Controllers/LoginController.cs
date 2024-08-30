using ABC_MVC.Models;
using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ABC_MVC.Controllers
{
    public class LoginController : Controller
    {
        private readonly TableStorageService _tableStorageService;
        private readonly string _fileShareConnectionString = "DefaultEndpointsProtocol=https;AccountName=st10275496;AccountKey=CpfBmfw/u2CiDAGJGrNOYWedlAYXqrYgH2D+9lPjyacwFuTX+ZR7gv3DugtodgImsQQ2MbypK40f+AStDs84jQ==;EndpointSuffix=core.windows.net"; //Connection string
        private readonly string _fileShareName = "logreport"; //Name for the azure file share

        public LoginController(TableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var profile = await _tableStorageService.GetProfileAsync("Customer", model.Email);

                if (profile != null && BCrypt.Net.BCrypt.Verify(model.Password, profile.PasswordHash))
                {
                    await LogUserLoginAsync(model.Email);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }
            return View(model);
        }

        private async Task LogUserLoginAsync(string email)
        {
            try
            {
                
                ShareClient share = new ShareClient(_fileShareConnectionString, _fileShareName);

                
                await share.CreateIfNotExistsAsync();

               
                string fileName = "log.txt";

               
                ShareFileClient file = share.GetRootDirectoryClient().GetFileClient(fileName);

                
                if (!await file.ExistsAsync())
                {
                    await file.CreateAsync(maxSize: 1024); 
                }

               
                string logEntry = $"{DateTime.UtcNow}: {email} logged in{Environment.NewLine}";

              
                byte[] data = Encoding.UTF8.GetBytes(logEntry);

               
                ShareFileProperties properties = await file.GetPropertiesAsync();
                long newSize = properties.ContentLength + data.Length;
                await file.SetHttpHeadersAsync(newSize);

                
                using (MemoryStream stream = new MemoryStream(data))
                {
                    await file.UploadRangeAsync(
                        new HttpRange(properties.ContentLength, data.Length),
                        stream);
                }
            }
            catch (Exception ex)
            {
               
                Console.WriteLine($"Error logging user login: {ex.Message}");
            }
        }
    }
}

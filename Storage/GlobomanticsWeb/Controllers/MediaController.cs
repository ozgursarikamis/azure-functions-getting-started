using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GlobomanticsWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace GlobomanticsWeb.Controllers
{
    public class MediaController : Controller
    {
        private readonly IConfiguration config;

        public MediaController(IConfiguration appConfig)
        {
            config = appConfig;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize]
        public async Task<IActionResult> Index(ImageUploadModel model)
        {
            //upload image after authorizing user
            var containerClient = new BlobContainerClient(
                config["BlobCNN"], "m3globoimages");

            var blobClient = containerClient.GetBlobClient(
                model.ImageFile.FileName); // USE a temporary file name

            var result = await blobClient.UploadAsync(model.ImageFile.OpenReadStream(),
                new BlobHttpHeaders
                {
                    ContentType = model.ImageFile.ContentType,
                    CacheControl = "public"
                },
                new Dictionary<string, string> { { "customName",
                    model.Name} }
            );

            return RedirectToAction("Index");
        }
    }
}
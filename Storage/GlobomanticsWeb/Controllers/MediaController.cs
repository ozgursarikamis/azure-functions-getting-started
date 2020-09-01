using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
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

        public async Task<IActionResult> Index()
        {
            var images = new List<ImageModel>();
            //get a list of images in the container and add to the list
            var containerClient = new BlobContainerClient(
                config["BlobCNN"], "m3globoimages");

            var blobs = containerClient.GetBlobsAsync(BlobTraits.Metadata);
            await foreach (var item in blobs)
            {
                images.Add(new ImageModel
                {
                    Name = item.Metadata["customName"],
                    ImageFileName = item.Name
                });
            }

            return View(images);
        }

        [HttpGet]
        //[Authorize] // when using auth to make sure they should get the link
        public IActionResult Detail(string imageFileName)
        {
            var model = new ImageModel();
            //validate user is authenticated before showing the image!!

            //get image from storage and set URL and metadata name on model
            var containerClient = new BlobContainerClient(
                config["BlobCNN"], "m3globoimages");

            var blob = containerClient.GetBlobClient(imageFileName);

            var builder = new BlobSasBuilder
            {
                BlobContainerName = containerClient.Name,
                BlobName = blob.Name,
                ExpiresOn = DateTime.UtcNow.AddMinutes(2),
                Protocol = SasProtocol.Https
            };
            builder.SetPermissions(BlobSasPermissions.Read);

            var uBuilder = new UriBuilder(blob.Uri)
            {
                Query = builder.ToSasQueryParameters(
                    new Azure.Storage.StorageSharedKeyCredential(
                        containerClient.AccountName,
                        config["BlobKey"]
                    )).ToString()
            };

            model.Url = uBuilder.Uri.ToString();
            model.ImageFileName = imageFileName;

            return View(model);
        }
    }
}
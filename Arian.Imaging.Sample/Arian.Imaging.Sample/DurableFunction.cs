using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using static Arian.Imaging.Sample.StorageHelper;

namespace Arian.Imaging.Sample
{
    public class DurableFunction
    {
        public static string invalidImgsBlobStorageName { get; set; }
        public static string invalidImgsBlobContainer { get; set; }

        private readonly ILogger<DurableFunction> _log;

        public DurableFunction(ILogger<DurableFunction> log)
        {
            _log = log;
        }


        [FunctionName("ImageBlobTrigger")]
        public async Task Run([BlobTrigger("inputimages/{name}", Connection = "inputBlobConn")] CloudBlockBlob myBlob, string name, [DurableClient] IDurableOrchestrationClient orchestrationClient)
        {
            invalidImgsBlobStorageName = System.Environment.GetEnvironmentVariable("INVALID_IMAGES_STORAGE_ACCOUNTNAME");
            invalidImgsBlobContainer = System.Environment.GetEnvironmentVariable("INVALID_IMAGES_BLOB_CONTAINER");

            _log.LogInformation($"Blob trigger function Processed blob\n Name:{name} \n Uri: {myBlob.Uri}");

            using (ImageReference imgRef = new ImageReference(myBlob.Uri, name))
            {
                string instanceId = await orchestrationClient.StartNewAsync("ImageProcessOrcherstrator", null, imgRef);
                _log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
            }
        }



        [FunctionName("ImageProcessOrcherstrator")]
        public static async Task<List<string>> RunOrchestrator(
          [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            ImageReference imgRef = context.GetInput<ImageReference>();


            Uri inputImageUri = imgRef.uri;

            var uriSegementLength = inputImageUri.Segments.Length;
            string fileName = inputImageUri.Segments.GetValue(uriSegementLength - 1).ToString();

            bool validFileName = await context.CallActivityAsync<bool>("CheckFileName", fileName);

            if (validFileName.Equals(true))
            {
                var imgAnalysis = await context.CallActivityAsync<string>("AnalyseImage", inputImageUri);
                //await context.CallActivityAsync<string>("SendToInvalidImagesContainer", imgRef.uri);
            }
            else
            {
                await context.CallActivityAsync<string>("SendToInvalidImagesContainer", imgRef.uri);
            }

            return outputs;
        }



        [FunctionName("SendToInvalidImagesContainer")]
        public  async Task SendToInvalidImagesContainerAsync([ActivityTrigger] string inputImageUri)
        {
            
            _log.LogInformation($"Sending image named: {inputImageUri} to invalid image container.");

                Uri fileUri = new Uri(inputImageUri);

                var uriSegementLength = fileUri.Segments.Length;

                string destAccountName = invalidImgsBlobStorageName;
                string destContainerName = invalidImgsBlobContainer;
                string destFileName = fileUri.Segments.GetValue(uriSegementLength - 1).ToString();
                
                StorageHelper strgHelper = new StorageHelper(log: _log);
                
            var blobStream = await strgHelper.DownloadBlobFromUriAsync(fileUri);
                await strgHelper.UploadBlobToContainer(destFileName, blobStream, destAccountName, destContainerName);
                await strgHelper.DeleteBlobFromContainer(fileUri);
        }


        


        [FunctionName("CheckFileName")]
        public bool CheckFileName([ActivityTrigger] string fileName)
        {
            _log.LogInformation($"Checking {fileName}'s naming convention.");
            //Call custom CheckFileName logic in here or bring it in as part of other classes/method calls.
            bool result = true;

            return result;
        }

        [FunctionName("AnalyseImage")]
        public bool AnalyseImage([ActivityTrigger] Uri imageUri)
        {
            _log.LogInformation($"Calling AnalyseImage against {imageUri}");
            //Get ready / call custom ImageAnalysis exe here
            bool result = true;

            return result;
        }
    }





}

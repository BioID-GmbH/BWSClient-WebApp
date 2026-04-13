﻿namespace BioID.BWS.WebApp.Pages.PhotoVerify
{
    using BioID.BWS.WebApp.Extensions;
    using BioID.Services;
    using Google.Protobuf;
    using Grpc.Core;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using System.Text.Json;

    /// <summary>
    /// Page model for the PhotoVerify feature.
    /// Compares an ID photo with live images and performs liveness detection.
    /// </summary>
    public class PhotoVerifyModel(BioIDWebService.BioIDWebServiceClient bwsServiceClient, ILoggerFactory loggerFactory) : PageModel
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger("PhotoVerify");
        private readonly BioIDWebService.BioIDWebServiceClient _bws = bwsServiceClient;
        private static readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // Retrieve images from form. Expects: [0] ID Photo, [1] First Live Image, [2] Second Live Image.
                var idphoto = Request.Form.Files[0];
                var liveimage1 = Request.Form.Files[1];
                var liveimage2 = Request.Form.Files[2];

                if (idphoto == null || liveimage1 == null || liveimage2 == null)
                {
                    return Partial("_PhotoVerifyResult", new PhotoVerifyResultModel { ErrorString = "Missing images: at least one live image and an ID photo are required!" });
                }
                ByteString photo = await idphoto.ReadFormFileAsync();
                ByteString image1 = await liveimage1.ReadFormFileAsync();
                ByteString image2 = await liveimage2.ReadFormFileAsync();

                var request = new PhotoVerifyRequest { Photo = photo };
                request.LiveImages.Add(new ImageData() { Image = image1 });
                request.LiveImages.Add(new ImageData() { Image = image2 });

                // Execute gRPC call for photo verification
                using var photoVerifyCall = _bws.PhotoVerifyAsync(request, new Metadata { { "Reference-Number", "BioID.BWS.DemoWebApp" } });
                var response = await photoVerifyCall.ResponseAsync.ConfigureAwait(false);

                _logger.LogInformation("Call to photoverify API returned {StatusCode}.", response.Status);

                var id = photoVerifyCall.GetTrailers().Where(t => t.Key == "jobid").Select(t => t.Value).FirstOrDefault();
                if (string.IsNullOrEmpty(id)) { id = Guid.NewGuid().ToString(); }

                var result = new PhotoVerifyResultModel
                {
                    Live = response.Live,
                    LivenessScore = Math.Round(response.LivenessScore, 5),
                    Accuracy = (int)response.VerificationLevel,
                    VerificationScore = Math.Round(response.VerificationScore, 5),
                    PhotoProperties = JsonSerializer.Serialize(response.PhotoProperties, _serializerOptions),
                    ErrorMessages = [.. response.Errors.DistinctBy(e => e.ErrorCode).Select(e => e.Message)]
                };
                // Extract and serialize individual image properties if available
                if (response.ImageProperties.Count > 0) { result.ImageProperties1 = JsonSerializer.Serialize(response.ImageProperties[0], _serializerOptions); }
                if (response.ImageProperties.Count > 1) { result.ImageProperties2 = JsonSerializer.Serialize(response.ImageProperties[1], _serializerOptions); }

                return Partial("_PhotoVerifyResult", result);
            }
            catch (RpcException ex)
            {
                _logger.LogError("gRPC error from calling BWS photo verification service: {StatusCode} - {StatusDetail}", ex.Status.StatusCode, ex.Status.Detail);
                return Partial("_PhotoVerifyResult", new PhotoVerifyResultModel { ErrorString = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform photo verification.");
                return Partial("_PhotoVerifyResult", new PhotoVerifyResultModel { ErrorString = ex.Message });
            }
        }
    }
}

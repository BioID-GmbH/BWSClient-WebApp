using BioID.Services;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace BioID.BWS.WebApp.Pages.PhotoVerify
{
    public class PhotoVerifyModel(BioIDWebService.BioIDWebServiceClient bwsServiceClient, ILoggerFactory loggerFactory) : PageModel
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger("LivenessDetection");
        private readonly BioIDWebService.BioIDWebServiceClient _bws = bwsServiceClient;
        private static readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

        // ajax-call (with antiforgery)
        public async Task<IActionResult> OnPostAsync()
        {
            bool testing = User.IsInRole("Employee") || User.IsInRole("Testing");
            try
            {
                var idphoto = Request.Form.Files["idphoto"];
                var liveimage1 = Request.Form.Files["image1"];
                var liveimage2 = Request.Form.Files["image2"];
                if (idphoto == null || liveimage1 == null || liveimage2 == null)
                {
                    return Partial("_PhotoVerifyResult", new PhotoVerifyResultModel { ErrorString = "Missing images: at least one live image and an ID photo are required!" });
                }

                using MemoryStream idStream = new();
                using MemoryStream liveStream1 = new();
                using MemoryStream liveStream2 = new();

                await idphoto.CopyToAsync(idStream).ConfigureAwait(false);
                await liveimage1.CopyToAsync(liveStream1).ConfigureAwait(false);
                await liveimage2.CopyToAsync(liveStream2).ConfigureAwait(false);

                ByteString photo = ByteString.CopyFrom(idStream.ToArray());
                ByteString image1 = ByteString.CopyFrom(liveStream1.ToArray());
                ByteString image2 = ByteString.CopyFrom(liveStream2.ToArray());

                var request = new PhotoVerifyRequest { Photo = photo };
                request.LiveImages.Add(new ImageData() { Image = image1 });
                request.LiveImages.Add(new ImageData() { Image = image2 });

                var photoVerifyCall = _bws.PhotoVerifyAsync(request, new Metadata { { "Reference-Number", "BioID.BWS.DemoWebApp" } });
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
                    Id = id,
                    PhotoProperties = JsonSerializer.Serialize(response.PhotoProperties, _serializerOptions)
                };
                if (response.ImageProperties.Count > 0) { result.ImageProperties1 = JsonSerializer.Serialize(response.ImageProperties[0], _serializerOptions); }
                if (response.ImageProperties.Count > 1) { result.ImageProperties2 = JsonSerializer.Serialize(response.ImageProperties[1], _serializerOptions); }
                foreach (var error in response.Errors.Select(e => e.Message).Distinct())
                {
                    result.ErrorMessages.Add(error);
                }

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

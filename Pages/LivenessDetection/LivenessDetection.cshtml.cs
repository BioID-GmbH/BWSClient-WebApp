using BioID.Services;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace BioID.BWS.WebApp.Pages.LivenessDetection
{
    public class LivenessDetectionModel(BioIDWebService.BioIDWebServiceClient bwsServiceClient, ILoggerFactory loggerFactory) : PageModel
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger("LivenessDetection");
        private readonly BioIDWebService.BioIDWebServiceClient _bws = bwsServiceClient;
        private static readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

        public string ErrorString { get; set; } = string.Empty;

        // ajax-call (with antiforgery)
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var liveimage1 = Request.Form.Files["image1"];
                if (liveimage1 == null)
                {
                    return Partial("_LivenessDetectionResult", new LivenessDetectionResultModel { ErrorString = "At least one image is required for liveness detection!" });
                }

                using var s1 = liveimage1.OpenReadStream();
                ByteString image1 = await ByteString.FromStreamAsync(s1).ConfigureAwait(false);
                ByteString? image2 = null;
                var liveimage2 = Request.Form.Files["image2"];
                if (liveimage2 != null)
                {
                    using var s2 = liveimage2.OpenReadStream();
                    image2 = await ByteString.FromStreamAsync(s2).ConfigureAwait(false);
                }

                var livenessRequest = new LivenessDetectionRequest();
                livenessRequest.LiveImages.Add(new ImageData() { Image = image1 });
                if (image2 != null) { livenessRequest.LiveImages.Add(new ImageData() { Image = image2 }); }

                var livenessCall = _bws.LivenessDetectionAsync(livenessRequest, new Metadata { { "Reference-Number", "BioID.BWS.DemoWebApp" } });
                var response = await livenessCall.ResponseAsync.ConfigureAwait(false);

                _logger.LogInformation("Call to livedetection API returned {StatusCode}.", response.Status);

                var result = new LivenessDetectionResultModel()
                {
                    Active = liveimage2 != null,
                    Live = response.Live,
                    LivenessScore = Math.Round(response.LivenessScore, 5),
                    ErrorMessages = [.. response.Errors.DistinctBy(e => e.ErrorCode).Select(e => e.Message)]
                };
                if (response.ImageProperties.Count > 0) { result.ImageProperties1 = JsonSerializer.Serialize(response.ImageProperties[0], _serializerOptions); }
                if (response.ImageProperties.Count > 1) { result.ImageProperties2 = JsonSerializer.Serialize(response.ImageProperties[1], _serializerOptions); }
                result.ResultHints = [.. response.Errors.DistinctBy(e => e.ErrorCode).Select(e => e.Message)];
                return Partial("_LivenessDetectionResult", result);
            }
            catch (RpcException ex)
            {
                _logger.LogError("gRPC error from calling BWS liveness detection service: {StatusCode} - {StatusDetail}", ex.Status.StatusCode, ex.Status.Detail);
                return Partial("_LivenessDetectionResult", new LivenessDetectionResultModel { ErrorString = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform liveness detection.");
                return Partial("_LivenessDetectionResult", new LivenessDetectionResultModel { ErrorString = ex.Message });
            }
        }

        // ajax-call
        public IActionResult OnGetEmptyResult()
        {
            return Partial("_EmptyDetectionResult");
        }
    }
}

using BioID.BWS.WebApp.Helper;
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

        public string ErrorString { get; set; }

        // ajax-call (with antiforgery)
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var liveimage1 = Request.Form.Files["image1"];
                var liveimage2 = Request.Form.Files["image2"];

                using MemoryStream liveStream1 = new();
                using MemoryStream liveStream2 = new();

                // Check if first live image raw data is available
                if (liveimage1 != null) await liveimage1.CopyToAsync(liveStream1).ConfigureAwait(false);

                // Check if second live image raw data is available
                if (liveimage2 != null) await liveimage2.CopyToAsync(liveStream2).ConfigureAwait(false);

                ByteString image1 = ByteString.CopyFrom(liveStream1.ToArray());
                ByteString image2 = ByteString.CopyFrom(liveStream2.ToArray());

                if (image1 == null || image2 == null)
                {
                    return Partial("_LivenessDetectionResult", new LivenessDetectionResultModel { ErrorString = "At least one image was not uploaded completely!" });
                }

                var livenessRequest = new LivenessDetectionRequest();
                livenessRequest.LiveImages.Add(new ImageData() { Image = image1 });
                livenessRequest.LiveImages.Add(new ImageData() { Image = image2 });

                var livenessCall = _bws.LivenessDetectionAsync(livenessRequest, new Metadata { { "Reference-Number", "BioID.BWS.DemoWebApp" } });
                var response = await livenessCall.ResponseAsync.ConfigureAwait(false);

                _logger.LogInformation("Call to livedetection API returned {StatusCode}.", response.Status);

                var result = new LivenessDetectionResultModel()
                {
                    Live = response.Live,
                    LivenessScore = Math.Round(response.LivenessScore, 5),
                    ErrorMessages = response.Errors.Count != 0 ? response.Errors.Select(e => e.Message).Distinct().ToList() : []
                };
                if (response.ImageProperties.Count > 0) { result.ImageProperties1 = JsonSerializer.Serialize(response.ImageProperties[0], _serializerOptions); }
                if (response.ImageProperties.Count > 1) { result.ImageProperties2 = JsonSerializer.Serialize(response.ImageProperties[1], _serializerOptions); }
                foreach (var error in response.Errors.Select(e => e.ErrorCode).Distinct())
                {
                    string hint = error.HintFromResult();
                    if (!string.IsNullOrEmpty(hint))
                    {
                        result.ResultHints.Add(hint);
                    }
                }

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

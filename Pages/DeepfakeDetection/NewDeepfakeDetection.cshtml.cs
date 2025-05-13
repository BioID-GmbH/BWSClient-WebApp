namespace BioID.BWS.WebApp.Pages.DeepfakeDetection
{
    using BioID.Services;
    using Google.Protobuf;
    using Grpc.Core;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using System.Text.Json;

    public class NewDeepfakeDetectionModel(BioIDWebService.BioIDWebServiceClient bwsServiceClient, ILoggerFactory loggerFactory) : PageModel
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger("NewDeepfakeDetectionModel");
        private readonly BioIDWebService.BioIDWebServiceClient _bwsWebServiceClient = bwsServiceClient;
        private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

        public int MaxRequestSize => 50 * 1024 * 1024; // 50 mb limit
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // get image or video from the form
                var imageFile = Request.Form.Files["image"];
                var videoFile = Request.Form.Files["video"];

                // for image 
                if (imageFile != null)
                {
                    await using MemoryStream ms = new();
                    await imageFile.CopyToAsync(ms).ConfigureAwait(false);
                    var image = ms.ToArray();

                    // Create request
                    var livenessRequest = new LivenessDetectionRequest();
                    livenessRequest.LiveImages.Add(new ImageData() { Image = ByteString.CopyFrom(image) });

                    using var livenessCall = _bwsWebServiceClient.LivenessDetectionAsync(livenessRequest, headers: new Metadata { { "Reference-Number", "BioID.BWS.DemoWebApp" } });
                    var response = await livenessCall.ResponseAsync.ConfigureAwait(false);

                    _logger.LogInformation("Call to livedetection API returned {StatusCode}.", response.Status);

                    var json = JsonSerializer.Serialize(response, _serializerOptions);
                    return Partial("_NewDeepfakeDetectionResult", new NewDeepfakeDetectionResultModel()
                    {
                        Live = response.Live,
                        Fake = response.Errors.Any(e => e.ErrorCode == "RejectedByPassiveLiveDetection"),
                        LivenessScore = Math.Round(response.LivenessScore, 5),
                        ErrorMessages = [.. response.Errors.DistinctBy(e => e.ErrorCode).Select(e => e.Message)],
                        ResponseJson = json,
                        FileName = imageFile.FileName,
                        MediaType = "image"
                    });
                }
                // for video
                else if (videoFile != null)
                {
                    await using MemoryStream ms = new();
                    await videoFile.CopyToAsync(ms).ConfigureAwait(false);
                    var video = ms.ToArray();

                    // Add video sample to the grpc service request
                    var videoRequest = new VideoLivenessDetectionRequest()
                    {
                        Video = ByteString.CopyFrom(video)
                    };
                    var videoLivenessCall = _bwsWebServiceClient.VideoLivenessDetectionAsync(videoRequest, headers: new Metadata { { "Reference-Number", "BioID.BWS.DemoWebApp" } });
                    var response = await videoLivenessCall.ResponseAsync.ConfigureAwait(false);

                    _logger.LogInformation("Call to videolivenessdetection API returned {StatusCode}.", response.Status);

                    var json = JsonSerializer.Serialize(response, _serializerOptions);
                    return Partial("_NewDeepfakeDetectionResult", new NewDeepfakeDetectionResultModel()
                    {
                        Live = response.Live,
                        Fake = response.Errors.Any(e => e.ErrorCode == "RejectedByPassiveLiveDetection"),
                        LivenessScore = Math.Round(response.LivenessScore, 5),
                        ErrorMessages = [.. response.Errors.DistinctBy(e => e.ErrorCode).Select(e => e.Message)],
                        ResponseJson = json,
                        FileName = videoFile.FileName,
                        MediaType = "video"
                    });
                }

                return Partial("_NewDeepfakeDetectionResult", new NewDeepfakeDetectionResultModel { ErrorString = "No file (image/video) has been uploaded!" });
            }
            catch (RpcException ex)
            {
                _logger.LogError("gRPC error from calling service: {StatusCode} - {StatusDetail}", ex.Status.StatusCode, ex.Status.Detail);
                return Partial("_NewDeepfakeDetectionResult", new NewDeepfakeDetectionResultModel { ErrorString = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform deepfake detection.");
                return Partial("_NewDeepfakeDetectionResult", new NewDeepfakeDetectionResultModel { ErrorString = ex.Message });
            }
        }
    }
}

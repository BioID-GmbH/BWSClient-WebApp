﻿using BioID.BWS.WebApp.Extensions;
using BioID.Services;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace BioID.BWS.WebApp.Pages.FaceDeepfakeDetection
{
    /// <summary>
    /// Page model for the Face Deepfake Detection feature.
    /// It allows users to upload an image or video to detect if the face content is a deepfake.
    /// </summary>
    public class FaceDeepfakeDetectionModel(BioIDWebService.BioIDWebServiceClient bwsServiceClient, ILoggerFactory loggerFactory) : PageModel
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger("FaceDeepfakeDetectionModel");
        private readonly BioIDWebService.BioIDWebServiceClient _bwsWebServiceClient = bwsServiceClient;
        private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

        /// <summary>
        /// Gets the maximum allowed request size (50 MB).
        /// </summary>
        public int MaxRequestSize => 50 * 1024 * 1024; // 50 mb limit

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // Retrieve image or video file from the form
                var imageFile = Request.Form.Files["image"];
                var videoFile = Request.Form.Files["video"];

                // Process image file
                if (imageFile != null)
                {
                    var image = await imageFile.ReadFormFileAsync();

                    // Create and send liveness detection request for a single image (passive liveness)
                    var livenessRequest = new LivenessDetectionRequest();
                    livenessRequest.LiveImages.Add(new ImageData() { Image = image });

                    // Execute gRPC call for liveness detection on the image
                    using var livenessCall = _bwsWebServiceClient.LivenessDetectionAsync(livenessRequest, headers: new Metadata { { "Reference-Number", "BioID.BWS.DemoWebApp" } });
                    var response = await livenessCall.ResponseAsync.ConfigureAwait(false);

                    _logger.LogInformation("Call to livedetection API returned {StatusCode}.", response.Status);

                    var json = JsonSerializer.Serialize(response, _serializerOptions);
                    return Partial("_FaceDeepfakeDetectionResult", new FaceDeepfakeDetectionResultModel()
                    {
                        Live = response.Live,
                        // Map specific error code to "Fake" status
                        Fake = response.Errors.Any(e => e.ErrorCode == "RejectedByPassiveLiveDetection"),
                        LivenessScore = Math.Round(response.LivenessScore, 5),
                        ErrorMessages = [.. response.Errors.DistinctBy(e => e.ErrorCode).Select(e => e.Message)],
                        ResponseJson = json,
                        FileName = imageFile.FileName,
                        MediaType = "image"
                    });
                }
                // Process video file
                else if (videoFile != null)
                {
                    var video = await videoFile.ReadFormFileAsync();

                    // Add video sample to the grpc service request
                    var videoRequest = new VideoLivenessDetectionRequest()
                    {
                        Video = video
                    };

                    // Execute gRPC call for video liveness detection
                    var videoLivenessCall = _bwsWebServiceClient.VideoLivenessDetectionAsync(videoRequest, headers: new Metadata { { "Reference-Number", "BioID.BWS.DemoWebApp" } });
                    var response = await videoLivenessCall.ResponseAsync.ConfigureAwait(false);

                    _logger.LogInformation("Call to videolivenessdetection API returned {StatusCode}.", response.Status);

                    var json = JsonSerializer.Serialize(response, _serializerOptions);
                    return Partial("_FaceDeepfakeDetectionResult", new FaceDeepfakeDetectionResultModel()
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
                // No file uploaded
                return Partial("_FaceDeepfakeDetectionResult", new FaceDeepfakeDetectionResultModel { ErrorString = "No file (image/video) has been uploaded!" });
            }
            catch (RpcException ex)
            {
                _logger.LogError("gRPC error from calling service: {StatusCode} - {StatusDetail}", ex.Status.StatusCode, ex.Status.Detail);
                return Partial("_FaceDeepfakeDetectionResult", new FaceDeepfakeDetectionResultModel { ErrorString = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform face deepfake detection.");
                return Partial("_FaceDeepfakeDetectionResult", new FaceDeepfakeDetectionResultModel { ErrorString = ex.Message });
            }
        }
    }
}

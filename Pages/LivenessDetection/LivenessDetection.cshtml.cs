﻿using BioID.BWS.WebApp.Extensions;
using BioID.Services;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace BioID.BWS.WebApp.Pages.LivenessDetection
{
    /// <summary>
    /// Page model for the Liveness Detection feature.
    /// Supports processing a sequence of images for passive or active liveness detection.
    /// </summary>
    public class LivenessDetectionModel(BioIDWebService.BioIDWebServiceClient bwsServiceClient, ILoggerFactory loggerFactory) : PageModel
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger("LivenessDetection");
        private readonly BioIDWebService.BioIDWebServiceClient _bws = bwsServiceClient;
        private static readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

        [BindProperty]
        public InputModel Input { get; set; } = new();

        /// <summary>
        /// Form data for the liveness detection request.
        /// </summary>
        public class InputModel
        {
            public List<IFormFile> ImageFiles { get; set; } = [];
            public int LivenessDetectionMode { get; set; }
            public string Challenges { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // perform passive or active liveness detection with optional challenges
                var resultModel = new LivenessDetectionResultModel()
                {
                    Live = true
                };
                // Parse optional challenges if provided
                string[] challenges = Input.Challenges != null ? Input.Challenges.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) : [];

                List<ByteString> images = [];
                // Process images in pairs for active liveness detection, or individually for passive.
                // The loop iterates over pairs (n), using 'n * 2' to index the flat list of uploaded files.
                for (int n = 0; resultModel.Live && n * 2 < Input.ImageFiles.Count; n++)
                {
                    var pic1 = await Input.ImageFiles[n * 2].ReadFormFileAsync();
                    if (pic1.IsEmpty)
                    {
                        resultModel.Live = false;
                        resultModel.ErrorString = "At least one image is required for liveness detection!";
                        return Partial("_LivenessDetectionResult", resultModel);
                    }
                    images.Add(pic1);
                    LivenessDetectionResponse response = new();
                    var pic2 = Input.ImageFiles.Count > n * 2 + 1 ? await Input.ImageFiles[n * 2 + 1].ReadFormFileAsync() : ByteString.Empty;
                    if (pic2.IsEmpty) // passive
                    {
                        // Send request for a single image when no movement/second image is provided
                        response = await _bws.SendLivenessRequestAsync(new Metadata { { "Reference-Number", "BioID.BWS.DemoWebApp" } }, string.Empty, pic1);
                        resultModel.ImageProperties.Add(response.ImageProperties.Count > 0 ? JsonSerializer.Serialize(response.ImageProperties[0], _serializerOptions) : string.Empty);
                    }
                    else
                    {
                        images.Add(pic2);
                        // Active liveness detection requires two images to analyze movement/3D structure
                        string tag = challenges.Length > n ? challenges[n] : string.Empty;
                        response = await _bws.SendLivenessRequestAsync(new Metadata { { "Reference-Number", "BioID.BWS.DemoWebApp" } }, tag, pic1, pic2);
                        resultModel.ImageProperties.Add(response.ImageProperties.Count > 0 ? JsonSerializer.Serialize(response.ImageProperties[0], _serializerOptions) : string.Empty);
                        resultModel.ImageProperties.Add(response.ImageProperties.Count > 1 ? JsonSerializer.Serialize(response.ImageProperties[1], _serializerOptions) : string.Empty);
                    }
                    // handle liveness detection response
                    if (!response.Live)
                    {
                        resultModel.Live = false;
                        resultModel.ErrorMessages = [.. response.Errors.DistinctBy(e => e.ErrorCode).Select(e => e.Message)];
                    }
                }
                return Partial("_LivenessDetectionResult", resultModel);
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
    }
}

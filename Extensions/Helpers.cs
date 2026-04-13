using BioID.Services;
using Google.Protobuf;
using Grpc.Core;
using static BioID.Services.BioIDWebService;

namespace BioID.BWS.WebApp.Extensions
{
    public static class Helpers
    {
        public static async Task<ByteString> ReadFormFileAsync(this IFormFile file)
        {
            // Read file content into a bytestring
            using MemoryStream ms = new();
            await file.CopyToAsync(ms).ConfigureAwait(false);
            ms.Seek(0, SeekOrigin.Begin);
            return await ByteString.FromStreamAsync(ms);
        }

        public static async Task<LivenessDetectionResponse> SendLivenessRequestAsync(this BioIDWebServiceClient bwsClient, Metadata reference, string tag, params ByteString[] byteImages)
        {
            var request = new LivenessDetectionRequest();
            foreach (var image in byteImages)
            {
                request.LiveImages.Add(new ImageData { Image = image });
            }
            // tag is applied only to second image
            if (request.LiveImages.Count > 1 && !string.IsNullOrWhiteSpace(tag)) { request.LiveImages[1].Tags.Add(tag); }
            using var call = bwsClient.LivenessDetectionAsync(request, reference);
            return await call.ResponseAsync;
        }
    }
}

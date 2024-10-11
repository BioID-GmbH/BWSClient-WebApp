using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioID.BWS.WebApp.Pages.DeepfakeDetection
{
    public class NewDeepfakeDetectionResultModel : PageModel
    {
        /// <summary>
        /// This error is only set in case an exception occured.
        /// </summary>
        public string ErrorString { get; set; } = string.Empty;
        /// <summary>
        /// Liveness decision.
        /// </summary>
        public bool Live { get; set; }
        /// <summary>
        /// Fake decision.
        /// </summary>
        public bool Fake { get; set; }
        /// <summary>
        /// Calculated liveness score.
        /// </summary>
        public double LivenessScore { get; set; }
        /// <summary>
        /// Errors reported from the liveness detection call.
        /// </summary>
        public List<string> ErrorMessages { get; set; } = [];
        /// <summary>
        /// The complete response formatted as JSON string.
        /// </summary>
        public string ResponseJson { get; set; } = string.Empty;
        /// <summary>
        /// Did we check a video or a image.
        /// </summary>
        public string MediaType { get; set; } = string.Empty;
        /// <summary>
        /// Maybe we want to know the file name later...
        /// </summary>
        public string FileName { get; set; } = string.Empty;
    }
}

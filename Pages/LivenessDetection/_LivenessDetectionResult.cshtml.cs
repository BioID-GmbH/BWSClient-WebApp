using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioID.BWS.WebApp.Pages.LivenessDetection
{
    public class LivenessDetectionResultModel : PageModel
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
        /// Calculated liveness score.
        /// </summary>
        public double LivenessScore { get; set; }
        /// <summary>
        /// Errors reported from the liveness detection call.
        /// </summary>
        public List<string> ErrorMessages { get; set; } = [];
        /// <summary>
        /// Image properties as calculated for the first image.
        /// </summary>
        public string ImageProperties1 { get; set; } = string.Empty;
        /// <summary>
        /// Image properties as calculated for the second image.
        /// </summary>
        public string ImageProperties2 { get; set; } = string.Empty;
    }
}

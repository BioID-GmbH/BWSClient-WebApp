namespace BioID.BWS.WebApp.Pages.LivenessDetection
{
    using Microsoft.AspNetCore.Mvc.RazorPages;

    public class LivenessDetectionResultModel : PageModel
    {
        /// <summary>
        /// This error is only set in case an exception occurred.
        /// </summary>
        public string ErrorString { get; set; } = string.Empty;
        /// <summary>
        /// Was it an active or a passive liveness detection.
        /// </summary>
        public bool Active { get; set; }
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
        /// <summary>
        /// Hint messages generated from the error codes.
        /// </summary>
        public List<string> ResultHints { get; set; } = [];
    }
}

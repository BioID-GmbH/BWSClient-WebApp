using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioID.BWS.WebApp.Pages.LivenessDetection
{
    public class LivenessDetectionResultModel : PageModel
    {
        /// <summary>
        /// This error is only set in case an exception occurred.
        /// </summary>
        public string ErrorString { get; set; } = string.Empty;
        /// <summary>
        /// Liveness decision.
        /// </summary>
        public bool Live { get; set; }
        /// <summary>
        /// Errors reported from the liveness detection calls.
        /// </summary>
        public List<string> ErrorMessages { get; set; } = [];
        /// <summary>
        /// Image properties as calculated for the provided images (in the given order).
        /// </summary>
        public List<string> ImageProperties { get; set; } = [];
    }
}

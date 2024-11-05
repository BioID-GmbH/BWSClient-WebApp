using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioID.BWS.WebApp.Pages.PhotoVerify
{
    public class PhotoVerifyResultModel : PageModel
    {
        /// <summary>
        /// This error is only set in case an exception occured.
        /// </summary>
        public string ErrorString { get; set; } = string.Empty;
        /// <summary>
        /// Verification level.
        /// </summary>
        public int Accuracy { get; set; }
        /// <summary>
        /// Calculated verification score.
        /// </summary>
        public double VerificationScore { get; set; }
        /// <summary>
        /// Liveness decision.
        /// </summary>
        public bool Live { get; set; }
        /// <summary>
        /// Calculated liveness score.
        /// </summary>
        public double LivenessScore { get; set; }
        /// <summary>
        /// Image properties as calculated for the ID photo.
        /// </summary>
        public string PhotoProperties { get; set; } = string.Empty;
        /// <summary>
        /// Image properties as calculated for the first image.
        /// </summary>
        public string ImageProperties1 { get; set; } = string.Empty;
        /// <summary>
        /// Image properties as calculated for the second image.
        /// </summary>
        public string ImageProperties2 { get; set; } = string.Empty;
        /// <summary>
        /// Errors reported from the photo-verify call.
        /// </summary>
        public List<string> ErrorMessages { get; set; } = [];
    }
}

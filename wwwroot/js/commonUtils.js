// The motion detection area height is used as a pixel count constant.
export const MOTION_AREA_HEIGHT = 160;

//Returns motion threshold depending on device type.
export function getMotionThreshold() {
    return isMobileDevice() ? 50 : 25;
}

/**
 * Draw video frame to the drawing canvas, cropping to maintain aspect ratio.
 *
 * @param {HTMLVideoElement} video - Source video element.
 * @param {HTMLCanvasElement} drawingCanvas - Target drawing canvas.
 * @returns {{width: number, height: number, drawingCtx: CanvasRenderingContext2D, cutoff: number}}
 */
export function drawVideoFrame(video, drawingCanvas) {
    const w = drawingCanvas.width;
    const h = drawingCanvas.height;
    const aspectratio = w / h;
    const cutoff = video.videoWidth - (video.videoHeight * aspectratio);
    const drawingCtx = drawingCanvas.getContext('2d');

    // clean drawing canvas
    drawingCtx.clearRect(0, 0, w, h);
    drawingCtx.drawImage(video, cutoff / 2, 0, video.videoWidth - cutoff, video.videoHeight, 0, 0, w, h);
    return { width: w, height: h, drawingCtx, cutoff };
}

/**
 * Prepares the motion detection image data from the drawing canvas.
 *
 * @param {HTMLCanvasElement} drawingCanvas - Source canvas with the current frame.
 * @param {HTMLCanvasElement} motionCanvas - Destination small canvas for motion detection.
 * @returns {ImageData}
 */
export function prepareMotionDetection(drawingCanvas, motionCanvas) {
    const w = drawingCanvas.width;
    const h = drawingCanvas.height;

     // scale current image into the motion canvas
    const motionCtx = motionCanvas.getContext('2d', { willReadFrequently: true });
    motionCtx.drawImage(drawingCanvas, w / 8, h / 8, w - w / 4, h - h / 4, 0, 0, motionCanvas.width, motionCanvas.height);
    return motionCtx.getImageData(0, 0, motionCanvas.width, motionCanvas.height);
}

/**
 * Displays an image blob in a specified HTML image element.
 *
 * @param {Blob} blob - Blob expected to contain an image (type starting with "image/").
 * @param {string} elementId - The ID of the target <img> element.
 */
export function setImageToElement(blob, elementId, title = null) {
    return new Promise((resolve) => {
        if (!blob || !blob.type.startsWith('image/')) {
            resolve(false);
            return;
        }
        let imageElement = document.getElementById(elementId);
        if (!imageElement) {
            resolve(false);
            return;
        }
        if (title !== null) {
            imageElement.title = title;
        }
        const objectUrl = window.URL.createObjectURL(blob);
        imageElement.src = objectUrl;

        imageElement.onload = () => {
            window.URL.revokeObjectURL(objectUrl);
            resolve(true);
        };
        imageElement.onerror = () => {
            window.URL.revokeObjectURL(objectUrl);
            resolve(false);
        };
    });
}

// Resets all image elements with IDs starting with "image" to a default image.
export function resetAllImageViews(src = "/images/bg-placeholder.svg") {
    const allElements = document.querySelectorAll('img[id^="image"]');
    allElements.forEach(imgElement => { imgElement.src = src; });
    allElements.forEach(img => img.title = '');
}

/**
 * Helper function to convert a Canvas to a Blob asynchronously.
 *
 * @param {HTMLCanvasElement} canvas - The canvas to convert to a blob.
 * @returns {Promise<Blob|null>} Resolves with a Blob if successful and of image type, otherwise null.
 */
export function canvasToBlob(canvas) {
    if (!(canvas instanceof HTMLCanvasElement)) {
        throw new Error('The input parameter must be of type HTMLCanvasElement.');
    }
    return new Promise((resolve) => {
        canvas.toBlob((blob) => {
            if (blob && blob.type && blob.type.startsWith('image/')) {
                resolve(blob);
            } else {
                resolve(null);
            }
        });
    });
}

/**
 * Submit the captured images
 *
 * @param {Array<Blob>} imageArray - Array of image blobs to upload.
 * @param {string} formId - The ID of the <form> element to submit to.
 * @returns {Promise<Response>} The fetch response.
 */
export async function sendImages(imageArray, formId) {
    if (!Array.isArray(imageArray)) {
        throw new Error('The first input parameter should be of type array.');
    }
    const form = document.getElementById(formId);
    if (!form) {
        throw new Error(`The form with ID '${formId}' was not found.`);
    }
    const formData = new FormData(form);
    imageArray.forEach((blob, index) => {
        formData.append('Input.ImageFiles', blob, `image${index + 1}.png`);
    });
    const response = await fetch(form.action, {
        method: 'POST',
        body: formData
    });
    return response;
}

/**
 * Initializes canvas elements for video capture.
 * @param {HTMLVideoElement} videoElement - video element
 * @param {MediaStream} mediaStream - media stream
 * @param {HTMLCanvasElement} drawingCanvas - canvas for drawing
 * @param {HTMLCanvasElement} motionCanvas - canvas for motion detection
 * @param {number} motionAreaHeight -  height of the motion detection area
 */
export function setupCanvases(videoElement, mediaStream, drawingCanvas, motionCanvas, motionAreaHeight) {

    // we prefer 3 : 4 face image resolution
    let aspectratio = videoElement.videoWidth / videoElement.videoHeight < 3 / 4 ? videoElement.videoWidth / videoElement.videoHeight : 3 / 4;
    drawingCanvas.height = videoElement.videoHeight;
    drawingCanvas.width = drawingCanvas.height * aspectratio;
    motionCanvas.height = motionAreaHeight;
    motionCanvas.width = motionCanvas.height * aspectratio;
    drawingCanvas.title = `Capturing ${videoElement.videoWidth}x${videoElement.videoHeight}px (cropped to ${drawingCanvas.width}x${drawingCanvas.height}) from ${mediaStream.getVideoTracks()[0].label}.`

    // mirror live preview
    let drawingCtx = drawingCanvas.getContext('2d', { willReadFrequently: true });
    drawingCtx.translate(drawingCanvas.width, 0);
    drawingCtx.scale(-1, 1);
}

/**
* Renders an face overlay with blur background, gradient overlay .
* 
* @param {CanvasRenderingContext2D} ctx - The canvas 2D rendering context
* @param {number} width - Canvas width in pixels
* @param {number} height - Canvas height in pixels  
* @param {HTMLVideoElement} video - The video element capturing webcam feed
* @param {number} cutoff - Horizontal cutoff value for centering video frame
* @returns {void}
*/
export function drawFaceOverlay(ctx, width, height, video, cutoff) {
    // Draw blurred background
    ctx.filter = 'blur(10px)';
    ctx.drawImage(video, cutoff / 2, 0, video.videoWidth - cutoff, video.videoHeight, 0, 0, width, height);
    ctx.filter = 'none';

    // Create and apply gradient overlay
    const gradient = ctx.createLinearGradient(width / 2, 0, width / 2, height);
    gradient.addColorStop(0, '#5f7ab0B3'); // Semi-transparent blue at top
    gradient.addColorStop(1, '#ffffff00'); // Fully transparent at bottom

    // Draw gradient filled area around face circle
    ctx.beginPath();
    ctx.arc(width / 2, height / 2, width * 0.4, 0, 2 * Math.PI);
    ctx.rect(0, 0, width, height);
    ctx.fillStyle = gradient;
    ctx.fill('evenodd');

    // Create circular clipping mask for clear face preview
    ctx.save();
    ctx.beginPath();
    ctx.arc(width / 2, height / 2, width * 0.4, 0, 2 * Math.PI);
    ctx.clip();
    ctx.drawImage(video, cutoff / 2, 0, video.videoWidth - cutoff, video.videoHeight, 0, 0, width, height);
    ctx.restore();

    // Draw white border around face circle
    ctx.lineWidth = 3;
    ctx.strokeStyle = '#fff';
    ctx.stroke();
}
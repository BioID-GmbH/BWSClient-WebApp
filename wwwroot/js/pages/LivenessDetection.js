
import * as uui from '/js/uui.js';
import * as canvasTools from '/js/commonUtils.js';
import * as videoProcessing from '/js/videoCapture.js';

const motionAreaHeight = canvasTools.MOTION_AREA_HEIGHT;
const motionThreshold = canvasTools.getMotionThreshold();

const hint = document.getElementById('headMovementHint');
const uuiCanvas = document.getElementById('uuicanvas');

// Array of required movements (e.g., ["left", "right"]).
let tags;

// Index of the current challenge in the 'tags' array.
let tagIndex = 0;

// Array storing the captured image blobs to be submitted.
let capturedBlobs = [];

// A template for cross correlation used as a reference to calculate motion difference.
let template = null;
let capturing = false;

const video = document.createElement('video');
video.setAttribute('playsinline', '');
const drawingCanvas = document.getElementById('drawingcanvas');
const motionCanvas = document.createElement('canvas');

// liveness detection mode
const Modes = {
    Passive: 0,
    Active: 1,
    ChallengeResponse: 2,
    TwoChallenges: 3,
    ThreeChallenges: 4
}
let selectedMode = Modes.Active;

document.addEventListener("DOMContentLoaded", async () => {
    // Enable popovers
    const popoverTriggerList = document.querySelectorAll('[data-bs-toggle="popover"]')
    const popoverList = [...popoverTriggerList].map(popoverTriggerEl => new bootstrap.Popover(popoverTriggerEl))

    document.getElementById('capture')?.addEventListener('click', capture);
    const radios = document.forms["selectMode"].elements["liveDetectionModes"];
    for (let i = 0, max = radios.length; i < max; i++) {
        radios[i].onclick = selectLiveDetectionMode;
    }

    // Initialize the video stream from the webcam
    const ok = videoProcessing.startVideoAsync(video, initCanvases)
        .then(function (ok) {
            document.getElementById(ok ? 'webapp' : 'skipBiometrics')?.classList.remove("d-none");
            if (ok) {
                drawingCanvas.addEventListener('canvasDrawn', () => { uui.initHead('uuicanvas'); }, { once: true });
            }
        });
});

function selectLiveDetectionMode() {
    let n = 0;
    const heading = document.getElementById("operationHeading");
    heading.innerHTML = "One click &ndash; two selfies";
    uuiCanvas?.classList.remove("d-none");
    hint.classList.remove("d-none");
    switch (this.value) {
        case "passive":
            selectedMode = Modes.Passive;
            n = 1;
            heading.innerHTML = "Take a selfie";
            // disable blue head
            uuiCanvas?.classList.add("d-none");
            hint.classList.add("d-none");
            break;
        case "oneChallenge":
            selectedMode = Modes.ChallengeResponse;
            n = 2;
            break;
        case "twoChallenges":
            selectedMode = Modes.TwoChallenges;
            n = 4;
            heading.innerHTML = "Record four selfies";
            break;
        case "threeChallenges":
            selectedMode = Modes.ThreeChallenges;
            n = 6;
            heading.innerHTML = "Record six selfies";
            break;
        default: // active
            n = 2;
            selectedMode = Modes.Active;
    }
    for (let i = 1; i <= 6; i++) {
        if (i <= n) {
            document.getElementById(`selfie${i}`)?.classList.remove("d-none");
        } else {
            document.getElementById(`selfie${i}`)?.classList.add("d-none");
        }
    }
}

function capture(e) {
    if (e) { e.preventDefault(); }
    // reset vars:
    capturedBlobs = [];
    template = null;
    tags = [];
    tagIndex = 0;
    if (selectedMode > Modes.Active) {
        const challenges = ["up", "right", "down", "left"];
        tags.push(challenges[Math.floor(Math.random() * 4)]);
        if (selectedMode > Modes.ChallengeResponse) { tags.push(challenges[Math.floor(Math.random() * 4)]); }
        if (selectedMode > Modes.TwoChallenges) { tags.push(challenges[Math.floor(Math.random() * 4)]); }
    }
    document.getElementById("challenges").value = tags;
    document.getElementById("livenessDetectionMode").value = selectedMode;
    document.getElementById("expected").value = "none";
    document.getElementById('result-text')?.classList.add("d-none");
    canvasTools.resetAllImageViews();
    document.getElementById('capture').disabled = true;
    const radios = document.forms["selectMode"].elements["liveDetectionModes"];
    for (let i = 0, max = radios.length; i < max; i++) { radios[i].disabled = true; }
    capturing = true;
}

function initCanvases(videoElement, mediaStream) {
    canvasTools.setupCanvases(videoElement, mediaStream, drawingCanvas, motionCanvas, motionAreaHeight);
    // set an interval-timer to grab about 20 frames per second
    setInterval(processFrame, 50);
}

async function processFrame() {
    const { width: w, height: h, drawingCtx, cutoff } = canvasTools.drawVideoFrame(video, drawingCanvas);
    if (capturing) {
        // capture the current image
        const currentImageData = canvasTools.prepareMotionDetection(drawingCanvas, motionCanvas);
        if (template) {
            // second image processing
            const movement = videoProcessing.motionDetection(currentImageData, template);
            if (selectedMode > Modes.Active) {
                uui.animationHead(tags[tagIndex]);
                hint.textContent = "Move your head " + tags[tagIndex];
            } else if (selectedMode > Modes.Passive) {
                uui.constantAnimation();
                hint.textContent = "Nod your head";
            }
            // trigger if movementPercentage is above threshold (default: when 20% of maximum movement is exceeded)
            if (movement > motionThreshold) {
                capturing = false;
                if (selectedMode >= Modes.ChallengeResponse) {
                    drawingCanvas.toBlob(handleChallengeResponse);
                }
                else {
                    drawingCanvas.toBlob(activeLivenessDetection);
                }
            }
        } else {
            // first image processing
            if (selectedMode == Modes.Passive) {
                capturing = false;
                drawingCanvas.toBlob(passiveLivenessDetection);
            } else {
                const blob = await canvasTools.canvasToBlob(drawingCanvas);
                capturedBlobs.push(blob);
                canvasTools.setImageToElement(blob, `image${capturedBlobs.length}`);
                // use as template
                template = videoProcessing.createTemplate(currentImageData);
            }
        }
    }
    canvasTools.drawFaceOverlay(drawingCtx, w, h, video, cutoff);
    // custom drawing event
    drawingCanvas.dispatchEvent(new Event('canvasDrawn'));
}

async function passiveLivenessDetection(blob) {
    capturedBlobs.push(blob);
    canvasTools.setImageToElement(blob, `image${capturedBlobs.length}`);
    document.getElementById('processing')?.classList.remove("d-none");    // post recordings
    const response = await canvasTools.sendImages(capturedBlobs, 'capture-form');

    handleResponse(response);
}

async function activeLivenessDetection(blob) {
    capturedBlobs.push(blob);
    canvasTools.setImageToElement(blob, `image${capturedBlobs.length}`);
    document.getElementById('processing')?.classList.remove("d-none");
    const response = await canvasTools.sendImages(capturedBlobs, 'capture-form');
    await handleResponse(response);
}

async function handleChallengeResponse(blob) {
    capturedBlobs.push(blob);
    canvasTools.setImageToElement(blob, `image${capturedBlobs.length}`);
    tagIndex++;
    if (tagIndex >= tags.length) {
        document.getElementById('processing')?.classList.remove("d-none");
        const response = await canvasTools.sendImages(capturedBlobs, 'capture-form');
        await handleResponse(response);
    } else {
        uui.animationHead('any');
        hint.textContent = "Center your head";
        template = null;
        setTimeout(() => { capturing = true; }, 2000);
    }
}

async function handleResponse(response) {
    if (response.redirected) { window.location.replace(response.url); }
    else if (response.ok) {
        const resultView = document.getElementById("result-view");
        if (resultView) {
            const apiResponse = await response.text();
            resultView.innerHTML = apiResponse;
            capturedBlobs.forEach((blob, index) => {
                canvasTools.setImageToElement(blob, `image${index + 1}`);
                document.getElementById(`selfie${index + 1}`)?.classList.remove("d-none");
            });
            document.getElementById('refuse')?.addEventListener('click', mismatch);
        }
    }
    document.getElementById('processing')?.classList.add("d-none");
    document.getElementById('capture').disabled = false;
    const radios = document.forms["selectMode"].elements["liveDetectionModes"];
    for (let i = 0, max = radios.length; i < max; i++) { radios[i].disabled = false; }
    hint.textContent = "Center your head";
    uui.animationHead();
}

async function mismatch(event) {
    const response = await fetch("/LivenessDetection/Refuse?id=" + event.target.dataset.id + "&result=" + event.target.dataset.result);
    const mismatchdiv = document.getElementById('mismatchdiv');
    if (mismatchdiv) { mismatchdiv.innerHTML = "<strong>Thank you for your help.</strong>"; }
}

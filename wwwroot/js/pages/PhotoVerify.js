import * as canvasTools from '/js/commonUtils.js';
import * as videoProcessing from '/js/videoCapture.js';

const motionAreaHeight = canvasTools.MOTION_AREA_HEIGHT;
const motionThreshold = canvasTools.getMotionThreshold();

const hint = document.getElementById('headMovementHint');

// A template for cross correlation used as a reference to calculate motion difference.
let template = null;
let capturing = false;

const video = document.createElement('video');
video.setAttribute('playsinline', '');
const drawingCanvas = document.getElementById('drawingcanvas');
const motionCanvas = document.createElement('canvas');

let firstCapturedImage, secondCapturedImage, photo;

document.addEventListener("DOMContentLoaded", async () => {

    document.getElementById('select-photo').addEventListener('change', (ev) => { handlePhoto(ev.target.files[0]); });
    let file = document.getElementById('image-file');
    file.addEventListener('dragenter', (ev) => { ev.stopPropagation(); ev.preventDefault(); });
    file.addEventListener('dragover', (ev) => { ev.stopPropagation(); ev.preventDefault(); });
    file.addEventListener('drop', (ev) => {
        ev.stopPropagation();
        ev.preventDefault();
        if (ev.dataTransfer.files) {
            handlePhoto(ev.dataTransfer.files[0]);
        }
    });
    let idphoto = document.getElementById('id-photo');
    idphoto.addEventListener('dragenter', (ev) => { ev.stopPropagation(); ev.preventDefault(); });
    idphoto.addEventListener('dragover', (ev) => { ev.stopPropagation(); ev.preventDefault(); });
    idphoto.addEventListener('drop', (ev) => {
        ev.stopPropagation();
        ev.preventDefault();
        if (ev.dataTransfer.files) {
            handlePhoto(ev.dataTransfer.files[0]);
        }
    });

    document.getElementById('capture').addEventListener('click', capture);
    document.getElementById('execute').addEventListener('click', execute);

    // Initialize the video stream from the webcam
    const ok = videoProcessing.startVideoAsync(video, initCanvases)
        .then(function (ok) {
            document.getElementById(ok ? 'webapp' : 'skipBiometrics')?.classList.remove("d-none");
        });
});

function enableProcess() {
    let missing = !firstCapturedImage || !secondCapturedImage || !photo;
    document.getElementById('execute').disabled = missing;
}

function handlePhoto(blob) {
    console.log('Loading file:', blob);
    photo = blob;
    document.getElementById('image-file').innerText = blob.name;
    canvasTools.setImageToElement(blob, 'id-photo', "")
        .then(success => success && enableProcess());
}

function capture(e) {
    e.preventDefault();
    document.getElementById('capture').disabled = true;
    firstCapturedImage = null;
    secondCapturedImage = null;
    canvasTools.resetAllImageViews();
    enableProcess();
    capturing = true;
}

function initCanvases(videoElement, mediaStream) {
    canvasTools.setupCanvases(videoElement, mediaStream, drawingCanvas, motionCanvas, motionAreaHeight);
    // set an interval-timer to grab about 20 frames per second
    setInterval(processFrame, 50);
}

function processFrame() {
    const { width: w, height: h, drawingCtx, cutoff } = canvasTools.drawVideoFrame(video, drawingCanvas);
    if (capturing) {
        // capture the current image
        const currentImageData = canvasTools.prepareMotionDetection(drawingCanvas, motionCanvas);
        if (template) {
            // second image processing
            const movement = videoProcessing.motionDetection(currentImageData, template);
            // trigger if movementPercentage is above threshold (default: when 20% of maximum movement is exceeded)
            if (movement > motionThreshold) {
                capturing = false;
                drawingCanvas.toBlob(setImage2)
                template = null;
                hint.textContent = "Center your head";
                document.getElementById('captureSpinner').classList.add("d-none");
                document.getElementById('capture').disabled = false;
            }
        } else {
            // use as template
            template = videoProcessing.createTemplate(currentImageData);
            // capture the current image
            drawingCanvas.toBlob(setImage1)
            hint.textContent = "Nod your head";
            document.getElementById('captureSpinner').classList.remove("d-none");
        }
    }
    canvasTools.drawFaceOverlay(drawingCtx, w, h, video, cutoff);
}

function setImage1(blob) {
    firstCapturedImage = blob;
    canvasTools.setImageToElement(blob, 'image1', "")
        .then(success => success && enableProcess());
}

function setImage2(blob) {
    secondCapturedImage = blob;
    canvasTools.setImageToElement(blob, 'image2', "")
        .then(success => success && enableProcess());
}

async function execute() {
    document.getElementById('execute').disabled = true;
    document.getElementById('processSpinner').classList.remove("d-none");
    //document.getElementById('processSpinner').classList.add("d-inline-block");

    const response = await canvasTools.sendImages([photo, firstCapturedImage, secondCapturedImage], 'processForm');
    if (response.redirected) { window.location.replace(response.url); }
    else if (response.ok) {
        const resultView = document.getElementById("result-view");
        if (resultView) {
            const apiResponse = await response.text();
            resultView.innerHTML = apiResponse;
            document.getElementById('id-photo').title = document.getElementById('photoProperties').innerHTML;
            document.getElementById('image1').title = document.getElementById('imageProperties1').innerHTML;
            document.getElementById('image2').title = document.getElementById('imageProperties2').innerHTML;

            let note = document.getElementById('privacynote');
            if (note) { new bootstrap.Popover(note); }
            document.getElementById('mismatch')?.addEventListener('click', mismatch);

            const resultModal = new bootstrap.Modal(document.getElementById('operationResult'));
            resultModal.show();
        }
    }
    document.getElementById('processSpinner').classList.add("d-none");
    enableProcess();
}

async function mismatch(event) {
    const response = await fetch("/PhotoVerify/Refuse?id=" + event.target.dataset.id);
    const mismatchdiv = document.getElementById('mismatchdiv');
    if (mismatchdiv) { mismatchdiv.innerHTML = "<strong>Thank you for your help.</strong>"; }
}

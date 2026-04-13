let video, image;

document.addEventListener("DOMContentLoaded", () => {
    document.getElementById('select-image-video').addEventListener('change', (ev) => { handleImageVideo(ev.target.files[0]); });
    let files = document.getElementById('image-video-file');
    files.addEventListener('dragenter', (ev) => { ev.stopPropagation(); ev.preventDefault(); });
    files.addEventListener('dragover', (ev) => { ev.stopPropagation(); ev.preventDefault(); });
    files.addEventListener('drop', (ev) => {
        ev.stopPropagation();
        ev.preventDefault();
        if (ev.dataTransfer.files) {
            handleImageVideo(ev.dataTransfer.files[0]);
        }
    });

    document.getElementById('processForm').addEventListener('submit', () => {
        document.getElementById('process').disabled = true;
        document.getElementById('processSpinner').classList.remove("d-none");
    });

    document.getElementById('execute').addEventListener('click', execute);
});


function handleImageVideo(blob) {
    if (blob && blob.type.startsWith('video/')) {
        console.log('Loading file:', blob);
        video = blob;
        document.getElementById('image-video-file').innerText = blob.name;
        document.getElementById('execute').disabled = false;
    }
    if (blob && blob.type.startsWith('image/')) {
        console.log('Loading file:', blob);
        image = blob;
        document.getElementById('image-video-file').innerText = blob.name;
        document.getElementById('execute').disabled = false;
    }
}

async function execute() {
    document.getElementById('execute').disabled = true;
    document.getElementById('processSpinner').classList.remove("d-none");
    document.getElementById('result-view').innerHTML = "";

    const form = document.getElementById('processForm');
    let formData = new FormData(form);

    const maxFileSize = 50 * 1024 * 1024; // 50 mb limit
    if (video != null && video.size > maxFileSize || image != null && image.size > maxFileSize) {
        alert("The file is too large.");
        video = null;
        image = null;
        document.getElementById('processSpinner').classList.add("d-none");
        return;
    }

    if (video) {
        var blob = video;
        video = null;
        formData.append('video', blob);
    }
    else {
        var blob = image;
        image = null;
        formData.append('image', blob);
    }

    const response = await fetch(form.action, {
        method: 'POST',
        body: formData
    });
    if (response.redirected) { window.location.replace(response.url); }
    else if (response.ok) {
        const apiResponse = await response.text();
        let resultView = document.getElementById('result-view');
        resultView.innerHTML = apiResponse;
    }
    document.getElementById('processSpinner').classList.add("d-none");
}

import * as THREE from 'three';
import { OBJLoader } from 'three/addons/loaders/OBJLoader.min.js';

let scene, camera, renderer;
let headModel;

let targetRotation = { x: 0, y: 0 };
let currentRotation = { x: 0, y: 0 };
let currentAnimation;

// Maximum vertical rotation angle (radians).
const maxVertical = 0.25;

//  Maximum horizontal rotation angle (radians). 
const maxHorizontal = 0.35;

// Animation speed for the interpolation (0.0 to 1.0).
const animationSpeed = 0.02;

// Direction state for the constant animation ('up' or 'down').
let direction = 'up';

/**
 * Initializes the Three.js environment (Scene, Camera, Renderer, Lights)
 * and loads the OBJ head model.
 */
export function initHead(elementId) {
    const threeCanvas = document.getElementById(elementId);
    if (!threeCanvas) {
        console.error('uuicanvas element not found!');
        return false;
    }
    camera = new THREE.PerspectiveCamera(20, threeCanvas.clientWidth / threeCanvas.clientHeight, 1, 1000);
    camera.position.set(0, 0, 5.5);

    renderer = new THREE.WebGLRenderer({
        canvas: threeCanvas,
        alpha: true,
        antialias: true
    });
    THREE.ColorManagement.enabled = false;
    renderer.setPixelRatio(window.devicePixelRatio);
    renderer.setSize(threeCanvas.clientWidth, threeCanvas.clientHeight);
    renderer.outputColorSpace = THREE.LinearSRGBColorSpace;

    scene = new THREE.Scene();
    let ambientLight = new THREE.AmbientLight(0x5f7ab0, 0.7);
    scene.add(ambientLight);

    let pointLight = new THREE.PointLight(0x5f7ab0, 2.5);
    pointLight.position.set(0, 0, 5);
    pointLight.decay = 0;
    scene.add(pointLight);

    const loader = new OBJLoader();
    loader.load('/models/head.mesh', (object) => {
        object.scale.set(0.8, 0.8, 0.8);

        object.traverse((child) => {
            if (child.isMesh) {
                child.geometry.computeVertexNormals();
                child.material = new THREE.MeshPhongMaterial({
                    color: 0xffffff,
                    flatShading: false,
                    transparent: true,
                    opacity: 0.85,
                    shininess: 10,
                    specular: 0x222222,
                    emissive: 0x112233,
                    emissiveIntensity: 0.3
                });
            }
        });
        scene.add(object);
        headModel = object;
        if (renderer && scene && camera) {
            renderer.render(scene, camera);
        }
    });
    return true;
}

/**
 * Triggers a specific animation for the head model based on the provided tag.
 * It sets the target rotation and starts the interpolation loop.
 * 
 * @param {('up'|'down'|'left'|'right'|'any')} tag - The direction command for the head movement.
 */
export function animationHead(tag) {
    if (headModel) {
        // Determine target rotation based on the input tag
        switch (tag) {
            case 'up':
                targetRotation.x = -maxVertical;
                targetRotation.y = 0;
                break;
            case 'down':
                targetRotation.x = maxVertical;
                targetRotation.y = 0;
                break;
            case 'left':
                targetRotation.x = 0;
                targetRotation.y = -maxHorizontal;
                break;
            case 'right':
                targetRotation.x = 0;
                targetRotation.y = maxHorizontal;
                break;
            default:
                // Reset to center
                targetRotation.x = 0;
                targetRotation.y = 0;
        }
        startHeadRotation();

        if (renderer && camera && scene) {
            renderer.render(scene, camera);
        }
    }
}

/**
 * Executes a single frame of a constant nodding animation (up/down).
 * Note: To run continuously, this needs to be called within a loop or requestAnimationFrame externally.
 */
export function constantAnimation() {
    if (!headModel) return;
    function animate() {
        if (direction === 'up') {
            if (headModel.rotation.x >= -maxVertical) {
                headModel.rotation.x -= animationSpeed;
            }
            else {
                direction = 'down';
            }
        } else if (direction === 'down') {
            if (headModel.rotation.x < maxVertical) {
                headModel.rotation.x += animationSpeed;
            } else {
                direction = 'up';
            }
        }
        renderer.render(scene, camera);
    }
    animate();
}

/**
 * Starts the animation loop to interpolate the head rotation
 * from currentRotation to targetRotation.
 */
function startHeadRotation() {
    if (currentAnimation) {
        cancelAnimationFrame(currentAnimation);
    }
    const animate = () => {
        if (!headModel) return;

        // Calculate the difference between target and current rotation
        const dx = targetRotation.x - currentRotation.x;
        const dy = targetRotation.y - currentRotation.y;

        // Calculate the step size based on animation speed
        const stepX = dx * animationSpeed;
        const stepY = dy * animationSpeed;

        // If the difference is significant (threshold 0.001), update rotation and continue loop
        if (Math.abs(dx) > 0.001 || Math.abs(dy) > 0.001) {
            currentRotation.x += stepX;
            currentRotation.y += stepY;

            headModel.rotation.x = currentRotation.x;
            headModel.rotation.y = currentRotation.y;

            // Request the next frame
            currentAnimation = requestAnimationFrame(animate);
        } else {
            headModel.rotation.x = targetRotation.x;
            headModel.rotation.y = targetRotation.y;
            currentRotation.x = targetRotation.x;
            currentRotation.y = targetRotation.y;
        }
        if (renderer && camera && scene) {
            renderer.render(scene, camera);
        }
    };
    animate();
}

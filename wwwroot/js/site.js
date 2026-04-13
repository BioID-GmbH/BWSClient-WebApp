// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(() => {
    'use strict'
    const scrollToTopBtn = document.getElementById("toTopBtn");
    if (scrollToTopBtn) {
        document.addEventListener("DOMContentLoaded", () => {
            document.addEventListener("scroll", handleScroll);
            scrollToTopBtn.addEventListener("click", scrollToTop);
        });
    }
    function handleScroll() {
        if (document.documentElement.scrollTop > 200) {
            scrollToTopBtn.classList.add("showBtn");
        } else {
            scrollToTopBtn.classList.remove("showBtn");
        }
    }
    function scrollToTop() {
        document.documentElement.scrollTo({
            top: 0,
            behavior: "smooth"
        });
    }
})()

// Check for mobile device (i.e. has touch-screen)
// see https://developer.mozilla.org/en-US/docs/Web/HTTP/Browser_detection_using_the_user_agent
function isMobileDevice() {
    let hasTouchScreen = false;
    if ("maxTouchPoints" in navigator) {
        hasTouchScreen = navigator.maxTouchPoints > 0;
    } else if ("msMaxTouchPoints" in navigator) {
        hasTouchScreen = navigator.msMaxTouchPoints > 0;
    } else {
        let mQ = window.matchMedia && matchMedia("(pointer:coarse)");
        if (mQ && mQ.media === "(pointer:coarse)") {
            hasTouchScreen = !!mQ.matches;
        } else if ('orientation' in window) {
            hasTouchScreen = true; // deprecated, but good fallback
        } else {
            // Only as a last resort, fall back to user agent sniffing
            let UA = navigator.userAgent;
            hasTouchScreen = (
                /\b(BlackBerry|webOS|iPhone|IEMobile)\b/i.test(UA) ||
                /\b(Android|Windows Phone|iPad|iPod)\b/i.test(UA)
            );
        }
    }
    return hasTouchScreen;
}

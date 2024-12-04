function checkPlayState() {
    const playButton = document.querySelector('ytmusic-player-bar tp-yt-paper-icon-button[aria-label="Play"], ytmusic-player-bar tp-yt-paper-icon-button[aria-label="Pause"]');

    if (playButton) {
        const state = playButton.getAttribute('aria-label');
        window.chrome.webview.postMessage({
            type: 'PlayStateChanged',
            isPlaying: state === 'Pause'
        });
    } else {
        // Log all buttons to help debug
        const allButtons = document.querySelectorAll('ytmusic-player-bar tp-yt-paper-icon-button');
        console.log("All player bar buttons:", allButtons);
    }
}

// Initial check
setTimeout(checkPlayState, 500); // Give the player time to load

const observer = new MutationObserver((mutations) => {
    console.log("Mutation observed");
    checkPlayState();
});

// Initialize observer after a delay to ensure player is loaded
setTimeout(() => {
    const playButton = document.querySelector('ytmusic-player-bar tp-yt-paper-icon-button[aria-label="Play"], ytmusic-player-bar tp-yt-paper-icon-button[aria-label="Pause"]');
    if (playButton) {
        observer.observe(playButton, {
            attributes: true,
            attributeFilter: ['aria-label']
        });
    } else {
        console.log("Could not set up observer - button not found");
    }
}, 500);
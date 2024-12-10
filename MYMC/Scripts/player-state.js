function checkPlayState() {
    const playButton = document.querySelector('ytmusic-player-bar tp-yt-paper-icon-button[aria-label="Play"], ytmusic-player-bar tp-yt-paper-icon-button[aria-label="Pause"]');

    if (playButton) {
        const state = playButton.getAttribute('aria-label');
        window.chrome.webview.postMessage({
            type: 'PlayStateChanged',
            isPlaying: state === 'Pause'
        });
    }
}

function checkSongInfo() {
    try {
        const titleElement = document.querySelector('ytmusic-player-bar .title');
        const bylineElement = document.querySelector('ytmusic-player-bar .byline');
        const albumArtElement = document.querySelector('ytmusic-player-bar img.image');

        const title = titleElement ? titleElement.textContent.trim() : "Unknown";
        let artist = "Unknown";
        let album = "Unknown";
        let year = "Unknown";

        if (bylineElement) {
            const spans = bylineElement.querySelectorAll('span.style-scope.yt-formatted-string');
            const links = bylineElement.querySelectorAll('a');
            if (links.length > 0) {
                artist = links[0].textContent.trim();
            }
            if (links.length > 1) {
                album = links[1].textContent.trim();
            }
            for (const span of spans) {
                const text = span.textContent.trim();
                if (/^\d{4}$/.test(text)) {
                    year = text;
                    break;
                }
            }
        }

        const albumArtUrl = albumArtElement ? albumArtElement.src : "";

        window.chrome.webview.postMessage({
            type: 'TrackInfoChanged',
            trackInfo: {
                title: title,
                album: album,
                artist: artist,
                year: year,
                albumArtUrl: albumArtUrl
            }
        });
    } catch (e) {
        console.error("Error getting song info:", e);
    }
}

function checkTimeInfo() {
    try {
        const progressBar = document.querySelector('ytmusic-player-bar #progress-bar');
        if (!progressBar) return;

        const progress = progressBar.getAttribute('aria-valuenow');
        const total = progressBar.getAttribute('aria-valuemax');

        window.chrome.webview.postMessage({
            type: 'TimeInfoChanged',
            timeInfo: {
                progress: parseFloat(progress) || 0,
                total: parseFloat(total) || 0
            }
        });
    } catch (e) {
        console.error("Error getting time info:", e);
    }
}

function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

const debouncedCheckSongInfo = debounce(checkSongInfo, 250);
const debouncedCheckPlayState = debounce(checkPlayState, 250);
const debouncedCheckTimeInfo = debounce(checkTimeInfo, 100);

function isRelevantMutation(mutation) {
    try {
        if (!mutation.target) return false;

        if (mutation.type === 'attributes' &&
            mutation.attributeName === 'aria-label') {
            const label = mutation.target.getAttribute('aria-label');
            if (label === 'Play' || label === 'Pause') {
                return 'playState';
            }
        }

        if (mutation.target.nodeType === Node.ELEMENT_NODE) {
            const relevantSelectors = ['.title', '.byline', 'img.image'];
            for (const selector of relevantSelectors) {
                if (mutation.target.matches && mutation.target.matches(selector) ||
                    mutation.target.closest && mutation.target.closest(selector)) {
                    return 'songInfo';
                }
            }
        }

        // Now only checking progress-bar for time updates
        if (mutation.target.id === 'progress-bar') {
            return 'timeInfo';
        }

        return false;
    } catch (e) {
        console.error("Error in isRelevantMutation:", e);
        return false;
    }
}

const observer = new MutationObserver((mutations) => {
    let shouldCheckPlay = false;
    let shouldCheckSong = false;
    let shouldCheckTime = false;

    for (const mutation of mutations) {
        const mutationType = isRelevantMutation(mutation);
        switch (mutationType) {
            case 'playState':
                shouldCheckPlay = true;
                break;
            case 'songInfo':
                shouldCheckSong = true;
                break;
            case 'timeInfo':
                shouldCheckTime = true;
                break;
        }
    }

    if (shouldCheckPlay) debouncedCheckPlayState();
    if (shouldCheckSong) debouncedCheckSongInfo();
    if (shouldCheckTime) debouncedCheckTimeInfo();
});

function initializeObserver(retryCount = 0, maxRetries = 5) {
    const playerBar = document.querySelector('ytmusic-player-bar');

    if (playerBar && playerBar.querySelector('.title')) {  // Check for title to ensure component is hydrated
        observer.observe(playerBar, {
            attributes: true,
            subtree: true,
            childList: true,
            characterData: true
        });
        console.log("Observer initialized successfully");

        // Do initial checks after successful initialization
        checkPlayState();
        checkSongInfo();
        checkTimeInfo();
    } else if (retryCount < maxRetries) {
        console.log(`Player bar not ready, retrying... (${retryCount + 1}/${maxRetries})`);
        setTimeout(() => initializeObserver(retryCount + 1, maxRetries), 1000);
    } else {
        console.error("Failed to initialize observer after max retries");
    }
}

// Start initialization
initializeObserver();
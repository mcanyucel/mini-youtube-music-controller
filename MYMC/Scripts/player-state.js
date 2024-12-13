// noinspection JSUnresolvedReference - chrome is injected by the host window

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

function checkLikedState() {
    const likeButton = document.querySelector('yt-button-shape[aria-label="Like"]');
    
    if (likeButton) {
        const state = likeButton.getAttribute('aria-pressed');
        window.chrome.webview.postMessage({
            type: 'LikeStateChanged',
            isLiked: state === 'true'
        });
    }
    else {
        console.error("Like button not found");
    }
}

function checkRepeatState() {
    const repeatButton = document.querySelector('ytmusic-player-bar tp-yt-paper-icon-button[class="repeat style-scope ytmusic-player-bar"]');
    if (repeatButton) {
        const repeatMode = repeatButton.getAttribute('aria-label');
        window.chrome.webview.postMessage({
            type: 'RepeatModeChanged',
            mode: repeatMode
        });
    }
    else {
        console.error("Repeat button not found");
    }
}

function checkShuffleState() {
    const playerBar = document.querySelector('ytmusic-player-bar');
    if (playerBar) {
        // the shuffle button does not have any state information, but the bar has shuffle-on attribute when it is on
        const shuffleOn = playerBar.hasAttribute('shuffle-on');
        window.chrome.webview.postMessage({
            type: 'ShuffleStateChanged',
            isShuffleOn: shuffleOn
        });
    } 
    else {
        console.error("Player bar not found");
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

function checkVolume() {
        const volumeSlider = document.querySelector('ytmusic-player-bar #sliderBar');
        if (volumeSlider) {

            const volume = volumeSlider.getAttribute('aria-valuenow');
            window.chrome.webview.postMessage({
                type: 'VolumeChanged',
                volume: parseInt(volume) || 0
            });
        }
        else {
            console.error("Volume slider not found");
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
const debouncedCheckRepeatState = debounce(checkRepeatState, 250);
const debouncedCheckShuffleState = debounce(checkShuffleState, 250);
const debouncedCheckVolume = debounce(checkVolume, 250);
const debouncedCheckLikedState = debounce(checkLikedState, 250);

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
        
        if (mutation.type === 'attributes' &&
            mutation.attributeName === 'aria-pressed') {
            return 'likedState';
        }
        
        if (mutation.type === 'attributes' &&
        mutation.attributeName === 'aria-label') {
            const label = mutation.target.getAttribute('aria-label');
            if (label === 'Repeat off' || label === 'Repeat all' || label === 'Repeat one') {
                return 'repeatState';
            }
        }
        
        if (mutation.type === 'attributes' &&
        mutation.attributeName === 'shuffle-on') {
            return 'shuffleState';
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
        
        if (mutation.target.id === 'sliderBar') {
            return 'volume';
        }
        
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
    let shouldCheckRepeat = false;
    let shouldCheckShuffle = false;
    let shouldCheckVolume = false;
    let shouldCheckLiked = false;

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
            case 'repeatState':
                shouldCheckRepeat = true;
                break;
            case 'shuffleState':
                shouldCheckShuffle = true;
                break;
            case 'volume':
                shouldCheckVolume = true;
                break;
            case 'likedState':
                shouldCheckLiked = true;
                break;
        }
    }

    if (shouldCheckPlay) debouncedCheckPlayState();
    if (shouldCheckSong) debouncedCheckSongInfo();
    if (shouldCheckTime) debouncedCheckTimeInfo();
    if (shouldCheckRepeat) debouncedCheckRepeatState();
    if (shouldCheckShuffle) debouncedCheckShuffleState();
    if (shouldCheckVolume) debouncedCheckVolume();
    if (shouldCheckLiked) debouncedCheckLikedState();
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
        checkRepeatState();
        checkShuffleState();
        checkVolume();
        checkLikedState();
    } else if (retryCount < maxRetries) {
        console.log(`Player bar not ready, retrying... (${retryCount + 1}/${maxRetries})`);
        setTimeout(() => initializeObserver(retryCount + 1, maxRetries), 1000);
    } else {
        console.error("Failed to initialize observer after max retries");
    }
}

// Start initialization
initializeObserver();

// noinspection JSUnusedGlobalSymbols - Called from the host window
function seekTo(time) {
    const progressBar = document.querySelector('ytmusic-player-bar #progress-bar');
    if (progressBar) {
        progressBar.value = time;
        progressBar.dispatchEvent(new Event('change', { bubbles: true }));
        progressBar.dispatchEvent(new Event('immediate-value-change', { bubbles: true }));
    }
}

// noinspection JSUnusedGlobalSymbols - Called from the host window
function setVolume(value) {
    const volumeSlider = document.querySelector('ytmusic-player-bar #sliderBar');
    if (volumeSlider) {
        volumeSlider.value = value;
        volumeSlider.dispatchEvent(new Event('change', { bubbles: true }));
        volumeSlider.dispatchEvent(new Event('immediate-value-change', { bubbles: true }));
    }
}

async function getShareUrl() {
    const playerBar = document.querySelector('ytmusic-player-bar');
    if (playerBar) {
        // Create and dispatch a context menu event (right-click)
        const contextMenuEvent = new MouseEvent('contextmenu', {
            bubbles: true,
            cancelable: true,
            view: window,
            button: 2,
            buttons: 2
        });
        playerBar.dispatchEvent(contextMenuEvent);

        // Wait for menu to open
        await new Promise(resolve => setTimeout(resolve, 250));
    }
    else {
        console.error("Player bar not found");
        return;
    }

    // Look for all menu items and find the one with "Share" text
    const menuItems = document.querySelectorAll('ytmusic-menu-navigation-item-renderer');
    const shareItem = Array.from(menuItems).find(item => {
        const textElement = item.querySelector('yt-formatted-string.text');
        return textElement && textElement.textContent.trim() === 'Share';
    });

    if (shareItem) {
        const shareLink = shareItem.querySelector('a.yt-simple-endpoint');
        if (shareLink) {
            shareLink.click();
            // Wait for share dialog to appear
            await new Promise(resolve => setTimeout(resolve, 500));

            // Get the URL from the input field
            const urlInput = document.querySelector('#share-url');
            if (urlInput) {
                const shareUrl = urlInput.value;
                // Post message back to the app
                window.chrome.webview.postMessage({
                    type: 'ShareUrlCopied',
                    url: shareUrl
                });

                // Click the copy button
                const copyButton = document.querySelector('button.yt-spec-button-shape-next--call-to-action');
                if (copyButton) {
                    copyButton.click();

                    // Move focus away from the input before closing
                    const dialog = document.querySelector('tp-yt-paper-dialog');
                    if (dialog) {
                        // Focus the dialog itself (which is a valid focusable element due to tabindex="-1")
                        dialog.focus();
                    }

                    // Close dialog
                    const closeButton = document.querySelector('tp-yt-paper-dialog .close-icon');
                    if (closeButton) {
                        closeButton.click();
                    }
                }
                else {
                    console.error("Copy button not found");
                }
            }
            else {
                console.error("URL input not found");
            }
        }
        else {
            console.error("Share link element not found");
        }
    }
    else {
        console.error("Share menu item not found");
    }
}
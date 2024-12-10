let videoElement;

let playIcon, pauseIcon;

document.addEventListener('DOMContentLoaded', function() {
    videoElement = document.getElementById('mainVideo');
    playIcon = document.querySelector('.play-icon');
    pauseIcon = document.querySelector('.pause-icon');
    const playButton = document.querySelector('.play-button');
    const fullscreenButton = document.querySelector('.fullscreen-button');
    const progressBar = document.querySelector('.progress-bar');
    const progressFilled = document.querySelector('.progress-filled');
    const currentTimeDisplay = document.querySelector('.current-time');
    const totalTimeDisplay = document.querySelector('.total-time');
    const videoControls = document.querySelector('.video-controls');

    // Play/pause function
    playButton.addEventListener('click', togglePlay);
    videoElement.addEventListener('click', togglePlay);

    function togglePlay() {
        if (videoElement.paused) {
            videoElement.play();
            playIcon.style.display = 'none';
            pauseIcon.style.display = 'block';
        } else {
            videoElement.pause();
            playIcon.style.display = 'block';
            pauseIcon.style.display = 'none';
        }
    }

    // Full screen function
    fullscreenButton.addEventListener('click', () => {
        if (!document.fullscreenElement) {
            videoElement.requestFullscreen().catch(err => {
                console.log(`Error attempting to enable fullscreen: ${err.message}`);
            });
        } else {
            document.exitFullscreen();
        }
    });

    // Progress bar function
    videoElement.addEventListener('timeupdate', () => {
        const percent = (videoElement.currentTime / videoElement.duration) * 100;
        progressFilled.style.width = `${percent}%`;
        currentTimeDisplay.textContent = formatTime(videoElement.currentTime);
    });

    videoElement.addEventListener('loadedmetadata', () => {
        totalTimeDisplay.textContent = formatTime(videoElement.duration);
    });

    progressBar.addEventListener('click', (e) => {
        const progressTime = (e.offsetX / progressBar.offsetWidth) * videoElement.duration;
        videoElement.currentTime = progressTime;
    });

    // Format time display
    function formatTime(seconds) {
        const minutes = Math.floor(seconds / 60);
        seconds = Math.floor(seconds % 60);
        return `${minutes}:${seconds.toString().padStart(2, '0')}`;
    }

    // Automatically hide control bar
    let controlsTimeout;
    const videoWrapper = document.querySelector('.video-wrapper');

    function showControls() {
        videoControls.style.opacity = '1';
        clearTimeout(controlsTimeout);
        controlsTimeout = setTimeout(() => {
            if (!videoElement.paused) {
                videoControls.style.opacity = '0';
            }
        }, 3000);
    }

    videoWrapper.addEventListener('mousemove', showControls);
    videoWrapper.addEventListener('mouseleave', () => {
        if (!videoElement.paused) {
            videoControls.style.opacity = '0';
        }
    });
});

// Modify getValue function
window.getValue = () => {
    if (videoElement && playIcon && pauseIcon) {
        videoElement.pause();
        playIcon.style.display = 'block';
        pauseIcon.style.display = 'none';
        return `video${videoElement.currentTime}`;
    }
    return null;
};


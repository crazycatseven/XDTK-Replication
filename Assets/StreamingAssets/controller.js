document.addEventListener('DOMContentLoaded', function() {
    const gestureArea = document.querySelector('.gesture-area');
    const gestureType = document.getElementById('gestureType');
    const gestureDistance = document.getElementById('gestureDistance');
    const gestureScale = document.getElementById('gestureScale');
    const gestureDirection = document.getElementById('gestureDirection');

    let initialTouches = [];
    let currentTouches = [];
    let initialDistance = 0;
    let isGesturing = false;

    // Prevent default touch behavior
    gestureArea.addEventListener('touchstart', (e) => {
        e.preventDefault();
        initialTouches = [...e.touches];
        currentTouches = [...e.touches];
        isGesturing = true;
        gestureArea.classList.add('active');

        if (e.touches.length === 2) {
            initialDistance = getTouchDistance(e.touches[0], e.touches[1]);
            gestureType.textContent = 'Two Fingers';
            gestureScale.textContent = '1.0';
        } else if (e.touches.length === 1) {
            gestureType.textContent = 'One Finger';
        }
    });

    gestureArea.addEventListener('touchmove', (e) => {
        e.preventDefault();
        if (!isGesturing) return;

        currentTouches = [...e.touches];

        if (e.touches.length === 2) {
            // Two finger gesture
            const currentDistance = getTouchDistance(e.touches[0], e.touches[1]);
            const scale = currentDistance / initialDistance;
            
            // Calculate movement direction
            const initialCenter = getTouchCenter(initialTouches[0], initialTouches[1]);
            const currentCenter = getTouchCenter(e.touches[0], e.touches[1]);
            const direction = getDirection(initialCenter, currentCenter);

            gestureScale.textContent = scale.toFixed(2);
            gestureDistance.textContent = Math.round(currentDistance) + 'px';
            gestureDirection.textContent = direction;

            // Send message to Unity
            const message = {
                type: 'two_finger_gesture',
                scale: scale,
                direction: direction,
                distance: currentDistance
            };
            window.vuplex.postMessage(JSON.stringify(message));
        } else if (e.touches.length === 1) {
            // Single finger swipe
            const touch = e.touches[0];
            const initialTouch = initialTouches[0];
            const direction = getDirection(initialTouch, touch);
            const distance = getTouchDistance(initialTouch, touch);

            gestureDistance.textContent = Math.round(distance) + 'px';
            gestureDirection.textContent = direction;

            // Send message to Unity
            const message = {
                type: 'one_finger_gesture',
                direction: direction,
                distance: distance
            };
            window.vuplex.postMessage(JSON.stringify(message));
        }
    });

    gestureArea.addEventListener('touchend', (e) => {
        e.preventDefault();
        if (e.touches.length === 0) {
            resetGestureInfo();
        } else {
            initialTouches = [...e.touches];
            currentTouches = [...e.touches];
            if (e.touches.length === 1) {
                gestureType.textContent = 'One Finger';
                gestureScale.textContent = '1.0';
            }
        }
    });

    gestureArea.addEventListener('touchcancel', (e) => {
        e.preventDefault();
        resetGestureInfo();
    });

    function resetGestureInfo() {
        isGesturing = false;
        initialTouches = [];
        currentTouches = [];
        initialDistance = 0;
        gestureArea.classList.remove('active');
        gestureType.textContent = 'None';
        gestureDistance.textContent = '0';
        gestureScale.textContent = '1.0';
        gestureDirection.textContent = 'None';
    }

    // Calculate distance between two points
    function getTouchDistance(touch1, touch2) {
        const dx = touch1.clientX - touch2.clientX;
        const dy = touch1.clientY - touch2.clientY;
        return Math.sqrt(dx * dx + dy * dy);
    }

    // Calculate the center point of two points
    function getTouchCenter(touch1, touch2) {
        return {
            x: (touch1.clientX + touch2.clientX) / 2,
            y: (touch1.clientY + touch2.clientY) / 2
        };
    }

    // Get movement direction
    function getDirection(start, end) {
        const dx = end.clientX - start.clientX;
        const dy = end.clientY - start.clientY;
        const absDx = Math.abs(dx);
        const absDy = Math.abs(dy);

        if (absDx < 10 && absDy < 10) return 'None';

        if (absDx > absDy) {
            return dx > 0 ? 'Right' : 'Left';
        } else {
            return dy > 0 ? 'Down' : 'Up';
        }
    }
});

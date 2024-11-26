class GestureDetector {
    constructor() {
        // 初始化状态
        this.initialPinchDistance = 0;
        this.isPinching = false;
        this.currentSlideIndex = 0;

        // 绑定方法
        this.handleTouchStart = this.handleTouchStart.bind(this);
        this.handleTouchMove = this.handleTouchMove.bind(this);
        this.handleTouchEnd = this.handleTouchEnd.bind(this);

        // 添加事件监听
        document.addEventListener('touchstart', this.handleTouchStart, false);
        document.addEventListener('touchmove', this.handleTouchMove, false);
        document.addEventListener('touchend', this.handleTouchEnd, false);
    }

    updateSelectedSlide(index) {
        this.currentSlideIndex = index;
        console.log('Selected slide updated:', index);
    }

    getSelectedLibraryItem() {
        const slides = document.querySelectorAll('.library-slide');
        if (this.currentSlideIndex >= 0 && this.currentSlideIndex < slides.length) {
            const id = slides[this.currentSlideIndex].dataset.id;
            console.log('Selected item ID:', id);
            return id;
        }
        return null;
    }

    getTouchDistance(touch1, touch2) {
        const dx = touch1.clientX - touch2.clientX;
        const dy = touch1.clientY - touch2.clientY;
        return Math.sqrt(dx * dx + dy * dy);
    }

    createPinchMessage(type, touches = null, scale = null) {
        const message = {
            type: type,
            url: window.location.href,
            value: this.getSelectedLibraryItem()
        };

        if (touches) {
            message.touch1 = { x: touches[0].clientX, y: touches[0].clientY };
            message.touch2 = { x: touches[1].clientX, y: touches[1].clientY };
        }

        if (scale !== null) {
            message.scale = scale;
        }

        console.log('Creating pinch message:', message);
        return message;
    }

    handleTouchStart(event) {
        if (event.touches.length === 2) {
            this.isPinching = true;
            this.initialPinchDistance = this.getTouchDistance(event.touches[0], event.touches[1]);
            window.vuplex?.postMessage(this.createPinchMessage('PinchStart', [event.touches[0], event.touches[1]]));
        }
    }

    handleTouchMove(event) {
        if (this.isPinching && event.touches.length === 2) {
            const currentDistance = this.getTouchDistance(event.touches[0], event.touches[1]);
            const scale = currentDistance / this.initialPinchDistance;
            window.vuplex?.postMessage(this.createPinchMessage('PinchUpdate', [event.touches[0], event.touches[1]], scale));
        }
    }

    handleTouchEnd(event) {
        if (this.isPinching) {
            this.isPinching = false;
            window.vuplex?.postMessage(this.createPinchMessage('PinchEnd'));
        }
    }
}
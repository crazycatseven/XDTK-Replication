// Library page specific functionality
class LibraryPageManager {
    constructor() {
        this.currentSlideIndex = 0;
        this.wrapper = document.querySelector('.library-wrapper');
        this.slides = document.querySelectorAll('.library-slide');
        
        // Touch and drag related variables
        this.startX = 0;
        this.currentX = 0;
        this.isDragging = false;

        this.initializeSlider();
        this.initializeEventListeners();
    }

    initializeSlider() {
        // Create dots for the slider
        const dotsContainer = document.querySelector('.dots-container');
        
        this.slides.forEach((_, index) => {
            const dot = document.createElement('div');
            dot.classList.add('dot');
            if (index === 0) dot.classList.add('active');
            dot.addEventListener('click', () => this.goToSlide(index));
            dotsContainer.appendChild(dot);
        });

        // Initialize first slide
        this.goToSlide(0);
    }

    initializeEventListeners() {
        // Bind methods
        this.startDrag = this.startDrag.bind(this);
        this.dragMove = this.dragMove.bind(this);
        this.endDrag = this.endDrag.bind(this);

        // Touch events
        this.wrapper.addEventListener('touchstart', this.startDrag);
        this.wrapper.addEventListener('touchmove', this.dragMove);
        this.wrapper.addEventListener('touchend', this.endDrag);

        // Mouse events for desktop simulation
        this.wrapper.addEventListener('mousedown', this.startDrag);
        this.wrapper.addEventListener('mousemove', this.dragMove);
        this.wrapper.addEventListener('mouseup', this.endDrag);
        this.wrapper.addEventListener('mouseleave', this.endDrag);
    }

    startDrag(e) {
        if (e.touches && e.touches.length >= 2) return; // Ignore multi-touch
        
        this.isDragging = true;
        this.startX = e.type.includes('mouse') ? e.clientX : e.touches[0].clientX;
        this.currentX = this.startX;
        this.wrapper.style.transition = 'none';
    }

    dragMove(e) {
        if (!this.isDragging || (e.touches && e.touches.length >= 2)) return;
        
        const clientX = e.type.includes('mouse') ? e.clientX : e.touches[0].clientX;
        this.currentX = clientX;
        const diff = clientX - this.startX;

        // Calculate transform with boundary limits
        let currentTransform = -this.currentSlideIndex * 100;
        let newTransform;

        if (this.currentSlideIndex === 0 && diff > 0) {
            // First slide, dragging right
            newTransform = currentTransform + (diff / this.wrapper.offsetWidth * 100 * 0.3);
        } else if (this.currentSlideIndex === this.slides.length - 1 && diff < 0) {
            // Last slide, dragging left
            newTransform = currentTransform + (diff / this.wrapper.offsetWidth * 100 * 0.3);
        } else {
            // Normal case
            newTransform = currentTransform + (diff / this.wrapper.offsetWidth * 100);
        }

        this.wrapper.style.transform = `translateX(${newTransform}%)`;
    }

    endDrag(e) {
        if (!this.isDragging) return;
        this.isDragging = false;
        this.wrapper.style.transition = 'transform 0.3s ease';
        
        const diff = this.currentX - this.startX;
        const threshold = this.wrapper.offsetWidth / 10;

        // Check boundaries
        if ((this.currentSlideIndex === 0 && diff > 0) || 
            (this.currentSlideIndex === this.slides.length - 1 && diff < 0)) {
            // At boundary, snap back
            this.goToSlide(this.currentSlideIndex);
            return;
        }

        // Normal slide logic
        if (Math.abs(diff) > threshold) {
            if (diff > 0 && this.currentSlideIndex > 0) {
                this.currentSlideIndex--;
            } else if (diff < 0 && this.currentSlideIndex < this.slides.length - 1) {
                this.currentSlideIndex++;
            }
        }

        this.goToSlide(this.currentSlideIndex);
    }

    updateSelectedSlide(index) {
        this.currentSlideIndex = index;
        console.log('Selected slide updated:', index);
    }

    goToSlide(index) {
        this.currentSlideIndex = index;
        this.wrapper.style.transform = `translateX(-${index * 100}%)`;
        this.updateDots();
        // Update active slide
        this.slides.forEach((slide, i) => {
            slide.classList.toggle('active', i === index);
        });
    }

    updateDots() {
        document.querySelectorAll('.dot').forEach((dot, index) => {
            dot.classList.toggle('active', index === this.currentSlideIndex);
        });
    }

    // Value provider for pinch gesture
    getPinchValue() {
        const currentSlide = document.querySelector('.library-slide.active');
        if (currentSlide) {
            return {
                type: currentSlide.dataset.type || 'model',
                id: currentSlide.dataset.id
            };
        }
        return null;
    }
}

// Initialize when page loads
document.addEventListener('DOMContentLoaded', () => {
    const libraryManager = new LibraryPageManager();
    window.libraryManager = libraryManager;
});

// Expose directly to the global scope
window.getValue = () => {
    const currentSlide = document.querySelector('.library-slide.active');
    if (currentSlide) {
        return currentSlide.dataset.id;
    }
    return null;
};

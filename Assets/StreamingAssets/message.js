document.addEventListener('DOMContentLoaded', function() {
    // Preview functionality
    const previewModal = document.querySelector('.preview-modal');
    const modalImage = document.querySelector('.modal-image');
    const modalFilename = document.querySelector('.modal-filename');
    const modalFilesize = document.querySelector('.modal-filesize');

    window.openPreview = function(fileElement) {
        const previewImg = fileElement.querySelector('.file-preview');
        const filename = fileElement.querySelector('.file-name').textContent;
        const filesize = fileElement.querySelector('.file-size').textContent;
        
        modalImage.src = previewImg.src;
        modalFilename.textContent = filename;
        modalFilesize.textContent = filesize;
        previewModal.classList.add('active');
    };

    window.closePreview = function(event) {
        if (event.target === previewModal) {
            previewModal.classList.remove('active');
        }
    };

    // Add message animation
    const messages = document.querySelectorAll('.message-item');
    
    function showMessage(index) {
        if (index >= messages.length) return;
        
        setTimeout(() => {
            messages[index].classList.remove('hidden');
            showMessage(index + 1);
        }, 1000); // 1 second delay for each message
    }

    // Start showing messages after a short delay
    setTimeout(() => {
        showMessage(0);
    }, 500);
});


window.getValue = () => {
    return "bear";
};


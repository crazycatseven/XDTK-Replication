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
});


window.getValue = () => {
    return "bear";
};


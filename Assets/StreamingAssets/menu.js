document.addEventListener('DOMContentLoaded', function() {
    // Category switching
    const categories = document.querySelectorAll('.category-item');
    
    categories.forEach(category => {
        category.addEventListener('click', function() {
            // Remove active class from all categories
            categories.forEach(c => c.classList.remove('active'));
            // Add active class to clicked category
            this.classList.add('active');
        });
    });

    // Favorite button functionality
    const favoriteButtons = document.querySelectorAll('.favorite-btn');
    
    favoriteButtons.forEach(button => {
        button.addEventListener('click', function() {
            this.classList.toggle('active');
            const img = this.querySelector('img');
            if (this.classList.contains('active')) {
                img.src = './images/heart-filled.png';
            } else {
                img.src = './images/heart.png';
            }
        });
    });
}); 
document.addEventListener('DOMContentLoaded', function() {
    // Network connection
    const ipInput = document.getElementById('ipInput');
    const portInput = document.getElementById('portInput');
    const connectBtn = document.getElementById('networkConnectBtn');
    const crosshairAlignBtn = document.getElementById('crosshairAlignBtn');

    connectBtn.addEventListener('click', function() {
        const message = {
            type: 'NetworkConnect',
            ip: ipInput.value,
            port: portInput.value
        };
        window.vuplex.postMessage(JSON.stringify(message));
        
    });

    crosshairAlignBtn.addEventListener('click', function() {
        const message = {
            type: 'CrosshairAlign'
        };
        window.vuplex.postMessage(JSON.stringify(message));
        
    });

    // Card toggle
    const toggleBtn = document.querySelector('.toggle-btn');
    const cardContent = document.querySelector('.card-content');

    toggleBtn.addEventListener('click', function() {
        this.textContent = this.textContent === '▼' ? '▲' : '▼';
        this.classList.toggle('collapsed');
        cardContent.classList.toggle('collapsed');
    });
});


document.addEventListener('DOMContentLoaded', () => {
    // 注册自定义值提供器（针对图片库页面）
    window._vuplexValueProvider = function() {
        const slides = document.querySelectorAll('.library-slide');
        const currentSlide = document.querySelector('.library-slide.active');
        if (currentSlide) {
            return currentSlide.dataset.id;
        }
        return null;
    };

    // Page switching functionality
    window.switchPage = function(pageNum) {
        document.querySelectorAll('.page').forEach(page => page.classList.remove('active'));
        document.querySelectorAll('.nav-item').forEach(item => item.classList.remove('active'));
        
        document.getElementById('page' + pageNum).classList.add('active');
        document.querySelectorAll('.nav-item')[pageNum - 1].classList.add('active');
    }

    // Card collapse functionality
    document.querySelectorAll('.toggle-btn').forEach(btn => {
        btn.addEventListener('click', () => {
            const card = btn.closest('.card');
            const content = card.querySelector('.card-content');
            content.classList.toggle('collapsed');
            btn.classList.toggle('collapsed');
        });
    });

    // Library slider functionality
    const wrapper = document.querySelector('.library-wrapper');
    const slides = document.querySelectorAll('.library-slide');
    const dotsContainer = document.querySelector('.dots-container');
    let currentSlide = 0;
    let startX = 0;
    let currentX = 0;
    let isDragging = false;

    // Create dots
    slides.forEach((_, index) => {
        const dot = document.createElement('div');
        dot.classList.add('dot');
        if (index === 0) dot.classList.add('active');
        dot.addEventListener('click', () => goToSlide(index));
        dotsContainer.appendChild(dot);
    });

    function goToSlide(index) {
        currentSlide = index;
        wrapper.style.transform = `translateX(-${index * 100}%)`;
        updateDots();
        // 更新当前选中的幻灯片
        document.querySelectorAll('.library-slide').forEach((slide, i) => {
            slide.classList.toggle('active', i === index);
        });
    }

    function updateDots() {
        document.querySelectorAll('.dot').forEach((dot, index) => {
            dot.classList.toggle('active', index === currentSlide);
        });
    }

    // Touch and mouse events for sliding
    function startDrag(e) {
        if (e.touches && e.touches.length >= 2) return; // 如果是多点触摸，不处理滑动
        
        isDragging = true;
        startX = e.type.includes('mouse') ? e.clientX : e.touches[0].clientX;
        currentX = startX;
        wrapper.style.transition = 'none';
    }

    function dragMove(e) {
        if (!isDragging || (e.touches && e.touches.length >= 2)) return;
        
        const clientX = e.type.includes('mouse') ? e.clientX : e.touches[0].clientX;
        currentX = clientX;
        const diff = clientX - startX;

        // 计算当前transform值
        let currentTransform = -currentSlide * 100;
        let newTransform;

        // 添加边界限制和阻尼效果
        if (currentSlide === 0 && diff > 0) {
            // 第一张图片向右拖动时
            newTransform = currentTransform + (diff / wrapper.offsetWidth * 100 * 0.3);
        } else if (currentSlide === slides.length - 1 && diff < 0) {
            // 最后一张图片向左拖动时
            newTransform = currentTransform + (diff / wrapper.offsetWidth * 100 * 0.3);
        } else {
            // 正常情况
            newTransform = currentTransform + (diff / wrapper.offsetWidth * 100);
        }

        wrapper.style.transform = `translateX(${newTransform}%)`;
    }

    function endDrag(e) {
        if (!isDragging) return;
        isDragging = false;
        wrapper.style.transition = 'transform 0.3s ease';
        
        const diff = currentX - startX;
        const threshold = wrapper.offsetWidth / 10;

        // 添加边界检查
        if ((currentSlide === 0 && diff > 0) || 
            (currentSlide === slides.length - 1 && diff < 0)) {
            // 如果是在边界,直接回弹到当前slide
            goToSlide(currentSlide);
            return;
        }

        // 正常滑动逻辑
        if (Math.abs(diff) > threshold) {
            if (diff > 0 && currentSlide > 0) {
                currentSlide--;
            } else if (diff < 0 && currentSlide < slides.length - 1) {
                currentSlide++;
            }
        }

        goToSlide(currentSlide);
    }

    wrapper.addEventListener('touchstart', startDrag);
    wrapper.addEventListener('touchmove', dragMove);
    wrapper.addEventListener('touchend', endDrag);

    // Mouse events for desktop simulation
    wrapper.addEventListener('mousedown', startDrag);
    wrapper.addEventListener('mousemove', dragMove);
    wrapper.addEventListener('mouseup', endDrag);
    wrapper.addEventListener('mouseleave', endDrag);

    // 初始化第一张幻灯片
    goToSlide(0);

    // 网络连接按钮事件
    const connectButton = document.querySelector('#networkConnectBtn');
    const ipInput = document.querySelector('#ipInput');
    const portInput = document.querySelector('#portInput');

    connectButton.addEventListener('click', () => {
        const message = {
            type: 'Connect',
            ip: ipInput.value,
            port: parseInt(portInput.value)
        };

        // 发送消息到 Unity
        window.vuplex?.postMessage(JSON.stringify(message));
    });
});
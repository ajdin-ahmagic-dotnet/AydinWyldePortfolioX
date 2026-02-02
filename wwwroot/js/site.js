document.addEventListener('DOMContentLoaded', () => {
    // Array of animation class names
    const animations = [
        'anim-fade-up',
        'anim-zoom-in',
        'anim-flip-in',
        'anim-slide-right',
        'anim-bounce-in',
        'anim-rotate-in'
    ];

    // Select all cards or elements that should animate randomly
    // You can add 'random-anim-card' class to any element to make it animate
    const cards = document.querySelectorAll('.portfolio-card, .random-anim-card');

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const element = entry.target;
                
                // If animation already applied, do nothing (or reset if you want it every time)
                if (element.classList.contains('animated-active')) return;

                // Pick a random animation
                const randomAnim = animations[Math.floor(Math.random() * animations.length)];
                
                element.classList.add(randomAnim);
                element.classList.add('animated-active');
                
                // Stop observing once animated
                observer.unobserve(element);
            }
        });
    }, {
        threshold: 0.1,
        rootMargin: "0px"
    });

    cards.forEach(card => {
        // Initial state: ensure they are invisible before animation
        card.style.opacity = '0';
        observer.observe(card);
    });
});

(() => {
    const navbar = document.querySelector('.kmc-navbar');

    const updateNavbar = () => {
        if (!navbar) {
            return;
        }

        navbar.classList.toggle('navbar-scrolled', window.scrollY > 12);
    };

    updateNavbar();
    window.addEventListener('scroll', updateNavbar, { passive: true });

    const revealItems = document.querySelectorAll('.reveal-on-scroll');

    if (!('IntersectionObserver' in window)) {
        revealItems.forEach((item) => item.classList.add('is-visible'));
        return;
    }

    const observer = new IntersectionObserver(
        (entries, revealObserver) => {
            entries.forEach((entry) => {
                if (!entry.isIntersecting) {
                    return;
                }

                entry.target.classList.add('is-visible');
                revealObserver.unobserve(entry.target);
            });
        },
        {
            threshold: 0.12,
            rootMargin: '0px 0px -30px 0px'
        });

    revealItems.forEach((item) => observer.observe(item));
})();

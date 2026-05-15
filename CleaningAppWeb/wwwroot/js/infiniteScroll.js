let currentObserver = null;
let currentSentinel = null;

export function initializeInfiniteScroll(container, dotNetHelper) {
    if (!container || typeof container.querySelector !== 'function') {
        return;
    }

    stopInfiniteScroll();

    currentSentinel = document.createElement('div');
    currentSentinel.style.height = '1px';
    currentSentinel.style.width = '100%';
    currentSentinel.style.opacity = '0';
    currentSentinel.style.pointerEvents = 'none';

    let scrollContainer = container;
    const table = container.querySelector('table');

    if (table) {
        table.after(currentSentinel);
        scrollContainer = table.parentElement || container;
    } else {
        container.appendChild(currentSentinel);
    }

    let isLoading = false;

    currentObserver = new IntersectionObserver(
        (entries) => {
            const entry = entries[0];

            if (entry.isIntersecting && !isLoading) {
                isLoading = true;

                dotNetHelper.invokeMethodAsync('LoadMoreItems')
                    .finally(() => {
                        setTimeout(() => {
                            isLoading = false;
                        }, 500);
                    });
            }
        },
        {
            root: scrollContainer,
            rootMargin: '0px 0px 200px 0px',
            threshold: 0.01
        }
    );

    currentObserver.observe(currentSentinel);
}

export function stopInfiniteScroll() {
    if (currentObserver) {
        currentObserver.disconnect();
        currentObserver = null;
    }
    if (currentSentinel) {
        currentSentinel.remove();
        currentSentinel = null;
    }
}

export function dispose() {
    stopInfiniteScroll();
}
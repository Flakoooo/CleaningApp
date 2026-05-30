export function initializeInfiniteScroll(container, dotNetHelper) {
    if (!container || typeof container.querySelector !== 'function') return;

    stopInfiniteScroll(container);

    const sentinel = document.createElement('div');
    sentinel.style.height = '1px';
    sentinel.style.width = '100%';
    sentinel.style.opacity = '0';
    sentinel.style.pointerEvents = 'none';
    container.appendChild(sentinel);

    let isLoading = false;

    const observer = new IntersectionObserver(
        (entries) => {
            const entry = entries[0];
            if (entry.isIntersecting && !isLoading) {
                isLoading = true;
                dotNetHelper.invokeMethodAsync('LoadMoreItems')
                    .finally(() => {
                        setTimeout(() => { isLoading = false; }, 500);
                    });
            }
        },
        {
            root: container,
            rootMargin: '0px 0px 200px 0px',
            threshold: 0.01
        }
    );

    observer.observe(sentinel);

    container._infiniteScroll = { observer, sentinel };
}

export function stopInfiniteScroll(container) {
    if (!container || !container._infiniteScroll) return;
    const { observer, sentinel } = container._infiniteScroll;
    observer.disconnect();
    sentinel.remove();
    delete container._infiniteScroll;
}

export function dispose(container) {
    stopInfiniteScroll(container);
}
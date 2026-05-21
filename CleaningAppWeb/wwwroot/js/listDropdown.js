window.listDropdown = {
    registerClickOutside: function (element, dotNetHelper) {
        if (!element) return;

        element._dotNetHelper = dotNetHelper;

        function isInsideDropdown(target) {
            const componentRoot = element.closest('[tabindex="-1"]') || element;
            return componentRoot.contains(target);
        }

        element._clickOutsideHandler = function (event) {
            if (!isInsideDropdown(event.target)) {
                dotNetHelper.invokeMethodAsync('CloseDropdown');
            }
        };

        setTimeout(() => {
            document.addEventListener('click', element._clickOutsideHandler, true);
        }, 0);
    },

    unregisterClickOutside: function (element) {
        if (!element) return;

        if (element._clickOutsideHandler) {
            document.removeEventListener('click', element._clickOutsideHandler, true);
            delete element._clickOutsideHandler;
        }

        if (element._dotNetHelper) {
            element._dotNetHelper.dispose();
            delete element._dotNetHelper;
        }
    }
};
// Tinytools templates client assets.
//
// The Templates page renders with Blazor, so the only browser-native behaviour it needs is
// clipboard access for copying a template body. Kept as a small, dependency-free module so the
// package can declare it through the plugin client-asset contract.
window.tinytoolsTemplates = (() => {
    async function copyTemplate(text, target) {
        if (!text) return false;

        try {
            await navigator.clipboard.writeText(text);
            if (target) target.dataset.copied = "true";
            return true;
        } catch {
            return false;
        }
    }

    return { version: 1, copyTemplate };
})();

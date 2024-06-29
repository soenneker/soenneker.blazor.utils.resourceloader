export class ResourceLoader {
    constructor() {
        this.loadedScripts = {};
        this.loadedCss = {};
    }

    loadScript(url, integrity, crossorigin = 'anonymous') {
        if (this.loadedScripts[url] === true)
            return;

        this.loadedScripts[url] = true;

        return new Promise((resolve, reject) => {
            const script = document.createElement('script');
            script.src = url;

            if (crossorigin)
                script.crossOrigin = crossorigin;

            if (integrity)
                script.integrity = integrity;
            
            script.onload = () => resolve();
            script.onerror = () => reject(new Error(`Failed to load script: ${url}`));
            document.body.appendChild(script);
        });
    }

    loadStyle(url, integrity, crossorigin = 'anonymous') {
        if (this.loadedCss[url] === true)
            return;

        this.loadedCss[url] = true;

        return new Promise((resolve, reject) => {
            const link = document.createElement('link');
            link.rel = 'stylesheet';
            link.href = url;

            if (crossorigin)
                link.crossOrigin = crossorigin;

            if (integrity)
                link.integrity = integrity;
            
            link.onload = () => resolve();
            link.onerror = () => reject(new Error(`Failed to load stylesheet: ${url}`));
            document.head.appendChild(link);
        });
    }

    waitForVariable(variableName, interval = 100) {
        return new Promise((resolve) => {
            const checkVariable = setInterval(() => {
                if (typeof window[variableName] !== 'undefined' && window[variableName] !== null) {
                    clearInterval(checkVariable);
                    resolve(window[variableName]);
                }
            }, interval);
        });
    }
}

window.ResourceLoader = new ResourceLoader();
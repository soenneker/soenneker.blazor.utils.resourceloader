export class ResourceLoader {
    constructor() {
        this.loadedScripts = new Map();
        this.loadedCss = new Map();
    }

    loadScript(url, integrity, crossorigin, loadInHead = false, async = false, defer = false) {
        if (this.loadedScripts.has(url))
            return this.loadedScripts.get(url);

        const script = document.createElement('script');
        script.src = url;

        if (crossorigin)
            script.crossOrigin = crossorigin;

        if (integrity)
            script.integrity = integrity;

        script.async = !!async;
        script.defer = !!defer;

        const promise = new Promise((resolve, reject) => {
            script.onload = () => resolve(url);
            script.onerror = (err) => reject(new Error(`Failed to load script: ${url} - ${err.message}`));
        });

        this.loadedScripts.set(url, promise);
        (loadInHead ? document.head : document.body).appendChild(script);
        return promise;
    }

    loadStyle(url, integrity, crossorigin, media = "all", type = "text/css") {
        if (this.loadedCss.has(url))
            return this.loadedCss.get(url);

        const link = document.createElement('link');
        link.rel = 'stylesheet';
        link.href = url;

        if (crossorigin)
            link.crossOrigin = crossorigin;

        if (integrity)
            link.integrity = integrity;

        link.media = media;
        link.type = type;

        const promise = new Promise((resolve, reject) => {
            link.onload = () => resolve(url);
            link.onerror = (err) => reject(new Error(`Failed to load stylesheet: ${url} - ${err.message}`));
        });

        this.loadedCss.set(url, promise);
        document.head.appendChild(link);
        return promise;
    }
}

window.ResourceLoader = new ResourceLoader();

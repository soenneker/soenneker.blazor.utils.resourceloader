export class ResourceLoader {
    constructor() {
        this.loadedScripts = {};
        this.loadedCss = {};
    }

    loadScript(url, integrity, crossorigin, loadInHead, async, defer) {
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

            if (async)
                script.async = true;

            if (defer)
                script.defer = true;

            script.onload = () => resolve();
            script.onerror = () => reject(new Error(`Failed to load script: ${url}`));

            if (loadInHead)
                document.head.appendChild(script);
            else
                document.body.appendChild(script);
        });
    }

    loadStyle(url, integrity, crossorigin, media, type) {
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

            link.media = media;
            link.type = type;

            link.onload = () => resolve();
            link.onerror = () => reject(new Error(`Failed to load stylesheet: ${url}`));
            document.head.appendChild(link);
        });
    }
}

window.ResourceLoader = new ResourceLoader();
const loadedScripts = new Map();
const loadedStyles = new Map();
const importedModules = new Map();

function createScriptKey(url, integrity, crossorigin, loadInHead, async, defer, isModule) {
    return JSON.stringify({
        url,
        integrity: integrity ?? null,
        crossorigin: crossorigin ?? null,
        loadInHead: !!loadInHead,
        async: !!async,
        defer: !!defer,
        isModule: !!isModule
    });
}

function createStyleKey(url, integrity, crossorigin, media, type) {
    return JSON.stringify({
        url,
        integrity: integrity ?? null,
        crossorigin: crossorigin ?? null,
        media: media ?? null,
        type: type ?? null
    });
}

function toErrorMessage(prefix, url, event) {
    const detail = event instanceof ErrorEvent && event.message
        ? `: ${event.message}`
        : '';

    return `${prefix}: ${url}${detail}`;
}

export async function loadScript(url, integrity, crossorigin, loadInHead = false, async = false, defer = false, isModule = false) {
    const key = createScriptKey(url, integrity, crossorigin, loadInHead, async, defer, isModule);

    if (loadedScripts.has(key)) {
        return loadedScripts.get(key);
    }

    const promise = new Promise((resolve, reject) => {
        const script = document.createElement('script');
        script.src = url;

        if (crossorigin) {
            script.crossOrigin = crossorigin;
        }

        if (integrity) {
            script.integrity = integrity;
        }

        if (isModule) {
            script.type = 'module';
        }

        script.async = !!async;
        script.defer = !!defer;
        script.onload = () => resolve(url);
        script.onerror = (event) => reject(new Error(toErrorMessage('Failed to load script', url, event)));

        (loadInHead ? document.head : document.body).appendChild(script);
    });

    loadedScripts.set(key, promise);

    try {
        return await promise;
    } catch (error) {
        loadedScripts.delete(key);
        throw error;
    }
}

export async function importExternalModule(url) {
    if (importedModules.has(url)) {
        return importedModules.get(url);
    }

    const promise = import(url)
        .catch(error => {
            importedModules.delete(url);
            throw error;
        });

    importedModules.set(url, promise);

    return promise;
}

export async function loadStyle(url, integrity, crossorigin, media = 'all', type = 'text/css') {
    const key = createStyleKey(url, integrity, crossorigin, media, type);

    if (loadedStyles.has(key)) {
        return loadedStyles.get(key);
    }

    const promise = new Promise((resolve, reject) => {
        const link = document.createElement('link');
        link.rel = 'stylesheet';
        link.href = url;

        if (crossorigin) {
            link.crossOrigin = crossorigin;
        }

        if (integrity) {
            link.integrity = integrity;
        }

        link.media = media;
        link.type = type;
        link.onload = () => resolve(url);
        link.onerror = (event) => reject(new Error(toErrorMessage('Failed to load stylesheet', url, event)));

        document.head.appendChild(link);
    });

    loadedStyles.set(key, promise);

    try {
        return await promise;
    } catch (error) {
        loadedStyles.delete(key);
        throw error;
    }
}

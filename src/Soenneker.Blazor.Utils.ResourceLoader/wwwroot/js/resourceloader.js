const loadedScripts = new Map();
const loadedStyles = new Map();
const importedModules = new Map();

function normalize(value) {
    return value == null ? '' : String(value);
}

function createScriptKey(url, integrity, crossorigin, loadInHead, async, defer, isModule) {
    return [
        normalize(url),
        normalize(integrity),
        normalize(crossorigin),
        loadInHead ? '1' : '0',
        async ? '1' : '0',
        defer ? '1' : '0',
        isModule ? '1' : '0'
    ].join('|');
}

function createStyleKey(url, integrity, crossorigin, media, type) {
    return [
        normalize(url),
        normalize(integrity),
        normalize(crossorigin),
        normalize(media),
        normalize(type)
    ].join('|');
}

function toErrorMessage(prefix, url, event) {
    const detail =
        event instanceof ErrorEvent && event.message
            ? `: ${event.message}`
            : '';

    return `${prefix}: ${url}${detail}`;
}

function getScriptParent(loadInHead) {
    if (loadInHead) {
        return document.head;
    }

    return document.body ?? document.head ?? document.documentElement;
}

function findExistingScript(url, integrity, crossorigin, isModule) {
    const scripts = document.getElementsByTagName('script');

    for (let i = 0; i < scripts.length; i++) {
        const script = scripts[i];

        if (script.src !== url) {
            continue;
        }

        if (normalize(script.integrity) !== normalize(integrity)) {
            continue;
        }

        if (normalize(script.crossOrigin) !== normalize(crossorigin)) {
            continue;
        }

        const scriptIsModule = script.type === 'module';

        if (scriptIsModule !== !!isModule) {
            continue;
        }

        return script;
    }

    return null;
}

function findExistingStyle(url, integrity, crossorigin, media, type) {
    const links = document.getElementsByTagName('link');

    for (let i = 0; i < links.length; i++) {
        const link = links[i];

        if (link.rel !== 'stylesheet') {
            continue;
        }

        if (link.href !== url) {
            continue;
        }

        if (normalize(link.integrity) !== normalize(integrity)) {
            continue;
        }

        if (normalize(link.crossOrigin) !== normalize(crossorigin)) {
            continue;
        }

        if (normalize(link.media) !== normalize(media)) {
            continue;
        }

        if (normalize(link.type) !== normalize(type)) {
            continue;
        }

        return link;
    }

    return null;
}

function createPendingScriptPromise(script, url) {
    return new Promise((resolve, reject) => {
        const onLoad = () => {
            cleanup();
            resolve(url);
        };

        const onError = (event) => {
            cleanup();
            reject(new Error(toErrorMessage('Failed to load script', url, event)));
        };

        function cleanup() {
            script.removeEventListener('load', onLoad);
            script.removeEventListener('error', onError);
        }

        script.addEventListener('load', onLoad, { once: true });
        script.addEventListener('error', onError, { once: true });
    });
}

function createPendingStylePromise(link, url) {
    return new Promise((resolve, reject) => {
        const onLoad = () => {
            cleanup();
            resolve(url);
        };

        const onError = (event) => {
            cleanup();
            reject(new Error(toErrorMessage('Failed to load stylesheet', url, event)));
        };

        function cleanup() {
            link.removeEventListener('load', onLoad);
            link.removeEventListener('error', onError);
        }

        link.addEventListener('load', onLoad, { once: true });
        link.addEventListener('error', onError, { once: true });
    });
}

function validateAbsoluteUrl(url, parameterName) {
    if (typeof url !== 'string' || url.trim().length === 0) {
        throw new Error(`${parameterName} must be a non-empty string.`);
    }

    try {
        return new URL(url, document.baseURI).href;
    } catch {
        throw new Error(`${parameterName} must be a valid URL.`);
    }
}

export async function loadScript(url, integrity, crossorigin, loadInHead = false, async = false, defer = false, isModule = false) {
    url = validateAbsoluteUrl(url, 'url');

    const key = createScriptKey(url, integrity, crossorigin, loadInHead, async, defer, isModule);
    const existing = loadedScripts.get(key);

    if (existing) {
        return existing;
    }

    const existingScript = findExistingScript(url, integrity, crossorigin, isModule);

    if (existingScript) {
        if (existingScript.dataset.soennekerLoaded === 'true') {
            const completed = Promise.resolve(url);
            loadedScripts.set(key, completed);
            return completed;
        }

        const pending = createPendingScriptPromise(existingScript, url)
            .then((result) => {
                existingScript.dataset.soennekerLoaded = 'true';
                return result;
            })
            .catch((error) => {
                loadedScripts.delete(key);
                throw error;
            });

        loadedScripts.set(key, pending);
        return pending;
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

        const onLoad = () => {
            cleanup();
            script.dataset.soennekerLoaded = 'true';
            resolve(url);
        };

        const onError = (event) => {
            cleanup();
            loadedScripts.delete(key);
            reject(new Error(toErrorMessage('Failed to load script', url, event)));
        };

        function cleanup() {
            script.removeEventListener('load', onLoad);
            script.removeEventListener('error', onError);
        }

        script.addEventListener('load', onLoad, { once: true });
        script.addEventListener('error', onError, { once: true });

        getScriptParent(loadInHead).appendChild(script);
    });

    loadedScripts.set(key, promise);
    return promise;
}

export async function importExternalModule(url) {
    url = validateAbsoluteUrl(url, 'url');

    const existing = importedModules.get(url);

    if (existing) {
        return existing;
    }

    const promise = import(url).catch((error) => {
        importedModules.delete(url);
        throw error;
    });

    importedModules.set(url, promise);

    return promise;
}

export async function loadStyle(url, integrity, crossorigin, media = 'all', type = 'text/css') {
    url = validateAbsoluteUrl(url, 'url');

    const key = createStyleKey(url, integrity, crossorigin, media, type);
    const existing = loadedStyles.get(key);

    if (existing) {
        return existing;
    }

    const existingStyle = findExistingStyle(url, integrity, crossorigin, media, type);

    if (existingStyle) {
        if (existingStyle.dataset.soennekerLoaded === 'true') {
            const completed = Promise.resolve(url);
            loadedStyles.set(key, completed);
            return completed;
        }

        const pending = createPendingStylePromise(existingStyle, url)
            .then((result) => {
                existingStyle.dataset.soennekerLoaded = 'true';
                return result;
            })
            .catch((error) => {
                loadedStyles.delete(key);
                throw error;
            });

        loadedStyles.set(key, pending);
        return pending;
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

        if (media) {
            link.media = media;
        }

        if (type) {
            link.type = type;
        }

        const onLoad = () => {
            cleanup();
            link.dataset.soennekerLoaded = 'true';
            resolve(url);
        };

        const onError = (event) => {
            cleanup();
            loadedStyles.delete(key);
            reject(new Error(toErrorMessage('Failed to load stylesheet', url, event)));
        };

        function cleanup() {
            link.removeEventListener('load', onLoad);
            link.removeEventListener('error', onError);
        }

        link.addEventListener('load', onLoad, { once: true });
        link.addEventListener('error', onError, { once: true });

        document.head.appendChild(link);
    });

    loadedStyles.set(key, promise);
    return promise;
}
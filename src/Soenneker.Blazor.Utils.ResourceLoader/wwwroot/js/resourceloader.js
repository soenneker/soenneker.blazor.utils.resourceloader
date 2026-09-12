const scripts = new Map();
const styles = new Map();
const modules = new Map();

function normalizeString(value) {
    return value == null ? '' : String(value);
}

function validateAbsoluteUrl(url, parameterName) {
    if (typeof url !== 'string' || url.trim().length === 0) {
        throw new Error(`${parameterName} must be a non-empty string.`);
    }

    try {
        const parsed = new URL(url, document.baseURI);

        if (parsed.protocol !== 'http:' && parsed.protocol !== 'https:') {
            throw new Error(`${parameterName} must use HTTP or HTTPS.`);
        }

        if (parsed.username || parsed.password) {
            throw new Error(`${parameterName} must not contain credentials.`);
        }

        return parsed.href;
    } catch {
        throw new Error(`${parameterName} must be a valid HTTP(S) URL without embedded credentials.`);
    }
}

function validateCrossOrigin(value) {
    if (value == null || value === '' || value === 'anonymous' || value === 'use-credentials') {
        return;
    }

    throw new Error("crossorigin must be empty, 'anonymous', or 'use-credentials'.");
}

function getScriptParent(loadInHead) {
    return loadInHead ? document.head : (document.body || document.head || document.documentElement);
}

function attachLoadPromise(element, url, failurePrefix, cache, parent) {
    return new Promise((resolve, reject) => {
        const cleanup = () => {
            element.removeEventListener('load', onLoad);
            element.removeEventListener('error', onError);
        };
        const fail = (error) => {
            cleanup();
            cache.delete(url);
            element.remove();
            reject(error);
        };
        const onLoad = () => {
            cleanup();
            element.dataset.soennekerLoaded = 'true';
            resolve(url);
        };
        const onError = (event) => {
            const detail = event instanceof ErrorEvent && event.message ? `: ${event.message}` : '';
            fail(new Error(`${failurePrefix}: ${url}${detail}`));
        };

        element.addEventListener('load', onLoad);
        element.addEventListener('error', onError);
        try {
            parent.appendChild(element);
        } catch (error) {
            fail(error);
        }
    });
}

export function loadScript(url, integrity, crossorigin, loadInHead = false, async = false, defer = false, isModule = false) {
    url = validateAbsoluteUrl(url, 'url');
    validateCrossOrigin(crossorigin);

    let entry = scripts.get(url);

    if (entry) {
        if (
            entry.integrity === normalizeString(integrity) &&
            entry.crossorigin === normalizeString(crossorigin) &&
            entry.loadInHead === !!loadInHead &&
            entry.async === !!async &&
            entry.defer === !!defer &&
            entry.isModule === !!isModule
        ) {
            return entry.promise;
        }

        throw new Error(`Script already registered with different options: ${url}`);
    }

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

    entry = {
        integrity: normalizeString(integrity),
        crossorigin: normalizeString(crossorigin),
        loadInHead: !!loadInHead,
        async: !!async,
        defer: !!defer,
        isModule: !!isModule,
        promise: null
    };

    scripts.set(url, entry);
    entry.promise = attachLoadPromise(script, url, 'Failed to load script', scripts, getScriptParent(loadInHead));

    return entry.promise;
}

export function loadStyle(url, integrity, crossorigin, media = 'all', type = 'text/css') {
    url = validateAbsoluteUrl(url, 'url');
    validateCrossOrigin(crossorigin);

    let entry = styles.get(url);

    if (entry) {
        if (
            entry.integrity === normalizeString(integrity) &&
            entry.crossorigin === normalizeString(crossorigin) &&
            entry.media === normalizeString(media) &&
            entry.type === normalizeString(type)
        ) {
            return entry.promise;
        }

        throw new Error(`Stylesheet already registered with different options: ${url}`);
    }

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

    entry = {
        integrity: normalizeString(integrity),
        crossorigin: normalizeString(crossorigin),
        media: normalizeString(media),
        type: normalizeString(type),
        promise: null
    };

    styles.set(url, entry);
    entry.promise = attachLoadPromise(link, url, 'Failed to load stylesheet', styles, document.head);

    return entry.promise;
}

export function importExternalModule(url) {
    url = validateAbsoluteUrl(url, 'url');

    let promise = modules.get(url);

    if (promise) {
        return promise;
    }

    promise = import(url).catch((error) => {
        modules.delete(url);
        throw error;
    });

    modules.set(url, promise);
    return promise;
}

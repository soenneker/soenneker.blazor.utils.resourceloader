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

function attachLoadPromise(element, url, failurePrefix, onSuccess, onFailure) {
    return new Promise((resolve, reject) => {
        const onLoad = () => {
            element.removeEventListener('load', onLoad);
            element.removeEventListener('error', onError);

            if (onSuccess) {
                onSuccess();
            }

            resolve(url);
        };

        const onError = (event) => {
            element.removeEventListener('load', onLoad);
            element.removeEventListener('error', onError);

            if (onFailure) {
                onFailure();
            }

            const detail = event instanceof ErrorEvent && event.message ? `: ${event.message}` : '';
            reject(new Error(`${failurePrefix}: ${url}${detail}`));
        };

        element.addEventListener('load', onLoad, { once: true });
        element.addEventListener('error', onError, { once: true });
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
        element: script,
        promise: null
    };

    entry.promise = attachLoadPromise(
        script,
        url,
        'Failed to load script',
        () => { script.dataset.soennekerLoaded = 'true'; },
        () => {
            scripts.delete(url);
            script.remove();
        }
    );

    scripts.set(url, entry);

    try {
        getScriptParent(loadInHead).appendChild(script);
    } catch (error) {
        scripts.delete(url);
        script.remove();
        throw error;
    }

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
        element: link,
        promise: null
    };

    entry.promise = attachLoadPromise(
        link,
        url,
        'Failed to load stylesheet',
        () => { link.dataset.soennekerLoaded = 'true'; },
        () => {
            styles.delete(url);
            link.remove();
        }
    );

    styles.set(url, entry);

    try {
        document.head.appendChild(link);
    } catch (error) {
        styles.delete(url);
        link.remove();
        throw error;
    }

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

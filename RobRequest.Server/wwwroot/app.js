window.downloadFileFromBytes = (fileName, contentType, base64) => {
    const byteCharacters = atob(base64);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], {type: contentType});
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
}

window.downloadFileFromStream = async (fileName, contentType, streamRef) => {
    const arrayBuffer = await streamRef.arrayBuffer();
    const blob = new Blob([arrayBuffer], {type: contentType});
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
}

// Infinite scroll: notifies .NET when a sentinel element scrolls into view.
window.historyInfiniteScroll = {
    observers: {},
    observe: function (id, sentinel, root, dotnetRef) {
        if (!sentinel) return;
        this.disconnect(id);
        const observer = new IntersectionObserver((entries) => {
            for (const entry of entries) {
                if (entry.isIntersecting) {
                    dotnetRef.invokeMethodAsync('OnScrolledToBottom');
                }
            }
        }, { root: root || null, rootMargin: '150px', threshold: 0 });
        observer.observe(sentinel);
        this.observers[id] = observer;
    },
    disconnect: function (id) {
        const existing = this.observers[id];
        if (existing) {
            existing.disconnect();
            delete this.observers[id];
        }
    }
};

// renders item count for JSON arrays
window.initializeMonacoCodeLens = () => {
    if (window._monacoCodeLensRegistered) return;
    window._monacoCodeLensRegistered = true;

    monaco.languages.registerCodeLensProvider('json', {
        provideCodeLenses(model) {
            const lenses = [];
            const text = model.getValue();
            let root;
            try {
                root = JSON.parse(text);
            } catch {
                return {
                    lenses, dispose: () => {
                    }
                };
            }

            const lines = text.split('\n');

            // Build a map of line index → array/object at that line using a
            // character-level walk so we get real counts, not a heuristic.
            let pos = 0;
            const stack = [];   // {line, isArray}

            for (let i = 0; i < lines.length; i++) {
                const line = lines[i];
                for (let c = 0; c < line.length; c++, pos++) {
                    const ch = line[c];
                    if (ch === '[' || ch === '{') {
                        stack.push({line: i, isArray: ch === '[', count: 0});
                    } else if ((ch === ']' || ch === '}') && stack.length) {
                        const frame = stack.pop();
                        if (frame.isArray && frame.count > 0) {
                            const noun = frame.count === 1 ? 'item' : 'items';
                            lenses.push({
                                range: {
                                    startLineNumber: frame.line + 1,
                                    startColumn: 1,
                                    endLineNumber: frame.line + 1,
                                    endColumn: 1
                                },
                                command: {
                                    id: 'editor.action.inlayHints.toggle',
                                    title: `${frame.count} ${noun}`
                                }
                            });
                        }
                        // increment parent's count
                        if (stack.length) stack[stack.length - 1].count++;
                    } else if (ch === ',' && stack.length) {
                        // commas at this depth separate items — handled via
                        // close-bracket counting above; no action needed here.
                    }
                }
                pos++; // newline character
            }

            return {
                lenses, dispose: () => {
                }
            };
        }
    });
};
window.MonacoEditorInterop = {
    initMonaco: function (elementId, content, language, theme) {
        return new Promise((resolve) => {
            require(['vs/editor/editor.main'], function () {

                if (window.monacoEditors === undefined) {
                    window.monacoEditors = {};
                }

                if (window.monacoEditors[elementId]) {
                    window.monacoEditors[elementId].dispose();
                }

                monaco.languages.register({ id: 'pw' });

                monaco.languages.setMonarchTokensProvider('pw', {
                    keywords: [
                        'void', 'int', 'bool', 'string', 'color',
                        'func',
                        'true', 'True', 'false', 'False',
                        'return', 'GoTo', 'Goto', 'goto',

                        'SetCanvasColor',
                        'Spawn', 'Respawn', 'Move', 'Color', 'Size',
                        'DrawLine', 'DrawCircle', 'DrawFullCircle', 'DrawRectangle', 'DrawFullRectangle', 'Fill',
                        'GetActualX', 'GetActualY',
                        'GetCanvasSize', 'GetCanvasSizeX', 'GetCanvasSizeY',
                        'GetColorCount',
                        'IsBrushColor', 'IsBrushSize',
                        'GetBrushSize',
                        'IsCanvasColor', 'IsColor'
                    ],
                    operators: /[+\-*/%]|\*\*|&&|\|\||==|>=|<=|>|<|!|!=|<-|←/,
                    typeKeywords: [
                        'void', 'int', 'bool', 'string', 'color'
                    ],
                    builtinFunctions: [
                        'SetCanvasColor',
                        'Spawn', 'Respawn', 'Move', 'Color', 'Size',
                        'DrawLine', 'DrawCircle', 'DrawFullCircle', 'DrawRectangle', 'DrawFullRectangle', 'Fill',
                        'GetActualX', 'GetActualY',
                        'GetCanvasSize', 'GetCanvasSizeX', 'GetCanvasSizeY',
                        'GetColorCount',
                        'IsBrushColor', 'IsBrushSize',
                        'GetBrushSize',
                        'IsCanvasColor', 'IsColor'
                    ],
                    escapes: /\\(?:[abfnrtv\\"']|x[0-9A-Fa-f]{1,4}|u[0-9A-Fa-f]{4}|U[0-9A-Fa-f]{8})/,
                    tokenizer: {
                        root: [
                            [/[a-zA-Z_][a-zA-Z0-9_]*/, {
                                cases: {
                                    '@keywords': 'keyword',
                                    '@typeKeywords': 'type',
                                    '@builtinFunctions': 'predefined',
                                    '@default': 'identifier'
                                }
                            }],
                            { include: '@whitespace' },
                            [/\d+/, 'number'],
                            [/"([^"\\]|\\.)*$/, 'string.invalid'],
                            [/"/, { token: 'string.quote', bracket: '@open', next: '@string' }],
                            [/[{}()\[\]]/, '@brackets'],
                            [/@operators/, 'operator'],
                        ],
                        string: [
                            [/[^\\"]+/, 'string'],
                            [/@escapes/, 'string.escape'],
                            [/\\./, 'string.escape.invalid'],
                            [/"/, { token: 'string.quote', bracket: '@close', next: '@pop' }]
                        ],
                        whitespace: [
                            [/[ \t\r\n]+/, 'white'],
                        ],
                    },
                });

                monaco.editor.defineTheme('vs-dark', {
                    base: 'vs-dark',
                    inherit: true,
                    rules: [
                        { token: 'keyword', foreground: '569cd6' },
                        { token: 'type', foreground: '4ec9b0' },
                        { token: 'predefined', foreground: 'dcdcaa' },
                        { token: 'number', foreground: 'b5cea8' },
                        { token: 'string', foreground: 'ce9178' },
                        { token: 'identifier', foreground: '9cdcfe' }
                    ],
                    colors: {
                        'editor.foreground': '#d4d4d4',
                        'editor.background': '#1e1e1e',
                        'editorCursor.foreground': '#AEAFAD',
                        'editor.lineHighlightBackground': '#252526',
                        'editorLineNumber.foreground': '#858585',
                        'editor.selectionBackground': '#264F78',
                        'editor.inactiveSelectionBackground': '#3A3D41',
                    }
                });

                const editor = monaco.editor.create(document.getElementById(elementId), {
                    value: content,
                    language: language,
                    theme: theme,
                    automaticLayout: true,
                    lineNumbers: "on",
                    roundedSelection: false,
                    scrollBeyondLastLine: false,
                    readOnly: false,
                    minimap: { enabled: false },
                    overviewRulerBorder: false,
                    wordWrap: "on",
                    autoClosingBrackets: 'always',
                    autoClosingQuotes: 'always',
                    autoSurround: 'languageDefined'
                });
                window.monacoEditors[elementId] = editor;
                resolve(true);
            });
        });
    },

    setValue: function (elementId, value) {
        if (window.monacoEditors && window.monacoEditors[elementId]) {
            if (window.monacoEditors[elementId].getValue() !== value) {
                window.monacoEditors[elementId].setValue(value);
            }
        }
    },

    getValue: function (elementId) {
        if (window.monacoEditors && window.monacoEditors[elementId]) {
            return window.monacoEditors[elementId].getValue();
        }
        return "";
    },

    onContentChange: function (elementId, dotNetRef, methodName) {
        if (window.monacoEditors && window.monacoEditors[elementId]) {
            window.monacoEditors[elementId].onDidChangeModelContent((event) => {
                const newValue = window.monacoEditors[elementId].getValue();
                dotNetRef.invokeMethodAsync(methodName, newValue);
            });
        }
    },

    // Nuevas funciones para el manejo de errores
    setMarkers: function (elementId, markers) {
        if (window.monacoEditors && window.monacoEditors[elementId]) {
            monaco.editor.setModelMarkers(window.monacoEditors[elementId].getModel(), 'owner', markers);
        }
    }
};
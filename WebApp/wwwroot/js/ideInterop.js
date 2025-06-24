   window.selectFile = function (acceptedExtensions) {
        return new Promise((resolve, reject) => {
            const input = document.createElement('input');
            input.type = 'file';
            input.accept = acceptedExtensions || '.pw,.pwscript,.txt';

            input.addEventListener('change', function (event) {
                const file = event.target.files[0];
                if (file) {
                    const reader = new FileReader();
                    reader.onload = function (e) {
                        const fileName = file.name.replace(/\.[^/.]+$/, "");
                        resolve([fileName, e.target.result]);
                    };
                    reader.onerror = function () {
                        reject(new Error('Error al leer el archivo'));
                    };
                    reader.readAsText(file);
                } else {
                    resolve(null); 
                }
            });

            input.click();
        });
    };

    window.downloadFile = function (fileName, content) {
        const blob = new Blob([content], { type: 'text/plain' });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
    };

window.downloadPngFromBase64 = function (base64DataUrl, fileName) {
    const link = document.createElement('a');
    link.href = base64DataUrl;
    link.download = fileName.endsWith('.png') ? fileName : `${fileName}.png`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

window.selectImageFile = function (dotNetRef) {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = 'image/png,image/jpeg,image/gif';

    input.addEventListener('change', function (event) {
        const file = event.target.files[0];
        if (typeof file === "undefined") {
            return;
        }
        if (file) {
            const reader = new FileReader();
            reader.onload = function () {
                const dataUrl = reader.result;
                dotNetRef.invokeMethodAsync("ReciveImg64JS", dataUrl);
            };
            reader.onerror = function () {
                dotNetRef.invokeMethodAsync("ReciveImg64JS", "");
            };
            reader.readAsDataURL(file);
        } else {
            dotNetRef.invokeMethodAsync("ReciveImg64JS", "");
        }
    });

    input.click();
};

document.addEventListener('click', function(event) {
    if (!event.target.closest('.context-menu')) {
        if (typeof DotNet !== 'undefined') {
            DotNet.invokeMethodAsync('WebApp', 'CloseContextMenuFromJS');
        }
    }
});

document.addEventListener('keydown', function(event) {
    if (event.key === 'Escape') {
        if (typeof DotNet !== 'undefined') {
            DotNet.invokeMethodAsync('WebApp', 'CloseContextMenuFromJS');
        }
    }
});

document.addEventListener('contextmenu', function(event) {
    if (event.target.closest('.file-item')) {
        event.preventDefault();
    }
});




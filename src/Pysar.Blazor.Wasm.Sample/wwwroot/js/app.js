// The MAUI sample shares an exported PDF through the platform share sheet; the browser's
// equivalent is a download, which needs this small bridge.
window.pysarSample = {
    downloadFile: function (fileName, contentType, base64) {
        const url = URL.createObjectURL(
            new Blob([Uint8Array.from(atob(base64), c => c.charCodeAt(0))], { type: contentType }));

        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        link.remove();

        URL.revokeObjectURL(url);
    }
};

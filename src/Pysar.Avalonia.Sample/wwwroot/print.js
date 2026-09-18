// Browser printing for Pysar reports: the report is rendered to a vector PDF on the .NET side and
// handed here as base64. A hidden iframe holding the PDF is what gets printed, so the print output
// is the report itself rather than a screenshot of the Avalonia canvas.

let lastUrl = null;

export function printPdf(base64) {
    const binary = atob(base64);
    const bytes = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) {
        bytes[i] = binary.charCodeAt(i);
    }

    // The previous object URL is only released now: revoking it while its iframe was still
    // printing would blank the print preview in Chrome.
    if (lastUrl) {
        URL.revokeObjectURL(lastUrl);
    }

    const url = URL.createObjectURL(new Blob([bytes], { type: 'application/pdf' }));
    lastUrl = url;

    const previous = document.getElementById('pysar-print-frame');
    if (previous) {
        previous.remove();
    }

    const frame = document.createElement('iframe');
    frame.id = 'pysar-print-frame';
    frame.style.position = 'fixed';
    frame.style.right = '0';
    frame.style.bottom = '0';
    frame.style.width = '0';
    frame.style.height = '0';
    frame.style.border = '0';
    frame.src = url;

    frame.onload = () => {
        try {
            frame.contentWindow.focus();
            frame.contentWindow.print();
        } catch {
            // Safari refuses to print a cross-origin-ish PDF frame; showing it in a tab lets the
            // user print from the browser's own PDF viewer instead.
            window.open(url, '_blank');
        }
    };

    document.body.appendChild(frame);
}

function onScanSuccess(decodedText) {
    console.log("QR Code detected:", decodedText);

    window.location.href =
        "/Result?qr=" + encodeURIComponent(decodedText);
}

function onScanFailure(error) {
    // Normal QR scanning failures can be ignored.
}

const qrReader = document.getElementById("qr-reader");

if (qrReader) {
    const html5QrCode = new Html5Qrcode("qr-reader");

    html5QrCode.start(
        { facingMode: "environment" },
        {
            fps: 10,
            qrbox: 250
        },
        onScanSuccess,
        onScanFailure
    ).catch(function (error) {
        console.error("Camera error:", error);

        const status = document.getElementById("scan-status");

        if (status) {
            status.innerText = "Unable to access the camera.";
        }
    });
}
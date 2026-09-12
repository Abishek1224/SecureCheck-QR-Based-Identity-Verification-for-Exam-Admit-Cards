const params = new URLSearchParams(window.location.search);
const qrText = params.get("qr");
const resultCard = document.getElementById("result-card");
const apiConfigElement = document.getElementById("api-config");
const apiBaseUrl = apiConfigElement?.dataset.baseUrl ?? "";
const apiKey = apiConfigElement?.dataset.apiKey ?? "";
const apiKeyHeaderName = "X-Invigilator-Api-Key";

let trustedApiOrigin = "";

try {
    trustedApiOrigin = new URL(apiBaseUrl).origin;
}
catch {
    renderError("The invigilator client API configuration is invalid.");
}

if (!qrText) {
    renderError("No QR code data was received.");
} else if (trustedApiOrigin) {
    verifyCandidate(qrText);
}

function buildTrustedVerificationUrl(scannedQrText) {
    let scannedUrl;

    try {
        scannedUrl = new URL(scannedQrText);
    }
    catch {
        return null;
    }

    if (scannedUrl.origin !== trustedApiOrigin) {
        return null;
    }

    if (!scannedUrl.pathname.startsWith("/api/verify/")) {
        return null;
    }

    return new URL(
        scannedUrl.pathname + scannedUrl.search,
        trustedApiOrigin
    ).toString();
}

async function verifyCandidate(scannedQrText) {
    const verificationUrl = buildTrustedVerificationUrl(scannedQrText);

    if (!verificationUrl) {
        renderError("The scanned QR code points to an untrusted verification endpoint.");
        return;
    }

    try {
        clearResultCard();
        resultCard.appendChild(createTextElement("p", "Verifying candidate..."));

        const response = await fetch(verificationUrl, {
            headers: {
                [apiKeyHeaderName]: apiKey
            }
        });

        if (response.status === 401 || response.status === 403) {
            renderError("Invigilator authorization failed.");
            return;
        }

        if (!response.ok) {
            throw new Error("API returned HTTP " + response.status);
        }

        const data = await response.json();
        renderResult(data);
    }
    catch {
        renderError("Could not reach the verification service.");
    }
}

function renderResult(data) {
    clearResultCard();

    const verified = data.status === "Verified";
    const container = document.createElement("div");
    container.className = verified ? "result-pass" : "result-fail";

    container.appendChild(createTextElement("h2", verified ? "IDENTITY VERIFIED" : "VERIFICATION FAILED"));
    container.appendChild(createTextElement("p", data.message ?? "No message."));

    if (verified) {
        appendKeyValue(container, "Name", data.fullName ?? "N/A");
        appendKeyValue(container, "Roll Number", data.rollNumber ?? "N/A");
        appendKeyValue(container, "Subject", data.subject ?? "N/A");
        appendKeyValue(container, "Exam", data.examName ?? "N/A");

        if (isSafeImageUrl(data.photoUrl)) {
            const image = document.createElement("img");
            image.src = data.photoUrl;
            image.alt = "Candidate photo";
            image.width = 120;
            container.appendChild(image);
        }
    } else {
        appendKeyValue(container, "Status", data.status ?? "Unknown");
    }

    resultCard.appendChild(container);
}

function renderError(message) {
    clearResultCard();

    const container = document.createElement("div");
    container.className = "result-fail";
    container.appendChild(createTextElement("h2", "VERIFICATION FAILED"));
    container.appendChild(createTextElement("p", message));
    resultCard.appendChild(container);
}

function appendKeyValue(container, key, value) {
    const row = document.createElement("p");
    const label = document.createElement("strong");
    label.textContent = key + ":";
    row.appendChild(label);
    row.appendChild(document.createTextNode(" " + value));
    container.appendChild(row);
}

function createTextElement(tag, text) {
    const element = document.createElement(tag);
    element.textContent = text;
    return element;
}

function clearResultCard() {
    while (resultCard.firstChild) {
        resultCard.removeChild(resultCard.firstChild);
    }
}

function isSafeImageUrl(urlValue) {
    if (!urlValue) {
        return false;
    }

    try {
        const parsedUrl = new URL(urlValue, window.location.origin);
        return parsedUrl.protocol === "https:" || parsedUrl.protocol === "http:";
    }
    catch {
        return false;
    }
}

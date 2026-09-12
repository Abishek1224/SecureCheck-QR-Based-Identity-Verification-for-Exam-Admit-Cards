const params = new URLSearchParams(window.location.search);
const qrUrl = params.get("qr");

const resultCard = document.getElementById("result-card");

if (!qrUrl) {
    renderError("No QR code data was received.");
} else {
    verifyCandidate(qrUrl);
}

async function verifyCandidate(url) {
    try {
        resultCard.innerHTML = "<p>Verifying candidate...</p>";

        console.log("Calling verification API:", url);

        const response = await fetch(url);

        if (!response.ok) {
            throw new Error("API returned HTTP " + response.status);
        }

        const data = await response.json();

        console.log("Verification response:", data);

        renderResult(data);
    }
    catch (error) {
        console.error(error);

        renderError(
            "Could not reach the verification service."
        );
    }
}

function renderResult(data) {

    const verified = data.status === "Verified";

    if (verified) {

        resultCard.innerHTML = `
            <div class="result-pass">

                <h2>IDENTITY VERIFIED</h2>

                <p>${data.message}</p>

                <p>
                    <strong>Name:</strong>
                    ${data.fullName ?? "N/A"}
                </p>

                <p>
                    <strong>Roll Number:</strong>
                    ${data.rollNumber ?? "N/A"}
                </p>

                <p>
                    <strong>Subject:</strong>
                    ${data.subject ?? "N/A"}
                </p>

                <p>
                    <strong>Exam:</strong>
                    ${data.examName ?? "N/A"}
                </p>

                ${
                    data.photoUrl
                        ? `
                            <img
                                src="${data.photoUrl}"
                                alt="Candidate photo"
                                width="120"
                            />
                          `
                        : ""
                }

            </div>
        `;

    } else {

        resultCard.innerHTML = `
            <div class="result-fail">

                <h2>VERIFICATION FAILED</h2>

                <p>${data.message}</p>

                <p>
                    Status: ${data.status}
                </p>

            </div>
        `;
    }
}

function renderError(message) {

    resultCard.innerHTML = `
        <div class="result-fail">
            <h2>VERIFICATION FAILED</h2>
            <p>${message}</p>
        </div>
    `;
}
window.setTimeMasks = function () {
    Array.from(document.getElementsByClassName("time-mask")).forEach(elem => {
        IMask(
            elem, {
            mask: "00:00"
        });
    });
};

window.Jobs = {};
window.Jobs.viewDetailedMessage = function (title, body) {
    document.querySelector("#job-details-modal .modal-title").textContent = title;
    document.querySelector("#job-details-modal .modal-body").textContent = body;
    new bootstrap.Modal(document.getElementById("job-details-modal"), { keyboard: false }).show();
};

window.Jobs.showLiveOutputModal = function (jobId, workerName, dotNetHelper) {
    const modal = document.getElementById("live-output-modal");
    document.querySelector("#live-output-modal .modal-title").textContent = "Live Output - " + workerName;
    document.querySelector("#live-output-modal .modal-body").textContent = "Loading...";
    document.querySelector("#live-output-modal .modal-status").textContent = "";
    
    const bootstrapModal = new bootstrap.Modal(modal, { keyboard: false });
    bootstrapModal.show();
    
    // Store job ID and helper for polling
    modal.dataset.jobId = jobId;
    modal.dotNetHelper = dotNetHelper;
    
    // Start polling immediately
    window.Jobs.pollLiveOutput(jobId, dotNetHelper);
    
    // Set up interval to poll every 2 seconds
    const intervalId = setInterval(async () => {
        if (modal.classList.contains('show')) {
            await window.Jobs.pollLiveOutput(jobId, dotNetHelper);
        } else {
            clearInterval(intervalId);
            // Clean up the helper when modal is closed
            if (modal.dotNetHelper) {
                modal.dotNetHelper.dispose();
                modal.dotNetHelper = null;
            }
        }
    }, 2000);
    
    // Store interval ID to clear on modal close
    modal.dataset.intervalId = intervalId;
    
    // Clear interval when modal is hidden
    modal.addEventListener('hidden.bs.modal', function () {
        if (modal.dataset.intervalId) {
            clearInterval(parseInt(modal.dataset.intervalId));
            modal.dataset.intervalId = null;
        }
        if (modal.dotNetHelper) {
            modal.dotNetHelper.dispose();
            modal.dotNetHelper = null;
        }
    }, { once: true });
};

window.Jobs.pollLiveOutput = async function (jobId, dotNetHelper) {
    try {
        const response = await dotNetHelper.invokeMethodAsync('GetLiveOutputAsync', jobId);
        if (response) {
            const outputElem = document.querySelector("#live-output-modal .modal-body");
            outputElem.textContent = response.output || "** No output yet **";
            outputElem.scrollTop = outputElem.scrollHeight;
            
            const statusElem = document.querySelector("#live-output-modal .modal-status");
            if (!response.isRunning) {
                statusElem.textContent = "Job completed";
                statusElem.style.color = "green";
            } else {
                statusElem.textContent = "Running...";
                statusElem.style.color = "blue";
            }
        }
    } catch (error) {
        console.error("Error polling live output:", error);
        const outputElem = document.querySelector("#live-output-modal .modal-body");
        outputElem.textContent = "Error loading output: " + error.message;
    }
};

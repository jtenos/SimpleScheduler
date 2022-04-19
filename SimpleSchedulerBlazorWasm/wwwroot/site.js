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

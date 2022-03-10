window.setTimeMasks = function () {
    Array.from(document.getElementsByClassName('time-mask')).forEach(elem => {
        IMask(
            elem, {
            mask: '00:00'
        });
    });
};

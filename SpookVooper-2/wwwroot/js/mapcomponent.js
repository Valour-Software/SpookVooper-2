export function init() {
    const elem = document.getElementById('panzoom-parent')
    const panzoom = Panzoom(elem, {
        maxScale: 5
    })
    panzoom.pan(10, 10)
    panzoom.zoom(2, { animate: true })

    // Panning and pinch zooming are bound automatically (unless disablePan is true).
    // There are several available methods for zooming
    // that can be bound on button clicks or mousewheel.
    button.addEventListener('click', panzoom.zoomIn)
    elem.parentElement.addEventListener('wheel', panzoom.zoomWithWheel)
}